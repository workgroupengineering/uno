﻿#nullable enable

using System;
using System.Collections.Generic;
using Gtk;
using Microsoft.UI.Windowing;
using Windows.UI.Xaml;
using Uno.Disposables;
using Uno.Extensions.Specialized;
using Uno.Foundation.Logging;
using Uno.UI.Runtime.Skia.Gtk.Helpers.Dpi;
using Uno.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using WinUIApplication = Windows.UI.Xaml.Application;
using WinUIWindow = Windows.UI.Xaml.Window;

namespace Uno.UI.Runtime.Skia.Gtk.UI.Controls;

internal class GtkWindowWrapper : NativeWindowWrapperBase
{
	private readonly UnoGtkWindow _gtkWindow;
	private readonly DpiHelper _dpiHelper;

	private List<PendingWindowStateChangedInfo>? _pendingWindowStateChanged = new();
	private bool _wasShown;

	public GtkWindowWrapper(UnoGtkWindow gtkWindow, WinUIWindow window, XamlRoot xamlRoot) : base(window, xamlRoot)
	{
		_gtkWindow = gtkWindow ?? throw new ArgumentNullException(nameof(gtkWindow));
		_gtkWindow.Shown += OnWindowShown;
		_gtkWindow.Host.SizeChanged += OnHostSizeChanged;
		_gtkWindow.DeleteEvent += OnWindowClosing;
		_gtkWindow.Destroyed += OnWindowClosed;
		_gtkWindow.WindowStateEvent += OnWindowStateChanged;
		_dpiHelper = new DpiHelper(_gtkWindow);
		_dpiHelper.DpiChanged += OnDpiChanged;
	}

	private void OnDpiChanged(object? sender, EventArgs e) => RasterizationScale = _dpiHelper.RasterizationScale;

	public override string Title
	{
		get => _gtkWindow.Title;
		set => _gtkWindow.Title = value;
	}

	/// <summary>
	/// GTK overrides show as the initialization is asynchronous.
	/// </summary>
	public override async void Show(bool activateWindow)
	{
		try
		{
			await _gtkWindow.Host.InitializeAsync();
			base.Show(activateWindow);
		}
		catch (Exception ex)
		{
			this.Log().Error("Failed to initialize the UnoGtkWindow", ex);
		}
	}

	protected override void ShowCore() => _gtkWindow.ShowAll();

	private void OnWindowShown(object? sender, EventArgs e)
	{
		_wasShown = true;
		ReplayPendingWindowStateChanges();
	}

	public override object NativeWindow => _gtkWindow;

	internal protected override void Activate() => _gtkWindow.Activate();

	protected override void CloseCore()
	{
		if (_wasShown)
		{
			_gtkWindow.Close();
		}
	}

	public override void ExtendContentIntoTitleBar(bool extend)
	{
		base.ExtendContentIntoTitleBar(extend);
		_gtkWindow.ExtendContentIntoTitleBar(extend);
	}

	private void OnWindowClosed(object? sender, EventArgs e)
	{
		var windows = global::Gtk.Window.ListToplevels();
		if (!windows.Where(w => w is UnoGtkWindow && w != NativeWindow).Any())
		{
			WinUIApplication.Current.Exit();
		}
	}

	private void OnWindowClosing(object sender, DeleteEventArgs args)
	{
		var closingArgs = RaiseClosing();
		if (closingArgs.Cancel)
		{
			args.RetVal = true;
			return;
		}

		// Closing should continue, perform suspension.
		WinUIApplication.Current.RaiseSuspending();

		// All prerequisites passed, can safely close.
		args.RetVal = false;
	}

	private void OnHostSizeChanged(object? sender, Windows.Foundation.Size size)
	{
		Bounds = new Rect(default, size);
		VisibleBounds = Bounds;
		Size = size.ToSizeInt32();
	}

	private void OnWindowStateChanged(object o, WindowStateEventArgs args)
	{
		var newState = args.Event.NewWindowState;
		var changedMask = args.Event.ChangedMask;

		if (this.Log().IsEnabled(LogLevel.Debug))
		{
			this.Log().Debug($"OnWindowStateChanged: {newState}/{changedMask}");
		}

		if (_wasShown)
		{
			ProcessWindowStateChanged(newState, changedMask);
		}
		else
		{
			// Store state changes to replay once the application has been
			// initalized completely (initialization can be delayed if the render
			// surface is automatically detected).
			_pendingWindowStateChanged?.Add(new(newState, changedMask));
		}
	}

	private void ReplayPendingWindowStateChanges()
	{
		if (_pendingWindowStateChanged is not null)
		{
			foreach (var state in _pendingWindowStateChanged)
			{
				ProcessWindowStateChanged(state.newState, state.changedMask);
			}

			_pendingWindowStateChanged = null;
		}
	}

	private void ProcessWindowStateChanged(Gdk.WindowState newState, Gdk.WindowState changedMask)
	{
		var winUIApplication = WinUIApplication.Current;

		var isVisible =
			!(newState.HasFlag(Gdk.WindowState.Withdrawn) ||
			newState.HasFlag(Gdk.WindowState.Iconified));

		var isVisibleChanged =
			changedMask.HasFlag(Gdk.WindowState.Withdrawn) ||
			changedMask.HasFlag(Gdk.WindowState.Iconified);

		var focused = newState.HasFlag(Gdk.WindowState.Focused);
		var focusChanged = changedMask.HasFlag(Gdk.WindowState.Focused);

		if (!focused && focusChanged)
		{
			ActivationState = CoreWindowActivationState.Deactivated;
		}

		if (isVisibleChanged)
		{
			if (isVisible)
			{
				winUIApplication?.RaiseLeavingBackground(() => IsVisible = isVisible);
			}
			else
			{
				IsVisible = isVisible;
				winUIApplication?.RaiseEnteredBackground(null);
			}
		}

		if (focused && focusChanged)
		{
			ActivationState = Windows.UI.Core.CoreWindowActivationState.CodeActivated;
		}
	}

	protected override IDisposable ApplyFullScreenPresenter()
	{
		_gtkWindow.Fullscreen();

		return Disposable.Create(() => _gtkWindow.Unfullscreen());
	}

	protected override IDisposable ApplyOverlappedPresenter(OverlappedPresenter presenter)
	{
		presenter.SetNative(new NativeOverlappedPresenter(_gtkWindow));
		return Disposable.Create(() => presenter.SetNative(null));
	}
}

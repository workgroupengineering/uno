﻿using System;
using Android.App;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using Uno.Disposables;
using Uno.UI.Extensions;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Size = Windows.Foundation.Size;

namespace Uno.UI.Xaml.Controls;

internal class NativeWindowWrapper : NativeWindowWrapperBase
{
	private static readonly Lazy<NativeWindowWrapper> _instance = new(() => new NativeWindowWrapper());

	private readonly ActivationPreDrawListener _preDrawListener;
	private readonly DisplayInformation _displayInformation;

	private Rect _previousTrueVisibleBounds;

	public NativeWindowWrapper()
	{
		_preDrawListener = new ActivationPreDrawListener(this);
		CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBarChanged += RaiseNativeSizeChanged;

		_displayInformation = DisplayInformation.GetForCurrentViewSafe() ?? throw new InvalidOperationException("DisplayInformation must be available when the window is initialized");
		_displayInformation.DpiChanged += (s, e) => DispatchDpiChanged();
		DispatchDpiChanged();
	}

	public override object NativeWindow => Windows.UI.Xaml.ApplicationActivity.Instance?.Window;

	internal static NativeWindowWrapper Instance => _instance.Value;

	private void DispatchDpiChanged() =>
		RasterizationScale = (float)_displayInformation.RawPixelsPerViewPixel;

	public override string Title
	{
		get => Windows.UI.Xaml.ApplicationActivity.Instance.Title;
		set => Windows.UI.Xaml.ApplicationActivity.Instance.Title = value;
	}

	internal int SystemUiVisibility { get; set; }

	internal void OnNativeVisibilityChanged(bool visible) => IsVisible = visible;

	internal void OnActivityCreated() => AddPreDrawListener();

	internal void OnNativeActivated(CoreWindowActivationState state) => ActivationState = state;

	internal void OnNativeClosed() => RaiseClosing();

	internal bool IsStatusBarTranslucent()
	{
		if (!(ContextHelper.Current is Activity activity))
		{
			throw new Exception("Cannot check NavigationBar translucent property. Activity is not defined yet.");
		}

		return activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.TranslucentStatus)
			|| activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.LayoutNoLimits);
	}

	internal void RaiseNativeSizeChanged()
	{
		var (windowSize, visibleBounds, trueVisibleBounds) = GetVisualBounds();

		Bounds = new Rect(default, windowSize);
		VisibleBounds = visibleBounds;
		Size = new((int)(windowSize.Width * RasterizationScale), (int)(windowSize.Height * RasterizationScale));

		if (_previousTrueVisibleBounds != trueVisibleBounds)
		{
			_previousTrueVisibleBounds = trueVisibleBounds;

			// TODO: Adjust when multiple windows are supported on Android #13827
			ApplicationView.GetForCurrentView()?.SetTrueVisibleBounds(trueVisibleBounds);
		}
	}

	protected override void ShowCore() => RemovePreDrawListener();

	private (Size windowSize, Rect visibleBounds, Rect trueVisibleBounds) GetVisualBounds()
	{
		if (ContextHelper.Current is not Activity activity)
		{
			return default;
		}

		var windowInsets = GetWindowInsets(activity);

		var insetsTypes = WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout(); // == WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars() | WindowInsets.Type.CaptionBar();

		var opaqueInsetsTypes = insetsTypes;
		if (IsStatusBarTranslucent())
		{
			opaqueInsetsTypes &= ~WindowInsetsCompat.Type.StatusBars();
		}
		if (IsNavigationBarTranslucent())
		{
			opaqueInsetsTypes &= ~WindowInsetsCompat.Type.NavigationBars();
		}

		var insets = windowInsets?.GetInsets(insetsTypes).ToThickness() ?? default;
		var opaqueInsets = windowInsets?.GetInsets(opaqueInsetsTypes).ToThickness() ?? default;
		var translucentInsets = insets.Minus(opaqueInsets);

		// The native display size does not include any insets, so we remove the "opaque" insets under which we cannot draw anything
		var windowBounds = new Rect(default, GetDisplaySize().Subtract(opaqueInsets));

		// The visible bounds is the windows bounds on which we remove also translucentInsets
		var visibleBounds = windowBounds.DeflateBy(translucentInsets);

		return (windowBounds.PhysicalToLogicalPixels().Size, visibleBounds.PhysicalToLogicalPixels(), visibleBounds.PhysicalToLogicalPixels());
	}

	private bool IsNavigationBarTranslucent()
	{
		if (!(ContextHelper.Current is Activity activity))
		{
			throw new Exception("Cannot check NavigationBar translucent property. Activity is not defined yet.");
		}

		var flags = activity.Window.Attributes.Flags;
		return flags.HasFlag(WindowManagerFlags.TranslucentNavigation)
			|| flags.HasFlag(WindowManagerFlags.LayoutNoLimits);
	}

	private WindowInsetsCompat GetWindowInsets(Activity activity)
	{
		if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
		{
			return WindowInsetsCompat.ToWindowInsetsCompat(activity.WindowManager?.CurrentWindowMetrics.WindowInsets);
		}

		var decorView = activity.Window.DecorView;
		if (decorView.IsAttachedToWindow)
		{
			return ViewCompat.GetRootWindowInsets(decorView);
		}

		return null;
	}

	private Size GetDisplaySize()
	{
		if (ContextHelper.Current is not Activity activity)
		{
			return default;
		}

		Size displaySize = default;

		if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.R)
		{
			var windowMetrics = (ContextHelper.Current as Activity)?.WindowManager?.CurrentWindowMetrics;
			displaySize = new Size(windowMetrics.Bounds.Width(), windowMetrics.Bounds.Height());
		}
		else
		{
			SetDisplaySizeLegacy();
		}

		return displaySize;

		void SetDisplaySizeLegacy()
		{
			using var realMetrics = new DisplayMetrics();

#pragma warning disable 618
#pragma warning disable CA1422 // Validate platform compatibility
			activity.WindowManager?.DefaultDisplay.GetRealMetrics(realMetrics);
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore 618

			displaySize = new Size(realMetrics.WidthPixels, realMetrics.HeightPixels);
		}
	}

	protected override IDisposable ApplyFullScreenPresenter()
	{
		UpdateFullScreenMode(true);
		return Disposable.Create(() => UpdateFullScreenMode(false));
	}

	private void UpdateFullScreenMode(bool isFullscreen)
	{
#pragma warning disable 618
		var activity = ContextHelper.Current as Activity;
#pragma warning disable CA1422 // Validate platform compatibility
		var uiOptions = (int)activity.Window.DecorView.SystemUiVisibility;
#pragma warning restore CA1422 // Validate platform compatibility

		if (isFullscreen)
		{
			uiOptions |= (int)SystemUiFlags.Fullscreen;
			uiOptions |= (int)SystemUiFlags.ImmersiveSticky;
			uiOptions |= (int)SystemUiFlags.HideNavigation;
			uiOptions |= (int)SystemUiFlags.LayoutHideNavigation;
		}
		else
		{
			uiOptions &= ~(int)SystemUiFlags.Fullscreen;
			uiOptions &= ~(int)SystemUiFlags.ImmersiveSticky;
			uiOptions &= ~(int)SystemUiFlags.HideNavigation;
			uiOptions &= ~(int)SystemUiFlags.LayoutHideNavigation;
		}

#pragma warning disable CA1422 // Validate platform compatibility
		activity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)uiOptions;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore 618
	}

	private void AddPreDrawListener()
	{
		if (Uno.UI.ContextHelper.Current is Android.App.Activity activity &&
			activity.Window.DecorView is { } decorView)
		{
			decorView.ViewTreeObserver.AddOnPreDrawListener(_preDrawListener);
		}
	}

	private void RemovePreDrawListener()
	{
		if (Uno.UI.ContextHelper.Current is Android.App.Activity activity &&
			activity.Window.DecorView is { } decorView)
		{
			decorView.ViewTreeObserver.RemoveOnPreDrawListener(_preDrawListener);
		}
	}

	private sealed class ActivationPreDrawListener : Java.Lang.Object, ViewTreeObserver.IOnPreDrawListener
	{
		private readonly NativeWindowWrapper _windowWrapper;

		public ActivationPreDrawListener(NativeWindowWrapper windowWrapper)
		{
			_windowWrapper = windowWrapper;
		}

		public ActivationPreDrawListener(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public bool OnPreDraw() => _windowWrapper.IsVisible;
	}
}

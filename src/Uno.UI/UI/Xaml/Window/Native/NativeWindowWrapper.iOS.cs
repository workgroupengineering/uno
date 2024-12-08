﻿using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Uno.Disposables;
using Uno.UI.Controls;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.Graphics;

namespace Uno.UI.Xaml.Controls;

internal class NativeWindowWrapper : NativeWindowWrapperBase
{
	private static readonly Lazy<NativeWindowWrapper> _instance = new(() => new NativeWindowWrapper());

	private Uno.UI.Controls.Window _nativeWindow;

	private RootViewController _mainController;
	private NSObject _orientationRegistration;
	private readonly DisplayInformation _displayInformation;

	public NativeWindowWrapper()
	{
		_nativeWindow = new Uno.UI.Controls.Window();

		_mainController = Windows.UI.Xaml.Window.ViewControllerGenerator?.Invoke() ?? new RootViewController();
		_mainController.View.BackgroundColor = UIColor.Clear;
		_mainController.NavigationBarHidden = true;

		ObserveOrientationAndSize();

#if __MACCATALYST__
		_nativeWindow.SetOwner(CoreWindow.GetForCurrentThreadSafe());
#endif

		_displayInformation = DisplayInformation.GetForCurrentViewSafe() ?? throw new InvalidOperationException("DisplayInformation must be available when the window is initialized");
		_displayInformation.DpiChanged += (s, e) => DispatchDpiChanged();
		DispatchDpiChanged();
	}

	public override Uno.UI.Controls.Window NativeWindow => _nativeWindow;

	internal static NativeWindowWrapper Instance => _instance.Value;

	private void DispatchDpiChanged() =>
		RasterizationScale = (float)_displayInformation.RawPixelsPerViewPixel;

	protected override void ShowCore()
	{
		_nativeWindow.RootViewController = _mainController;
		_nativeWindow.MakeKeyAndVisible();
	}

	internal RootViewController MainController => _mainController;

	internal void OnNativeVisibilityChanged(bool visible) => IsVisible = visible;

	internal void OnNativeActivated(CoreWindowActivationState state) => ActivationState = state;

	internal void OnNativeClosed() => RaiseClosing(); // TODO: Handle closing cancellation when multiwindow is supported #13847

	internal void RaiseNativeSizeChanged()
	{
		var newWindowSize = GetWindowSize();

		Bounds = new Rect(default, newWindowSize);
		Size = new((int)(newWindowSize.Width * RasterizationScale), (int)(newWindowSize.Height * RasterizationScale));

		SetVisibleBounds(_nativeWindow, newWindowSize);
	}

	private void ObserveOrientationAndSize()
	{
		_orientationRegistration = UIApplication
			.Notifications
			.ObserveDidChangeStatusBarOrientation(
				(sender, args) => RaiseNativeSizeChanged()
			);

		_orientationRegistration = UIApplication
			.Notifications
			.ObserveDidChangeStatusBarFrame(
				(sender, args) => RaiseNativeSizeChanged()
			);

		_nativeWindow.FrameChanged +=
			() => RaiseNativeSizeChanged();

		_mainController.VisibleBoundsChanged +=
			() => RaiseNativeSizeChanged();

		var statusBar = StatusBar.GetForCurrentView();
		statusBar.Showing += (o, e) => RaiseNativeSizeChanged();
		statusBar.Hiding += (o, e) => RaiseNativeSizeChanged();

		RaiseNativeSizeChanged();
	}

	internal Size GetWindowSize()
	{
		var nativeFrame = NativeWindow?.Frame ?? CGRect.Empty;

		return new Size(nativeFrame.Width, nativeFrame.Height);
	}

	private void SetVisibleBounds(UIKit.UIWindow keyWindow, Windows.Foundation.Size windowSize)
	{
		var windowBounds = new Windows.Foundation.Rect(default, windowSize);

		var inset = UseSafeAreaInsets
				? keyWindow.SafeAreaInsets
				: UIEdgeInsets.Zero;

		// Not respecting its own documentation. https://developer.apple.com/documentation/uikit/uiview/2891103-safeareainsets?language=objc
		// iOS returns all zeros for SafeAreaInsets on non-iPhones and iOS11. (ignoring nav bars or status bars)
		// So we need to update the top inset depending of the status bar visibility on other devices
		var statusBarHeight = UIApplication.SharedApplication.StatusBarHidden
				? 0
				: UIApplication.SharedApplication.StatusBarFrame.Size.Height;

		inset.Top = (nfloat)Math.Max(inset.Top, statusBarHeight);

		var newVisibleBounds = new Windows.Foundation.Rect(
			x: windowBounds.Left + inset.Left,
			y: windowBounds.Top + inset.Top,
			width: windowBounds.Width - inset.Right - inset.Left,
			height: windowBounds.Height - inset.Top - inset.Bottom
		);

		VisibleBounds = newVisibleBounds;
	}

	private static bool UseSafeAreaInsets => UIDevice.CurrentDevice.CheckSystemVersion(11, 0);

	protected override IDisposable ApplyFullScreenPresenter()
	{
		CoreDispatcher.CheckThreadAccess();
		UIApplication.SharedApplication.StatusBarHidden = true;
		return Disposable.Create(() => UIApplication.SharedApplication.StatusBarHidden = false);
	}
}

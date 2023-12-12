﻿using System;
using System.Threading.Tasks;
using MUXControlsTestApp.Utilities;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;

namespace Uno.UI.RuntimeTests.MUX.Helpers
{
	internal static class FlyoutHelper
	{
		public static FrameworkElement GetOpenFlyoutPresenter(XamlRoot xamlRoot)
		{
#if NETFX_CORE
			var popups = VisualTreeHelper.GetOpenPopups(Window.Current);
#else
			var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(xamlRoot);
#endif
			if (popups.Count != 1)
			{
				throw new InvalidOperationException("Expected exactly one open Popup.");
			}

			return popups[0] ?? throw new InvalidOperationException("Popup child should not be null.");
		}

		public static void HideFlyout<T>(T flyoutControl)
			where T : FlyoutBase
		{
#if WINDOWS_UWP
			flyoutControl.Hide();
#else
			flyoutControl.Close();
#endif
		}

		internal static void OpenFlyout<T>(T flyoutControl, FrameworkElement target, FlyoutOpenMethod openMethod)
			where T : FlyoutBase
		{
#if WINDOWS_UWP
			flyoutControl.ShowAt(target);
#else
			flyoutControl.Open();
#endif
		}

		public static void ValidateOpenFlyoutOverlayBrush(string name)
		{
			throw new NotImplementedException();
		}

		public static async Task<Border> CreateTarget(
			double width,
			double height,
			Thickness margin,
			HorizontalAlignment halign,
			VerticalAlignment valign)
		{
			Border target = null;

			await RunOnUIThread.ExecuteAsync(() =>
			{
				target = new Border();
				target.Background = new SolidColorBrush(Colors.RoyalBlue);
				target.Height = height;
				target.Width = width;
				target.Margin = margin;
				target.HorizontalAlignment = halign;
				target.VerticalAlignment = valign;
			});

			return target;
		}
	}

	internal enum FlyoutOpenMethod
	{
		Mouse,
		Touch,
		Pen,
		Keyboard,
		Gamepad,
		Programmatic_ShowAt,
		Programmatic_ShowAttachedFlyout
	}
}

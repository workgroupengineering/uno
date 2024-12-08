﻿using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MUXControlsTestApp.Utilities;
using Uno.Disposables;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using Uno.Extensions;
using Private.Infrastructure;

namespace Uno.UI.RuntimeTests.MUX.Helpers
{
	internal static class ControlHelper
	{
		internal static async Task DoClickUsingTap(ButtonBase button)
		{
			var clickEvent = new TaskCompletionSource<object>();

			void OnButtonOnClick(object sender, RoutedEventArgs args)
			{
				clickEvent.TrySetResult(null);
			}


			await RunOnUIThread.ExecuteAsync(async () =>
			{
				button.Click += OnButtonOnClick;

				await InputHelper.Tap(button);

			});

			using var _ = Disposable.Create(() => button.Click -= OnButtonOnClick);

			await clickEvent.Task;
		}

		internal static async Task DoClickUsingAP(ButtonBase button)
		{
			var clickEvent = new TaskCompletionSource<object>();

			void OnButtonOnClick(object sender, RoutedEventArgs args)
			{
				clickEvent.TrySetResult(null);
			}


			await RunOnUIThread.ExecuteAsync(() =>
			{
				button.Click += OnButtonOnClick;

				var buttonAP = FrameworkElementAutomationPeer.CreatePeerForElement(button) as ButtonAutomationPeer;

				buttonAP.Invoke();
			});

			using var _ = Disposable.Create(() => button.Click -= OnButtonOnClick);

			await clickEvent.Task;
		}


		public static async Task ClickFlyoutCloseButton(DependencyObject element, bool isAccept)
		{
			// The Flyout close button could either be a part of the Flyout itself or it could be in the AppBar
			// We look in both places.
			ButtonBase button = default;

			await RunOnUIThread.ExecuteAsync(() =>
			{
				string buttonName = isAccept ? "AcceptButton" : "DismissButton";

				button = TreeHelper.GetVisualChildByNameFromOpenPopups(buttonName, element) as ButtonBase;
				;

				//if (button == null)
				//{
				//	var cmdBar = TestServices.Utilities.RetrieveOpenBottomCommandBar();
				//	if (cmdBar != null)
				//	{
				//		button = cmdBar.PrimaryCommands[isAccept ? 0 : 1] as ButtonBase;
				//	}
				//}
			});

			Assert.IsNotNull(button, "Close button not found");

			await DoClickUsingAP(button);
		}

		public static void ValidateUIElementTree(
			Size windowSizeOverride,
			double scale,
			Func<Task<Panel>> setup,
			Func<Task> cleanup = null,
			bool disableHittestingOnRoot = true,
			bool ignorePopups = false)
		{
			throw new NotImplementedException();
		}

		public static async Task<Rect> GetBounds(FrameworkElement element)
		{
			var rect = new Rect();
			await RunOnUIThread.ExecuteAsync(() =>
			{
				var point1 = element.TransformToVisual(null).TransformPoint(new Point(0, 0));
				var point2 = element.TransformToVisual(null).TransformPoint(new Point(element.ActualWidth, element.ActualHeight));

				rect.X = Math.Min(point1.X, point2.X);
				rect.Y = Math.Min(point1.Y, point2.Y);
				rect.Width = Math.Abs(point1.X - point2.X);
				rect.Height = Math.Abs(point1.Y - point2.Y);
			});

			return rect;
		}

		public static bool IsContainedIn(Rect inner, Rect outer)
		{
			var outerRight = outer.X + outer.Width;
			var outerBottom = outer.Y + outer.Height;
			var innerRight = inner.X + inner.Width;
			var innerBottom = inner.Y + inner.Height;

			return outer.X <= inner.X
				&& outer.Y <= inner.Y
				&& outerRight >= innerRight
				&& outerBottom >= innerBottom;
		}

		public static async Task<bool> IsInVisualState(Control control, string visualStateGroupName, string visualStateName)
		{
			bool result = false;
			await RunOnUIThread.ExecuteAsync(() =>
			{
				var rootVisual = (FrameworkElement)VisualTreeHelper.GetChild(control, 0);
				var visualStateGroup = GetVisualStateGroup(rootVisual, visualStateGroupName);
				result = visualStateGroup != null && visualStateName == visualStateGroup.CurrentState.Name;
			});
			return result;
		}

		public static void RemoveItem<T>(IList<T> items, T item)
		{
			var index = items?.IndexOf(item) ?? -1;
			if (index == -1)
			{
				throw new ArgumentOutOfRangeException("The item was not in the collection.");
			}
			items.RemoveAt(index);
		}

		private static VisualStateGroup GetVisualStateGroup(FrameworkElement control, string groupName)
		{
			VisualStateGroup group = null;
			var visualStateGroups = VisualStateManager.GetVisualStateGroups(control);
			if (visualStateGroups == null && control is ContentControl contentControl)
			{
				visualStateGroups = VisualStateManager.GetVisualStateGroups(contentControl);
			}

			if (visualStateGroups == null)
			{
				return group;
			}

			foreach (var visualStateGroup in visualStateGroups)
			{
				if (visualStateGroup.Name == groupName)
				{
					group = visualStateGroup;
					return group;
				}
			}
			return group;
		}

		internal static async Task<Point> GetCenterOfElement(FrameworkElement element)
		{
			Point offsetFromCenter = default;

			return await GetOffsetCenterOfElement(element, offsetFromCenter);
		}

		private static async Task<Point> GetOffsetCenterOfElement(FrameworkElement element, Point offsetFromCenter)
		{
			Point result = default;
			await RunOnUIThread.ExecuteAsync(() =>
			{
				var offsetCenterLocal = new Point((element.ActualWidth / 2) + offsetFromCenter.X, (element.ActualHeight / 2) + offsetFromCenter.Y);
				result = element.TransformToVisual(null).TransformPoint(offsetCenterLocal);
			});
			return result;
		}

		public static async Task EnsureFocused(Control control)
		{
			bool hasFocus = false;
			var gotFocusRegistration = Private.Infrastructure.TestServices.CreateSafeEventRegistration<Control, RoutedEventHandler>("GotFocus");
			gotFocusRegistration.Attach(control, (s, e) => hasFocus = true);

			await RunOnUIThread.ExecuteAsync(() =>
			{
				if (control.FocusState == FocusState.Unfocused)
				{
					control.Focus(FocusState.Programmatic);
				}
				else
				{
					hasFocus = true;
				}
			});

			await TestServices.WindowHelper.WaitFor(() => hasFocus);
			await TestServices.WindowHelper.WaitForIdle();
		}
	}
}

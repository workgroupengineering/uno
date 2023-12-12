﻿using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Preview.Injection;
using Uno.UI.RuntimeTests.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static Private.Infrastructure.TestServices;

using Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls.CommandBarPages;

#if __IOS__
using Uno.UI.Controls;
using Uno.UI.Helpers.WinUI;
using UIKit;
#endif

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls
{
	[TestClass]
	public class Given_CommandBar
	{
		[TestMethod]
		[RunsOnUIThread]
#if !__SKIA__
		[Ignore("InputInjector is only supported on skia")]
#endif
		public async Task When_Popup_Open_Then_Click_Outside()
		{
			var SUT = new CommandBar
			{
				SecondaryCommands =
				{
					new AppBarButton
					{
						Label = "secondary",
						Name = "SecondaryButton"
					}
				}
			};

			WindowHelper.WindowContent = SUT;
			await WindowHelper.WaitForIdle();

			var moreButton = (Button)SUT.FindName("MoreButton");

			var injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("Failed to init the InputInjector");
			using var finger = injector.GetFinger();

			Point GetCenter(Rect rect) => new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
			finger.Press(GetCenter(moreButton.GetAbsoluteBounds()));
			finger.Release();

			await WindowHelper.WaitForIdle();

			var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(SUT.XamlRoot);
			Assert.AreEqual(1, popups.Count);

			var secondaryButton = (AppBarButton)SUT.FindName("SecondaryButton");
			var bounds = secondaryButton.GetAbsoluteBounds();
			finger.Press(bounds.Bottom + 10, (bounds.Left + bounds.Right) / 2);
			finger.Release();

			await WindowHelper.WaitForIdle();

			Assert.AreEqual(0, VisualTreeHelper.GetOpenPopupsForXamlRoot(SUT.XamlRoot).Count);
		}

#if __IOS__
		[TestMethod]
		[RunsOnUIThread]
		[RequiresFullWindow]

		public async Task Can_Navigate_Forward_And_Backwards()
		{
			var frame = new Frame() { Width = 400, Height = 400 };
			var content = new Grid { Children = { frame } };

			WindowHelper.WindowContent = content;
			await WindowHelper.WaitForIdle();

			var firstNavBar = await frame.NavigateAndGetNavBar<CommandBarFirstPage>();

			await WindowHelper.WaitForLoaded(firstNavBar);

			var secondNavBar = await frame.NavigateAndGetNavBar<CommandBarSecondPage>();

			await WindowHelper.WaitForLoaded(secondNavBar);

			await Task.Delay(1000);

			frame.GoBack();

			await WindowHelper.WaitForLoaded(firstNavBar);
		}
#endif
	}

#if __IOS__
	public static class NavigationBarTestHelper
	{
		public static UINavigationBar GetNativeNavBar(this CommandBar navBar) => navBar
			?.TryGetNative<CommandBar, CommandBarRenderer, UINavigationBar>(out var native) ?? false ? native : null;

		public static UINavigationItem GetNativeNavItem(this CommandBar navBar) => navBar
			?.TryGetNative<CommandBar, CommandBarNavigationItemRenderer, UINavigationItem>(out var native) ?? false ? native : null;


		public static Task<CommandBar> NavigateAndGetNavBar<TPage>(this Frame frame) where TPage : Page
		{
			return frame.NavigateAndGetNavBar(typeof(TPage));
		}

		public static async Task<CommandBar> NavigateAndGetNavBar(this Frame frame, Type pageType)
		{
			frame.Navigate(pageType);
			await WindowHelper.WaitForIdle();

			var page = frame.Content as Page;
			await WindowHelper.WaitForLoaded(page!);
			return SharedHelpers.FindInVisualTreeByType<CommandBar>(page);
		}
	}
#endif
}

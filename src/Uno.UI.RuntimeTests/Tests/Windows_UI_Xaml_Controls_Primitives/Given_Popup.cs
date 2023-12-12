﻿using System;
using System.Threading.Tasks;
using Private.Infrastructure;
using Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls_Primitives.PopupPages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using static Private.Infrastructure.TestServices;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls_Primitives
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_Popup
	{
		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task Check_Can_Reach_Main_Visual_Tree()
		{
			var page = new ReachMainTreePage();
			WindowHelper.WindowContent = page;

			await WindowHelper.WaitForLoaded(page);

			Assert.IsTrue(CanReach(page.DummyTextBlock, page));

			try
			{
				page.TargetPopup.IsOpen = true;
				await WindowHelper.WaitForLoaded(page.PopupButton);

				Assert.IsTrue(CanReach(page.PopupButton, page));
			}
			finally
			{
				page.TargetPopup.IsOpen = false;
			}
		}

#if __ANDROID__
		[TestMethod]
		public async Task Check_Can_Reach_Main_Visual_Tree_Alternate_Mode()
		{
			var originalConfig = FeatureConfiguration.Popup.UseNativePopup;
			FeatureConfiguration.Popup.UseNativePopup = !originalConfig;
			try
			{
				await Check_Can_Reach_Main_Visual_Tree();
			}
			finally
			{
				FeatureConfiguration.Popup.UseNativePopup = originalConfig;
			}
		}
#endif

		[TestMethod]
		public void When_IsLightDismissEnabled_Default()
		{
			var popup = new Popup();
			Assert.IsFalse(popup.IsLightDismissEnabled);
		}

		[TestMethod]
		public void When_Closed_Immediately()
		{
			var popup = new Popup();
			popup.XamlRoot = TestServices.WindowHelper.XamlRoot;
			popup.IsOpen = true;
			// Should not throw
			popup.IsOpen = false;
		}

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_Removed_From_VisualTree()
		{
			var stackPanel = new StackPanel();
			var button = new Button() { Content = "Test" };
			var popup = new Popup()
			{
				Child = new Button() { Content = "Test" }
			};
			stackPanel.Children.Add(button);
			stackPanel.Children.Add(popup);
			WindowHelper.WindowContent = stackPanel;
			await WindowHelper.WaitForLoaded(stackPanel);

			Assert.IsFalse(popup.IsOpen);

			popup.IsOpen = true;

			Assert.AreEqual(1, VisualTreeHelper.GetOpenPopupsForXamlRoot(WindowHelper.XamlRoot).Count);

			stackPanel.Children.Remove(popup);
			await WindowHelper.WaitForIdle();

			Assert.IsFalse(popup.IsOpen);
			Assert.AreEqual(0, VisualTreeHelper.GetOpenPopupsForXamlRoot(WindowHelper.XamlRoot).Count);

			popup.IsOpen = true;
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(1, VisualTreeHelper.GetOpenPopupsForXamlRoot(WindowHelper.XamlRoot).Count);

			popup.IsOpen = false;
		}

		private static bool CanReach(DependencyObject startingElement, DependencyObject targetElement)
		{
			var currentElement = startingElement;
			while (currentElement != null)
			{
				if (currentElement == targetElement)
				{
					return true;
				}

				// Quoting WCT DataGrid:
				//		// Walk up the visual tree. Try using the framework element's
				//		// parent.  We do this because Popups behave differently with respect to the visual tree,
				//		// and it could have a parent even if the VisualTreeHelper doesn't find it.
				DependencyObject parent = null;
				if (currentElement is FrameworkElement fe)
				{
					parent = fe.Parent;
				}
				if (parent == null)
				{
					parent = VisualTreeHelper.GetParent(currentElement);
				}

				currentElement = parent;
			}

			// Did not hit targetElement
			return false;
		}
	}
}

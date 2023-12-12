﻿using System;
using Windows.UI;
using System.Threading.Tasks;
using Private.Infrastructure;
using Uno.UI.RuntimeTests.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_ToolTip
	{
		[TestMethod]
		public async Task When_DataContext_Set_On_ToolTip_Owner()
		{
			try
			{
				var textBlock = new TextBlock();
				var SUT = new ToolTip();
				ToolTipService.SetToolTip(textBlock, SUT);
				var stackPanel = new StackPanel
				{
					Children =
					{
						textBlock,
					}
				};

				TestServices.WindowHelper.WindowContent = stackPanel;
				await TestServices.WindowHelper.WaitForIdle();

				stackPanel.DataContext = "DataContext1";

				Assert.AreEqual("DataContext1", textBlock.DataContext);
				Assert.AreEqual("DataContext1", SUT.DataContext);

				SUT.IsOpen = true;

				stackPanel.DataContext = "DataContext2";

				Assert.AreEqual("DataContext2", textBlock.DataContext);
				Assert.AreEqual("DataContext2", SUT.DataContext);
			}
			finally
			{
#if HAS_UNO
				Microsoft.UI.Xaml.Media.VisualTreeHelper.CloseAllPopups(TestServices.WindowHelper.XamlRoot);
#endif
			}
		}

		[TestMethod]
		public async Task When_ToggleButton_DataContext_Set_On_ToolTip_Owner_After()
		{
#if HAS_UNO
			if (!FeatureConfiguration.ToolTip.UseToolTips)
			{
				Assert.Inconclusive();
			}
#endif

			try
			{
				var toggleButton = new ToggleButton();

				var textBlock = new TextBlock();
				textBlock.SetBinding(TextBlock.TextProperty, new Binding { Path = new(".") });

				var SUT = new ToolTip() { Content = textBlock };
				ToolTipService.SetToolTip(toggleButton, SUT);

				var stackPanel = new StackPanel
				{
					Children =
					{
						toggleButton,
					}
				};

				TestServices.WindowHelper.WindowContent = stackPanel;
				await TestServices.WindowHelper.WaitForIdle();

				stackPanel.DataContext = "DataContext1";

				Assert.AreEqual("DataContext1", toggleButton.DataContext);
				Assert.AreEqual("DataContext1", SUT.DataContext);

				SUT.IsOpen = true;

				stackPanel.DataContext = "DataContext2";

				Assert.AreEqual("DataContext2", toggleButton.DataContext);
				Assert.AreEqual("DataContext2", SUT.DataContext);
				Assert.AreEqual("DataContext2", textBlock.DataContext);
			}
			finally
			{
#if HAS_UNO
				Microsoft.UI.Xaml.Media.VisualTreeHelper.CloseAllPopups(TestServices.WindowHelper.XamlRoot);
#endif
			}
		}

		[TestMethod]
		public async Task When_ToggleButton_DataContext_Set_On_ToolTip_Owner_Before()
		{
#if HAS_UNO
			if (!FeatureConfiguration.ToolTip.UseToolTips)
			{
				Assert.Inconclusive();
			}
#endif

			try
			{
				var toggleButton = new ToggleButton();

				var textBlock = new TextBlock();
				textBlock.SetBinding(TextBlock.TextProperty, new Binding { Path = new(".") });

				var SUT = new ToolTip() { Content = textBlock };
				ToolTipService.SetToolTip(toggleButton, SUT);

				var stackPanel = new StackPanel
				{
					Children =
					{
						toggleButton,
					}
				};

				TestServices.WindowHelper.WindowContent = stackPanel;
				await TestServices.WindowHelper.WaitForIdle();

				stackPanel.DataContext = "DataContext1";

				Assert.AreEqual("DataContext1", toggleButton.DataContext);
				Assert.AreEqual("DataContext1", SUT.DataContext);

				// Set the datacontext before opening
				stackPanel.DataContext = "DataContext2";

				SUT.IsOpen = true;

				Assert.AreEqual("DataContext2", toggleButton.DataContext);
				Assert.AreEqual("DataContext2", SUT.DataContext);
				Assert.AreEqual("DataContext2", textBlock.DataContext);

				stackPanel.DataContext = "DataContext3";

				Assert.AreEqual("DataContext3", toggleButton.DataContext);
				Assert.AreEqual("DataContext3", SUT.DataContext);
				Assert.AreEqual("DataContext3", textBlock.DataContext);

				SUT.IsOpen = false;

				stackPanel.DataContext = "DataContext4";

				SUT.IsOpen = true;

				Assert.AreEqual("DataContext4", toggleButton.DataContext);
				Assert.AreEqual("DataContext4", SUT.DataContext);
				Assert.AreEqual("DataContext4", textBlock.DataContext);
			}
			finally
			{
#if HAS_UNO
				Microsoft.UI.Xaml.Media.VisualTreeHelper.CloseAllPopups(TestServices.WindowHelper.XamlRoot);
#endif
			}
		}

		[TestMethod]
		public async Task When_ToggleButton_DataContext_Set_On_ToolTip_Owner_Nested()
		{
#if HAS_UNO
			if (!FeatureConfiguration.ToolTip.UseToolTips)
			{
				Assert.Inconclusive();
			}
#endif

			try
			{
				var toggleButton = new ToggleButton();

				var textBlock = new TextBlock();
				textBlock.SetBinding(TextBlock.TextProperty, new Binding { Path = new(".") });

				var innerStackPanel = new StackPanel();
				innerStackPanel.Children.Add(textBlock);

				var SUT = new ToolTip() { Content = innerStackPanel };
				ToolTipService.SetToolTip(toggleButton, SUT);

				var stackPanel = new StackPanel
				{
					Children =
					{
						toggleButton,
					}
				};

				TestServices.WindowHelper.WindowContent = stackPanel;
				await TestServices.WindowHelper.WaitForIdle();

				SUT.DataContextChanged += (s, e) =>
				{
				};

				stackPanel.DataContext = "DataContext1";

				Assert.AreEqual("DataContext1", toggleButton.DataContext);
				Assert.AreEqual("DataContext1", SUT.DataContext);

				SUT.IsOpen = true;

				stackPanel.DataContext = "DataContext2";

				Assert.AreEqual("DataContext2", toggleButton.DataContext);
				Assert.AreEqual("DataContext2", SUT.DataContext);
				Assert.AreEqual("DataContext2", textBlock.DataContext);
			}
			finally
			{
#if HAS_UNO
				Microsoft.UI.Xaml.Media.VisualTreeHelper.CloseAllPopups(TestServices.WindowHelper.XamlRoot);
#endif
			}
		}

#if !__IOS__ // Disabled due to #10791
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		[TestMethod]
		public Task When_Switch_Theme_UWP() => When_Switch_Theme_Inner(brush => (brush as SolidColorBrush).Color);

#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		[TestMethod]
		public async Task When_Switch_Theme_Fluent()
		{
			using var _ = StyleHelper.UseFluentStyles();
			await When_Switch_Theme_Inner(brush => (brush as AcrylicBrush).TintColor);
		}

		private async Task When_Switch_Theme_Inner(Func<Brush, Color> backgroundColorGetter)
		{
#if HAS_UNO
			var originalToolTipsSetting = Uno.UI.FeatureConfiguration.ToolTip.UseToolTips;
			Uno.UI.FeatureConfiguration.ToolTip.UseToolTips = true;
#endif
			try
			{
				var textBlock = new TextBlock() { Text = "Test" };
				var SUT = new ToolTip()
				{
					Content = "I'm a ToolTip!"
				};
				ToolTipService.SetToolTip(textBlock, SUT);
				var stackPanel = new StackPanel
				{
					Children =
					{
						textBlock,
					}
				};

				TestServices.WindowHelper.WindowContent = stackPanel;
				await TestServices.WindowHelper.WaitForIdle();

				SUT.IsOpen = true;
				await TestServices.WindowHelper.WaitForIdle();
				await Task.Delay(1000);
				await TestServices.WindowHelper.WaitForIdle();

				var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(textBlock.XamlRoot);
				var popup = popups[0];
				var toolTipChild = popup.Child as ToolTip;
				Assert.AreEqual(SUT, toolTipChild);
				var color = backgroundColorGetter(toolTipChild.Background);
				Assert.IsTrue(color.R > 100 && color.G > 100 && color.B > 100);

				using var _ = ThemeHelper.UseDarkTheme();

				await TestServices.WindowHelper.WaitForIdle();
				color = backgroundColorGetter(toolTipChild.Background);
				Assert.IsTrue(color.R < 100 && color.G < 100 && color.B < 100);
			}
			finally
			{
#if HAS_UNO
				VisualTreeHelper.CloseAllPopups(TestServices.WindowHelper.XamlRoot);
				Uno.UI.FeatureConfiguration.ToolTip.UseToolTips = originalToolTipsSetting;
#endif
			}
		}
#endif

		[TestMethod]
		public async Task When_ToolTip_Popup_XamlRoot()
		{
#if HAS_UNO
			var originalToolTipsSetting = Uno.UI.FeatureConfiguration.ToolTip.UseToolTips;
			Uno.UI.FeatureConfiguration.ToolTip.UseToolTips = true;
#endif
			var toolTip = new ToolTip();
			try
			{
				var host = new Button() { Content = "Asd" };
				toolTip.Content = new Button() { Content = "Test" };

				TestServices.WindowHelper.WindowContent = host;
				await TestServices.WindowHelper.WaitForIdle();
				await TestServices.WindowHelper.WaitForLoaded(host);

				ToolTipService.SetToolTip(host, toolTip);
				toolTip.IsOpen = true;

				await TestServices.WindowHelper.WaitForIdle();
				await TestServices.WindowHelper.WaitForIdle();

				Assert.AreEqual(host.XamlRoot, toolTip.XamlRoot);
				var popups = VisualTreeHelper.GetOpenPopupsForXamlRoot(host.XamlRoot);
				Assert.AreEqual(host.XamlRoot, popups[0].XamlRoot);
				Assert.AreEqual(host.XamlRoot, popups[0].Child.XamlRoot);
			}
			finally
			{
				toolTip.IsOpen = false;
#if HAS_UNO
				Uno.UI.FeatureConfiguration.ToolTip.UseToolTips = originalToolTipsSetting;
#endif
			}
		}
#if HAS_UNO && !__MACOS__
#if __IOS__ || __ANDROID__
		[Ignore("Currently fails on Android and iOS")]
#endif
		[TestMethod]
		public async Task When_ToolTip_Owner_Clicked()
		{
			Button button = new Button()
			{
				Content = "Click when tooltip is visible",
			};
			ToolTip tooltip = new ToolTip
			{
				Content = "Tooltip should disappear when button is clicked!",
			};
			ToolTipService.SetToolTip(button, tooltip);
			TestServices.WindowHelper.WindowContent = button;
			await TestServices.WindowHelper.WaitForLoaded(button);
			await TestServices.WindowHelper.WaitForIdle();
			tooltip.IsOpen = true;
			await TestServices.WindowHelper.WaitForIdle();
			button.SafeRaiseEvent(UIElement.TappedEvent, new TappedRoutedEventArgs());
			await TestServices.WindowHelper.WaitForIdle();
			Assert.IsFalse(tooltip.IsOpen);
		}
#endif
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.UI.RuntimeTests.Helpers;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using static Private.Infrastructure.TestServices;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Uno.UI.RuntimeTests.Tests.Uno_UI_Xaml_Core;
using Windows.UI.Input.Preview.Injection;
using Microsoft.UI.Xaml.Input;
using Uno.Extensions;


#if NETFX_CORE
using Uno.UI.Extensions;
#elif __IOS__
using UIKit;
using _UIViewController = UIKit.UIViewController;
using Uno.UI.Controls;

using Windows.UI.Core;
using Microsoft.UI.Xaml.Media.Animation;
using static Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls.MultiFrame;
using Microsoft.UI.Xaml.Controls.Primitives;

#elif __MACOS__
using AppKit;
#else
using Uno.UI;
#endif

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_ComboBox
	{
		private ResourceDictionary _testsResources;

		private Style CounterComboBoxContainerStyle => _testsResources["CounterComboBoxContainerStyle"] as Style;

		private Style ComboBoxWithSeparatorStyle => _testsResources["ComboBoxWithSeparatorStyle"] as Style;

		private DataTemplate CounterItemTemplate => _testsResources["CounterItemTemplate"] as DataTemplate;

		private ItemsPanelTemplate MeasureCountCarouselPanelTemplate => _testsResources["MeasureCountCarouselPanel"] as ItemsPanelTemplate;

		[TestInitialize]
		public void Init()
		{
			_testsResources = new TestsResources();

			CounterGrid.Reset();
			CounterGrid2.Reset();
			MeasureCountCarouselPanel.Reset();
		}

		const int BorderThicknessAdjustment = 2; // Deduct BorderThickness on PopupBorder

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_ComboBox_MinWidth()
		{
			var source = Enumerable.Range(0, 5).ToArray();
			const int minWidth = 172;
			const int expectedItemWidth = minWidth - BorderThicknessAdjustment;
			var SUT = new ComboBox
			{
				MinWidth = minWidth,
				ItemsSource = source
			};

			try
			{
				WindowHelper.WindowContent = SUT;

				await WindowHelper.WaitForLoaded(SUT);

				await WindowHelper.WaitFor(() => SUT.ActualWidth == minWidth); // Needed for iOS where ComboBox may be initially too wide, for some reason

				SUT.IsDropDownOpen = true;

				ComboBoxItem cbi = null;
				foreach (var item in source)
				{
					await WindowHelper.WaitFor(() => (cbi = SUT.ContainerFromItem(item) as ComboBoxItem) != null);
					await WindowHelper.WaitForLoaded(cbi); // Required on Android
					Assert.AreEqual(expectedItemWidth, cbi.ActualWidth);
				}
			}
			finally
			{
				SUT.IsDropDownOpen = false;
				WindowHelper.WindowContent = null;
			}
		}

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_ComboBox_Constrained_By_Parent()
		{
			var source = Enumerable.Range(0, 5).ToArray();
			const int width = 133;
			const int expectedItemWidth = width - BorderThicknessAdjustment;
			var SUT = new ComboBox
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				ItemsSource = source
			};
			var grid = new Grid
			{
				Width = width
			};
			grid.Children.Add(SUT);

			try
			{
				WindowHelper.WindowContent = grid;

				await WindowHelper.WaitForLoaded(SUT);

				SUT.IsDropDownOpen = true;

				ComboBoxItem cbi = null;
				foreach (var item in source)
				{
					await WindowHelper.WaitFor(() => (cbi = SUT.ContainerFromItem(item) as ComboBoxItem) != null);
					await WindowHelper.WaitForLoaded(cbi); // Required on Android
					Assert.AreEqual(expectedItemWidth, cbi.ActualWidth);
				}
			}
			finally
			{
				SUT.IsDropDownOpen = false;
				WindowHelper.WindowContent = null;
			}
		}

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task Check_Creation_Count_Few_Items()
		{
			var source = Enumerable.Range(0, 5).ToArray();
			using (FeatureConfigurationHelper.UseTemplatePooling()) // If pooling is disabled, then the 'is template a container' check creates an extra template root
			{
				var SUT = new ComboBox
				{
					ItemsSource = source,
					ItemContainerStyle = CounterComboBoxContainerStyle,
					ItemTemplate = CounterItemTemplate
				};

				try
				{
					Assert.AreEqual(0, CounterGrid.CreationCount);
					Assert.AreEqual(0, CounterGrid2.CreationCount);
					WindowHelper.WindowContent = SUT;

					await WindowHelper.WaitForLoaded(SUT);

#if !__ANDROID__ && !__IOS__ // This does not hold on Android or iOS, possibly because ComboBox is not virtualized
					Assert.AreEqual(0, CounterGrid.CreationCount);
					Assert.AreEqual(0, CounterGrid2.CreationCount);
#endif

					SUT.IsDropDownOpen = true;

					ComboBoxItem cbi = null;
					await WindowHelper.WaitFor(() => (cbi = SUT.ContainerFromItem(source[0]) as ComboBoxItem) != null);
					await WindowHelper.WaitForLoaded(cbi); // Required on Android

					Assert.AreEqual(5, CounterGrid.CreationCount);
					Assert.AreEqual(5, CounterGrid2.CreationCount);
					Assert.AreEqual(5, CounterGrid.BindCount);
					Assert.AreEqual(5, CounterGrid2.BindCount);
				}
				finally
				{
					SUT.IsDropDownOpen = false;
					WindowHelper.WindowContent = null;
				}
			}
		}

		[TestMethod]
#if __IOS__ || __ANDROID__
		[Ignore("ComboBox is currently not virtualized on iOS and Android - #556")] // https://github.com/unoplatform/uno/issues/556
#endif
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task Check_Creation_Count_Many_Items()
		{
			var source = Enumerable.Range(0, 500).ToArray();
			var SUT = new ComboBox
			{
				ItemsSource = source,
				ItemContainerStyle = CounterComboBoxContainerStyle,
				ItemTemplate = CounterItemTemplate
			};

			try
			{
				WindowHelper.WindowContent = SUT;

				await WindowHelper.WaitForLoaded(SUT);

				Assert.AreEqual(0, CounterGrid.CreationCount);
				Assert.AreEqual(0, CounterGrid2.CreationCount);

				SUT.IsDropDownOpen = true;

				ComboBoxItem cbi = null;
				await WindowHelper.WaitFor(() => source.Any(i => (cbi = SUT.ContainerFromItem(i) as ComboBoxItem) != null)); // Windows loads up CarouselPanel with no selected item around the middle, other platforms may not
																															 //await WindowHelper.WaitFor(() => (cbi = SUT.ContainerFromItem(source[0]) as ComboBoxItem) != null);
				await WindowHelper.WaitForLoaded(cbi); // Required on Android

				const int maxCount = 30;
				NumberAssert.Less(CounterGrid.CreationCount, maxCount);
				NumberAssert.Less(CounterGrid2.CreationCount, maxCount);
			}
			finally
			{
				SUT.IsDropDownOpen = false;
				WindowHelper.WindowContent = null;
			}
		}

#if __IOS__
		[TestMethod]
		[RunsOnUIThread]
		public async Task Check_DropDown_Flyout_Margin_When_In_Modal()
		{
			MultiFrame multiFrame = new();
			var showModalButton = new Button();

			multiFrame.Children.Add(showModalButton);

			var source = Enumerable.Range(0, 6).ToArray();
			var SUT = new ComboBox
			{
				ItemsSource = source,
				Text = "Alignment",
				VerticalAlignment = VerticalAlignment.Center,
				PlaceholderText = "Testing",
				Style = ComboBoxWithSeparatorStyle
			};

			var modalPage = new Page();
			var gridContainer = new Grid()
			{
				Background = SolidColorBrushHelper.LightGreen
			};

			async void OpenModal(object sender, RoutedEventArgs e)
			{
				gridContainer.Children.Add(SUT);
				modalPage.Content = gridContainer;

				await multiFrame.OpenModal(FrameSectionsTransitionInfo.NativeiOSModal, modalPage);
			}

			try
			{
				var homePage = new Page();
				showModalButton.Click += OpenModal;
				homePage.Content = multiFrame;

				WindowHelper.WindowContent = homePage;

				await WindowHelper.WaitForLoaded(homePage);

				// Open Modal
				showModalButton.RaiseClick();

				await WindowHelper.WaitForLoaded(modalPage);

				SUT.IsDropDownOpen = true;

				await WindowHelper.WaitForIdle();

				var locationX = SUT.GetAbsoluteBoundsRect().Location.X;

				var popup = SUT.FindFirstChild<Popup>();
				var childX = popup?.Child?.Frame.X ?? 0;

				Assert.IsNotNull(ComboBoxWithSeparatorStyle);
				Assert.IsNotNull(popup);
				Assert.IsTrue(popup.IsOpen);
				Assert.AreEqual(locationX, childX, "ComboBox vs ComboBox.PopUp.Child Frame.X are not equal");
			}
			finally
			{
				showModalButton.Click -= OpenModal;
				SUT.IsDropDownOpen = false;
				await multiFrame.CloseModal();
				WindowHelper.WindowContent = null;
			}
		}
#endif

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task Check_Dropdown_Measure_Count()
		{
			var source = Enumerable.Range(0, 500).ToArray();
			var SUT = new ComboBox
			{
				ItemsSource = source,
				ItemsPanel = MeasureCountCarouselPanelTemplate
			};

			try
			{
				WindowHelper.WindowContent = SUT;

				await WindowHelper.WaitForLoaded(SUT);

				Assert.AreEqual(0, MeasureCountCarouselPanel.MeasureCount);
				Assert.AreEqual(0, MeasureCountCarouselPanel.ArrangeCount);

				SUT.IsDropDownOpen = true;

				ComboBoxItem cbi = null;
				await WindowHelper.WaitFor(() => source.Any(i => (cbi = SUT.ContainerFromItem(i) as ComboBoxItem) != null)); // Windows loads up CarouselPanel with no selected item around the middle, other platforms may not
				await WindowHelper.WaitForLoaded(cbi); // Required on Android

				NumberAssert.Greater(MeasureCountCarouselPanel.MeasureCount, 0);
				NumberAssert.Greater(MeasureCountCarouselPanel.ArrangeCount, 0);

#if __IOS__
				const int MaxAllowedCount = 15; // TODO: figure out why iOS measures more times
#else
				const int MaxAllowedCount = 5;
#endif
				NumberAssert.Less(MeasureCountCarouselPanel.MeasureCount, MaxAllowedCount);
				NumberAssert.Less(MeasureCountCarouselPanel.ArrangeCount, MaxAllowedCount);
			}
			finally
			{
				SUT.IsDropDownOpen = false;
				WindowHelper.WindowContent = null;
			}
		}

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_Fluent_And_Theme_Changed()
		{
			using (StyleHelper.UseFluentStyles())
			{
				var comboBox = new ComboBox
				{
					ItemsSource = new[] { 1, 2, 3 },
					PlaceholderText = "Select..."
				};

				WindowHelper.WindowContent = comboBox;
				await WindowHelper.WaitForLoaded(comboBox);

				var placeholderTextBlock = comboBox.FindFirstChild<TextBlock>(tb => tb.Name == "PlaceholderTextBlock");

				Assert.IsNotNull(placeholderTextBlock);

				var lightThemeForeground = TestsColorHelper.ToColor("#9E000000");
				var darkThemeForeground = TestsColorHelper.ToColor("#C5FFFFFF");

				Assert.AreEqual(lightThemeForeground, (placeholderTextBlock.Foreground as SolidColorBrush)?.Color);

				using (ThemeHelper.UseDarkTheme())
				{
					Assert.AreEqual(darkThemeForeground, (placeholderTextBlock.Foreground as SolidColorBrush)?.Color);
				}

				Assert.AreEqual(lightThemeForeground, (placeholderTextBlock.Foreground as SolidColorBrush)?.Color);
			}
		}

		[TestMethod]
		public async Task When_SelectedItem_Set_Before_ItemsSource()
		{
			var SUT = new ComboBox();
			var items = Enumerable.Range(0, 10);

			await UITestHelper.Load(SUT);

			SUT.SelectedItem = 5;
			await WindowHelper.WaitForIdle();
			Assert.IsNull(SUT.SelectedItem);

			SUT.ItemsSource = items;
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(5, SUT.SelectedItem);
		}

		[TestMethod]
		public async Task When_SelectedItem_Set_Then_SelectedIndex_Then_ItemsSource()
		{
			var SUT = new ComboBox();
			var items = Enumerable.Range(0, 10);

			await UITestHelper.Load(SUT);

			SUT.SelectedIndex = 3;
			await WindowHelper.WaitForIdle();
			Assert.IsNull(SUT.SelectedItem);
			Assert.AreEqual(-1, SUT.SelectedIndex);

			SUT.SelectedItem = 5;
			await WindowHelper.WaitForIdle();
			Assert.IsNull(SUT.SelectedItem);
			Assert.AreEqual(-1, SUT.SelectedIndex);

			SUT.ItemsSource = items;
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(5, SUT.SelectedItem);
			Assert.AreEqual(5, SUT.SelectedIndex);
		}

		[TestMethod]
		public async Task When_Tabbed()
		{
			var SUT = new ComboBox
			{
				Items =
				{
					new ComboBoxItem { Content = new TextBox { Text = "item1" } },
					new ComboBoxItem { Content = new TextBox { Text = "item2" } }
				}
			};
			var btn = new Button();
			var grid = new Grid
			{
				Children =
				{
					SUT,
					btn
				}
			};

			WindowHelper.WindowContent = grid;
			await WindowHelper.WaitForIdle();

			SUT.Focus(FocusState.Programmatic);
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(SUT, FocusManager.GetFocusedElement(SUT.XamlRoot));

			KeyboardHelper.Tab();
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(btn, FocusManager.GetFocusedElement(SUT.XamlRoot));
		}

		[TestMethod]
		public async Task When_Popup_Open_Tabbed()
		{
			var SUT = new ComboBox
			{
				Items =
				{
					new ComboBoxItem { Content = new TextBox { Text = "item1" } },
					new ComboBoxItem { Content = new TextBox { Text = "item2" } }
				}
			};
			var btn = new Button();
			var grid = new Grid
			{
				Children =
				{
					SUT,
					btn
				}
			};

			WindowHelper.WindowContent = grid;
			await WindowHelper.WaitForIdle();

			SUT.Focus(FocusState.Programmatic);
			await WindowHelper.WaitForIdle();

			Assert.AreEqual(SUT, FocusManager.GetFocusedElement(SUT.XamlRoot));

			KeyboardHelper.Space();
			await WindowHelper.WaitForIdle();

			Assert.IsTrue(SUT.IsDropDownOpen);
			Assert.IsTrue(FocusManager.GetFocusedElement(SUT.XamlRoot) is ComboBoxItem);

			KeyboardHelper.Tab();
			await WindowHelper.WaitForIdle();

			Assert.IsFalse(SUT.IsDropDownOpen);
			Assert.AreEqual(btn, FocusManager.GetFocusedElement(SUT.XamlRoot));
		}

		[TestMethod]
		public void When_Index_Is_Out_Of_Range_And_Later_Becomes_Valid()
		{
			var comboBox = new ComboBox();
			comboBox.SelectedIndex = 2;
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(2, comboBox.SelectedIndex);
		}

		[TestMethod]
		public void When_Index_Set_With_No_Items_Repeated()
		{
			var comboBox = new ComboBox();
			comboBox.SelectedIndex = 1;
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(1, comboBox.SelectedIndex);
			comboBox.Items.Clear();
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.SelectedIndex = 2;
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(2, comboBox.SelectedIndex);
		}

		[TestMethod]
		public void When_Index_Set_Out_Of_Range_When_Items_Exist()
		{
			var comboBox = new ComboBox();
			comboBox.Items.Add(new ComboBoxItem());
			Assert.ThrowsException<ArgumentException>(() => comboBox.SelectedIndex = 2);
		}

		[TestMethod]
		public void When_Index_Set_Negative_Out_Of_Range_When_Items_Exist()
		{
			var comboBox = new ComboBox();
			comboBox.Items.Add(new ComboBoxItem());
			Assert.ThrowsException<ArgumentException>(() => comboBox.SelectedIndex = -2);
		}

		[TestMethod]
		public void When_Index_Set_Negative_Out_Of_Range_When_Items_Do_Not_Exist()
		{
			var comboBox = new ComboBox();
			comboBox.SelectedIndex = -2;
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
		}

		[TestMethod]
		public void When_Index_Is_Explicitly_Set_To_Negative_After_Out_Of_Range_Value()
		{
			var comboBox = new ComboBox();
			comboBox.SelectedIndex = 2;
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.SelectedIndex = -1; // While SelectedIndex was already -1 (assert above), this *does* make a difference.
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex);
			comboBox.Items.Add(new ComboBoxItem());
			Assert.AreEqual(-1, comboBox.SelectedIndex); // Will no longer become 2
		}

		[TestMethod]
		public async Task When_Collection_Reset()
		{
			var SUT = new ComboBox();
			try
			{
				WindowHelper.WindowContent = SUT;

				var c = new MyObservableCollection<string>();
				c.Add("One");
				c.Add("Two");
				c.Add("Three");

				SUT.ItemsSource = c;

				await WindowHelper.WaitForIdle();

				Assert.AreEqual(SUT.Items.Count, 3);

				using (c.BatchUpdate())
				{
					c.Add("Four");
					c.Add("Five");
				}

				SUT.IsDropDownOpen = true;

				// Items are materialized when the popup is opened
				await WindowHelper.WaitForIdle();

				Assert.AreEqual(SUT.Items.Count, 5);
				Assert.IsNotNull(SUT.ContainerFromItem("One"));
				Assert.IsNotNull(SUT.ContainerFromItem("Four"));
				Assert.IsNotNull(SUT.ContainerFromItem("Five"));
			}
			finally
			{
				SUT.IsDropDownOpen = false;
			}
		}

		[TestMethod]
		public async Task When_ComboBoxItem_DataContext_Cleared()
		{
			// Arrange
			var stackPanel = new StackPanel();
			stackPanel.DataContext = Guid.NewGuid().ToString();
			var comboBox = new ComboBox();
			var originalSource = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			comboBox.ItemsSource = originalSource;
			comboBox.SelectedIndex = 0;
			stackPanel.Children.Add(comboBox);
			WindowHelper.WindowContent = stackPanel;
			await WindowHelper.WaitForLoaded(stackPanel);
			FrameworkElement itemContainer = null;

			// Act
			comboBox.IsDropDownOpen = true;
			await WindowHelper.WaitFor(() => (itemContainer = comboBox.ContainerFromIndex(0) as ComboBoxItem) is not null);
			itemContainer.DataContextChanged += (s, e) =>
			{
				// Assert
				Assert.AreNotEqual(stackPanel.DataContext, itemContainer.DataContext);
			};
			comboBox.IsDropDownOpen = false;
			await WindowHelper.WaitForIdle();
			comboBox.IsDropDownOpen = true;
			await WindowHelper.WaitForIdle();
			comboBox.IsDropDownOpen = false;
			var updatedSource = new List<int>() { 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
			comboBox.ItemsSource = updatedSource;
			await WindowHelper.WaitForIdle();
			comboBox.IsDropDownOpen = true;
			await WindowHelper.WaitForIdle();
			comboBox.IsDropDownOpen = false;
		}

		[TestMethod]
		public async Task When_Binding_Change()
		{
			var SUT = new ComboBox();
			try
			{
				WindowHelper.WindowContent = SUT;

				var c = new ObservableCollection<string>();
				c.Add("One");
				c.Add("Two");
				c.Add("Three");
				c.Add("Four");
				c.Add("Five");
				c.Add("Six");
				c.Add("2-One");
				c.Add("2-Two");
				c.Add("2-Three");
				c.Add("2-Four");
				c.Add("2-Five");
				c.Add("2-Six");

				SUT.SetBinding(ComboBox.ItemsSourceProperty, new Microsoft.UI.Xaml.Data.Binding() { Path = new("MySource") });
				SUT.SetBinding(ComboBox.SelectedItemProperty, new Microsoft.UI.Xaml.Data.Binding() { Path = new("SelectedItem"), Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay });

				SUT.DataContext = new { MySource = c, SelectedItem = "One" };

				await WindowHelper.WaitForIdle();

				SUT.IsDropDownOpen = true;

				await Task.Delay(100);

				WindowHelper.WindowContent = null;

				await Task.Delay(100);

				SUT.DataContext = null;

				await WindowHelper.WaitForIdle();

				WindowHelper.WindowContent = SUT;
				SUT.DataContext = new { MySource = c, SelectedItem = "Two" };

				Assert.AreEqual(SUT.Items.Count, 12);
			}
			finally
			{
				SUT.IsDropDownOpen = false;
			}
		}

#if HAS_UNO
		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_Full_Collection_Reset()
		{
			var SUT = new ComboBox();
			SUT.ItemTemplate = new DataTemplate(() =>
			{

				var tb = new TextBlock();
				tb.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = "Text" });
				tb.SetBinding(TextBlock.NameProperty, new Microsoft.UI.Xaml.Data.Binding { Path = "Text" });

				return tb;
			});

			try
			{
				WindowHelper.WindowContent = SUT;

				var c = new MyObservableCollection<ItemModel>();
				c.Add(new ItemModel { Text = "One" });
				c.Add(new ItemModel { Text = "Two" });
				c.Add(new ItemModel { Text = "Three" });

				SUT.ItemsSource = c;

				await WindowHelper.WaitForIdle();
				SUT.IsDropDownOpen = true;

				await WindowHelper.WaitForIdle();
				SUT.IsDropDownOpen = false;

				Assert.AreEqual(SUT.Items.Count, 3);

				using (c.BatchUpdate())
				{
					c.Clear();
					c.Add(new ItemModel { Text = "Five" });
					c.Add(new ItemModel { Text = "Six" });
					c.Add(new ItemModel { Text = "Seven" });
				}

				SUT.IsDropDownOpen = true;

				// Items are materialized when the popup is opened
				await WindowHelper.WaitForIdle();

				Assert.AreEqual(3, SUT.Items.Count);
				Assert.IsNotNull(SUT.ContainerFromItem(c[0]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[1]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[2]));

				Assert.IsNotNull(SUT.FindName("Seven"));
				Assert.IsNotNull(SUT.FindName("Five"));
				Assert.IsNotNull(SUT.FindName("Six"));
			}
			finally
			{
				SUT.IsDropDownOpen = false;
			}
		}
#endif

#if HAS_UNO
		[TestMethod]
		public async Task When_Recycling_Explicit_Items()
		{
			var SUT = new ComboBox();
			SUT.ItemTemplate = new DataTemplate(() =>
			{

				var tb = new TextBlock();
				tb.SetBinding(TextBlock.TextProperty, new Microsoft.UI.Xaml.Data.Binding { Path = "Text" });
				tb.SetBinding(TextBlock.NameProperty, new Microsoft.UI.Xaml.Data.Binding { Path = "Text" });

				return tb;
			});

			try
			{
				WindowHelper.WindowContent = SUT;

				var c = new MyObservableCollection<ComboBoxItem>();
				c.Add(new ComboBoxItem { Content = "One" });
				c.Add(new ComboBoxItem { Content = "Two" });
				c.Add(new ComboBoxItem { Content = "Three" });

				SUT.ItemsSource = c;

				await WindowHelper.WaitForIdle();
				SUT.IsDropDownOpen = true;

				await WindowHelper.WaitForIdle();

				Assert.AreEqual(3, SUT.Items.Count);
				Assert.IsNotNull(SUT.ContainerFromItem(c[0]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[1]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[2]));
				Assert.AreEqual("One", c[0].Content);
				Assert.AreEqual("Two", c[1].Content);
				Assert.AreEqual("Three", c[2].Content);

				await WindowHelper.WaitForIdle();

				SUT.IsDropDownOpen = false;

				await WindowHelper.WaitForIdle();

				// Open again to ensure that the items are recycled
				SUT.IsDropDownOpen = true;

				await WindowHelper.WaitForIdle();

				Assert.AreEqual(3, SUT.Items.Count);
				Assert.IsNotNull(SUT.ContainerFromItem(c[0]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[1]));
				Assert.IsNotNull(SUT.ContainerFromItem(c[2]));
				Assert.AreEqual("One", c[0].Content);
				Assert.AreEqual("Two", c[1].Content);
				Assert.AreEqual("Three", c[2].Content);

			}
			finally
			{
				SUT.IsDropDownOpen = false;
			}
		}
#endif

#if HAS_UNO
		[TestMethod]
		public void When_SelectedItem_TwoWay_Binding()
		{
			var itemsControl = new ItemsControl()
			{
				ItemsPanel = new ItemsPanelTemplate(() => new StackPanel()),
				ItemTemplate = new DataTemplate(() =>
				{
					var comboBox = new ComboBox();
					comboBox.Name = "combo";

					comboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Path = new("Numbers") });
					comboBox.SetBinding(
						ComboBox.SelectedItemProperty,
						new Binding { Path = new("SelectedNumber"), Mode = BindingMode.TwoWay });

					return comboBox;
				})
			};

			var test = new[] {
				new TwoWayBindingItem()
			};

			WindowHelper.WindowContent = itemsControl;

			itemsControl.ItemsSource = test;

			var comboBox = itemsControl.FindName("combo") as ComboBox;

			Assert.AreEqual(3, test[0].SelectedNumber);
			Assert.AreEqual(3, comboBox.SelectedItem);
		}
#endif

#if HAS_UNO
		[TestMethod]
		[RequiresFullWindow]
		[RunsOnUIThread]
#if !__SKIA__
		[Ignore("Pointer injection supported only on skia for now.")]
#endif
		public async Task When_Mouse_Opened_And_Closed()
		{
			// Create a comboBox with some sample items
			ComboBox comboBox = new ComboBox();
			for (int i = 0; i < 10; i++)
			{
				comboBox.Items.Add(i);
			}
			var stackPanel = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				Spacing = 10,
				Padding = new Thickness(10),
				VerticalAlignment = VerticalAlignment.Top
			};

			var text = new TextBlock() { Text = "Click me", VerticalAlignment = VerticalAlignment.Top };
			stackPanel.Children.Add(comboBox);
			stackPanel.Children.Add(text);

			// Set the comboBox as Window content
			WindowHelper.WindowContent = stackPanel;

			// Wait for it to load
			await WindowHelper.WaitForLoaded(comboBox);

			comboBox.SelectedItem = 5;
			await WindowHelper.WaitForIdle();

			// Take a screenshot of the comboBox before opening
			var screenshotBefore = await TakeScreenshot(stackPanel);

			// Use input injection to tap the comboBox and open the popup
			var comboBoxCenter = comboBox.GetAbsoluteBounds().GetCenter();

			using var finger = InputInjector.TryCreate().GetFinger();
			finger.MoveTo(comboBoxCenter);
			finger.Press(comboBoxCenter);
			await WindowHelper.WaitForIdle();
			finger.Release();

			// Wait for the popup to load and render
			await WindowHelper.WaitFor(() => VisualTreeHelper.GetOpenPopupsForXamlRoot(WindowHelper.XamlRoot).Count > 0);
			await WindowHelper.WaitForIdle();

			// Take a screenshot of the UI after opening the comboBox
			var screenshotOpened = await TakeScreenshot(stackPanel);

			// Verify that the UI changed
			await ImageAssert.AreNotEqualAsync(screenshotBefore, screenshotOpened);

			var textCenter = text.GetAbsoluteBounds().GetCenter();
			finger.Press(textCenter.X, textCenter.Y + 100);
			finger.Release();

			await WindowHelper.WaitForIdle();

			// Wait for the popup to close and the UI to stabilize
			// Take a screenshot of the UI after closing the comboBox
			var screenshotAfter = await TakeScreenshot(stackPanel);

			// Verify that the UI looks the same as at the beginning
			await ImageAssert.AreEqualAsync(screenshotBefore, screenshotAfter);
		}

		[TestMethod]
		[RequiresFullWindow]
		[RunsOnUIThread]
#if !__SKIA__
		[Ignore("Pointer injection supported only on skia for now.")]
#endif
		public async Task When_Mouse_Opened_And_Closed_Fluent()
		{
			using (StyleHelper.UseFluentStyles())
			{
				await When_Mouse_Opened_And_Closed();
			}
		}

		private async Task<RawBitmap> TakeScreenshot(FrameworkElement SUT)
		{
			var renderer = new RenderTargetBitmap();
			await renderer.RenderAsync(SUT);
			var result = await RawBitmap.From(renderer, SUT);
			return result;
		}
#endif

		[TestMethod]
		public async Task When_SelectedItem_TwoWay_Binding_Clear()
		{
			var root = new Grid();

			var comboBox = new ComboBox();

			root.Children.Add(comboBox);

			comboBox.SetBinding(ComboBox.ItemsSourceProperty, new Binding { Path = new("Items") });
			comboBox.SetBinding(ComboBox.SelectedItemProperty, new Binding { Path = new("Item"), Mode = BindingMode.TwoWay });

			WindowHelper.WindowContent = root;

			var dc = new TwoWayBindingClearViewModel();
			root.DataContext = dc;

			await WindowHelper.WaitForIdle();

			dc.Dispose();
			root.DataContext = null;

			Assert.AreEqual(1, dc.ItemGetCount);
			Assert.AreEqual(1, dc.ItemSetCount);
		}

		public sealed class TwoWayBindingClearViewModel : IDisposable
		{
			public enum Themes
			{
				Invalid,
				Day,
				Night
			}

			public TwoWayBindingClearViewModel()
			{
				_item = Items[0];
			}

			public Themes[] Items { get; } = new Themes[] { Themes.Day, Themes.Night };
			private Themes _item;
			private bool _isDisposed;
			public Themes Item
			{
				get
				{
					ItemGetCount++;

					if (_isDisposed)
					{
						return Themes.Invalid;
					}

					return _item;
				}
				set
				{
					ItemSetCount++;

					if (_isDisposed)
					{
						_item = Themes.Invalid;
						return;
					}

					_item = value;
				}
			}

			public int ItemGetCount { get; private set; }
			public int ItemSetCount { get; private set; }

			public void Dispose()
			{
				_isDisposed = true;
			}
		}

		public class TwoWayBindingItem : System.ComponentModel.INotifyPropertyChanged
		{
			private int _selectedNumber;

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

			public TwoWayBindingItem()
			{
				Numbers = new List<int> { 1, 2, 3, 4 };
				SelectedNumber = 3;
			}

			public List<int> Numbers { get; }

			public int SelectedNumber
			{
				get
				{
					return _selectedNumber;
				}
				set
				{
					_selectedNumber = value;
					PropertyChanged?.Invoke(this, new(nameof(Item)));
				}
			}
		}

		public class ItemModel
		{
			public string Text { get; set; }

			public override string ToString() => Text;
		}

		public class MyObservableCollection<TType> : ObservableCollection<TType>
		{
			private int _batchUpdateCount;

			public IDisposable BatchUpdate()
			{
				++_batchUpdateCount;

				return Uno.Disposables.Disposable.Create(Release);

				void Release()
				{
					if (--_batchUpdateCount <= 0)
					{
						OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					}
				}
			}

			protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
			{
				if (_batchUpdateCount > 0)
				{
					return;
				}

				base.OnCollectionChanged(e);
			}

			public void Append(TType item) => Add(item);

			public TType GetAt(int index) => this[index];
		}

	}


#if __IOS__
	#region "Helper classes for the iOS Modal Page (UIModalPresentationStyle.pageSheet)"
	public partial class MultiFrame : Grid
	{
		private readonly TaskCompletionSource<bool> _isReady = new TaskCompletionSource<bool>();

		private CoreDispatcher _dispatcher => Dispatcher;

		public MultiFrame()
		{
			Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			_isReady.TrySetResult(true);
		}

		public async Task OpenModal(FrameSectionsTransitionInfo transitionInfo, Page page) // Runs on background thread.
		{
			var uiViewController = new UiViewController(page);

			var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

			await rootController.PresentViewControllerAsync(uiViewController, animated: false);
		}

		public async Task CloseModal()
		{
			try
			{
				var rootController = UIApplication.SharedApplication.KeyWindow.RootViewController;

				await rootController.DismissViewControllerAsync(false);
			}
			catch (Exception) { /* purposely */ }
		}

		public class UiViewController : _UIViewController
		{
			public UiViewController(Page frame)
			{
				View = frame;
			}

			public UIViewControllerSectionsTransitionInfo OpeningTransitionInfo { get; set; }

			public void SetTransitionInfo(UIViewControllerSectionsTransitionInfo transitionInfo)
			{
				ModalInPresentation = !transitionInfo.AllowDismissFromGesture;
				ModalPresentationStyle = transitionInfo.ModalPresentationStyle;
				ModalTransitionStyle = transitionInfo.ModalTransitionStyle;
			}
		}

		public abstract class FrameSectionsTransitionInfo : SectionsTransitionInfo
		{
			/// <summary>
			/// The type of <see cref="FrameSectionsTransitionInfo"/>.
			/// </summary>
			public abstract FrameSectionsTransitionInfoTypes Type { get; }

			/// <summary>
			/// Gets the transition info for a suppressed transition. There is not visual animation when using this transition info.
			/// </summary>
			public static DelegatingFrameSectionsTransitionInfo SuppressTransition { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSuppressTransition);

			/// <summary>
			/// The new frame fades in or the previous frame fades out, depending on the layering.
			/// </summary>
			public static DelegatingFrameSectionsTransitionInfo FadeInOrFadeOut { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteFadeInOrFadeOut);

			/// <summary>
			/// The new frame slides up, hiding the previous frame.
			/// </summary>
			public static DelegatingFrameSectionsTransitionInfo SlideUp { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSlideUp);

			/// <summary>
			/// The previous frame slides down, revealing the new frame.
			/// </summary>
			public static DelegatingFrameSectionsTransitionInfo SlideDown { get; } = new DelegatingFrameSectionsTransitionInfo(ExecuteSlideDown);

			/// <summary>
			/// The frames are animated using a UIViewController with the default configuration.
			/// </summary>
			public static UIViewControllerSectionsTransitionInfo NativeiOSModal { get; } = new UIViewControllerSectionsTransitionInfo();

			private static Task ExecuteSlideDown(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
			{
				return Animations.SlideFrame1DownToRevealFrame2(frameToHide, frameToShow);
			}

			private static Task ExecuteSlideUp(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
			{
				return Animations.SlideFrame2UpwardsToHideFrame1(frameToHide, frameToShow);
			}

			private static Task ExecuteFadeInOrFadeOut(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
			{
				if (frameToShowIsAboveFrameToHide)
				{
					return Animations.FadeInFrame2ToHideFrame1(frameToHide, frameToShow);
				}
				else
				{
					return Animations.FadeOutFrame1ToRevealFrame2(frameToHide, frameToShow);
				}
			}

			private static Task ExecuteSuppressTransition(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
			{
				return Animations.CollapseFrame1AndShowFrame2(frameToHide, frameToShow);
			}
		}

		public enum FrameSectionsTransitionInfoTypes
		{
			/// <summary>
			/// The transition is applied by changing properties or animating properties of <see cref="Frame"/> objects.
			/// This is associated with the <see cref="DelegatingFrameSectionsTransitionInfo"/> class.
			/// </summary>
			FrameBased,

			/// <summary>
			/// The transition is applied by using the native iOS transitions offered by UIKit.
			/// This is associated with the <see cref="UIViewControllerSectionsTransitionInfo"/> class.
			/// </summary>
			UIViewControllerBased
		}

		public class DelegatingFrameSectionsTransitionInfo : FrameSectionsTransitionInfo
		{
			private readonly FrameSectionsTransitionDelegate _frameTranstion;

			/// <summary>
			/// Creates a new instance of <see cref="DelegatingFrameSectionsTransitionInfo"/>.
			/// </summary>
			/// <param name="frameTranstion">The method describing the transition.</param>
			public DelegatingFrameSectionsTransitionInfo(FrameSectionsTransitionDelegate frameTranstion)
			{
				_frameTranstion = frameTranstion;
			}

			///<inheritdoc/>
			public override FrameSectionsTransitionInfoTypes Type => FrameSectionsTransitionInfoTypes.FrameBased;

			/// <summary>
			/// Runs the transition.
			/// </summary>
			/// <param name="frameToHide">The <see cref="Frame"/> that must be hidden after the transition.</param>
			/// <param name="frameToShow">The <see cref="Frame"/> that must be visible after the transition.</param>
			/// <param name="frameToShowIsAboveFrameToHide">Flag indicating whether the frame to show is above the frame to hide in their parent container.</param>
			/// <returns>Task running the transition operation.</returns>
			public Task Run(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide)
			{
				return _frameTranstion(frameToHide, frameToShow, frameToShowIsAboveFrameToHide);
			}
		}

		public delegate Task FrameSectionsTransitionDelegate(Frame frameToHide, Frame frameToShow, bool frameToShowIsAboveFrameToHide);

		public class UIViewControllerSectionsTransitionInfo : FrameSectionsTransitionInfo
		{
			public UIViewControllerSectionsTransitionInfo(bool allowDismissFromGesture = true, UIModalPresentationStyle modalPresentationStyle = UIModalPresentationStyle.PageSheet, UIModalTransitionStyle modalTransitionStyle = UIModalTransitionStyle.CoverVertical)
			{
				AllowDismissFromGesture = allowDismissFromGesture;
				ModalPresentationStyle = modalPresentationStyle;
				ModalTransitionStyle = modalTransitionStyle;
			}

			public bool AllowDismissFromGesture { get; }

			public UIModalPresentationStyle ModalPresentationStyle { get; }

			public UIModalTransitionStyle ModalTransitionStyle { get; }

			public override FrameSectionsTransitionInfoTypes Type => FrameSectionsTransitionInfoTypes.UIViewControllerBased;
		}

		public static class Animations
		{
			/// <summary>
			/// The default duration of built-in animations, in seconds.
			/// </summary>
			public const double DefaultDuration = 0.250;

			/// <summary>
			/// Fades out <paramref name="frame1"/> to reveal <paramref name="frame2"/>.
			/// </summary>
			public static Task FadeOutFrame1ToRevealFrame2(Frame frame1, Frame frame2)
			{
				// 1. Disable the currently visible frame during the animation.
				frame1.IsHitTestVisible = false;

				// 2. Make the next frame visible so that we see it as the previous frame fades out.
				frame2.Opacity = 1;

				frame2.Visibility = Visibility.Visible;
				frame2.IsHitTestVisible = true;

				// 3. Fade out the frame.
				var storyboard = new Storyboard();
				AddFadeOut(storyboard, frame1);
				storyboard.Begin();

				return Task.CompletedTask;
			}

			/// <summary>
			/// Fades in <paramref name="frame1"/> to hide <paramref name="frame2"/>.
			/// </summary>
			public static Task FadeInFrame2ToHideFrame1(Frame frame1, Frame frame2)
			{
				// 1. Disable the currently visible frame during the animation.
				frame1.IsHitTestVisible = false;

				// 2. Make the next frame visible, but transparent.
				frame2.Opacity = 0;
				frame2.Visibility = Visibility.Visible;

				// 3. Fade in the frame.
				var storyboard = new Storyboard();
				AddFadeIn(storyboard, frame2);
				storyboard.Begin();

				// 4. Once the next frame is visible, enable it.
				frame2.IsHitTestVisible = true;

				return Task.CompletedTask;
			}

			/// <summary>
			/// Slides <paramref name="frame2"/> upwards to hide <paramref name="frame1"/>.
			/// </summary>
			public static Task SlideFrame2UpwardsToHideFrame1(Frame frame1, Frame frame2)
			{
				frame1.IsHitTestVisible = false;
				((TranslateTransform)frame2.RenderTransform).Y = frame1.ActualHeight;
				frame2.Opacity = 1;
				frame2.Visibility = Visibility.Visible;

				var storyboard = new Storyboard();
				AddSlideInFromBottom(storyboard, (TranslateTransform)frame2.RenderTransform);
				storyboard.Begin();

				frame2.IsHitTestVisible = true;

				return Task.CompletedTask;
			}

			/// <summary>
			/// Slides down <paramref name="frame1"/> to releave <paramref name="frame2"/>.
			/// </summary>
			public static Task SlideFrame1DownToRevealFrame2(Frame frame1, Frame frame2)
			{
				frame1.IsHitTestVisible = false;
				frame2.Opacity = 1;
				frame2.Visibility = Visibility.Visible;

				var storyboard = new Storyboard();
				AddSlideBackToBottom(storyboard, (TranslateTransform)frame1.RenderTransform, frame2.ActualHeight);
				storyboard.Begin();

				frame2.IsHitTestVisible = true;

				return Task.CompletedTask;
			}

			/// <summary>
			/// Collapses <paramref name="frame1"/> and make <paramref name="frame2"/> visible.
			/// </summary>
			public static Task CollapseFrame1AndShowFrame2(Frame frame1, Frame frame2)
			{
				frame1.Visibility = Visibility.Collapsed;
				frame2.IsHitTestVisible = false;

				frame2.Visibility = Visibility.Visible;
				frame2.Opacity = 1;
				frame2.IsHitTestVisible = true;

				return Task.CompletedTask;
			}

			private static void AddFadeIn(Storyboard storyboard, DependencyObject target)
			{
				var animation = new DoubleAnimation()
				{
					To = 1,
					Duration = new Duration(TimeSpan.FromSeconds(DefaultDuration)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Opacity");

				storyboard.Children.Add(animation);
			}

			private static void AddFadeOut(Storyboard storyboard, DependencyObject target)
			{
				var animation = new DoubleAnimation()
				{
					To = 0,
					Duration = new Duration(TimeSpan.FromSeconds(DefaultDuration)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Opacity");

				storyboard.Children.Add(animation);
			}

			private static void AddSlideInFromBottom(Storyboard storyboard, TranslateTransform target)
			{
				var animation = new DoubleAnimation()
				{
					To = 0,
					Duration = new Duration(TimeSpan.FromSeconds(DefaultDuration)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Y");

				storyboard.Children.Add(animation);
			}

			private static void AddSlideBackToBottom(Storyboard storyboard, TranslateTransform target, double translation)
			{
				var animation = new DoubleAnimation()
				{
					To = translation,
					Duration = new Duration(TimeSpan.FromSeconds(DefaultDuration)),
					EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
				};

				Storyboard.SetTarget(animation, target);
				Storyboard.SetTargetProperty(animation, "Y");

				storyboard.Children.Add(animation);
			}
		}

		public abstract class SectionsTransitionInfo
		{
		}
	}
	#endregion
#endif
}

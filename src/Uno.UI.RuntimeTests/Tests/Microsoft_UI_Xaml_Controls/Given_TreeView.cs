﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Private.Infrastructure;
using Uno.UI.RuntimeTests.ListViewPages;
#if NETFX_CORE
using Uno.UI.Extensions;
#elif __IOS__
using UIKit;
#elif __MACOS__
using AppKit;
#else
using Uno.UI;
#endif
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using static Private.Infrastructure.TestServices;
using Windows.Foundation;
using Windows.UI;
using Microsoft.UI.Xaml.Media;
using FluentAssertions;
using FluentAssertions.Execution;
using Uno.Extensions;
using Uno.UI.RuntimeTests.Helpers;
using System.ComponentModel;
using Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Controls;

namespace Uno.UI.RuntimeTests.Tests.Microsoft_UI_Xaml_Controls
{
	[TestClass]
	[RunsOnUIThread]
	public partial class Given_TreeView
	{
		private ResourceDictionary _testsResources;

		private DataTemplate TreeViewItemTemplate => _testsResources["TreeViewItemTemplate"] as DataTemplate;

		[TestInitialize]
		public void Init()
		{
			_testsResources = new TestsResources();
		}

		[TestMethod]
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_Sublist_Of_Last_Item_Cleared()
		{
			var initial_Depth_0 = 0;
			var initial_Depth_1 = 64;
			var initial_Depth_2 = 96;

			var treeView = new Microsoft.UI.Xaml.Controls.TreeView();
			treeView.ItemTemplate = TreeViewItemTemplate;
			var testViewModelItems = Enumerable.Range(0, 2).Select(_ => Get_Depth_0_Item()).ToArray();
			treeView.ItemsSource = testViewModelItems;
			WindowHelper.WindowContent = treeView;

			await WindowHelper.WaitForLoaded(treeView);

			await WindowHelper.WaitForNonNull(() => treeView.ContainerFromItem(testViewModelItems.Last().Items.First().Items.Last()));

			var currentItem = testViewModelItems.First().Items.Last().Items.First();

			Assert.IsNotNull(treeView.ContainerFromItem(currentItem));

			testViewModelItems.First().Items.Last().Items.Clear();

			await WindowHelper.WaitFor(() => treeView.ContainerFromItem(currentItem) == null);

			TestViewModelItem Get_Depth_0_Item() => new TestViewModelItem
			{
				Label = $"Item {++initial_Depth_0}",
				Items =
				{
					Get_Depth_1_Item(),
					Get_Depth_1_Item(),
					Get_Depth_1_Item(),
				}
			};

			TestViewModelItem Get_Depth_1_Item() => new TestViewModelItem
			{
				Label = $"Subitem {(char)(++initial_Depth_1)}",
				Items =
				{
					Get_Depth_2_Item(),
					Get_Depth_2_Item(),
				}
			};
			TestViewModelItem Get_Depth_2_Item() => new TestViewModelItem { Label = $"Subitem {(char)(++initial_Depth_2)}" };
		}

		public class TestViewModelItem : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public bool IsExpanded { get; set; } = true;

			public ObservableCollection<TestViewModelItem> Items { get; } = new ObservableCollection<TestViewModelItem>();

			public string Label
			{
				get { return _label; }
				set
				{
					if (_label != value)
					{
						_label = value;
						PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
					}
				}
			}
			string _label;
		}
	}
}

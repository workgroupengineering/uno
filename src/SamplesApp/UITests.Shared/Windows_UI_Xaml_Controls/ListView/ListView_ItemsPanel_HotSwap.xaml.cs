﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Shared.Windows_UI_Xaml_Controls.ListView
{

	[SampleControlInfo("ListView", nameof(ListView_ItemsPanel_HotSwap))]
	public sealed partial class ListView_ItemsPanel_HotSwap : UserControl
	{
		private readonly Random random = new Random(312);

		public ListView_ItemsPanel_HotSwap()
		{
			this.InitializeComponent();
			this.SampleListView.ItemsSource = Enumerable.Range(3, 12).ToArray();
		}

		private void SwapPanelButton_Click(object sender, RoutedEventArgs e)
		{
			var button = (global::Microsoft.UI.Xaml.Controls.Button)sender;
			var xaml = $"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><{button.Tag} /></ItemsPanelTemplate>"
				.Replace('\'', '"');

			this.SampleListView.ItemsPanel = (ItemsPanelTemplate)XamlReader.Load(xaml);
		}

		private void UpdateItemsSourceButton_Click(object sender, RoutedEventArgs e)
		{
			var start = random.Next(-50, 50);
			var count = random.Next(10, 20);

			this.SampleListView.ItemsSource = Enumerable.Range(start, count).ToArray();
		}
	}
}

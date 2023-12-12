﻿using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.UI;
using Uno.UI.Samples.Controls;

namespace UITests.Windows_UI_Xaml_Controls.Repeater
{
	[Sample("ItemsRepeater")]
	public sealed partial class StackLayout_Simple : Page
	{
		public StackLayout_Simple()
		{
			this.InitializeComponent();
		}

		public int[] Items { get; } = Enumerable.Range(1, 5000).ToArray();

		private void Scroll(object server, RoutedEventArgs routedEventArgs)
		{
			if (server is Button btn)
			{
				if (double.TryParse(btn.Tag as string, out var pct))
				{
					if (layout.Orientation == Orientation.Horizontal)
					{
						var offset = RepeaterScrollViewer.ScrollableHeight * pct;
						RepeaterScrollViewer.ChangeView(null, offset, null, true);
					}
					else
					{
						var offset = RepeaterScrollViewer.ScrollableWidth * pct;
						RepeaterScrollViewer.ChangeView(offset, null, null, true);
					}
				}
			}
		}

		private void Tree(object server, RoutedEventArgs routedEventArgs)
		{
#if HAS_UNO || HAS_UNO_WINUI
			var txt = this.ShowLocalVisualTree(0);
			Console.WriteLine(txt);
#endif
		}
	}
}

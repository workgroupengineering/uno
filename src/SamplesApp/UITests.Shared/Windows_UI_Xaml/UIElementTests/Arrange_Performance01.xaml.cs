﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Private.Infrastructure;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Shared.Windows_UI_Xaml.UIElementTests
{
	[SampleControlInfo("UIElement", nameof(Arrange_Performance01))]
	public sealed partial class Arrange_Performance01 : UserControl
	{

		public Arrange_Performance01()
		{
			this.InitializeComponent();
		}

		private void OnTest01(object sender, RoutedEventArgs e)
		{
			var _ = UnitTestDispatcherCompat.From(this).RunAsync(
				UnitTestDispatcherCompat.Priority.Normal,
				async () =>
				{
					var sw = Stopwatch.StartNew();

					const int iterationCount = 1000;

					for (int i = 0; i < iterationCount; i++)
					{
						test01Element.Arrange(new Rect(0, 0, i % 100, i % 100));
						await Task.Yield();
					}

					sw.Stop();

					result01.Text = $"Results {iterationCount} iterations {sw.Elapsed}";
				}
			);
		}
	}
}

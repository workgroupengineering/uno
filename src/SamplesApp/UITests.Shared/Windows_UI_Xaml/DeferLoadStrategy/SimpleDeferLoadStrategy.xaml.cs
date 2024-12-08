﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Globalization;

namespace Uno.UI.Samples.Content.UITests.DeferLoadStrategy
{
	[SampleControlInfo("XAML", "SimpleDeferLoadStrategy", typeof(Presentation.SamplePages.DeferLoadStrategyViewModel),
		Description = "SimpleDeferLoadStrategy - content should load after a brief delay")]
	public sealed partial class SimpleDeferLoadStrategy : UserControl
	{
		public SimpleDeferLoadStrategy()
		{
			this.InitializeComponent();
		}

		private void OnBorderLoaded(object sender, object args)
		{
			if (FindName("loadedResult") is TextBlock tb)
			{
				tb.Text = "Border is loaded after 3s";
			}
		}
	}
}

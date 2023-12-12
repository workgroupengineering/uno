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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace SamplesApp.Windows_UI_Xaml_Media.Geometry
{
	[SampleControlInfo("Geometry", "LineSegment")]
	public sealed partial class LineSegmentPage : Page
	{
		public LineSegmentPage()
		{
			this.InitializeComponent();
		}

		public void MovePointClick(object sender, RoutedEventArgs args)
		{
			LineToChange.Point = new Point(LineToChange.Point.X - 20, LineToChange.Point.Y - 20);
		}
	}
}

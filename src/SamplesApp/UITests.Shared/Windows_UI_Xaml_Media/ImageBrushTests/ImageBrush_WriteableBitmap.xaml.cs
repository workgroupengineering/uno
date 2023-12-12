﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Uno.UI.Samples.Controls;

namespace UITests.Windows_UI_Xaml_Media.ImageBrushTests
{
	[Sample("Brushes", "WriteableBitmap", IgnoreInSnapshotTests = true)]
	public sealed partial class ImageBrush_WriteableBitmap : Page
	{
		public ImageBrush_WriteableBitmap()
		{
			this.InitializeComponent();

			Loaded += OnSampleLoaded;
		}

		private void OnSampleLoaded(object sender, RoutedEventArgs e)
		{
			const int w = 10, h = 10;

			var pixel = new[] { Colors.BlueViolet.B, Colors.BlueViolet.G, Colors.BlueViolet.R, Colors.BlueViolet.A };
			var bitmap = new WriteableBitmap(w, h);

			using (var pixels = bitmap.PixelBuffer.AsStream())
			{
				for (var i = 0; i < bitmap.PixelBuffer.Length; i += pixel.Length)
				{
					pixels.Write(pixel, 0, pixel.Length);
				}
			}

			var brush = new ImageBrush
			{
				ImageSource = bitmap,
				Stretch = Stretch.Fill
			};
			SUT.Background = brush;
		}
	}
}

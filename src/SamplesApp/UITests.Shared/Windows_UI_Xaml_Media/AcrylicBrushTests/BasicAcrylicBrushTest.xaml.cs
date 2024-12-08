﻿using Uno.UI.Samples.Controls;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace UITests.Windows_UI_Xaml_Media.AcrylicBrushTests
{
	[SampleControlInfo("Brushes", description: "Demonstrates a basic Acrylic brush")]
	public sealed partial class BasicAcrylicBrushTest : Page
	{
		public BasicAcrylicBrushTest()
		{
			this.InitializeComponent();
		}

		private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			acrylicBrush.TintOpacity = e.NewValue / 100.0d;
		}

		private void LuminositySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			acrylicBrush.TintLuminosityOpacity = e.NewValue / 100.0d;
		}
	}
}

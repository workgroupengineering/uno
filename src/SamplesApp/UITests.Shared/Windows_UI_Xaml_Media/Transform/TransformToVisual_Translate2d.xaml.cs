﻿using Windows.Foundation;
using Microsoft.UI.Xaml.Controls;
using Uno.UI.Samples.Controls;

namespace SamplesApp.Wasm.Windows_UI_Xaml_Media.Transform
{
	[SampleControlInfo("Transform", "TransformToVisual_Translate2d", description: "The ellipses should track the x- and y-positions of the duck inside the scroll viewer, using TransformToVisual")]
	public sealed partial class TransformToVisual_Translate2d : UserControl
	{
		public TransformToVisual_Translate2d()
		{
			this.InitializeComponent();
		}

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			var transform = TargetView.TransformToVisual(EnclosingView);
			var target = transform.TransformPoint(new Point(0, 0));
			Microsoft.UI.Xaml.Controls.Canvas.SetLeft(TrackerViewX, target.X);
			Microsoft.UI.Xaml.Controls.Canvas.SetTop(TrackerViewY, target.Y);

			OffsetTextBlock.Text = target.ToString();
		}
	}
}

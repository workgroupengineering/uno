﻿using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Uno.UI.Samples.Controls;
using System.Threading.Tasks;
using UITests.Shared.Helpers;

namespace SamplesApp.Wasm.Windows_UI_Xaml_Media.Transform
{
	[SampleControlInfo("Transform", "TransformToVisual_Translate2d", description: "The ellipses should track the x- and y-positions of the duck inside the scroll viewer, using TransformToVisual")]
	public sealed partial class TransformToVisual_Translate2d : UserControl, IWaitableSample
	{
		private readonly Task _samplePreparedTask;

		public TransformToVisual_Translate2d()
		{
			this.InitializeComponent();
			_samplePreparedTask = WaitableSampleImageHelpers.WaitAllImages(image1);
		}

		public Task SamplePreparedTask => _samplePreparedTask;

		private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
		{
			var transform = TargetView.TransformToVisual(EnclosingView);
			var target = transform.TransformPoint(new Point(0, 0));
			Windows.UI.Xaml.Controls.Canvas.SetLeft(TrackerViewX, target.X);
			Windows.UI.Xaml.Controls.Canvas.SetTop(TrackerViewY, target.Y);

			OffsetTextBlock.Text = target.ToString();
		}
	}
}

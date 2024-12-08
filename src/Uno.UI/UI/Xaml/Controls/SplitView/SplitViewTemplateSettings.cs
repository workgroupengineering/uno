﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace Windows.UI.Xaml.Controls.Primitives
{
	public sealed partial class SplitViewTemplateSettings : DependencyObject
	{
		public SplitViewTemplateSettings(SplitView splitView)
		{
			InitializeBinder();

			CompactPaneLength = splitView.CompactPaneLength;
			OpenPaneLength = splitView.OpenPaneLength;
		}

		public GridLength CompactPaneGridLength { get { return new GridLength((float)CompactPaneLength, GridUnitType.Pixel); } }
		public GridLength OpenPaneGridLength { get { return new GridLength((float)OpenPaneLength, GridUnitType.Pixel); } }
		public double NegativeOpenPaneLength { get { return -OpenPaneLength; } }
		public double NegativeOpenPaneLengthMinusCompactLength { get { return NegativeOpenPaneLength - CompactPaneLength; } }
		public double OpenPaneLengthMinusCompactLength { get { return OpenPaneLength - CompactPaneLength; } }

		public double OpenPaneLength { get; internal set; }
		public double CompactPaneLength { get; internal set; }

		/// <summary>
		/// These properties were added to facilitate clipping while RectangleGeometry.Transform is not supported
		/// TODO: Remove and use NegativeOpenPaneLengthMinusCompactLength and OpenPaneLengthMinusCompactLength instead
		/// </summary>
		public RectangleGeometry LeftClip { get { return new RectangleGeometry { Rect = new Rect(0, 0, CompactPaneLength, ViewHeight) }; } }
		public RectangleGeometry RightClip { get { return new RectangleGeometry { Rect = new Rect(OpenPaneLengthMinusCompactLength, 0, CompactPaneLength, ViewHeight) }; } }

		public double ViewHeight { get; internal set; } = 2000;
	}
}

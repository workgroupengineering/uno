﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Microsoft.UI.Xaml.Controls
{
	internal partial class PickerFlyoutPopupPanel : FlyoutBasePopupPanel
	{
		public PickerFlyoutPopupPanel(FlyoutBase flyout) : base(flyout)
		{
		}

		protected override Rect CalculatePopupPlacement(Popup popup, Size desiredSize, Size maxSize)
		{
			// A picker is often displayed at the bottom of the screen [...]
			// The width of a picker is either the width of the screen or its enclosing view, depending on the device and context.
			// -- https://developer.apple.com/design/human-interface-guidelines/ios/controls/pickers/
			return new Rect(
				0,
				Math.Max(0, ActualHeight - desiredSize.Height),
				ActualWidth,
				Math.Min(desiredSize.Height, ActualHeight)
			);
		}
	}
}

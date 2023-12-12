﻿using System;
using System.Collections.Generic;
using Windows.Foundation;
using System.Text;
using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno;
using Uno.UI;

#if __ANDROID__
using View = Android.Views.View;
using Font = Android.Graphics.Typeface;
using Android.Graphics;
#elif __IOS__
using UIKit;
using View = UIKit.UIView;
using Color = UIKit.UIColor;
using Font = UIKit.UIFont;
#else
#endif

namespace Microsoft.UI.Xaml.Controls
{
	partial class StackPanel
	{
		protected override Size MeasureOverride(Size availableSize)
		{
			var borderAndPaddingSize = BorderAndPaddingSize;
			availableSize = availableSize.Subtract(borderAndPaddingSize);

			var desiredSize = default(Size);
			var isHorizontal = Orientation == Microsoft.UI.Xaml.Controls.Orientation.Horizontal;
			var slotSize = availableSize;

			if (isHorizontal)
			{
				slotSize.Width = float.PositiveInfinity;
			}
			else
			{
				slotSize.Height = float.PositiveInfinity;
			}

			// Shadow variables for evaluation performance
			var spacing = Spacing;
			var count = Children.Count;

			for (int i = 0; i < count; i++)
			{
				var view = Children[i];

				var measuredSize = MeasureElement(view, slotSize);
				var addSpacing = i != count - 1;

				if (isHorizontal)
				{
					desiredSize.Width += measuredSize.Width;
					desiredSize.Height = Math.Max(desiredSize.Height, measuredSize.Height);

					if (addSpacing)
					{
						desiredSize.Width += spacing;
					}
				}
				else
				{
					desiredSize.Width = Math.Max(desiredSize.Width, measuredSize.Width);
					desiredSize.Height += measuredSize.Height;

					if (addSpacing)
					{
						desiredSize.Height += spacing;
					}
				}
			}

			return desiredSize.Add(borderAndPaddingSize);
		}

		protected override Size ArrangeOverride(Size arrangeSize)
		{
			var borderAndPaddingSize = BorderAndPaddingSize;
			arrangeSize = arrangeSize.Subtract(borderAndPaddingSize);

			var childRectangle = new Windows.Foundation.Rect(BorderThickness.Left + Padding.Left, BorderThickness.Top + Padding.Top, arrangeSize.Width, arrangeSize.Height);

			var isHorizontal = Orientation == Microsoft.UI.Xaml.Controls.Orientation.Horizontal;
			var previousChildSize = 0.0;

			if (this.Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
			{
				this.Log().Debug($"StackPanel/{Name}: Arranging {Children.Count} children.");
			}

			// Shadow variables for evaluation performance
			var spacing = Spacing;
			var count = Children.Count;

			var snapPoints = (_snapPoints ??= new List<float>(count)) as List<float>;

			var snapPointsChanged = snapPoints.Count != count;

			if (snapPoints.Capacity < count)
			{
				snapPoints.Capacity = count;
			}

			while (snapPoints.Count < count)
			{
				snapPoints.Add(default);
			}

			while (snapPoints.Count > count)
			{
				snapPoints.RemoveAt(count);
			}

			for (var i = 0; i < count; i++)
			{
				var view = Children[i];
				var desiredChildSize = GetElementDesiredSize(view);
				var addSpacing = i != 0;

				if (isHorizontal)
				{
					childRectangle.X += previousChildSize;

					if (addSpacing)
					{
						childRectangle.X += spacing;
					}

					previousChildSize = desiredChildSize.Width;
					childRectangle.Width = desiredChildSize.Width;
					childRectangle.Height = Math.Max(arrangeSize.Height, desiredChildSize.Height);

					var snapPoint = (float)(childRectangle.Right + Margin.Right);
					snapPointsChanged |= snapPoints[i] == snapPoint;
					snapPoints[i] = snapPoint;
				}
				else
				{
					childRectangle.Y += previousChildSize;

					if (addSpacing)
					{
						childRectangle.Y += spacing;
					}

					previousChildSize = desiredChildSize.Height;
					childRectangle.Height = desiredChildSize.Height;
					childRectangle.Width = Math.Max(arrangeSize.Width, desiredChildSize.Width);

					var snapPoint = (float)(childRectangle.Bottom + Margin.Bottom);
					snapPointsChanged |= snapPoints[i] == snapPoint;
					snapPoints[i] = snapPoint;
				}

				var adjustedRectangle = childRectangle;

				ArrangeElement(view, adjustedRectangle);
			}

			var finalSizeWithBorderAndPadding = arrangeSize.Add(borderAndPaddingSize);

			if (snapPointsChanged)
			{
				if (isHorizontal)
				{
					HorizontalSnapPointsChanged?.Invoke(this, this);
				}
				else
				{
					VerticalSnapPointsChanged?.Invoke(this, this);
				}
			}

			return finalSizeWithBorderAndPadding;
		}
	}
}

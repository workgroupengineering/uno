﻿using Windows.Foundation;
using Uno.Extensions;
using Windows.UI.Composition;
using System.Numerics;
using SkiaSharp;

namespace Windows.UI.Xaml.Shapes
{
	partial class Ellipse : Shape
	{
		public Ellipse()
		{
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			var (_, renderingArea) = ArrangeRelativeShape(finalSize);

			Render(renderingArea.Width > 0 && renderingArea.Height > 0
				? GetGeometry(renderingArea)
				: null);

			return finalSize;
		}

		private SkiaGeometrySource2D GetGeometry(Rect renderingArea)
		{
			var path = new SKPath();
			path.AddOval(new SKRect((float)renderingArea.X, (float)renderingArea.Y, (float)renderingArea.Right, (float)renderingArea.Bottom));
			var geometry = new SkiaGeometrySource2D(path);

			return geometry;
		}
	}
}

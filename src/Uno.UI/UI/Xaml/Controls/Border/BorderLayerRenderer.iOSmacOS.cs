﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using CoreAnimation;
using CoreGraphics;
using CoreImage;
using Foundation;
using Uno;
using Uno.Disposables;
using Uno.Extensions;
using Uno.UI;
using Uno.UI.Extensions;
using Uno.UI.Xaml.Media;
using ObjCRuntime;

#if __IOS__
using UIKit;
using _View = UIKit.UIView;
using _Color = UIKit.UIColor;
using _Image = UIKit.UIImage;
#elif __MACOS__
using AppKit;
using _View = AppKit.NSView;
using _Color = AppKit.NSColor;
using _Image = AppKit.NSImage;
#endif

using RadialGradientBrush = Microsoft.UI.Xaml.Media.RadialGradientBrush;

namespace Microsoft.UI.Xaml.Shapes
{
	partial class BorderLayerRenderer
	{
		// Creates a unique native CGColor for the transparent color, and make sure to keep a strong ref on it
		// https://github.com/unoplatform/uno/issues/10283
		private static readonly CGColor _transparent = Colors.Transparent;

		private LayoutState _previousLayoutState;

		private SerialDisposable _layerDisposable = new SerialDisposable();

		/// <summary>
		/// Updates or creates a sublayer to render a border-like shape.
		/// </summary>
		/// <param name="owner">The parent layer to apply the shape</param>
		/// <param name="area">The rendering area</param>
		/// <param name="background">The background brush</param>
		/// <param name="borderThickness">The border thickness</param>
		/// <param name="borderBrush">The border brush</param>
		/// <param name="cornerRadius">The corner radius</param>
		/// <param name="backgroundImage">The background image in case of a ImageBrush background</param>
		/// <returns>An updated BoundsPath if the layer has been created or updated; null if there is no change.</returns>
		public CGPath UpdateLayer(
			_View owner,
			Brush background,
			BackgroundSizing backgroundSizing,
			Thickness borderThickness,
			Brush borderBrush,
			CornerRadius cornerRadius,
			_Image backgroundImage)
		{
			// Bounds is captured to avoid calling twice calls below.
			var bounds = owner.Bounds;
			var area = new CGRect(0, 0, bounds.Width, bounds.Height);

			var newState = new LayoutState(area, borderBrush, borderThickness, cornerRadius, background, backgroundImage, backgroundSizing);
			if (!newState.Equals(_previousLayoutState))
			{
#if __MACOS__
				owner.WantsLayer = true;
#endif

				_layerDisposable.Disposable = null;
				_layerDisposable.Disposable = InnerCreateLayer(owner as UIElement, owner.Layer, newState, out var updatedBoundsPath);

				_previousLayoutState = newState;

				return updatedBoundsPath;
			}


			return null; // no change
		}

		/// <summary>
		/// Removes the added layers during a call to <see cref="UpdateLayer" />.
		/// </summary>
		internal void Clear()
		{
			_layerDisposable.Disposable = null;
			_previousLayoutState = null;
		}

		private static Point GetCorner(Rect rect, Corner corner)
		{
			return corner switch
			{
				Corner.TopLeft => new(rect.Left, rect.Top),
				Corner.TopRight => new(rect.Right, rect.Top),
				Corner.BottomRight => new(rect.Right, rect.Bottom),
				Corner.BottomLeft => new(rect.Left, rect.Bottom),

				_ => throw new ArgumentOutOfRangeException($"Invalid corner: {corner}"),
			};
		}

		private static Point GetRelativePoint(Rect rect, int numpadDirection)
		{
			var x = numpadDirection switch
			{
				1 or 4 or 7 => rect.Left,
				2 or 5 or 8 => rect.GetMidX(),
				3 or 6 or 9 => rect.Right,

				_ => throw new ArgumentOutOfRangeException(),
			};
			var y = numpadDirection switch
			{
				7 or 8 or 9 => rect.Top,
				4 or 5 or 6 => rect.GetMidY(),
				1 or 2 or 3 => rect.Bottom,

				_ => throw new ArgumentOutOfRangeException(),
			};

			return new(x, y);
		}

		private static IDisposable InnerCreateLayer(UIElement owner, CALayer parent, LayoutState state, out CGPath updatedBoundsPath)
		{
			updatedBoundsPath = null;

			// nothing to draw, until the control is measured or when is measured to be 0
			if (state.Area.IsEmpty)
			{
				return Disposable.Empty;
			}

			var area = state.Area;
			var background = state.Background;
			var borderThickness = state.BorderThickness;
			var borderBrush = state.BorderBrush;
			var cornerRadius = state.CornerRadius;
			var backgroundSizing = state.BackgroundSizing;

			var disposables = new CompositeDisposable();
			var sublayers = new List<CALayer>();

			var adjustedArea = area.Shrink(borderThickness);

			if (cornerRadius != CornerRadius.None)
			{
				var outerLayer = new CAShapeLayer();
				var backgroundLayer = new CAShapeLayer();
				backgroundLayer.FillColor = null;
				outerLayer.FillRule = CAShapeLayer.FillRuleEvenOdd;
				outerLayer.LineWidth = 0;

				var path = GetRoundedBorder(area, adjustedArea, cornerRadius, borderThickness);
				var outerPath = GetRoundedPath(default, area, cornerRadius, borderThickness);
				var innerPath = GetRoundedPath(default, adjustedArea, cornerRadius, borderThickness, inner: true);

				var isInnerBorderEdge = backgroundSizing == BackgroundSizing.InnerBorderEdge;
				var backgroundPath = isInnerBorderEdge ? innerPath : outerPath;
				var backgroundArea = isInnerBorderEdge ? adjustedArea : area;

				var insertionIndex = 0;

				var caLayer = background switch
				{
					GradientBrush gradientBackground => gradientBackground.GetLayer(backgroundArea.Size),
					RadialGradientBrush radialBackground => radialBackground.GetLayer(backgroundArea.Size),
					_ => null,
				};

				if (caLayer is not null)
				{
					var fillMask = new CAShapeLayer()
					{
						Path = backgroundPath,
						Frame = area,
						// We only use the fill color to create the mask area
						FillColor = _Color.White.CGColor,
					};

					CreateGradientBrushLayers(area, backgroundArea, parent, sublayers, ref insertionIndex, caLayer, fillMask);
				}
				else if (background is ImageBrush imgBackground)
				{
					var imgSrc = imgBackground.ImageSource;
					if (imgSrc != null &&
						imgSrc.TryOpenSync(out var imageData) &&
						imageData.Kind == Uno.UI.Xaml.Media.ImageDataKind.NativeImage &&
						imageData.NativeImage.Size != CGSize.Empty)
					{
						var fillMask = new CAShapeLayer()
						{
							Path = backgroundPath,
							Frame = area,
							// We only use the fill color to create the mask area
							FillColor = _Color.White.CGColor,
						};

						CreateImageBrushLayers(area, backgroundArea, parent, sublayers, ref insertionIndex, imgBackground, fillMask);
					}
				}
				else if (background is AcrylicBrush acrylicBrush)
				{
					var fillMask = new CAShapeLayer()
					{
						Path = backgroundPath,
						Frame = area,
						// We only use the fill color to create the mask area
						FillColor = _Color.White.CGColor,
					};

					acrylicBrush.Subscribe(owner, area, backgroundArea, parent, sublayers, ref insertionIndex, fillMask)
						.DisposeWith(disposables);
				}
				else if (background is XamlCompositionBrushBase or SolidColorBrush)
				{
					Action onInvalidateRender = () => backgroundLayer.FillColor = Brush.GetFallbackColor(background);
					onInvalidateRender();
					background.InvalidateRender += onInvalidateRender;
					new DisposableAction(() => background.InvalidateRender -= onInvalidateRender).DisposeWith(disposables);
				}
				else
				{
					backgroundLayer.FillColor = _transparent;
				}

				outerLayer.Path = path;
				backgroundLayer.Path = backgroundPath;

				sublayers.Add(outerLayer);
				sublayers.Add(backgroundLayer);
				parent.AddSublayer(outerLayer);
				parent.InsertSublayer(backgroundLayer, insertionIndex);

				var borderCALayer = borderBrush switch
				{
					GradientBrush gradientBorder => gradientBorder.GetLayer(area.Size),
					RadialGradientBrush radialBorder => radialBorder.GetLayer(area.Size),
					_ => null,
				};

				if (borderCALayer is not null)
				{
					var fillMask = new CAShapeLayer()
					{
						Path = path,
						Frame = area,
						// We only use the fill color to create the mask area
						FillColor = _Color.White.CGColor,
					};

					var borderLayerIndex = parent.Sublayers.Length;
					CreateGradientBrushLayers(area, area, parent, sublayers, ref borderLayerIndex, borderCALayer, fillMask);
				}
				else if (borderBrush is SolidColorBrush scbBorder || borderBrush == null)
				{
					Action onInvalidateRender = () =>
					{
						CGColor color = Brush.GetFallbackColor(borderBrush);
						outerLayer.StrokeColor = color;
						outerLayer.FillColor = color;

						// Make sure to hold native object ref until it has been retained by native itself
						// https://github.com/unoplatform/uno/issues/10283
						GC.KeepAlive(color);
					};

					if (borderBrush is null)
					{
						onInvalidateRender();
					}
					else
					{
						onInvalidateRender();
						borderBrush.InvalidateRender += onInvalidateRender;
						new DisposableAction(() => borderBrush.InvalidateRender -= onInvalidateRender).DisposeWith(disposables);
					}
				}

				parent.Mask = new CAShapeLayer()
				{
					Path = outerPath,
					Frame = area,
					// We only use the fill color to create the mask area
					FillColor = _Color.White.CGColor,
				};

				if (owner != null)
				{
					owner.ClippingIsSetByCornerRadius = true;
				}

				updatedBoundsPath = outerPath;
			}
			else
			{
				var backgroundLayer = new CAShapeLayer();
				backgroundLayer.FillColor = null;

				var outerPath = GetRectangularPath(default, area);
				var innerPath = GetRectangularPath(default, adjustedArea, inner: true);

				var isInnerBorderEdge = backgroundSizing == BackgroundSizing.InnerBorderEdge;
				var backgroundPath = isInnerBorderEdge ? innerPath : outerPath;
				var backgroundArea = isInnerBorderEdge ? adjustedArea : area;

				var insertionIndex = 0;

				var caLayer = background switch
				{
					GradientBrush gradientBackground => gradientBackground.GetLayer(backgroundArea.Size),
					RadialGradientBrush radialBackground => radialBackground.GetLayer(backgroundArea.Size),
					_ => null,
				};

				if (caLayer is not null)
				{
					CreateGradientBrushLayers(area, backgroundArea, parent, sublayers, ref insertionIndex, caLayer, fillMask: null);
				}
				else if (background is SolidColorBrush)
				{
					Action onInvalidateRender = () => backgroundLayer.FillColor = Brush.GetFallbackColor(background);

					onInvalidateRender();
					background.InvalidateRender += onInvalidateRender;
					new DisposableAction(() => background.InvalidateRender -= onInvalidateRender)
						.DisposeWith(disposables);

					// This is required because changing the CornerRadius changes the background drawing
					// implementation and we don't want a rectangular background behind a rounded background.
					Disposable.Create(() => parent.BackgroundColor = null)
						.DisposeWith(disposables);
				}
				else if (background is ImageBrush imgBackground)
				{
					var bgSrc = imgBackground.ImageSource;
					if (bgSrc != null &&
						bgSrc.TryOpenSync(out var imageData) &&
						imageData.Kind == ImageDataKind.NativeImage &&
						imageData.NativeImage is _Image uiImage &&
						uiImage.Size != CGSize.Empty)
					{
						CreateImageBrushLayers(area, backgroundArea, parent, sublayers, ref insertionIndex, imgBackground, fillMask: null);
					}
				}
				else if (background is AcrylicBrush acrylicBrush)
				{
					acrylicBrush.Subscribe(owner, area, backgroundArea, parent, sublayers, ref insertionIndex, fillMask: null);
				}
				else if (background is XamlCompositionBrushBase)
				{
					Action onInvalidateRender = () => backgroundLayer.FillColor = Brush.GetFallbackColor(background);
					background.InvalidateRender += onInvalidateRender;
					onInvalidateRender();
					new DisposableAction(() => background.InvalidateRender -= onInvalidateRender)
						.DisposeWith(disposables);

					// This is required because changing the CornerRadius changes the background drawing
					// implementation and we don't want a rectangular background behind a rounded background.
					Disposable
						.Create(() => backgroundLayer.FillColor = null)
						.DisposeWith(disposables);
				}
				else
				{
					backgroundLayer.FillColor = _transparent;
				}

				if (borderThickness != Thickness.Empty)
				{
					var borderPath = GetRectangularBorder(area, adjustedArea);
					var layer = new CAShapeLayer();

					layer.FillRule = CAShapeLayer.FillRuleEvenOdd;
					layer.LineWidth = 0;
					layer.Path = borderPath;

					// Must be inserted below the other subviews, which may happen when
					// the current view has subviews.
					sublayers.Add(layer);
					parent.AddSublayer(layer);

					var borderCALayer = borderBrush switch
					{
						GradientBrush gradientBorder => gradientBorder.GetLayer(area.Size),
						RadialGradientBrush radialBorder => radialBorder.GetLayer(area.Size),
						_ => null,
					};

					if (borderCALayer is not null)
					{
						var fillMask = new CAShapeLayer()
						{
							Path = borderPath,
							Frame = area,
							// We only use the fill color to create the mask area
							FillColor = _Color.White.CGColor,
						};

						var borderLayerIndex = parent.Sublayers.Length;
						CreateGradientBrushLayers(area, area, parent, sublayers, ref borderLayerIndex, borderCALayer, fillMask);
					}
					else if (borderBrush is SolidColorBrush scbBorder)
					{
						Action onInvalidateRender = () => layer.FillColor = Brush.GetFallbackColor(borderBrush);
						onInvalidateRender();
						scbBorder.InvalidateRender += onInvalidateRender;
						new DisposableAction(() => scbBorder.InvalidateRender -= onInvalidateRender).DisposeWith(disposables);
					}
				}

				backgroundLayer.Path = backgroundPath;

				sublayers.Add(backgroundLayer);
				parent.InsertSublayer(backgroundLayer, insertionIndex);

				parent.Mask = new CAShapeLayer()
				{
					Path = outerPath,
					Frame = area,
					// We only use the fill color to create the mask area
					FillColor = _Color.White.CGColor,
				};

				updatedBoundsPath = outerPath;
			}

			disposables.Add(() =>
			{
				foreach (var sl in sublayers)
				{
					sl.RemoveFromSuperLayer();
					sl.Dispose();
				}

				if (owner != null)
				{
					owner.ClippingIsSetByCornerRadius = false;
				}
			}
			);
			return disposables;
		}

		/// <summary>
		/// Get a path that describes the rounded border.
		/// </summary>
		private static CGPath GetRoundedBorder(Rect bbox, Rect innerBbox, CornerRadius cr, Thickness bt)
		{
			var path = new CGPath();

			if (bbox is { Width: > 0, Height: > 0 })
			{
				GetRoundedPath(path, bbox, cr, bt);

				if (innerBbox is { Width: > 0, Height: > 0 })
				{
					GetRoundedPath(path, innerBbox, cr, bt, inner: true);
				}
			}

			return path;
		}

		private static readonly int[] _8624Array = new[] { 8, 6, 2, 4 };
		private static readonly int[] _2684Array = new[] { 2, 6, 8, 4 };

		/// <summary>
		/// Get the inner/outer contours of the rounded border.
		/// </summary>
		private static CGPath GetRoundedPath(CGPath path, Rect bbox, CornerRadius cr, Thickness bt, bool inner = false)
		{
			path ??= new();

			/* reference diagram:
			 *  outer   inner
			 * A1───2B D6───5C
			 * 0     3 7     4
			 * 7     4 0     3
			 * D6───5C A1───2B
			 */

			// the inner contour needs to be drawn in counter-clockwise (against the outer one),
			// otherwise the gradient brush paint will not exclude the inner region.
			var corners = !inner
				? new[] { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft }  // outer -> clockwise
				: new[] { Corner.BottomLeft, Corner.BottomRight, Corner.TopRight, Corner.TopLeft }; // inner -> counter-clockwise

			for (int i = 0; i < corners.Length; i++)
			{
				var corner = corners[i];
				var cornerBbox = CalculateBorderEllipseBbox(bbox, corner, cr, bt, inner);

				if (i == 0)
				{
					// p0 and p7 are not necessarily the same point, so it needs to be calculated from cornerBbox and not bbox.
					// note: here, we are looking for mid-point between A-0, not p0.
					path.MoveToPoint(GetRelativePoint(cornerBbox, numpadDirection: 4));
				}

				// no rounded corner to draw if there is no area
				if (cornerBbox.Width > 0 && cornerBbox.Height > 0)
				{
					var pCorner = GetCorner(cornerBbox, corner);
					var pNextMid = GetRelativePoint(cornerBbox, numpadDirection: (!inner
						? _8624Array
						: _2684Array
					)[i]);

					// given that AddArcToPoint can only draw arc of a circle (with equal width, height, and diameter),
					// we have to scale the Y-axis, so that the ellipse becomes a perfect circle,
					// and use a reverse scale transform on the drawing of arc to achieve the desired result.
					var scaleY = cornerBbox.Width / cornerBbox.Height;
					var scaleTransform = CGAffineTransform.MakeScale(1, (nfloat)(cornerBbox.Height / cornerBbox.Width));
					pCorner.Y *= scaleY;
					pNextMid.Y *= scaleY;

					// How AddArcToPoint works: https://stackoverflow.com/a/18992153
					path.AddArcToPoint(scaleTransform, (nfloat)pCorner.X, (nfloat)pCorner.Y, (nfloat)pNextMid.X, (nfloat)pNextMid.Y, radius: (nfloat)cornerBbox.Width / 2);
				}
				else
				{
					// however, we still need to drawn a line to that corner
					path.AddLineToPoint(GetCorner(cornerBbox, corner));
				}
			}

			path.CloseSubpath();

			return path;
		}

		/// <summary>
		/// Get a path that describes the border.
		/// </summary>
		private static CGPath GetRectangularBorder(Rect bbox, Rect innerBbox)
		{
			var path = new CGPath();

			if (bbox is { Width: > 0, Height: > 0 })
			{
				GetRectangularPath(path, bbox);

				if (innerBbox is { Width: > 0, Height: > 0 })
				{
					GetRectangularPath(path, innerBbox, inner: true);
				}
			}

			return path;
		}

		/// <summary>
		/// Get the inner/outer contours of the rectangular border.
		/// </summary>
		private static CGPath GetRectangularPath(CGPath path, Rect bbox, bool inner = false)
		{
			path ??= new();

			// the inner contour needs to be drawn in counter-clockwise (against the outer one),
			// otherwise the gradient brush paint will not exclude the inner region.
			var corners = !inner
				? new[] { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft }  // outer -> clockwise
				: new[] { Corner.BottomLeft, Corner.BottomRight, Corner.TopRight, Corner.TopLeft }; // inner -> counter-clockwise

			path.MoveToPoint(GetRelativePoint(bbox, 4));
			foreach (var corner in corners)
			{
				path.AddLineToPoint(GetCorner(bbox, corner));
			}
			path.CloseSubpath();

			return path;
		}

		/// <summary>
		/// Creates and add 2 layers for ImageBrush background
		/// </summary>
		/// <param name="fullArea">The full area available (includes the border)</param>
		/// <param name="insideArea">The "inside" of the border (excludes the border)</param>
		/// <param name="layer">The layer in which the image layers will be added</param>
		/// <param name="sublayers">List of layers to keep all references</param>
		/// <param name="insertionIndex">Where in the layer the new layers will be added</param>
		/// <param name="imageBrush">The ImageBrush containing all the image properties (UIImage, Stretch, AlignmentX, etc.)</param>
		/// <param name="fillMask">Optional mask layer (for when we use rounded corners)</param>
		private static void CreateImageBrushLayers(CGRect fullArea, CGRect insideArea, CALayer layer, List<CALayer> sublayers, ref int insertionIndex, ImageBrush imageBrush, CAShapeLayer fillMask)
		{
			imageBrush.ImageSource.TryOpenSync(out var imageData);

			if (imageData.Kind != Uno.UI.Xaml.Media.ImageDataKind.NativeImage)
			{
				return;
			}

			var uiImage = imageData.NativeImage;

			// This layer is the one we apply the mask on. It's the full size of the shape because the mask is as well.
			var imageContainerLayer = new CALayer
			{
				Frame = fullArea,
				Mask = fillMask,
				BackgroundColor = _transparent,
				MasksToBounds = true,
			};

			var imageFrame = imageBrush.GetArrangedImageRect(uiImage.Size, insideArea).ToCGRect();

			// This is the layer with the actual image in it. Its frame is the inside of the border.
			var imageLayer = new CALayer
			{
				Contents = uiImage.CGImage,
				Frame = imageFrame,
				MasksToBounds = true,
			};

			imageContainerLayer.AddSublayer(imageLayer);
			sublayers.Add(imageLayer);

			layer.InsertSublayer(imageContainerLayer, insertionIndex++);
			sublayers.Add(imageContainerLayer);
		}

		/// <summary>
		/// Creates and add 2 layers for LinearGradientBrush background
		/// </summary>
		/// <param name="fullArea">The full area available (includes the border)</param>
		/// <param name="insideArea">The "inside" of the border (excludes the border)</param>
		/// <param name="layer">The layer in which the gradient layers will be added</param>
		/// <param name="sublayers">List of layers to keep all references</param>
		/// <param name="insertionIndex">Where in the layer the new layers will be added</param>
		/// <param name="gradientLayer">The CALayer retrieved by brush.GetLayer(insideArea.Size)</param>
		/// <param name="fillMask">Optional mask layer (for when we use rounded corners)</param>
		private static void CreateGradientBrushLayers(CGRect fullArea, CGRect insideArea, CALayer layer, List<CALayer> sublayers, ref int insertionIndex, CALayer gradientLayer, CAShapeLayer fillMask)
		{
			// This layer is the one we apply the mask on. It's the full size of the shape because the mask is as well.
			var gradientContainerLayer = new CALayer
			{
				Frame = fullArea,
				Mask = fillMask,
				BackgroundColor = _transparent,
				MasksToBounds = true,
			};

			var gradientFrame = new CGRect(new CGPoint(insideArea.X, insideArea.Y), insideArea.Size);

			// This is the layer with the actual gradient in it. Its frame is the inside of the border.
			gradientLayer.Frame = gradientFrame;
			gradientLayer.MasksToBounds = true;

			gradientContainerLayer.AddSublayer(gradientLayer);
			sublayers.Add(gradientLayer);

			layer.InsertSublayer(gradientContainerLayer, insertionIndex++);
			sublayers.Add(gradientContainerLayer);
		}

		private record LayoutState(
			CGRect Area,
			Brush BorderBrush, Thickness BorderThickness, CornerRadius CornerRadius,
			Brush Background, _Image BackgroundImage, BackgroundSizing BackgroundSizing)
		{
			// internal CGPath BoundsPath { get; set; }
		}
	}
}

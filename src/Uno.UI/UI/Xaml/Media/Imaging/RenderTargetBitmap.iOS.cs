﻿#nullable enable

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;
using Windows.Graphics.Display;
using CoreGraphics;
using Foundation;
using UIKit;
using Uno.UI;
using Uno.UI.Xaml.Media;

namespace Microsoft.UI.Xaml.Media.Imaging
{
	partial class RenderTargetBitmap
	{
		private const int _bitsPerPixel = 32;
		private const int _bitsPerComponent = 8;
		private const int _bytesPerPixel = _bitsPerPixel / _bitsPerComponent;

		/// <inheritdoc />
		private protected override bool IsSourceReady => _buffer != null;

		/// <inheritdoc />
		private static ImageData Open(byte[] buffer, int bufferLength, int width, int height)
		{
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CGBitmapContext(
				buffer,
				width,
				height,
				_bitsPerComponent,
				width * _bytesPerPixel,
				colorSpace,
				CGBitmapFlags.ByteOrder32Little | CGBitmapFlags.PremultipliedFirst);

			using var cgImage = context.ToImage();
			if (cgImage is not null)
			{
				return ImageData.FromNative(new UIImage(cgImage));
			}

			return default;
		}

		private static (int ByteCount, int Width, int Height) RenderAsBgra8_Premul(UIElement element, ref byte[]? buffer, Size? scaledSize = null)
		{
			var size = new Size(element.ActualSize.X, element.ActualSize.Y);
			if (size == default)
			{
				return (0, 0, 0);
			}

			UIImage uiImage;
			try
			{
				var scale = (float)(DisplayInformation.GetForCurrentView()?.RawPixelsPerViewPixel ?? 1.0);
				UIGraphics.BeginImageContextWithOptions(size, false, scale);
				var ctx = UIGraphics.GetCurrentContext();
				ctx.SetFillColor(Colors.Transparent); // This is only for pixels not used, but the bitmap as the same size of the element. We keep it only for safety!
				element.Layer.RenderInContext(ctx);
				uiImage = UIGraphics.GetImageFromCurrentImageContext();
			}
			finally
			{
				UIGraphics.EndImageContext();
			}

			if (scaledSize.HasValue)
			{
				using var unscaled = uiImage;
				uiImage = unscaled.Scale(scaledSize.Value);
			}

			using (uiImage)
			{
				var cgImage = uiImage.CGImage!;
				var width = cgImage.Width;
				var height = cgImage.Height;
				var bytesPerRow = width * _bytesPerPixel;
				var bufferLength = (int)(bytesPerRow * height);

				EnsureBuffer(ref buffer, bufferLength);

				using var colorSpace = CGColorSpace.CreateDeviceRGB();
				using var context = new CGBitmapContext(
					buffer,
					width,
					height,
					_bitsPerComponent,
					bytesPerRow,
					colorSpace,
					CGBitmapFlags.ByteOrder32Little | CGBitmapFlags.PremultipliedFirst); // BGRA8

				var rect = new CGRect(0, 0, width, height);
				context.DrawImage(rect, cgImage);

				return (bufferLength, (int)width, (int)height);
			}
		}
	}
}

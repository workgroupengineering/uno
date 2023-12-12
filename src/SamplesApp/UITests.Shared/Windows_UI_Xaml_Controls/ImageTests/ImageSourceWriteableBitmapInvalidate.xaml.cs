﻿using System;
using System.Linq;
using System.Reflection;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Uno.UI.Samples.Controls;

#if NETFX_CORE
using System.Runtime.InteropServices.WindowsRuntime;
#endif

namespace UITests.Shared.Windows_UI_Xaml_Controls.ImageTests
{
	[Sample("WriteableBitmap")]
	public sealed partial class ImageSourceWriteableBitmapInvalidate : Page
	{
		private WriteableBitmap _bitmap;

		public ImageSourceWriteableBitmapInvalidate()
		{
			this.InitializeComponent();

			_bitmap = new WriteableBitmap(200, 200);
			_image.Source = _bitmap;
		}

		private void UpdateSource(object sender, RoutedEventArgs e)
		{
#if NETFX_CORE
			using (var data = _bitmap.PixelBuffer.AsStream())
			{
				// Half of the image in green, alpha 100% (bgra buffer)
				var pixel = new byte[] { 0, 255, 0, 255 };
				for (var i = 1; i < data.Length / 2; i += 4)
				{
					data.Write(pixel, 0, 4);
				}
				data.Flush();
			}
#else
			if (_bitmap.PixelBuffer is Windows.Storage.Streams.Buffer buffer
				&& buffer.GetType().GetField("_data", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(buffer) is Memory<byte> data)
			{
				var span = data.Span;
				// Half of the image in green, alpha 100% (bgra buffer)
				for (var i = 1; i < data.Length / 2; i += 2)
				{
					span[i] = byte.MaxValue;
				}
			}
			else
			{
				throw new InvalidOperationException("Could not access _data field in Buffer type.");
			}
#endif

			// This request to the image to redraw the buffer
			_bitmap.Invalidate();
		}
	}
}

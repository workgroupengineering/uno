﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Uno;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;

#if HAS_UNO_WINUI
using Microsoft.UI.Input;
using PointerDeviceType = Windows.Devices.Input.PointerDeviceType;
#else
using PointerDeviceType = Windows.Devices.Input.PointerDeviceType;
using Windows.Devices.Input;
using Windows.UI.Input;
#endif

namespace Microsoft.UI.Xaml.Input
{
	partial class PointerRoutedEventArgs
	{
		private static long _pseudoNextFrameId;
		private readonly uint _pseudoFrameId = (uint)Interlocked.Increment(ref _pseudoNextFrameId);
		private readonly ulong _pseudoTimestamp = (ulong)DateTime.UtcNow.Ticks;
		private readonly Point _point;

		public PointerRoutedEventArgs(Point point) : this()
		{
			_point = point;

			FrameId = _pseudoFrameId;
		}

		public PointerPoint GetCurrentPoint(UIElement relativeTo)
		{
			var device = Windows.Devices.Input.PointerDevice.For(PointerDeviceType.Mouse);
			var translation = relativeTo.TransformToVisual(null) as TranslateTransform;
			var offset = new Point(_point.X - translation.X, _point.Y - translation.Y);
			var properties = new PointerPointProperties() { IsInRange = true, IsPrimary = true };

			return new PointerPoint(FrameId, _pseudoTimestamp, device, 0, offset, offset, true, properties);
		}
	}
}

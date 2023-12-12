﻿#if UNO_HAS_MANAGED_POINTERS
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Windows.Devices.Input;
using Uno;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Core;

#if HAS_UNO_WINUI
using Microsoft.UI.Input;
using PointerDeviceType = Microsoft.UI.Input.PointerDeviceType;
#else
using Windows.UI.Input;
using PointerDeviceType = Windows.Devices.Input.PointerDeviceType;
#endif

namespace Microsoft.UI.Xaml.Input
{
	partial class PointerRoutedEventArgs
	{
		private readonly Windows.UI.Core.PointerEventArgs _pointerEventArgs;
		private readonly PointerPoint _currentPoint;

		internal PointerRoutedEventArgs(
			Windows.UI.Core.PointerEventArgs pointerEventArgs,
			UIElement source) : this()
		{
			_pointerEventArgs = pointerEventArgs;

#if HAS_UNO_WINUI
			_currentPoint = new PointerPoint(_pointerEventArgs.CurrentPoint);
#else
			_currentPoint = _pointerEventArgs.CurrentPoint;
#endif

			FrameId = pointerEventArgs.CurrentPoint.FrameId;
			Pointer = GetPointer(pointerEventArgs);
			KeyModifiers = pointerEventArgs.KeyModifiers;
			OriginalSource = source;
		}

		public PointerPoint GetCurrentPoint(UIElement relativeTo)
		{
			if (relativeTo is null)
			{
				return _currentPoint;
			}
			else
			{
				var absolutePosition = _pointerEventArgs.CurrentPoint.Position;
				var relativePosition = relativeTo.TransformToVisual(null).Inverse.TransformPoint(absolutePosition);

				return _currentPoint.At(relativePosition);
			}
		}

		private Pointer GetPointer(Windows.UI.Core.PointerEventArgs args)
			=> new Pointer(
				args.CurrentPoint.PointerId,
				(PointerDeviceType)args.CurrentPoint.PointerDevice.PointerDeviceType,
				isInContact: args.CurrentPoint.IsInContact,
				isInRange: args.CurrentPoint.Properties.IsInRange);
	}
}
#endif

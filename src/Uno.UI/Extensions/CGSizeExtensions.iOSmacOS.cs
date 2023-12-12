﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using CoreGraphics;
using Windows.Foundation;
using Microsoft.UI.Xaml;

namespace CoreGraphics
{
	public static class CGSizeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Pure]
		public static Windows.Foundation.Size ToFoundationSize(this CGSize size)
			=> new Size(
				size.Width,
				size.Height
			);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Pure]
		public static bool HasZeroArea(this CGSize size)
			=> size.Height == 0 || size.Width == 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Pure]
		internal static CGSize Add(this CGSize left, Thickness right)
			=> new CGSize(
				left.Width + right.Left + right.Right,
				left.Height + right.Top + right.Bottom
			);
	}
}

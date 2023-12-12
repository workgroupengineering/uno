﻿using System.Numerics;
using SkiaSharp;
using Microsoft.UI.Composition;
using Uno.Extensions;

namespace Microsoft.UI.Xaml.Media
{
	partial class LineGeometry
	{
		internal override SKPath GetSKPath() => CompositionGeometry.BuildLineGeometry(StartPoint.ToVector2(), EndPoint.ToVector2());
	}
}

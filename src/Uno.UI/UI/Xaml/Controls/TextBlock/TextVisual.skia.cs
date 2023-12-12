﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Uno;
using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno.UI.Xaml;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno.UI.Composition;

#nullable enable

namespace Microsoft.UI.Composition
{
	internal class TextVisual : Visual
	{
		private readonly WeakReference<TextBlock> _owner;

		public TextVisual(Compositor compositor, TextBlock owner) : base(compositor)
		{
			_owner = new WeakReference<TextBlock>(owner);
		}

		internal override void Draw(in DrawingSession session)
		{
			if (_owner.TryGetTarget(out var owner))
			{
				owner.Inlines.Draw(in session);
			}
		}
	}
}

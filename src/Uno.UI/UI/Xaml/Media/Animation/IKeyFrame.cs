﻿using System;
using System.Linq;

namespace Microsoft.UI.Xaml.Media.Animation
{
	internal interface IKeyFrame
	{
		public KeyTime KeyTime { get; }
	}

	internal interface IKeyFrame<out TValue> : IKeyFrame
	{
		public TValue Value { get; }
	}
}

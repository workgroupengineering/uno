﻿using System;

namespace Microsoft.UI.Xaml
{

	public delegate void ExceptionRoutedEventHandler(object sender, ExceptionRoutedEventArgs e);

	public partial class RoutedEventArgs : EventArgs
	{
		public RoutedEventArgs()
		{
		}

		internal RoutedEventArgs(object originalSource)
		{
			OriginalSource = originalSource;
		}

		public object OriginalSource { get; internal set; }

		/// <summary>
		/// If this event is currently coming from platform (native)
		/// and CAN bubble natively.
		/// </summary>
		/// <remarks>
		/// See Uno documentation to understand the difference
		/// between _native_ and _managed_ bubbling.
		/// </remarks>
		public bool CanBubbleNatively { get; internal set; }
	}
}

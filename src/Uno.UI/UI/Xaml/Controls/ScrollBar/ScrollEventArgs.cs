﻿namespace Microsoft.UI.Xaml.Controls.Primitives
{
	public partial class ScrollEventArgs : global::Microsoft.UI.Xaml.RoutedEventArgs
	{
		public double NewValue
		{
			get; internal set;
		}

		public ScrollEventType ScrollEventType
		{
			get; internal set;
		}

		public ScrollEventArgs() : base()
		{
		}
	}
}

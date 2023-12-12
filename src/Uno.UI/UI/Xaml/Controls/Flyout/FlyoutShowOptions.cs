﻿using Windows.Foundation;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
	public partial class FlyoutShowOptions
	{
		public Point? Position { get; set; }

		public FlyoutPlacementMode Placement { get; set; } = FlyoutPlacementMode.Auto;

		public FlyoutShowOptions()
		{
		}
	}
}

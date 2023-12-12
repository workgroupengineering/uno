﻿using Microsoft.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml
{
	internal interface IFrameworkElementInternal : IFrameworkElement
	{
		/// <summary>
		/// True if this <see cref="IFrameworkElement"/> implementation has a <see cref="Layouter"/> associated with it.
		/// </summary>
		bool HasLayouter { get; }
	}
}

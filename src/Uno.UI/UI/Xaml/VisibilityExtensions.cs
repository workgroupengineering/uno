﻿using System;
using System.ComponentModel;

namespace Microsoft.UI.Xaml;

public static class VisibilityExtensions
{
	/// <summary>
	/// Determines if the specified visibility is hidden
	/// </summary>
	public static bool IsHidden(this Visibility visibility) => visibility == Visibility.Collapsed;
}

﻿using NotImplementedException = System.NotImplementedException;

namespace Microsoft.UI.Xaml
{
	// This is a class to support code from WinUI
	internal static class DependencyPropertyFactory
	{
		// This is a method to support code from WinUI
		internal static void IsUnsetValue(object spDateFormat, out bool isUnsetValue)
		{
			isUnsetValue = spDateFormat is UnsetValue;
		}
	}
}

﻿#nullable enable

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Uno.UI.Xaml;

/// <summary>
/// An Uno Platform specific <see cref="Windows.UI.Xaml.Window"/> helper.
/// </summary>
public static class WindowHelper
{
	/// <summary>
	/// Sets the Window background
	/// </summary>
	public static void SetBackground(this Window window, Brush? background)
	{
		if (window is null)
		{
			throw new ArgumentNullException(nameof(window));
		}

		window.Background = background;
	}

	/// <summary>
	/// Retrieves the native Window object backing the given window.
	/// Only applicable on some targets.
	/// </summary>
	/// <param name="window">Window.</param>
	/// <returns>Native window type or null.</returns>
	public static object? GetNativeWindow(this Window window) => window.NativeWindow;
}

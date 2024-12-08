﻿#nullable enable

using System;
using Uno.UI.Xaml.Core;
using Windows.UI.Xaml;

namespace Uno.UI.Xaml.Controls;

partial class ContentManager
{
	static partial void AttachToWindowPlatform(UIElement rootElement, Windows.UI.Xaml.Window window)
	{
		WindowManagerInterop.SetRootElement(rootElement.HtmlId);
		UpdateRootAttributes(rootElement);
	}

	private static void UpdateRootAttributes(UIElement rootElement)
	{
		if (rootElement is null)
		{
			throw new InvalidOperationException("Private window root is not yet set.");
		}

		if (FeatureConfiguration.Cursors.UseHandForInteraction)
		{
			rootElement.SetAttribute("data-use-hand-cursor-interaction", "true");
		}
		else
		{
			rootElement.RemoveAttribute("data-use-hand-cursor-interaction");
		}
	}

	static partial void LoadRootElementPlatform(XamlRoot xamlRoot, UIElement rootElement)
	{
		xamlRoot.InvalidateMeasure();
		xamlRoot.InvalidateArrange();
	}
}

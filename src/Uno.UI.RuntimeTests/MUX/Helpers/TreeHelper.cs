﻿using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace Uno.UI.RuntimeTests.MUX.Helpers
{
	internal static class TreeHelper
	{
		public static FrameworkElement GetVisualChildByName(FrameworkElement parent, string name)
		{
			FrameworkElement child = default;

			var count = VisualTreeHelper.GetChildrenCount(parent);

			for (var i = 0; i < count && child == default; i++)
			{
				var current = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

				child = current?.Name == name
					? current
					: GetVisualChildByName(current, name);
			}

			return child;
		}

		public static void GetVisualChildrenByType<T>(UIElement parent, ref List<T> children) where T : UIElement
		{
			var count = VisualTreeHelper.GetChildrenCount(parent);

			for (var i = 0; i < count; i++)
			{
				var current = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

				if (current is T child)
				{
					children.Add(child);
				}
				else
				{
					GetVisualChildrenByType(current, ref children);
				}
			}
		}

		public static T GetVisualChildByType<T>(UIElement parent) where T : UIElement
		{
			T child = default;

			var count = VisualTreeHelper.GetChildrenCount(parent);

			for (var i = 0; i < count && child == default; i++)
			{
				var current = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

				child = current is T c
					? c
					: GetVisualChildByType<T>(current);
			}

			return child;
		}


		internal static FrameworkElement GetVisualChildByNameFromOpenPopups(string name, DependencyObject element)
		{
			var popups = GetOpenPopups(element);

			foreach (var popup in popups)
			{
				var popupChild = popup.Child as FrameworkElement;
				if (popupChild.Name == name)
				{
					return popupChild;
				}
				else
				{
					var child = GetVisualChildByName(popupChild, name);
					if (child != null)
					{
						return child;
					}
				}
			}

			return null;
		}

		internal static IEnumerable<Popup> GetOpenPopups(DependencyObject element)
		{
#if NETFX_CORE
			return VisualTreeHelper.GetOpenPopups(Window.Current);
#else
			return VisualTreeHelper.GetOpenPopupsForXamlRoot(GetXamlRoot(element));
#endif

		}

#if !NETFX_CORE
		internal static XamlRoot GetXamlRoot(DependencyObject obj)
		{
			XamlRoot xamlRoot = default;
			if (obj is UIElement e)
			{
				xamlRoot = e.XamlRoot;
			}
			else if (obj is TextElement te)
			{
				xamlRoot = te.XamlRoot;
			}
			else if (obj is Microsoft.UI.Xaml.Controls.Primitives.FlyoutBase fb)
			{
				xamlRoot = fb.XamlRoot;
			}
			else
			{
				throw new InvalidOperationException("TreeHelper::GetXamlRoot: Can't find XamlRoot for element");
			}
			return xamlRoot;
		}
#endif
	}
}

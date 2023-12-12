﻿#nullable enable

using System.Linq;
using Microsoft.UI.Composition;

namespace Microsoft.UI.Xaml.Hosting;

/// <summary>
/// Enables access to composition visual objects that back XAML elements in the XAML composition tree.
/// </summary>
public partial class ElementCompositionPreview
{
#if __SKIA__
	private const string ChildVisualName = "childVisual";
#else
	static readonly Compositor _compositor = new Compositor();
#endif

	/// <summary>
	/// Retrieves the Microsoft.UI.Composition.Visual object that backs a XAML element in the XAML composition tree.
	/// </summary>
	/// <param name="element">The element for which to retrieve the Visual.</param>
	/// <returns>The Microsoft.UI.Composition.Visual object that backs the XAML element.</returns>
	public static Visual GetElementVisual(UIElement element)
	{
#if __SKIA__
		return element.Visual;
#else
		return new Composition.Visual(_compositor) { NativeOwner = element };
#endif
	}

	/// <summary>
	/// Sets a custom Microsoft.UI.Composition.Visual as the last child of the element's visual tree.
	/// </summary>
	/// <param name="element">The element to add the child Visual to.</param>
	/// <param name="visual">The Visual to add to the element's visual tree.</param>
	public static void SetElementChildVisual(UIElement element, Visual visual)
	{
#if __IOS__
		element.Layer.AddSublayer(visual.NativeLayer);
		visual.NativeOwner = element;
		element.ClipsToBounds = false;

		if (element is FrameworkElement fe)
		{
			fe.SizeChanged +=
				(s, e) => visual.NativeLayer.Frame = new CoreGraphics.CGRect(0, 0, element.Frame.Width, element.Frame.Height);
		}
#elif __SKIA__

		var container = new Composition.ContainerVisual(element.Visual.Compositor) { Comment = ChildVisualName };
		container.Children.InsertAtTop(visual);

		if (element.Visual.Children.FirstOrDefault(v => v.Comment == ChildVisualName) is Composition.ContainerVisual cv)
		{
			element.Visual.Children.Remove(cv);
		}

		element.Visual.Children.InsertAtTop(container);
#endif
	}
}

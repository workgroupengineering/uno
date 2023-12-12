﻿#nullable enable

using System.Windows.Input;
using Uno.Foundation.Logging;
using Uno.UI.Runtime.Skia.Wpf.Extensions;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Xaml.Shapes.BorderLayerRenderer;
using WpfCanvas = System.Windows.Controls.Canvas;
using WpfUIElement = System.Windows.UIElement;

namespace Uno.UI.Runtime.Skia.Wpf
{
	internal partial class WpfCoreWindowExtension : ICoreWindowExtension
	{
		private readonly WpfHost? _host;
		private readonly CoreWindow _owner;

		public WpfCoreWindowExtension(object owner)
		{
			_owner = (CoreWindow)owner;
			_host = WpfHost.Current;
		}

		internal static WpfCanvas? GetOverlayLayer(XamlRoot xamlRoot) =>
			WpfManager.XamlRootMap.GetHostForRoot(xamlRoot)?.NativeOverlayLayer;

		public bool IsNativeElement(object content)
			=> content is System.Windows.UIElement;

		public void AttachNativeElement(object owner, object content)
		{
			if (owner is XamlRoot xamlRoot
				&& GetOverlayLayer(xamlRoot) is { } layer
				&& content is System.Windows.FrameworkElement contentAsFE
				&& contentAsFE.Parent != layer)
			{
				layer.Children.Add(contentAsFE);
			}
			else
			{
				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().Debug($"Unable to attach native element {content} in {owner}.");
				}
			}
		}

		public void DetachNativeElement(object owner, object content)
		{
			if (owner is XamlRoot xamlRoot
				&& GetOverlayLayer(xamlRoot) is { } layer
				&& content is System.Windows.FrameworkElement contentAsFE
				&& contentAsFE.Parent == layer)
			{
				layer.Children.Remove(contentAsFE);
			}
			else
			{
				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().Debug($"Unable to detach native element {content} in {owner}.");
				}
			}
		}

		public bool IsNativeElementAttached(object owner, object nativeElement) =>
			nativeElement is System.Windows.FrameworkElement contentAsFE
				&& owner is XamlRoot xamlRoot
				&& GetOverlayLayer(xamlRoot) is { } layer
				&& contentAsFE.Parent == layer;

		public void ArrangeNativeElement(object owner, object content, Windows.Foundation.Rect arrangeRect)
		{
			if (content is System.Windows.UIElement contentAsUIElement)
			{
				WpfCanvas.SetLeft(contentAsUIElement, arrangeRect.X);
				WpfCanvas.SetTop(contentAsUIElement, arrangeRect.Y);

				contentAsUIElement.Arrange(
					new(0, 0, arrangeRect.Width, arrangeRect.Height)
				);
			}
			else
			{
				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().Debug($"Unable to arrange native element {content} in {owner}.");
				}
			}
		}

		public Windows.Foundation.Size MeasureNativeElement(object owner, object content, Windows.Foundation.Size size)
		{
			return size;
		}
	}
}

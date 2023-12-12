﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.Disposables;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Private.Infrastructure;
using MUXControlsTestApp.Utilities;

#if HAS_UNO
using Uno.UI.Dispatching;
using Uno.UI.Xaml.Media;
#endif

namespace Uno.UI.RuntimeTests.Helpers
{
	public static class StyleHelper
	{
		/// <summary>
		/// Enables styles associated with native frame navigation (on Android and iOS) for the duration of the test.
		/// </summary>
		public static IDisposable UseNativeFrameNavigation()
		{
#if NETFX_CORE
			return null;
#else
			return new CompositeDisposable
			{
				UseNativeStyle<Frame>(),
				UseNativeStyle<CommandBar>(),
				UseNativeStyle<AppBarButton>(),
			};
#endif
		}

		/// <summary>
		/// Enables the native style for <typeparamref name="T"/> for the duration of the test, if one is available.
		/// </summary>
		public static IDisposable UseNativeStyle<T>() where T : Control
		{
#if NETFX_CORE
			return null;
#else
			IDisposable disposable;
			if (FeatureConfiguration.Style.UseUWPDefaultStylesOverride.TryGetValue(typeof(T), out var currentOverride))
			{
				disposable = Disposable.Create(() => FeatureConfiguration.Style.UseUWPDefaultStylesOverride[typeof(T)] = currentOverride);
			}
			else
			{
				disposable = Disposable.Create(() => FeatureConfiguration.Style.UseUWPDefaultStylesOverride.Remove(typeof(T)));
			}

			FeatureConfiguration.Style.UseUWPDefaultStylesOverride[typeof(T)] = false;

			return disposable;
#endif
		}

		/// <summary>
		/// Adds <paramref name="resources"/> to <see cref="Application.Resources"/> for the duration of the test, then removes it.
		/// </summary>
		public static IDisposable UseAppLevelResources(ResourceDictionary resources)
		{
			var appResources = Application.Current.Resources;
			appResources.MergedDictionaries.Add(resources);

			return Disposable.Create(() => appResources.MergedDictionaries.Remove(resources));
		}



		/// <summary>
		/// Ensure Fluent styles are available for the course of a single test.
		/// </summary>
		public static IDisposable UseFluentStyles()
		{
#if NETFX_CORE // Disabled on UWP for now because 19041 doesn't support WinUI 2.x; Fluent resources are used by default in SamplesApp.UWP
			return null;
#else

			NativeDispatcher.CheckThreadAccess();

			var resources = Application.Current.Resources;
			if (resources is Microsoft.UI.Xaml.Controls.XamlControlsResources || resources.MergedDictionaries.OfType<Microsoft.UI.Xaml.Controls.XamlControlsResources>().Any())
			{
				return null;
			}

			var xcr = new Microsoft.UI.Xaml.Controls.XamlControlsResources();
			resources.MergedDictionaries.Insert(0, xcr);

			// Force default brushes to be reloaded
			DefaultBrushes.ResetDefaultThemeBrushes();
			ResetIslandRootForeground();

			return new DisposableAction(() =>
			{
				resources.MergedDictionaries.Remove(xcr);
				DefaultBrushes.ResetDefaultThemeBrushes();
				ResetIslandRootForeground();
			});
#endif
		}

#if !NETFX_CORE
		private static void ResetIslandRootForeground()
		{
			if (TestServices.WindowHelper.IsXamlIsland && VisualTreeUtils.FindVisualChildByType<Control>(TestServices.WindowHelper.XamlRoot.Content) is { } control)
			{
				// Ensure the root element's Foreground is set correctly
				control.SetValue(Control.ForegroundProperty, DefaultBrushes.TextForegroundBrush, DependencyPropertyValuePrecedences.DefaultValue);
			}
		}
#endif
	}
}

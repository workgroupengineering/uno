﻿#if IS_UNIT_TESTS || __WASM__
#pragma warning disable CS0067
#endif

using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno.Disposables;
using System.Linq;
using Windows.Devices.Input;
using Windows.System;
using Microsoft.UI.Xaml.Controls;
using Uno.UI;
using Uno;
using Uno.UI.Controls;
using Uno.UI.Media;
using System;
using System.Numerics;
using System.Reflection;
using Microsoft.UI.Xaml.Markup;

using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using Uno.UI.Xaml;

#if __IOS__
using UIKit;
#endif

namespace Microsoft.UI.Xaml
{
	public partial class UIElement : DependencyObject, IXUidProvider
	{
		[GeneratedDependencyProperty(DefaultValue = true, ChangedCallback = true)]
		public static DependencyProperty IsHitTestVisibleProperty { get; } = CreateIsHitTestVisibleProperty();

		public bool IsHitTestVisible
		{
			get => GetIsHitTestVisibleValue();
			set => SetIsHitTestVisibleValue(value);
		}

		[GeneratedDependencyProperty(DefaultValue = 1.0, ChangedCallback = true)]
		public static DependencyProperty OpacityProperty { get; } = CreateOpacityProperty();

		public double Opacity
		{
			get => GetOpacityValue();
			set => SetOpacityValue(value);
		}

		/// <summary>
		/// Sets the visibility of the current view
		/// </summary>
		[GeneratedDependencyProperty(DefaultValue = Visibility.Visible, ChangedCallback = true, Options = FrameworkPropertyMetadataOptions.AffectsMeasure)]
		public static DependencyProperty VisibilityProperty { get; } = CreateVisibilityProperty();

		public
#if __ANDROID__
		new
#endif
		Visibility Visibility
		{
			get => GetVisibilityValue();
			set => SetVisibilityValue(value);
		}

		[GeneratedDependencyProperty(DefaultValue = null, ChangedCallback = true, Options = FrameworkPropertyMetadataOptions.LogicalChild)]
		public static DependencyProperty ContextFlyoutProperty { get; } = CreateContextFlyoutProperty();

		public FlyoutBase ContextFlyout
		{
			get => GetContextFlyoutValue();
			set => SetContextFlyoutValue(value);
		}

		[GeneratedDependencyProperty(DefaultValue = null)]
		internal static DependencyProperty KeyboardAcceleratorsProperty { get; } = CreateKeyboardAcceleratorsProperty();

		public IList<KeyboardAccelerator> KeyboardAccelerators
		{
			get => GetKeyboardAcceleratorsValue() ?? (KeyboardAccelerators = new List<KeyboardAccelerator>());
			set => SetKeyboardAcceleratorsValue(value);
		}
	}
}

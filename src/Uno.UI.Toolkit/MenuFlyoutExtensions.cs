﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Toolkit
{
	public class MenuFlyoutExtensions
	{
		#region Property: CancelTextIosOverride

		public static DependencyProperty CancelTextIosOverrideProperty { get; } = DependencyProperty.RegisterAttached(
			"CancelTextIosOverride",
			typeof(string),
			typeof(MenuFlyoutExtensions),
			new PropertyMetadata(default(string)));

		public static string GetCancelTextIosOverride(MenuFlyout obj) => (string)obj.GetValue(CancelTextIosOverrideProperty);

		public static void SetCancelTextIosOverride(MenuFlyout obj, string value) => obj.SetValue(CancelTextIosOverrideProperty, value);
		#endregion
	}
}

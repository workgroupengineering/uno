﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Uno.UI;

namespace UITests.Windows_UI_Xaml_Controls.ToolTip
{
	[SampleControlInfo(nameof(ToolTip), nameof(ToolTip_DynamicContent), description: SampleDescription)]
	public sealed partial class ToolTip_DynamicContent : Page
	{
		private const string SampleDescription =
			"ToolTip can be updated on demand, with exception of 'null' that should disable the tooltip. " +
			"Every other options, empty string included, should update the tooltip content, and re-enabled it if it was previously disabled with 'null'. \n" +
			"https://github.com/unoplatform/uno/issues/6050 \n" +
			"https://github.com/unoplatform/uno/issues/6200";

		public ToolTip_DynamicContent()
		{
			this.InitializeComponent();
		}

		private void SetTooltip(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var tag = button.Tag;

			ToolTipService.SetToolTip(TooltipTarget, tag);
		}
	}
}

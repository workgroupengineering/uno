﻿using SamplesApp.Windows_UI_Xaml_Controls.WebView;
using Uno.UI.Samples.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Uno.UI.Samples.Content.UITests.WebView
{
	[SampleControlInfo(viewModelType: typeof(WebViewViewModel), ignoreInSnapshotTests: true /*WebView may or may not have fully loaded*/)]
	public sealed partial class WebView_Events : UserControl
	{
		public WebView_Events()
		{
			this.InitializeComponent();
		}
	}
}

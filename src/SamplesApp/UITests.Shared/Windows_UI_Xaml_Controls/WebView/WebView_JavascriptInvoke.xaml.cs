﻿using Uno.UI.Samples.Controls;
using System;
using Microsoft.UI.Xaml;
using Windows.UI.Core;
using System.Threading;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using SamplesApp.Windows_UI_Xaml_Controls.WebView;

namespace Uno.UI.Samples.Content.UITests.WebView
{
	[SampleControlInfo("WebView", "WebView_JavascriptInvoke", typeof(WebViewViewModel))]
	public sealed partial class WebView_JavascriptInvoke : UserControl
	{
#if HAS_UNO
		public WebView_JavascriptInvoke()
		{
			this.InitializeComponent();
			MyButton.Click += MyButton_OnClick;
		}

		private void MyButton_OnClick(object sender, RoutedEventArgs e)
		{
			var t = Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
				async () => await InvokeScriptAsync(MyWebView2, CancellationToken.None, GetReloadJavascript(), new string[] { "" })
			);
		}
		public static async Task<string> InvokeScriptAsync(Microsoft.UI.Xaml.Controls.WebView webView, CancellationToken ct, string script, string[] arguments)
		{
			return await webView.InvokeScriptAsync(script, arguments).AsTask(ct);
		}

		private static string GetReloadJavascript()
		{
#if __IOS__
			return "location.reload(true);";
#else
			return "window.location.reload()";
#endif
		}
#endif
	}
}

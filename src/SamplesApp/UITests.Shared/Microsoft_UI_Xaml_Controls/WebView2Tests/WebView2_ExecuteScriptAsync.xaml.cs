﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace UITests.Microsoft_UI_Xaml_Controls.WebView2Tests
{
	[Uno.UI.Samples.Controls.Sample("WebView")]
	public sealed partial class WebView2_ExecuteScriptAsync : Page
	{
		public WebView2_ExecuteScriptAsync()
		{
			this.InitializeComponent();
			TestWebView.Loaded += TestWebView_Loaded;
		}

		private async void TestWebView_Loaded(object sender, RoutedEventArgs e)
		{
			await TestWebView.EnsureCoreWebView2Async();
			var testHtml = "<html><body><div id='test' style='width: 100px; height: 100px; background-color: blue;' /></body></html>";
			TestWebView.NavigateToString(testHtml);
		}

		private async void ChangeColor()
		{
			await TestWebView.ExecuteScriptAsync("document.getElementById('test').style.backgroundColor = 'red';");
		}

		private async void GetColor()
		{
			var color = await TestWebView.ExecuteScriptAsync("eval({ 'color' : document.getElementById('test').style.backgroundColor })");
			CurrentColorTextBlock.Text = color;
		}
	}
}

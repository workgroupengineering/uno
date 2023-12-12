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
using Uno.UI.Samples.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Windows_UI_Xaml_Controls.TextBox
{
	[Sample("TextBox")]
	public sealed partial class TextBox_Wrap : UserControl
	{

		public static readonly DependencyProperty LocalTextWrappingProperty = DependencyProperty.Register(
		"LocalTextWrapping", typeof(TextWrapping), typeof(TextBox_Wrap), new PropertyMetadata(TextWrapping.Wrap));

		public TextWrapping LocalTextWrapping
		{
			get { return (TextWrapping)GetValue(LocalTextWrappingProperty); }
			set { SetValue(LocalTextWrappingProperty, value); }
		}

		public TextBox_Wrap()
		{
			this.InitializeComponent();

			textWrapBind.Text = textWrap.Text;
		}

		private void OnWrapButtonClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			if (buttonWrap.Content.ToString() == TextWrapping.Wrap.ToString())
			{
				textWrap.TextWrapping = TextWrapping.NoWrap;
				textWrapBind.TextWrapping = TextWrapping.NoWrap;
				buttonWrap.Content = textWrap.TextWrapping.ToString();
				LocalTextWrapping = textWrap.TextWrapping;
				textWrapBind.Text = textWrap.Text;
			}
			else
			{
				textWrap.TextWrapping = TextWrapping.Wrap;
				buttonWrap.Content = textWrap.TextWrapping.ToString();
				textWrapBind.TextWrapping = TextWrapping.Wrap;
				LocalTextWrapping = textWrap.TextWrapping;
				textWrapBind.Text = textWrap.Text;
			}
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Uno.UI.Tests.Windows_UI_Xaml_Data.xBindTests.Controls
{
#pragma warning disable UXAML0002
	public sealed partial class xBind_UserControl_To_OtherControl_Inner /* : UserControl: Don't specify the base type so x:Bind generator lookups uses the XAML control type */
#pragma warning restore UXAML0002
	{
		public xBind_UserControl_To_OtherControl_Inner()
		{
			this.InitializeComponent();
		}

		public string MyProperty
		{
			get { return (string)GetValue(MyPropertyProperty); }
			set { SetValue(MyPropertyProperty, value); }
		}

		// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MyPropertyProperty =
			DependencyProperty.Register("MyProperty", typeof(string), typeof(xBind_UserControl_To_OtherControl_Inner), new PropertyMetadata("Default string"));
	}
}

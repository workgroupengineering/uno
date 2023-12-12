﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.UI.Tests.Windows_UI_Xaml_Data.BindingTests.Controls
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Binding_ElementName_In_Template_ItemsControl_NonUINested : Page
	{
		public Binding_ElementName_In_Template_ItemsControl_NonUINested()
		{
			this.InitializeComponent();
		}
	}

	public partial class Binding_ElementName_In_Template_ItemsControl_NonUINested_DO : DependencyObject
	{
		public object InnerProperty
		{
			get => (object)GetValue(InnerPropertyProperty);
			set => SetValue(InnerPropertyProperty, value);
		}

		// Using a DependencyProperty as the backing store for InnerProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InnerPropertyProperty =
			DependencyProperty.Register("InnerProperty",
							   typeof(object),
							   typeof(Binding_ElementName_In_Template_ItemsControl_NonUINested_DO),
							   new PropertyMetadata(null));
	}

	public partial class Binding_ElementName_In_Template_ItemsControl_NonUINested_Attached
	{

		public static Binding_ElementName_In_Template_ItemsControl_NonUINested_DO GetNonUIObject(DependencyObject obj) => (Binding_ElementName_In_Template_ItemsControl_NonUINested_DO)obj.GetValue(NonUIObjectProperty);

		public static void SetNonUIObject(DependencyObject obj, Binding_ElementName_In_Template_ItemsControl_NonUINested_DO value) => obj.SetValue(NonUIObjectProperty, value);

		// Using a DependencyProperty as the backing store for NonUIObject.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty NonUIObjectProperty =
			DependencyProperty.RegisterAttached("NonUIObject",
									   typeof(Binding_ElementName_In_Template_ItemsControl_NonUINested_DO),
									   typeof(Binding_ElementName_In_Template_ItemsControl_NonUINested_Attached),
									   new PropertyMetadata(null));
	}
}

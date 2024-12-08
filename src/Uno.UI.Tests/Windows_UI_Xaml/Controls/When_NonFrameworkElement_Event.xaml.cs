﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Uno.UI.Tests.Windows_UI_Xaml.Controls
{
	public sealed partial class When_NonFrameworkElement_Event : MyNonFrameworkElement
	{
		public When_NonFrameworkElement_Event()
		{
			this.InitializeComponent();
		}

		public int MyEventCalled { get; private set; }

		public void OnMyEvent(object sender, object args)
		{
			MyEventCalled++;
		}
	}

	[ContentProperty(Name = nameof(MyContent))]
	public partial class MyNonFrameworkElement : DependencyObject
	{
		public event EventHandler MyEvent;

		public void RaiseMyEvent()
		{
			MyEvent?.Invoke(this, null);
		}

		public object MyContent
		{
			get => (object)GetValue(MyContentProperty);
			set => SetValue(MyContentProperty, value);
		}

		public static readonly DependencyProperty MyContentProperty =
			DependencyProperty.Register("MyContent", typeof(object), typeof(MyNonFrameworkElement), new PropertyMetadata(null));
	}
}

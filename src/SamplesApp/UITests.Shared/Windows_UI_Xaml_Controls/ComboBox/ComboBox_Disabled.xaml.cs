﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SamplesApp.Windows_UI_Xaml_Controls.Models;
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

namespace UITests.Windows_UI_Xaml_Controls.ComboBox
{
	[SampleControlInfo("ComboBox", "ComboBox_Disabled", typeof(ListViewViewModel), description: "ComboBox should be disabled by default.")]
	public sealed partial class ComboBox_Disabled : UserControl
	{
		public ComboBox_Disabled()
		{
			this.InitializeComponent();
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UITests.Shared.Windows_UI_Xaml_Controls.TimePicker.Model;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace UITests.Shared.Windows_UI_Xaml_Controls.TimePicker
{
	[SampleControlInfo("Pickers", "Sample2", typeof(TimePickerViewModel), ignoreInSnapshotTests: true)]
	public sealed partial class Sample2 : UserControl
	{
		public Sample2()
		{
			this.InitializeComponent();
		}
	}
}

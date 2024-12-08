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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Windows_UI_Xaml_Controls.FlipView;

[SampleControlInfo(
	"FlipView",
	"FlipView_Background",
	description: "Shows a green Page with a red FlipView in the middle. \n" +
	"This tests that the FlipView background is displayed when the FlipView is empty.",
	isManualTest: true)]
public sealed partial class FlipView_Background : UserControl
{
	public FlipView_Background()
	{
		this.InitializeComponent();
	}
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using UITests.Shared.Helpers;
using System.Threading.Tasks;

namespace UITests.Windows_UI_Xaml_Controls.ImageTests;

[Sample("Image")]
public sealed partial class SvgImageSource_NoSize : Page, IWaitableSample
{
	private readonly Task _samplePreparedTask;

	public SvgImageSource_NoSize()
	{
		this.InitializeComponent();
		_samplePreparedTask = WaitableSampleImageHelpers.WaitAllImages(image1);
	}

	public Task SamplePreparedTask => _samplePreparedTask;
}

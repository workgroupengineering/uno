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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Shared.Windows_UI_Xaml_Controls.BorderTests
{
	[SampleControlInfo("Border", description: "Outline should look the same (aside from corners) with/without CornerRadius")]
	public sealed partial class Border_CornerRadius_Toggle : UserControl
	{
		public Border_CornerRadius_Toggle()
		{
			this.InitializeComponent();
		}

		private void ToggleCornerRadius(object sender, RoutedEventArgs args)
		{
			var currentRadius = SubjectBorder.CornerRadius;
			var newRadius = currentRadius.TopLeft == 0 ?
				CornerRadiusHelper.FromUniformRadius(5) :
				default(CornerRadius);
			SubjectBorder.CornerRadius = newRadius;
			ToggleCornerRadiusButton.Content = $"Toggle CornerRadius (current={newRadius.TopLeft})";
			StatusTextBlock.Text = newRadius.TopLeft.ToString();
		}
	}
}

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

namespace UITests.Windows_UI_Xaml_Media_Animation
{
	[Sample("Animations")]
	public sealed partial class ColorAnimation_Background : UserControl
	{
		public ColorAnimation_Background()
		{
			this.InitializeComponent();
		}

		private void PlayColorAnimation_Click(object sender, RoutedEventArgs args)
		{
			colorStoryboard.Completed += (o, e) =>
			{
				StatusText.Text = "Completed";
			};
			colorStoryboard.Begin();
		}

		private void CheckBrushEquality_Click(object sender, RoutedEventArgs args)
		{
			// Assert non-null
			var areEqual = ReferenceEquals(TargetBorder.Background, IndependentBorder.Background);
			BrushEqualityText.Text = areEqual.ToString().ToLowerInvariant();
		}
	}
}

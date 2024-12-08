﻿using Windows.UI.Xaml.Controls;
using SamplesApp.Windows_UI_Xaml_Controls.Models;
using Uno.UI.Samples.Controls;
using Windows.UI.Xaml;

namespace SamplesApp.Wasm.Windows_UI_Xaml_Controls.ComboBox
{
	[SampleControlInfo("ComboBox", "ComboBox_PlaceholderText", typeof(ListViewViewModel))]
	public sealed partial class ComboBox_PlaceholderText : UserControl
	{
		public ComboBox_PlaceholderText()
		{
			this.InitializeComponent();
		}

		void ResetSelection(object sender, RoutedEventArgs e)
		{
			TestBox.SelectedItem = null;
			TestBox2.SelectedItem = null;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.UI.Samples.Controls;
using Uno.UI.Samples.UITests.Helpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Private.Infrastructure;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Shared.Windows_UI_Xaml_Controls.ContentDialogTests
{
	[SampleControlInfo("Dialogs", "ContentDialog_ComboBox", description: "ContentDialog with a ComboBox inside")]
	public sealed partial class ContentDialog_ComboBox : UserControl
	{
		public ContentDialog_ComboBox()
		{
			this.InitializeComponent();
		}

		private void ShowComboBoxDialog_Click(object sender, RoutedEventArgs e)
		{
			var viewModel = new ComboBoxDialogViewModel(UnitTestDispatcherCompat.From(this));
			var dialog = new ContentDialog
			{
				Content = new ComboBoxContentDialog(),
				DataContext = viewModel,
				CloseButtonText = "Done"
			};

			ResultsTextBlock.DataContext = viewModel;
			dialog.XamlRoot = this.XamlRoot;
			var dummy = dialog.ShowAsync();
		}

		internal class ComboBoxDialogViewModel : ViewModelBase
		{
			public ComboBoxDialogViewModel(Private.Infrastructure.UnitTestDispatcherCompat dispatcher) : base(dispatcher)
			{

			}

			private object _selectedItem;
			public object SelectedItem
			{
				get => _selectedItem;
				set
				{
					if (!Equals(_selectedItem, value))
					{
						_selectedItem = value;
						RaisePropertyChanged();
						if (value is TextBlock)
						{
							RaisePropertyChanged(nameof(SelectedItemText));
						}
					}
				}
			}

			public object SelectedItemText => (SelectedItem as TextBlock)?.Text;
		}
	}
}

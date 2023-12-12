﻿using System.Windows.Input;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Data;
using Uno.UI.Samples.UITests.Helpers;

using ICommand = System.Windows.Input.ICommand;
using EventHandler = System.EventHandler;

namespace SamplesApp.Windows_UI_Xaml_Controls.Models
{
	[Bindable]
	internal class ComboBoxViewModel : ViewModelBase
	{
		private string _header;
		private const string HeaderText = "Please select:";

		public ComboBoxViewModel(CoreDispatcher dispatcher) : base(dispatcher)
		{
		}

		public string Header
		{
			get => _header;
			set
			{
				_header = value;
				RaisePropertyChanged();
			}
		}

		public ICommand ToggleHeader => GetOrCreateCommand(() => Header = Header == null ? HeaderText : null);
	}
}

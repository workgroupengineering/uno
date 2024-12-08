﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Uno.UI.Samples.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UITests.Windows_UI_Xaml.WindowTests
{
	[Sample("Windowing")]
	public sealed partial class Window_SetBackground : UserControl
	{
		private Color _selectedColor;

		public Window_SetBackground()
		{
			this.InitializeComponent();

#if HAS_UNO
			_selectedColor = (Windows.UI.Xaml.Window.Current?.Background as SolidColorBrush)?.Color ?? Colors.White;
#endif
		}

		public Color SelectedColor
		{
			get => _selectedColor;
			set
			{
				_selectedColor = value;

#if HAS_UNO
				if (Windows.UI.Xaml.Window.Current is { }) // could be null if on WinUI tree
				{
					Windows.UI.Xaml.Window.Current.Background = new SolidColorBrush(_selectedColor);
				}
#endif
			}
		}
	}
}

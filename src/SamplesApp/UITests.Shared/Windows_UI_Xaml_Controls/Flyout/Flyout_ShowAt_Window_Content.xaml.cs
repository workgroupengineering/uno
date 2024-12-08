﻿
using Uno.UI.Samples.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UITests.Windows_UI_Xaml_Controls.FlyoutTests
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	[Sample("Flyouts")]
	public sealed partial class Flyout_ShowAt_Window_Content : Page
	{
		public Flyout_ShowAt_Window_Content()
		{
			this.InitializeComponent();

#if HAS_UNO
			XamlRoot _xamlRoot = null;
			Loaded += (s, e) => _xamlRoot = XamlRoot;
			Unloaded += (s, e) =>
			{
				// close all popups at the end of the test.
				VisualTreeHelper.CloseAllPopups(_xamlRoot);
			};
#endif
		}

		private void ButtonButton_Click(object sender, RoutedEventArgs e)
		{
			Windows.UI.Xaml.Controls.Flyout flyout = new Windows.UI.Xaml.Controls.Flyout();
			flyout.Content =
				new Border()
				{
					Width = 300,
					Height = 300,
					Background = new SolidColorBrush(Windows.UI.Colors.Red),
				};

			flyout.ShowAt((Button)sender);
		}

		private void WindowButton_Click(object sender, RoutedEventArgs e)
		{
			Windows.UI.Xaml.Controls.Flyout flyout = new Windows.UI.Xaml.Controls.Flyout();
			flyout.Content =
				new Border()
				{
					Width = 300,
					Height = 300,
					Background = new SolidColorBrush(Windows.UI.Colors.Red),
				};

			flyout.ShowAt(XamlRoot?.Content as FrameworkElement);
		}
	}
}

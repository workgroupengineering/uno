﻿using System;
using System.Threading.Tasks;
using Uno.UI.Samples.Controls;
using Windows.UI.Core;
using Windows.UI.Popups;
using Microsoft.UI.Xaml.Controls;
using System.Threading;

namespace UITests.Shared.Windows_UI_Xaml_Controls.Popup
{
	[SampleControlInfo("Popup", "MessageDialog", description: "The dialog dims the screen behind it and blocks touch events from passing to the app's canvas until the user responds.")]
	public sealed partial class MessageDialog : Page
	{
		private const string _title = "Internet Connectivity";

		public MessageDialog()
		{
			this.InitializeComponent();

			WithoutTitle.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("No internet connection has been found.");

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};

			WithTitle.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("No internet connection has been found.");
				messageDialog.Title = "Internet Connectivity";

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};


			WithOneCommandAndTitle.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("No internet connection has been found.");

				messageDialog.Commands.Add(new UICommand("Acknowledge", new UICommandInvokedHandler(this.CommandInvokedHandler)));
				messageDialog.Title = "Internet Connectivity";

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};

			WithTwoCommandsAndTitle.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("No internet connection has been found.");
				messageDialog.Title = "Internet Connectivity";

				messageDialog.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler)));
				messageDialog.DefaultCommandIndex = 0;

				messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandInvokedHandler)));
				messageDialog.DefaultCommandIndex = 1;

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};

			WithThreeCommandsAndTitle.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("No internet connection has been found.");
				messageDialog.Title = "My Cool Title";

				messageDialog.Commands.Add(new UICommand("Try Again", new UICommandInvokedHandler(this.CommandInvokedHandler)));
				messageDialog.DefaultCommandIndex = 0;

				messageDialog.Commands.Add(new UICommand("Reset Network Settings", new UICommandInvokedHandler(this.CommandInvokedHandler)));

				messageDialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.CommandInvokedHandler)));
				messageDialog.CancelCommandIndex = 2;

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};

			WithEscapedCharacters.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("\"Sample \\\"force escape test\\\" \\n \\t \\r continued sample.\"");

				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync());
			};

			WithProgrammaticDismissal.Tapped += (snd, evt) =>
			{
				var messageDialog = new Windows.UI.Popups.MessageDialog("It will dismiss in 2000 ms", "Programatically Dismiss");

				var cts = new CancellationTokenSource(2000);
				_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await messageDialog.ShowAsync().AsTask(cts.Token));
			};
		}

		private void CommandInvokedHandler(IUICommand command)
		{
		}
	}
}

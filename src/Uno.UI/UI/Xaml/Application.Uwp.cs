﻿#if !HAS_UNO_WINUI
namespace Microsoft.UI.Xaml;

partial class Application
{
	/// <summary>
	/// Invoked when the application creates a window.
	/// </summary>
	/// <param name="args">Event data for the event.</param>
	protected virtual void OnWindowCreated(global::Microsoft.UI.Xaml.WindowCreatedEventArgs args)
	{
	}

	internal void RaiseWindowCreated(Microsoft.UI.Xaml.Window window) =>
		OnWindowCreated(new WindowCreatedEventArgs(window));
}
#endif

﻿#nullable enable

#if !__ANDROID__ && !__IOS__ && !__MACOS__
using System;
using Windows.Foundation;
using Windows.Media.Playback;
using Microsoft.UI.Xaml.Media;
using Uno.Media.Playback;
using Uno.Foundation.Extensibility;
using Uno.Foundation.Logging;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class MediaPlayerPresenter : Border
	{
		private IMediaPlayerPresenterExtension? _extension;

		partial void InitializePartial()
		{
			if (this.Log().IsEnabled(LogLevel.Debug))
			{
				this.Log().LogDebug($"Enter MediaPlayerPresenter InitializePartial");
			}
			if (!ApiExtensibility.CreateInstance<IMediaPlayerPresenterExtension>(this, out _extension))
			{
				if (this.Log().IsEnabled(LogLevel.Error))
				{
					this.Log().Error("The MediaPlayer extension is not installed. For more information aka.platform.uno/mediaplayerelement");
				}
			}
		}

		partial void OnMediaPlayerChangedPartial(MediaPlayer mediaPlayer)
			=> _extension?.MediaPlayerChanged();

		private void OnStretchChanged(Stretch newValue, Stretch oldValue)
			=> _extension?.StretchChanged();

		internal void ApplyStretch()
			=> _extension?.StretchChanged();

		internal void RequestFullScreen()
			=> _extension?.RequestFullScreen();

		internal void ExitFullScreen()
			=> _extension?.ExitFullScreen();

		internal void RequestCompactOverlay()
			=> _extension?.RequestCompactOverlay();

		internal void ExitCompactOverlay()
			=> _extension?.ExitCompactOverlay();
	}
}
#endif

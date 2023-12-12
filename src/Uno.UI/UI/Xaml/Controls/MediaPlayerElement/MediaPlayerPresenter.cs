﻿using System;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media;
using Uno.Foundation.Logging;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class MediaPlayerPresenter : Border
	{
		private double _currentRatio = 1;

		#region MediaPlayer Property

		public Windows.Media.Playback.MediaPlayer MediaPlayer
		{
			get { return (Windows.Media.Playback.MediaPlayer)GetValue(MediaPlayerProperty); }
			set { SetValue(MediaPlayerProperty, value); }
		}

		public static DependencyProperty MediaPlayerProperty { get; } =
			DependencyProperty.Register(
				nameof(MediaPlayer),
				typeof(Windows.Media.Playback.MediaPlayer),
				typeof(MediaPlayerPresenter),
				new FrameworkPropertyMetadata(default(Windows.Media.Playback.MediaPlayer), OnMediaPlayerChanged));

		private static void OnMediaPlayerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
		{
			if (sender is MediaPlayerPresenter presenter)
			{
				if (presenter.Log().IsEnabled(LogLevel.Debug))
				{
					presenter.Log().LogDebug($"MediaPlayerPresenter.OnMediaPlayerChanged({args.NewValue})");
				}
				if (args.OldValue is Windows.Media.Playback.MediaPlayer oldPlayer)
				{
					oldPlayer.VideoRatioChanged -= presenter.OnVideoRatioChanged;
					oldPlayer.MediaFailed -= presenter.OnMediaFailed;
					oldPlayer.SourceChanged -= presenter.OnSourceChanged;
				}

				if (args.NewValue is Windows.Media.Playback.MediaPlayer newPlayer)
				{
					newPlayer.VideoRatioChanged += presenter.OnVideoRatioChanged;
					newPlayer.MediaFailed += presenter.OnMediaFailed;
					newPlayer.SourceChanged += presenter.OnSourceChanged;

#if __IOS__ || __ANDROID__ || __MACOS__
					presenter.SetVideoSurface(newPlayer.RenderSurface);
#endif

					presenter.OnMediaPlayerChangedPartial(newPlayer);
				}
			}
		}

		#endregion

		partial void OnMediaPlayerChangedPartial(Windows.Media.Playback.MediaPlayer mediaPlayer);

		#region Stretch Property

		public Stretch Stretch
		{
			get { return (Stretch)GetValue(StretchProperty); }
			set { SetValue(StretchProperty, value); }
		}

		public static DependencyProperty StretchProperty { get; } =
			DependencyProperty.Register(
				nameof(Stretch),
				typeof(Stretch),
				typeof(MediaPlayerPresenter),
				new FrameworkPropertyMetadata(Stretch.Uniform, (s, e) => ((MediaPlayerPresenter)s).OnStretchChanged((Stretch)e.NewValue, (Stretch)e.OldValue)));

		#endregion

		#region IsFullWindow Property

		public bool IsFullWindow
		{
			get { return (bool)GetValue(IsFullWindowProperty); }
			set { SetValue(IsFullWindowProperty, value); }
		}

		public static DependencyProperty IsFullWindowProperty { get; } =
			DependencyProperty.Register(
				nameof(IsFullWindow),
				typeof(bool),
				typeof(MediaPlayerPresenter),
				new FrameworkPropertyMetadata(false));

		#endregion

		public MediaPlayerPresenter() : base()
		{
			InitializePartial();
		}

		partial void InitializePartial();

		/// <summary>
		/// Indicates whether or not the player is currently toggling the fullscreen mode.
		/// </summary>
		internal bool IsTogglingFullscreen { get; set; }

		private protected override void OnUnloaded()
		{
			// The control will get unloaded when going to full screen mode.
			// Similar to UWP, the video should keep playing while changing mode.
			if (!IsTogglingFullscreen)
			{
				MediaPlayer.Stop();
			}

			base.OnUnloaded();
		}

		private void OnVideoRatioChanged(Windows.Media.Playback.MediaPlayer sender, double args)
		{
			if (args > 0) // The VideoRect may initially be empty, ignore because a 0 ratio will lead to infinite dims being returned on measure, resulting in an exception
			{
				_currentRatio = args;
			}

			_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Visibility = Visibility.Visible;
			});

			InvalidateArrange();
		}

		private void OnMediaFailed(Windows.Media.Playback.MediaPlayer sender, MediaPlayerFailedEventArgs args)
		{
			_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Visibility = Visibility.Collapsed;
			});
		}
		private void OnSourceChanged(Windows.Media.Playback.MediaPlayer sender, object args)
		{
			_ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Visibility = Visibility.Visible;
			});
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			if (double.IsNaN(Width) && double.IsNaN(Height))
			{
				availableSize.Width = availableSize.Width;
				if (_currentRatio != 0)
				{
					availableSize.Height = availableSize.Width / _currentRatio;
				}
			}
			else if (double.IsNaN(Width))
			{
				availableSize.Width = Height * _currentRatio;
				availableSize.Height = Height;
			}
			else if (double.IsNaN(Height))
			{
				availableSize.Width = Width;
				availableSize.Height = Width / _currentRatio;
			}

			base.MeasureOverride(availableSize);

			return availableSize;
		}
	}
}

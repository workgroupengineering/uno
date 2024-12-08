﻿using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Uno.UI.Samples.Controls;


namespace Uno.UI.Samples.Content.UITests.Animations
{
	[SampleControlInfo("Animations", "DoubleAnimation_BeginTime")]
	public sealed partial class DoubleAnimation_BeginTime : UserControl
	{
		public DoubleAnimation_BeginTime()
		{
			this.InitializeComponent();
		}

		private void StartAnimation(object sender, TappedRoutedEventArgs e)
		{
			var animation = new DoubleAnimation
			{
				To = 0,
				BeginTime = TimeSpan.FromSeconds(3),
				Duration = new Duration(TimeSpan.FromSeconds(3))
			};
			Storyboard.SetTargetProperty(animation, nameof(TranslateTransform.Y));
			Storyboard.SetTarget(animation, _transform);

			new Storyboard
			{
				Children = { animation }
			}.Begin();
		}

		private void ResetAnimation(object sender, TappedRoutedEventArgs e)
		{
			_transform.Y = 150;
		}
	}
}

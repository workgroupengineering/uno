﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Windows.Foundation;
using Microsoft.UI.Xaml.Automation.Peers;
using DirectUI;
using Uno.Disposables;
using Uno.UI.DataBinding;
using Uno.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Controls
{
	partial class ScrollViewer
	{
		private IDisposable? _directManipulationHandlerSubscription;

		internal bool m_templatedParentHandlesMouseButton;

		// Indicates whether ScrollViewer should ignore mouse wheel scroll events (not zoom).
		internal bool ArePointerWheelEventsIgnored { get; set; }
		internal bool IsInManipulation => IsInDirectManipulation || m_isInConstantVelocityPan;

		/// <summary>
		/// Gets or set whether the <see cref="ScrollViewer"/> will allow scrolling outside of the ScrollViewer's Child bound.
		/// </summary>
		///
		private bool _forceChangeToCurrentView;
		internal bool ForceChangeToCurrentView
		{
			get => _forceChangeToCurrentView;
			set
			{
				_forceChangeToCurrentView = value;

#if __WASM__ || __SKIA__
				if (_presenter != null)
				{
					_presenter.ForceChangeToCurrentView = value;
				}
#endif
			}
		}
		internal bool IsInDirectManipulation { get; }
		internal bool TemplatedParentHandlesScrolling { get; set; }
		internal Func<AutomationPeer>? AutomationPeerFactoryIndex { get; set; }

		internal bool BringIntoViewport(Rect bounds,
			bool skipDuringTouchContact,
			bool skipAnimationWhileRunning,
			bool animate)
		{
#if __WASM__
			return ChangeView(bounds.X, bounds.Y, null, true);
#else
			return ChangeView(bounds.X, bounds.Y, null, !animate);
#endif
		}

		internal void SetDirectManipulationStateChangeHandler(IDirectManipulationStateChangeHandler? handler)
		{
			_directManipulationHandlerSubscription?.Dispose();

			if (handler is null)
			{
				return;
			}

			var weakHandler = WeakReferencePool.RentWeakReference(this, handler);
			UpdatesMode = ScrollViewerUpdatesMode.Synchronous;
			ViewChanged += OnViewChanged;
			_directManipulationHandlerSubscription = Disposable.Create(() =>
			{
				ViewChanged -= OnViewChanged;
				WeakReferencePool.ReturnWeakReference(this, weakHandler);
			});

			void OnViewChanged(object? sender, ScrollViewerViewChangedEventArgs args)
			{
				if (args.IsIntermediate)
				{
					return;
				}

				if (weakHandler.Target is IDirectManipulationStateChangeHandler h)
				{
					h.NotifyStateChange(DMManipulationState.DMManipulationCompleted, default, default, default, default, default, default, default, default);
				}
			}
		}
	}

	internal interface IDirectManipulationStateChangeHandler
	{
		// zCumulativeFactor: if the zoom factor was 1.5 at the beginning of the manipulation,
		// and the current zoom factor is 3.0, then zCumulativeFactor is 2.0.
		// xCenter/yCenter: these coordinates are in relation to the top/left corner of the
		// manipulated element. They might be negative if the ScrollViewer content is smaller
		// than the viewport.
		void NotifyStateChange(
			DMManipulationState state,
			float xCumulativeTranslation,
			float yCumulativeTranslation,
			float zCumulativeFactor,
			float xCenter,
			float yCenter,
			bool isInertial,
			bool isTouchConfigurationActivated,
			bool isBringIntoViewportConfigurationActivated);
	}
}

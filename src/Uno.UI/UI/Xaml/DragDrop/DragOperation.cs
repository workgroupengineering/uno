﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Uno.UI.Xaml.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.DragDrop.Core;
using Uno.UI.Dispatching;

namespace Windows.UI.Xaml
{
	/// <summary>
	/// The state machine of a dragging operation from the initiation of a dragging event
	/// when a pointer holds and moves to the completion of the event when the pointer
	/// is released
	/// </summary>
	internal class DragOperation
	{
		private static readonly TimeSpan _deferralTimeout = TimeSpan.FromSeconds(30); // Random value!

		private readonly InputManager _inputManager;
		private readonly IDragDropExtension? _extension;
		private readonly ICoreDropOperationTarget _target;
		private readonly DragView _view;
		private readonly IDisposable _viewHandle;
		private readonly CoreDragUIOverride _viewOverride;
		private readonly LinkedList<TargetAsyncTask> _queue = new LinkedList<TargetAsyncTask>();

		private bool _isRunning;
		private bool _isOverWindow; // This can probably be coalesced with the _state ...
		private State _state = State.None;
		private uint _lastFrameId;
		private bool _hasRequestedNativeDrag;
		private DataPackageOperation _acceptedOperation = DataPackageOperation.Copy | DataPackageOperation.Move | DataPackageOperation.Link;

		private enum State
		{
			None,
			Over,
			Completing,
			Completed
		}

		public DragOperation(InputManager inputManager, IDragDropExtension? extension, CoreDragInfo info, ICoreDropOperationTarget? target = null)
		{
			_inputManager = inputManager;
			_extension = extension;
			Info = info;

			_target = target ?? new DropUITarget(inputManager.ContentRoot.XamlRoot!); // The DropUITarget must be re-created for each drag operation! (Caching of the drag ui-override)
			_view = new DragView(info.DragUI as DragUI);
			_view.SetLocation(Info.Position);
			_viewHandle = inputManager.OpenDragAndDrop(_view);
			_viewOverride = new CoreDragUIOverride(); // UWP does re-use the same instance for each update on _target
		}

		public CoreDragInfo Info { get; }

		internal DataPackageOperation Moved(IDragEventSource src)
		{
			if (_state >= State.Completing || src.FrameId <= _lastFrameId)
			{
				return _acceptedOperation;
			}

			var wasOverWindow = _isOverWindow;

			_isOverWindow = _inputManager.ContentRoot.XamlRoot?.Bounds.Contains(src.GetPosition(null)) ?? false;

			Update(src); // It's required to do that as soon as possible in order to update the view's location

			if (wasOverWindow && !_isOverWindow)
			{
				Enqueue(RaiseRecoverableLeave);
			}
			else
			{
				Enqueue(RaiseEnterOrOver, isIgnorable: _state == State.Over); // This is ignorable only if we already over
			}

			return _acceptedOperation;
		}

		internal DataPackageOperation Aborted()
		{
			// For safety, we don't validate the FrameId for the finalizing actions, we rely only on the _state
			if (_state >= State.Completing)
			{
				return _acceptedOperation;
			}

			Enqueue(RaiseFinalLeave);

			return _acceptedOperation;
		}

		internal DataPackageOperation Released(IDragEventSource src)
		{
			// For safety, we don't validate the FrameId for the finalizing actions, we rely only on the _state
			if (_state >= State.Completing)
			{
				return _acceptedOperation;
			}

			Update(src);

			if (_acceptedOperation is DataPackageOperation.None)
			{
				Abort();
			}
			else
			{
				Enqueue(RaiseDrop);
			}

			return _acceptedOperation;
		}

		/// <summary>
		/// This is used by the manager to abort a pending DnD for any consideration without an event args for the given pointer
		/// It ** MUST ** be invoked on the UI thread.
		/// </summary>
		internal void Abort()
		{
			if (_state == State.None)
			{
				// The D&D didn't had time to be started (or has already left), we can just complete
				Complete(DataPackageOperation.None);
				return;
			}

			Enqueue(RaiseFinalLeave);
		}

		private async Task RaiseEnterOrOver(CancellationToken ct)
		{
			if (_state >= State.Completing)
			{
				return;
			}

			var isOver = _state == State.Over;
			_state = State.Over;

			if (!isOver)
			{
				await _target.EnterAsync(Info, _viewOverride).AsTask(ct);
				// No waiting for Idle here. This is similar to what happens on WinUI.
			}
			var acceptedOperation = await _target.OverAsync(Info, _viewOverride).AsTask(ct);

			_acceptedOperation = acceptedOperation;
			_view.Update(acceptedOperation, _viewOverride);
		}

		/// <summary>
		/// This is only for internally initiated drag operation for which we capture the pointer
		/// and which **may come back over the window**.
		/// </summary>
		private async Task RaiseRecoverableLeave(CancellationToken ct)
		{
			if (_state != State.Over)
			{
				return;
			}

			_state = State.None;
			_acceptedOperation = DataPackageOperation.None;
			_viewOverride.Clear();
			await _target.LeaveAsync(Info).AsTask(ct);

			// When the pointer goes out of the window, we hide our internal control and,
			// if supported by the OS, we request a Drag and Drop operation with the native UI.
			_viewHandle.Dispose();
			if (!_hasRequestedNativeDrag)
			{
				_extension?.StartNativeDrag(Info);
				_hasRequestedNativeDrag = true;
			}
		}

		private async Task RaiseFinalLeave(CancellationToken ct)
		{
			try
			{
				if (_state != State.Over)
				{
					return;
				}

				_state = State.Completing;
				_acceptedOperation = DataPackageOperation.None;
				await _target.LeaveAsync(Info).AsTask(ct);
			}
			finally
			{
				Complete(DataPackageOperation.None);
			}
		}

		private async Task RaiseDrop(CancellationToken ct)
		{
			var result = DataPackageOperation.None;
			try
			{
				if (_state != State.Over)
				{
					return;
				}

				_state = State.Completing;
#if __WASM__
				// firing OverAsync then DropAsync without a layout cycle in-between breaks ListView dragging
				// (since it causes a Refresh that clears all the containers and then waits for MeasureOverride
				// to recreate them). So on WASM, we don't call OverAsync, which shouldn't be too problematic.
				if (NativeDispatcher.IsThreadingSupported)
#endif
				{
					await _target.OverAsync(Info, _viewOverride).AsTask(ct);
					// give a chance for layout updates, etc. This is similar to what happens on WinUI.
					await NativeDispatcher.Main.EnqueueAsync(() => { }, NativeDispatcherPriority.Idle);
				}
				result = await _target.DropAsync(Info).AsTask(ct);
				result &= Info.AllowedOperations;

				_acceptedOperation = result;
			}
			finally
			{
				Complete(result);
			}
		}

		private void Update(IDragEventSource src)
		{
			// As ugly as it is, it's the behavior of UWP, the CoreDragInfo is a mutable object!
			Info.UpdateSource(src);

			// Updates the view location to follow the pointer
			// Note: This must be set AFTER the Info has been updated
			_view.SetLocation(Info.Position);

			// As we have multiple sources for the pointer events (capture of the dragged element and DragRoot),
			// we make sure to not process the same event twice.
			_lastFrameId = src.FrameId;
		}

		private void Enqueue(Func<CancellationToken, Task> action, bool isIgnorable = false)
		{
			// If possible we debounce multiple "over" update.
			// This might happen when the app uses the DragEventsArgs.GetDeferral (or customized the _target).
			if (_queue.Last?.Value.IsIgnorable ?? false)
			{
				_queue.RemoveLast();
			}

			_queue.AddLast(new TargetAsyncTask(action, isIgnorable));

			Run();
		}

		private async void Run()
		{
			// This ** MUST ** be run on the UI thread

			if (_isRunning || _state == State.Completed)
			{
				return;
			}

			try
			{
				_isRunning = true;
				while (_state != State.Completed && _queue.First is { } first)
				{
					_queue.RemoveFirst();

					try
					{
						using var ct = new CancellationTokenSource(_deferralTimeout);
						await first.Value.Invoke(ct.Token);
						ct.Cancel();
					}
					catch (OperationCanceledException)
					{
						Application.Current.RaiseRecoverableUnhandledException(new Exception("A Drag async took too long and has been cancelled"));
					}
					catch (Exception e)
					{
						Application.Current.RaiseRecoverableUnhandledException(new Exception("Drag async callback failed", e));
					}
				}
			}
			catch (Exception e)
			{
				Application.Current.RaiseRecoverableUnhandledException(new Exception("Drag event dispatch process failed", e));
			}
			finally
			{
				_isRunning = false;
			}
		}

		private void Complete(DataPackageOperation result)
		{
			if (_state == State.Completed)
			{
				return;
			}
			_state = State.Completed;

			_viewHandle.Dispose();
			Info.Complete(result);
		}

		private struct TargetAsyncTask
		{
			private readonly Func<CancellationToken, Task> _action;

			public bool IsIgnorable { get; }

			public TargetAsyncTask(Func<CancellationToken, Task> action, bool isIgnorable)
			{
				IsIgnorable = isIgnorable;
				_action = action;
			}

			public Task Invoke(CancellationToken ct)
				=> _action(ct);
		}
	}
}

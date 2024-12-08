﻿#nullable enable
#if WINUI || HAS_UNO_WINUI
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Uno.UI;
using Uno.UI.Toolkit.Extensions;

namespace Uno.Diagnostics.UI;

public sealed partial class DiagnosticsOverlay
{
	private const string _originSettingsKey = "__UNO__" + nameof(DiagnosticsOverlay) + "__placement" + nameof(_origin);
	private const string _locationXSettingsKey = "__UNO__" + nameof(DiagnosticsOverlay) + "__placement" + nameof(_location) + "_X";
	private const string _locationYSettingsKey = "__UNO__" + nameof(DiagnosticsOverlay) + "__placement" + nameof(_location) + "_Y";

	private const int _horizontalSnappingDistance = 25;
	private const int _verticalSnappingDistance = 25;
	private const int _rightDirectionDistance = 250;

	[Flags]
	private enum PlacementOrigin
	{
		Left = 0, // 0 << 1,
		Right = 1 << 1,

		Top = 0, // 0 << 2,
		Bottom = 1 << 2,

		TopLeft = Top | Left,
		TopRight = Top | Right,
		BottomLeft = Bottom | Left,
		BottomRight = Bottom | Right,
	}

	private readonly bool _magnetic = true;
	private readonly Thickness _edgePadding = new(4);

	private PlacementOrigin _origin;
	private Point _location; // The location, relative to the _origin
#if HAS_UNO_WINUI
	private StatusBar? _statusBar;
#endif
	private bool _isPlacementInit;

	private void InitPlacement()
	{
		CleanPlacement();
		RestoreLocation();

#if HAS_UNO_WINUI
		if (ApiInformation.IsMethodPresent(typeof(StatusBar), nameof(StatusBar.GetForCurrentView))
			&& StatusBar.GetForCurrentView() is { } statusBar)
		{
			_statusBar = statusBar;

			statusBar.Hiding += OnStatusBarChanged;
			statusBar.Showing += OnStatusBarChanged;
		}
#endif

		_isPlacementInit = true;
	}

	private void CleanPlacement()
	{
		_isPlacementInit = false;

#if HAS_UNO_WINUI
		if (_statusBar is { } statusBar)
		{
			statusBar.Hiding -= OnStatusBarChanged;
			statusBar.Showing -= OnStatusBarChanged;
		}
#endif
	}

	/// <summary>
	/// Gets the area where the top-left corner of the overlay can be placed (i.e. already excludes the overlay size).
	/// </summary>
	/// <returns></returns>
	public Rect GetSafeArea()
	{
#if HAS_UNO_WINUI
		var bounds = _root.Bounds;
		if (
			_statusBar?.OccludedRect is { Height: > 0 } occludedRect
#if __ANDROID__
			&& (Window.Current?.IsStatusBarTranslucent() ?? true)
#endif
			)
		{
			bounds.Y += occludedRect.Height;
			bounds.Height -= occludedRect.Height;
		}
#else
		var bounds = new Rect(default, _root.Size);
#endif

		bounds.Width -= _toolbar?.ActualWidth ?? ActualWidth;
		bounds.Height -= ActualHeight;

		bounds = bounds.DeflateBy(_edgePadding);

		return bounds;
	}

	public Point GetAbsoluteLocation()
	{
		var area = GetSafeArea();

		var x = _origin.HasFlag(PlacementOrigin.Right)
			? area.Right - _location.X
			: area.Left + _location.X;

		var y = _origin.HasFlag(PlacementOrigin.Bottom)
			? area.Bottom - _location.Y
			: area.Top + _location.Y;

		return new Point(x, y);
	}

	private void OnAnchorManipulated(object sender, ManipulationDeltaRoutedEventArgs e)
	{
		_location.X += _origin.HasFlag(PlacementOrigin.Right) ? -e.Delta.Translation.X : e.Delta.Translation.X;
		_location.Y += _origin.HasFlag(PlacementOrigin.Bottom) ? -e.Delta.Translation.Y : e.Delta.Translation.Y;

		ApplyLocation();
	}

	private void OnAnchorManipulatedCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
		=> UpdatePlacement();

#if HAS_UNO_WINUI
	private void OnStatusBarChanged(StatusBar sender, object args)
		=> UpdatePlacement();
#endif

	private void UpdatePlacement()
	{
		if (!_isPlacementInit)
		{
			return;
		}

		var area = GetSafeArea();

		// First constraints in the area, no matter the anchor type here
		_location.X = Math.Max(0, Math.Min(area.Width, _location.X));
		_location.Y = Math.Max(0, Math.Min(area.Height, _location.Y));

		// Snap to the edges of the area
		var location = GetAbsoluteLocation();
		var distanceToRight = Math.Max(0, area.Right - location.X);
		if (distanceToRight < _horizontalSnappingDistance && !_origin.HasFlag(PlacementOrigin.Right))
		{
			_origin |= PlacementOrigin.Right;
			_location.X = _magnetic ? 0 : distanceToRight;
		}

		var distanceToLeft = Math.Max(0, location.X - area.Left);
		if (distanceToLeft < _horizontalSnappingDistance && _origin.HasFlag(PlacementOrigin.Right))
		{
			_origin &= ~PlacementOrigin.Right;
			_location.X = _magnetic ? 0 : distanceToLeft;
		}

		var distanceToBottom = Math.Max(0, area.Bottom - location.Y);
		if (distanceToBottom < _verticalSnappingDistance && !_origin.HasFlag(PlacementOrigin.Bottom))
		{
			_origin |= PlacementOrigin.Bottom;
			_location.Y = _magnetic ? 0 : distanceToBottom;
		}

		var distanceToTop = Math.Max(0, location.Y - area.Top);
		if (distanceToTop < _verticalSnappingDistance && _origin.HasFlag(PlacementOrigin.Bottom))
		{
			_origin &= ~PlacementOrigin.Bottom;
			_location.Y = _magnetic ? 0 : distanceToTop;
		}

		// Update direction
		var direction = distanceToRight < Math.Min(_rightDirectionDistance, area.Width / 2.0)
			? HorizontalDirectionLeftVisualState
			: HorizontalDirectionRightVisualState;
		VisualStateManager.GoToState(this, direction, true);

		PersistLocation();
		ApplyLocation();
	}

	private void ApplyLocation()
	{
		if (RenderTransform is not TranslateTransform transform)
		{
			RenderTransform = transform = new TranslateTransform();
		}

		var location = GetAbsoluteLocation();
		transform.X = location.X;
		transform.Y = location.Y;
	}

	private void RestoreLocation()
	{
		try
		{
			if (ApplicationData.Current.LocalSettings.Values.TryGetValue(_locationXSettingsKey, out var storedX)
				&& ApplicationData.Current.LocalSettings.Values.TryGetValue(_locationYSettingsKey, out var storedY)
				&& storedX is double x
				&& storedY is double y)
			{
				_location = new Point(x, y);
			}
			if (ApplicationData.Current.LocalSettings.Values.TryGetValue(_originSettingsKey, out var storedOrigin)
				&& storedOrigin is int origin)
			{
				_origin = (PlacementOrigin)origin;
			}
		}
		catch { }
	}

	private void PersistLocation()
	{
		ApplicationData.Current.LocalSettings.Values[_locationXSettingsKey] = _location.X;
		ApplicationData.Current.LocalSettings.Values[_locationYSettingsKey] = _location.Y;
		ApplicationData.Current.LocalSettings.Values[_originSettingsKey] = (int)_origin;
	}
}
#endif

﻿using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.UI.Xaml
{
	public partial class UIElement : DependencyObject
	{
		public UIElement()
		{
			_isFrameworkElement = this is FrameworkElement; // Avoids unused field error
			Initialize();
			InitializePointers();
		}

		public IntPtr Handle { get; }

		internal bool IsMeasureDirtyPath => throw new NotSupportedException("Reference assembly");

		internal bool IsArrangeDirtyPath => throw new NotSupportedException("Reference assembly");

		internal bool IsPointerCaptured { get; set; }

		internal bool ShouldInterceptInvalidate { get; set; }

		internal void AddChild(UIElement child, int? index = null) => throw new NotSupportedException("Reference assembly");

		internal void MoveChildTo(int oldIndex, int newIndex) => throw new NotSupportedException("Reference assembly");

		internal bool RemoveChild(UIElement child) => throw new NotSupportedException("Reference assembly");

		internal UIElement ReplaceChild(int index, UIElement child) => throw new NotSupportedException("Reference assembly");

		internal void ClearChildren() => throw new NotSupportedException("Reference assembly");

		internal void UpdateHitTest() => throw new NotSupportedException("Reference assembly");

		internal void SetHitTestVisibilityForRoot() => throw new NotSupportedException("Reference assembly");

		internal void ClearHitTestVisibilityForRoot() => throw new NotSupportedException("Reference assembly");
	}
}

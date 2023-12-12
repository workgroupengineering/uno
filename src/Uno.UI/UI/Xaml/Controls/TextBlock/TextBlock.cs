﻿#pragma warning disable CS0109

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Uno.Disposables;
using Uno.Extensions;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml;
using Uno.UI.DataBinding;
using System;
using Uno.UI;
using System.Collections;
using System.Diagnostics;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.Foundation;
using Windows.UI.Input;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Automation.Peers;
using Uno;
using Uno.Foundation.Logging;

using RadialGradientBrush = Microsoft.UI.Xaml.Media.RadialGradientBrush;
using Uno.UI.Helpers;

#if __IOS__
using UIKit;
#endif

namespace Microsoft.UI.Xaml.Controls
{
	[ContentProperty(Name = nameof(Inlines))]
	public partial class TextBlock : DependencyObject
	{
		private InlineCollection _inlines;
		private string _inlinesText; // Text derived from the content of Inlines

#if !__WASM__
		private Hyperlink _hyperlinkOver;
#endif

		private Action _foregroundChanged;

		private Run _reusableRun;
		private bool _skipInlinesChangedTextSetter;

#if !UNO_REFERENCE_API
		public TextBlock()
		{
			IFrameworkElementHelper.Initialize(this);
			SetDefaultForeground(ForegroundProperty);

			InitializeProperties();

			InitializePartial();
		}

		/// <summary>
		/// Calls On[Property]Changed for most DPs to ensure the values are correctly applied to the native control
		/// </summary>
		private void InitializeProperties()
		{
			OnForegroundChanged();
			OnFontFamilyChanged();
			OnFontWeightChanged();
			OnFontStyleChanged();
			OnFontSizeChanged();
			OnTextTrimmingChanged();
			OnTextWrappingChanged();
			OnMaxLinesChanged();
			OnTextAlignmentChanged();
			OnTextChanged(string.Empty, Text);
		}
#endif

		#region Inlines

		/// <summary>
		/// Gets an InlineCollection containing the top-level Inline elements that comprise the contents of the TextBlock.
		/// </summary>
		/// <remarks>
		/// Accessing this property initializes an InlineCollection, whose content will be synchronized with the Text.
		/// This can have a significant impact on performance. Only access this property if absolutely necessary.
		/// </remarks>
		public InlineCollection Inlines
		{
			get
			{
				if (_inlines == null)
				{
					_inlines = new InlineCollection(this);
					UpdateInlines(Text);
				}

				return _inlines;
			}
		}

		internal void InvalidateInlines(bool updateText)
		{
			if (updateText)
			{
				if (Inlines.Count == 1 && Inlines[0] is Run run)
				{
					_inlinesText = run.Text;
				}
				else
				{
					_inlinesText = string.Concat(Inlines.Select(InlineExtensions.GetText));
				}

				if (!_skipInlinesChangedTextSetter)
				{
					Text = _inlinesText;
				}

				UpdateHyperlinks();
			}

			OnInlinesChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnInlinesChangedPartial();

		#endregion

		#region FontStyle Dependency Property

		public FontStyle FontStyle
		{
			get => (FontStyle)GetValue(FontStyleProperty);
			set => SetValue(FontStyleProperty, value);
		}

		public static DependencyProperty FontStyleProperty { get; } =
			DependencyProperty.Register(
				"FontStyle",
				typeof(FontStyle),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: FontStyle.Normal,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnFontStyleChanged()
				)
			);

		private void OnFontStyleChanged()
		{
			OnFontStyleChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnFontStyleChangedPartial();

		#endregion

		#region TextWrapping Dependency Property

		public TextWrapping TextWrapping
		{
			get => (TextWrapping)GetValue(TextWrappingProperty);
			set => SetValue(TextWrappingProperty, value);
		}

		public static DependencyProperty TextWrappingProperty { get; } =
			DependencyProperty.Register(
				"TextWrapping",
				typeof(TextWrapping),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: TextWrapping.NoWrap,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnTextWrappingChanged()
				)
			);

		private void OnTextWrappingChanged()
		{
			OnTextWrappingChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnTextWrappingChangedPartial();

		#endregion

		#region FontWeight Dependency Property

		public FontWeight FontWeight
		{
			get => (FontWeight)GetValue(FontWeightProperty);
			set => SetValue(FontWeightProperty, value);
		}

		public static DependencyProperty FontWeightProperty { get; } =
			DependencyProperty.Register(
				"FontWeight",
				typeof(FontWeight),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: FontWeights.Normal,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnFontWeightChanged()
				)
			);

		private void OnFontWeightChanged()
		{
			OnFontWeightChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnFontWeightChangedPartial();

		#endregion

		#region Text Dependency Property

		public
#if __IOS__
			new
#endif
			string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static DependencyProperty TextProperty { get; } =
			DependencyProperty.Register(
				"Text",
				typeof(string),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: string.Empty,
					coerceValueCallback: CoerceText,
					propertyChangedCallback: (s, e) =>
						((TextBlock)s).OnTextChanged((string)e.OldValue, (string)e.NewValue)
				)
			);

		internal static object CoerceText(DependencyObject dependencyObject, object baseValue, DependencyPropertyValuePrecedences _) =>
			baseValue is string
				? baseValue
				: string.Empty;

		protected virtual void OnTextChanged(string oldValue, string newValue)
		{
			UpdateInlines(newValue);

			OnTextChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnTextChangedPartial();

		#endregion

		#region FontFamily Dependency Property

#if __IOS__
		/// <summary>
		/// Supported font families: http://iosfonts.com/
		/// </summary>
#endif
		public FontFamily FontFamily
		{
			get => (FontFamily)GetValue(FontFamilyProperty);
			set => SetValue(FontFamilyProperty, value);
		}

		public static DependencyProperty FontFamilyProperty { get; } =
			DependencyProperty.Register(
				"FontFamily",
				typeof(FontFamily),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: FontFamily.Default,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnFontFamilyChanged()
				)
			);

		private void OnFontFamilyChanged()
		{
			OnFontFamilyChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnFontFamilyChangedPartial();

		#endregion

		#region FontSize Dependency Property

		public double FontSize
		{
			get => (double)GetValue(FontSizeProperty);
			set => SetValue(FontSizeProperty, value);
		}

		public static DependencyProperty FontSizeProperty { get; } =
			DependencyProperty.Register(
				"FontSize",
				typeof(double),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: 14.0,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnFontSizeChanged()
				)
			);

		private void OnFontSizeChanged()
		{
			OnFontSizeChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnFontSizeChangedPartial();

		#endregion

		#region MaxLines Dependency Property

		public int MaxLines
		{
			get => (int)GetValue(MaxLinesProperty);
			set => SetValue(MaxLinesProperty, value);
		}

		public static DependencyProperty MaxLinesProperty { get; } =
			DependencyProperty.Register(
				"MaxLines",
				typeof(int),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: 0,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnMaxLinesChanged()
				)
			);

		private void OnMaxLinesChanged()
		{
			OnMaxLinesChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnMaxLinesChangedPartial();

		#endregion

		#region TextTrimming Dependency Property

		public TextTrimming TextTrimming
		{
			get => (TextTrimming)GetValue(TextTrimmingProperty);
			set => SetValue(TextTrimmingProperty, value);
		}

		public static DependencyProperty TextTrimmingProperty { get; } =
			DependencyProperty.Register(
				"TextTrimming",
				typeof(TextTrimming),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: TextTrimming.None,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnTextTrimmingChanged()
				)
			);

		private void OnTextTrimmingChanged()
		{
			OnTextTrimmingChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnTextTrimmingChangedPartial();

		#endregion

		#region Foreground Dependency Property

		public
#if __ANDROID__
		new
#endif
			Brush Foreground
		{
			get => (Brush)GetValue(ForegroundProperty);
			set
			{
#if !__WASM__
				if (value is SolidColorBrush || value is GradientBrush || value is RadialGradientBrush || value is null)
				{
					SetValue(ForegroundProperty, value);
				}
				else
				{
					throw new NotSupportedException("Only SolidColorBrush or GradientBrush's FallbackColor are supported.");
				}
#else
				SetValue(ForegroundProperty, value);
#endif
			}
		}

		public static DependencyProperty ForegroundProperty { get; } =
			DependencyProperty.Register(
				"Foreground",
				typeof(Brush),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: SolidColorBrushHelper.Black,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).Subscribe((Brush)e.OldValue, (Brush)e.NewValue)
				)
			);

		private void Subscribe(Brush oldValue, Brush newValue)
		{
			var newOnInvalidateRender = _foregroundChanged ?? (() => OnForegroundChanged());
			Brush.SetupBrushChanged(oldValue, newValue, ref _foregroundChanged, newOnInvalidateRender);
		}

		private void OnForegroundChanged()
		{
			// The try-catch here is primarily for the benefit of Android. This callback is raised when (say) the brush color changes,
			// which may happen when the system theme changes from light to dark. For app-level resources, a large number of views may
			// be subscribed to changes on the brush, including potentially some that have been removed from the visual tree, collected
			// on the native side, but not yet collected on the managed side (for Xamarin targets).

			// On Android, in practice this could result in ObjectDisposedExceptions when calling RequestLayout(). The try/catch is to
			// ensure that callbacks are correctly raised for remaining views referencing the brush which *are* still live in the visual tree.
			try
			{
				OnForegroundChangedPartial();
				InvalidateTextBlock();
			}
			catch (Exception e)
			{
				if (this.Log().IsEnabled(LogLevel.Debug))
				{
					this.Log().LogDebug($"Failed to invalidate for brush changed: {e}");
				}
			}
		}

		partial void OnForegroundChangedPartial();

		#endregion

		#region IsTextSelectionEnabled Dependency Property

#if !__WASM__
		[NotImplemented("__ANDROID__", "__IOS__", "IS_UNIT_TESTS", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
#endif
		public bool IsTextSelectionEnabled
		{
			get => (bool)GetValue(IsTextSelectionEnabledProperty);
			set => SetValue(IsTextSelectionEnabledProperty, value);
		}

#if !__WASM__
		[NotImplemented("__ANDROID__", "__IOS__", "IS_UNIT_TESTS", "__SKIA__", "__NETSTD_REFERENCE__", "__MACOS__")]
#endif
		public static DependencyProperty IsTextSelectionEnabledProperty { get; } =
			DependencyProperty.Register(
				nameof(IsTextSelectionEnabled),
				typeof(bool),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: false,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnIsTextSelectionEnabledChangedPartial()
				)
			);

		partial void OnIsTextSelectionEnabledChangedPartial();

		#endregion

		#region TextAlignment Dependency Property

		public new TextAlignment TextAlignment
		{
			get => (TextAlignment)GetValue(TextAlignmentProperty);
			set => SetValue(TextAlignmentProperty, value);
		}

		public static DependencyProperty TextAlignmentProperty { get; } =
			DependencyProperty.Register(
				"TextAlignment",
				typeof(TextAlignment),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: TextAlignment.Left,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnTextAlignmentChanged()
				)
			);

		private void OnTextAlignmentChanged()
		{
			HorizontalTextAlignment = TextAlignment;
			OnTextAlignmentChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnTextAlignmentChangedPartial();

		#endregion

		#region HorizontalTextAlignment Dependency Property

		public new TextAlignment HorizontalTextAlignment
		{
			get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
			set => SetValue(HorizontalTextAlignmentProperty, value);
		}

		public static DependencyProperty HorizontalTextAlignmentProperty { get; } =
			DependencyProperty.Register(
				"HorizontalTextAlignment",
				typeof(TextAlignment),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: TextAlignment.Left,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnHorizontalTextAlignmentChanged()
				)
			);

		// This property provides the same functionality as the TextAlignment property.
		// If both properties are set to conflicting values, the last one set is used.
		// https://docs.microsoft.com/en-us/uwp/api/windows.ui.xaml.controls.textbox.horizontaltextalignment#remarks
		private void OnHorizontalTextAlignmentChanged() => TextAlignment = HorizontalTextAlignment;

		#endregion

		#region LineHeight Dependency Property

		public double LineHeight
		{
			get => (double)GetValue(LineHeightProperty);
			set => SetValue(LineHeightProperty, value);
		}

		public static DependencyProperty LineHeightProperty { get; } =
			DependencyProperty.Register("LineHeight", typeof(double), typeof(TextBlock), new FrameworkPropertyMetadata(0d,
				propertyChangedCallback: (s, e) => ((TextBlock)s).OnLineHeightChanged())
			);

		private void OnLineHeightChanged()
		{
			OnLineHeightChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnLineHeightChangedPartial();

		#endregion

		#region LineStackingStrategy Dependency Property

		public LineStackingStrategy LineStackingStrategy
		{
			get => (LineStackingStrategy)GetValue(LineStackingStrategyProperty);
			set => SetValue(LineStackingStrategyProperty, value);
		}

		public static DependencyProperty LineStackingStrategyProperty { get; } =
			DependencyProperty.Register("LineStackingStrategy", typeof(LineStackingStrategy), typeof(TextBlock),
				new FrameworkPropertyMetadata(LineStackingStrategy.MaxHeight,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnLineStackingStrategyChanged())
			);

		private void OnLineStackingStrategyChanged()
		{
			OnLineStackingStrategyChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnLineStackingStrategyChangedPartial();

		#endregion

		#region Padding Dependency Property

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		public static DependencyProperty PaddingProperty { get; } =
			DependencyProperty.Register(
				"Padding",
				typeof(Thickness),
				typeof(TextBlock),
				new FrameworkPropertyMetadata((Thickness)Thickness.Empty,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnPaddingChanged())
			);

		private void OnPaddingChanged()
		{
			OnPaddingChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnPaddingChangedPartial();

		#endregion

		#region CharacterSpacing Dependency Property

		public int CharacterSpacing
		{
			get => (int)GetValue(CharacterSpacingProperty);
			set => SetValue(CharacterSpacingProperty, value);
		}

		public static DependencyProperty CharacterSpacingProperty { get; } =
			DependencyProperty.Register(
				"CharacterSpacing",
				typeof(int),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: 0,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnCharacterSpacingChanged()
				)
			);

		private void OnCharacterSpacingChanged()
		{
			OnCharacterSpacingChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnCharacterSpacingChangedPartial();

		#endregion

		#region TextDecorations

		public TextDecorations TextDecorations
		{
			get => (TextDecorations)GetValue(TextDecorationsProperty);
			set => SetValue(TextDecorationsProperty, value);
		}

		public static DependencyProperty TextDecorationsProperty { get; } =
			DependencyProperty.Register(
				nameof(TextDecorations),
				typeof(TextDecorations),
				typeof(TextBlock),
				new FrameworkPropertyMetadata(
					defaultValue: TextDecorations.None,
					options: FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((TextBlock)s).OnTextDecorationsChanged()
				)
			);

		private void OnTextDecorationsChanged()
		{
			OnTextDecorationsChangedPartial();
			InvalidateTextBlock();
		}

		partial void OnTextDecorationsChangedPartial();

		#endregion

		/// <summary>
		/// Gets whether the TextBlock is using the fast path in which Inlines
		/// have not been initialized and don't need to be synchronized.
		/// </summary>
		private bool UseInlinesFastPath => _inlines == null;

#if __ANDROID__ || __WASM__
		/// <summary>
		/// Returns if the TextBlock is constrained by a maximum number of lines.
		/// </summary>
		private bool IsLayoutConstrainedByMaxLines => MaxLines > 0;
#endif

#if __ANDROID__ || __IOS__ || __MACOS__
		/// <summary>
		/// Gets the inlines which affect the typography of the TextBlock.
		/// </summary>
		private IEnumerable<(Inline inline, int start, int end)> GetEffectiveInlines()
		{
			if (UseInlinesFastPath)
			{
				yield break;
			}

			var start = 0;
			foreach (var inline in Inlines.SelectMany(InlineExtensions.Enumerate))
			{
				if (inline.HasTypographicalEffectWithin(this))
				{
					yield return (inline, start, start + inline.GetText().Length);
				}

				if (inline is Run || inline is LineBreak)
				{
					start += inline.GetText().Length;
				}
			}
		}
#endif
		private void UpdateInlines(string text)
		{
			if (UseInlinesFastPath)
			{
				return;
			}

			if (ReadLocalValue(TextProperty) is UnsetValue)
			{
				_skipInlinesChangedTextSetter = true;
				Inlines.Clear();
				_skipInlinesChangedTextSetter = false;
				ClearTextPartial();
			}
			else if (text != _inlinesText)
			{
				// Inlines must be updated
				_skipInlinesChangedTextSetter = true;

				if (Inlines.Count == 1 && Inlines[0] is Run run)
				{
					run.Text = text;
				}
				else
				{
					if (Inlines.Count > 0)
					{
						Inlines.Clear();
						ClearTextPartial();
					}

					(_reusableRun ??= new Run()).Text = text;

					Inlines.Add(_reusableRun);
				}

				_skipInlinesChangedTextSetter = false;
			}
		}

		partial void ClearTextPartial();

		#region Hyperlinks

#if __WASM__
		// As on wasm the TextElements are UIElement, when the hosting TextBlock will capture the pointer on Pressed,
		// the original source of the Release event will be this TextBlock (and we won't receive 'pointerup' nor 'click'
		// events on the Hyperlink itself - On FF we will still get the 'click').
		// To workaround that, we subscribe to the events directly on the Hyperlink, and make the Capture on this hyperlink.

		private void UpdateHyperlinks() { } // Events are subscribed in Hyperlink's ctor.

		internal static readonly PointerEventHandler OnPointerPressed = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is Hyperlink hyperlink
				&& e.GetCurrentPoint(hyperlink).Properties.IsLeftButtonPressed
				&& hyperlink.CapturePointer(e.Pointer))
			{
				hyperlink.SetPointerPressed(e.Pointer);
				e.Handled = true;
				// hyperlink.CompleteGesture(); No needs to complete the gesture as the TextBlock won't even receive the Pressed.
			}
			else if (sender is TextBlock textBlock && textBlock.IsTextSelectionEnabled)
			{
				// Selectable TextBlock should also handle pointer pressed to ensure
				// RootVisual does not steal its focus.
				e.Handled = true;
			}
		};

		internal static readonly PointerEventHandler OnPointerReleased = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is Hyperlink hyperlink
				&& hyperlink.IsCaptured(e.Pointer))
			{
				// Un UWP we don't get the Tapped event, so make sure to abort it
				(hyperlink.GetParent() as TextBlock)?.CompleteGesture();

				hyperlink.ReleasePointerPressed(e.Pointer);
			}

			// e.Handled = true; ==> On UWP the pointer released is **NOT** handled
		};

		internal static readonly PointerEventHandler OnPointerCaptureLost = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is Hyperlink hyperlink)
			{
				var handled = hyperlink.AbortPointerPressed(e.Pointer);

				e.Handled = handled;
			}
		};
#else
		private static readonly PointerEventHandler OnPointerPressed = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is not TextBlock { HasHyperlink: true } that)
			{
				return;
			}

			var point = e.GetCurrentPoint(that);
			if (!point.Properties.IsLeftButtonPressed)
			{
				return;
			}

			var hyperlink = that.FindHyperlinkAt(point.Position);
			if (hyperlink is null)
			{
				return;
			}

			if (!that.CapturePointer(e.Pointer))
			{
				return;
			}

			hyperlink.SetPointerPressed(e.Pointer);
			e.Handled = true;
			that.CompleteGesture(); // Make sure to mute Tapped
		};

		private static readonly PointerEventHandler OnPointerReleased = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is TextBlock that
				&& that.IsCaptured(e.Pointer))
			{
				// On UWP we don't get the Tapped event, so make sure to abort it.
				that.CompleteGesture();

				// On UWP we don't get any CaptureLost, so make sure to manually release the capture silently
				that.ReleasePointerCapture(e.Pointer.UniqueId, muteEvent: true);

				// KNOWN ISSUE:
				// On UWP the 'click' event is raised **after** the PointerReleased ... but deferring the event on the Dispatcher
				// would move it after the PointerExited. So prefer to raise it before (actually like a Button).
				if (!(that.FindHyperlinkAt(e.GetCurrentPoint(that).Position)?.ReleasePointerPressed(e.Pointer) ?? false))
				{
					// We failed to find the hyperlink that made this capture but we ** silently ** removed the capture,
					// so we won't receive the CaptureLost. So make sure to AbortPointerPressed on the Hyperlink which made the capture.
					that.AbortHyperlinkCaptures(e.Pointer);
				}
			}

			// e.Handled = true; ==> On UWP the pointer released is **NOT** handled
		};

		private static readonly PointerEventHandler OnPointerCaptureLost = (object sender, PointerRoutedEventArgs e) =>
		{
			if (sender is TextBlock that)
			{
				e.Handled = that.AbortHyperlinkCaptures(e.Pointer);
			}
		};

		private static readonly PointerEventHandler OnPointerMoved = (sender, e) =>
		{
			if (sender is not TextBlock { HasHyperlink: true } that)
			{
				return;
			}

			var point = e.GetCurrentPoint(that);

			var hyperlink = that.FindHyperlinkAt(point.Position);
			if (that._hyperlinkOver != hyperlink)
			{
				that._hyperlinkOver?.ReleasePointerOver(e.Pointer);
				that._hyperlinkOver = hyperlink;
				hyperlink?.SetPointerOver(e.Pointer);
			}
		};

		private static readonly PointerEventHandler OnPointerEntered = (sender, e) =>
		{
			if (sender is not TextBlock { HasHyperlink: true } that)
			{
				return;
			}

			// This assertion fails because we don't release pointer captures on PointerExited in InputManager
			// TODO: make it such that this assertion doesn't fail
			// global::System.Diagnostics.Debug.Assert(that._hyperlinkOver == null);

			var point = e.GetCurrentPoint(that);

			var hyperlink = that.FindHyperlinkAt(point.Position);

			that._hyperlinkOver = hyperlink;
			hyperlink?.SetPointerOver(e.Pointer);
		};

		private static readonly PointerEventHandler OnPointerExit = (sender, e) =>
		{
			if (sender is not TextBlock { HasHyperlink: true } that)
			{
				return;
			}

			global::System.Diagnostics.Debug.Assert(that.FindHyperlinkAt(e.GetCurrentPoint(that).Position) == null);

			that._hyperlinkOver?.ReleasePointerOver(e.Pointer);
			that._hyperlinkOver = null;
		};

		private bool AbortHyperlinkCaptures(Pointer pointer)
		{
			var aborted = false;
			foreach (var hyperlink in _hyperlinks.ToList()) // .ToList() : for a strange reason on WASM the collection gets modified
			{
				aborted |= hyperlink.hyperlink.AbortPointerPressed(pointer);
				aborted |= hyperlink.hyperlink.ReleasePointerOver(pointer);
			}

			aborted |= _hyperlinkOver?.ReleasePointerOver(pointer) ?? false;
			_hyperlinkOver = null;

			return aborted;
		}

		private readonly List<(int start, int end, Hyperlink hyperlink)> _hyperlinks =
			new List<(int start, int end, Hyperlink hyperlink)>();

		private void UpdateHyperlinks()
		{
			global::System.Diagnostics.Debug.Assert(_hyperlinkOver is null || _hyperlinks.Where(h => h.hyperlink == _hyperlinkOver).Count() == 1);

			if (UseInlinesFastPath) // i.e. no Inlines
			{
				if (HasHyperlink)
				{
					RemoveHandler(PointerPressedEvent, OnPointerPressed);
					RemoveHandler(PointerReleasedEvent, OnPointerReleased);
					RemoveHandler(PointerMovedEvent, OnPointerMoved);
					RemoveHandler(PointerEnteredEvent, OnPointerEntered);
					RemoveHandler(PointerExitedEvent, OnPointerExit);
					RemoveHandler(PointerCaptureLostEvent, OnPointerCaptureLost);

					// Make sure to clear the pressed state of removed hyperlinks
					foreach (var hyperlink in _hyperlinks)
					{
						hyperlink.hyperlink.AbortAllPointerState();
					}

					_hyperlinkOver = null;
					_hyperlinks.Clear();
				}

				return;
			}

			var previousHasHyperlinks = HasHyperlink;
			var previousHyperLinks = _hyperlinks.Select(h => h.hyperlink).ToList();
			_hyperlinkOver = null;
			_hyperlinks.Clear();

			var start = 0;
			foreach (var inline in Inlines.PreorderTree)
			{
				switch (inline)
				{
					case Hyperlink hyperlink:
						previousHyperLinks.Remove(hyperlink);
						_hyperlinks.Add((start, start + hyperlink.GetText().Length, hyperlink));
						break;
					case Span span:
						break;
					default: // Leaf node
						start += inline.GetText().Length;
						break;
				}
			}

			// Make sure to clear the pressed state of removed hyperlinks
			foreach (var removed in previousHyperLinks)
			{
				removed.AbortAllPointerState();
			}

			// Update events subscriptions if needed
			// Note: we subscribe to those events only if needed as they increase marshaling on Android and WASM
			if (HasHyperlink && !previousHasHyperlinks)
			{
				InsertHandler(PointerPressedEvent, OnPointerPressed);
				InsertHandler(PointerReleasedEvent, OnPointerReleased);
				InsertHandler(PointerMovedEvent, OnPointerMoved);
				InsertHandler(PointerEnteredEvent, OnPointerEntered);
				InsertHandler(PointerExitedEvent, OnPointerExit);
				InsertHandler(PointerCaptureLostEvent, OnPointerCaptureLost);
			}
			else if (!HasHyperlink && previousHasHyperlinks)
			{
				RemoveHandler(PointerPressedEvent, OnPointerPressed);
				RemoveHandler(PointerReleasedEvent, OnPointerReleased);
				RemoveHandler(PointerMovedEvent, OnPointerMoved);
				RemoveHandler(PointerEnteredEvent, OnPointerEntered);
				RemoveHandler(PointerExitedEvent, OnPointerExit);
				RemoveHandler(PointerCaptureLostEvent, OnPointerCaptureLost);
			}
		}

		private bool HasHyperlink
		{
			get
			{
				var hasHyperlink = _hyperlinks.Count > 0;

				global::System.Diagnostics.Debug.Assert(!(!hasHyperlink && _hyperlinkOver is { }));

				return hasHyperlink;
			}
		}

#if !__SKIA__
		private Hyperlink FindHyperlinkAt(Point point)
		{
			var characterIndex = GetCharacterIndexAtPoint(point);
			var hyperlink = _hyperlinks
				.FirstOrDefault(h => h.start <= characterIndex && h.end > characterIndex)
				.hyperlink;

			return hyperlink;
		}
#endif
#endif

		#endregion

		private void InvalidateTextBlock()
		{
			InvalidateTextBlockPartial();
			InvalidateMeasure();
		}

		partial void InvalidateTextBlockPartial();

		protected override AutomationPeer OnCreateAutomationPeer() => new TextBlockAutomationPeer(this);

		public override string GetAccessibilityInnerText() => Text;

		// This approximates UWP behavior
		private protected override double GetActualWidth() => DesiredSize.Width;
		private protected override double GetActualHeight() => DesiredSize.Height;

		internal override void UpdateThemeBindings(Data.ResourceUpdateReason updateReason)
		{
			base.UpdateThemeBindings(updateReason);

			SetDefaultForeground(ForegroundProperty);

			if (_inlines is not null)
			{
				foreach (var inline in _inlines)
				{
					((IDependencyObjectStoreProvider)inline).Store.UpdateResourceBindings(updateReason);
				}
			}
		}

		internal override bool CanHaveChildren() => true;

		public new bool Focus(FocusState value) => base.Focus(value);

		internal override bool IsFocusable =>
			/*IsActive() &&*/ //TODO Uno: No concept of IsActive in Uno yet.
			IsVisible() &&
			/*IsEnabled() &&*/ (IsTextSelectionEnabled || IsTabStop) &&
			AreAllAncestorsVisible();
	}
}

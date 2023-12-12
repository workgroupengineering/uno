﻿using System;
using System.Collections.Generic;
using System.Text;
using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno.UI.DataBinding;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections;
using System.Linq;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Windows.Foundation;
using Uno.UI;
using Microsoft.UI.Xaml.Media;
using Windows.UI.Text;
using Uno.UI.Xaml;

#if __ANDROID__
using View = Android.Views.View;
using ViewGroup = Android.Views.ViewGroup;
using Font = Android.Graphics.Typeface;
using Android.Graphics;
#elif __IOS__
using UIKit;
using View = UIKit.UIView;
using ViewGroup = UIKit.UIView;
using Color = UIKit.UIColor;
using Font = UIKit.UIFont;
#elif __MACOS__
using AppKit;
using View = AppKit.NSView;
using ViewGroup = AppKit.NSView;
using Color = AppKit.NSColor;
using Font = AppKit.NSFont;
#elif UNO_REFERENCE_API || IS_UNIT_TESTS
using View = Microsoft.UI.Xaml.UIElement;
using ViewGroup = Microsoft.UI.Xaml.UIElement;
#endif

namespace Microsoft.UI.Xaml.Controls
{
	[ContentProperty(Name = "Content")]
	public partial class ContentPresenter : FrameworkElement, IFrameworkTemplatePoolAware
#if !__CROSSRUNTIME__ && !IS_UNIT_TESTS
		, ICustomClippingElement
#endif
	{
		private bool _firstLoadResetDone;
		private View _contentTemplateRoot;

		/// <summary>
		/// Will be set to either the result of ContentTemplateSelector or to ContentTemplate, depending on which is used
		/// </summary>
		private DataTemplate _dataTemplateUsedLastUpdate;

		private void InitializeContentPresenter()
		{
			SetDefaultForeground(ForegroundProperty);
		}

		/// <summary>
		/// Indicates if the content should inherit templated parent from the presenter, or its templated parent.
		/// </summary>
		/// <remarks>Clear this flag to let the control nested directly under this ContentPresenter to inherit the correct templated parent</remarks>
		internal bool SynchronizeContentWithOuterTemplatedParent { get; set; } = true;

		/// <summary>
		/// Flag indicating whether the content presenter uses implicit text block to render its content.
		/// </summary>
		internal bool IsUsingDefaultTemplate { get; private set; }

		/// <summary>
		/// Determines if the current ContentPresenter is hosting a native control.
		/// </summary>
		/// <remarks>This is used to alter the propagation of the templated parent.</remarks>
		internal bool IsNativeHost { get; set; }

		protected override bool IsSimpleLayout => true;

		#region Content DependencyProperty

		public object Content
		{
			get { return (object)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public static DependencyProperty ContentProperty { get; } =
			DependencyProperty.Register(
				"Content",
				typeof(object),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					defaultValue: null,
					options: FrameworkPropertyMetadataOptions.None,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s)?.OnContentChanged(e.OldValue, e.NewValue)
				)
			);

		#endregion

		#region ContentTemplate DependencyProperty

		public DataTemplate ContentTemplate
		{
			get { return (DataTemplate)GetValue(ContentTemplateProperty); }
			set { SetValue(ContentTemplateProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ContentTemplate.  This enables animation, styling, binding, etc...
		public static DependencyProperty ContentTemplateProperty { get; } =
			DependencyProperty.Register(
				"ContentTemplate",
				typeof(DataTemplate),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					null,
					FrameworkPropertyMetadataOptions.ValueDoesNotInheritDataContext,
					(s, e) => ((ContentPresenter)s)?.OnContentTemplateChanged(e.OldValue as DataTemplate, e.NewValue as DataTemplate)
				)
			);
		#endregion

		#region ContentTemplateSelector DependencyProperty

		public DataTemplateSelector ContentTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(ContentTemplateSelectorProperty); }
			set { SetValue(ContentTemplateSelectorProperty, value); }
		}

		public static DependencyProperty ContentTemplateSelectorProperty { get; } =
			DependencyProperty.Register(
				"ContentTemplateSelector",
				typeof(DataTemplateSelector),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					null,
					(s, e) => ((ContentPresenter)s)?.OnContentTemplateSelectorChanged(e.OldValue as DataTemplateSelector, e.NewValue as DataTemplateSelector)
				)
			);
		#endregion

		#region Transitions Dependency Property

		public TransitionCollection ContentTransitions
		{
			get { return (TransitionCollection)this.GetValue(ContentTransitionsProperty); }
			set { this.SetValue(ContentTransitionsProperty, value); }
		}

		public static DependencyProperty ContentTransitionsProperty { get; } =
			DependencyProperty.Register("ContentTransitions", typeof(TransitionCollection), typeof(ContentPresenter), new FrameworkPropertyMetadata(null, OnContentTransitionsChanged));

		private static void OnContentTransitionsChanged(object dependencyObject, DependencyPropertyChangedEventArgs args)
		{
			var control = dependencyObject as ContentPresenter;

			if (control != null)
			{
				var oldValue = (TransitionCollection)args.OldValue;
				var newValue = (TransitionCollection)args.NewValue;

				control.UpdateContentTransitions(oldValue, newValue);
			}
		}

		#endregion

		#region BackgroundSizing DepedencyProperty
		[GeneratedDependencyProperty(DefaultValue = default(BackgroundSizing), ChangedCallback = true)]
		public static DependencyProperty BackgroundSizingProperty { get; } = CreateBackgroundSizingProperty();

		public BackgroundSizing BackgroundSizing
		{
			get => GetBackgroundSizingValue();
			set => SetBackgroundSizingValue(value);
		}
		private void OnBackgroundSizingChanged(DependencyPropertyChangedEventArgs e)
		{
			OnBackgroundSizingChangedPartial(e);
			base.OnBackgroundSizingChangedInner(e);
		}

		partial void OnBackgroundSizingChangedPartial(DependencyPropertyChangedEventArgs e);
		#endregion

		#region Foreground Dependency Property

		public
#if __ANDROID__
		new
#endif
		Brush Foreground
		{
			get { return (Brush)this.GetValue(ForegroundProperty); }
			set { this.SetValue(ForegroundProperty, value); }
		}

		public static DependencyProperty ForegroundProperty { get; } =
			DependencyProperty.Register(
				"Foreground",
				typeof(Brush),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					SolidColorBrushHelper.Black,
					FrameworkPropertyMetadataOptions.Inherits,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s)?.OnForegroundColorChanged(e.OldValue as Brush, e.NewValue as Brush)
				)
			);

		#endregion

		#region FontWeight

		public FontWeight FontWeight
		{
			get { return (FontWeight)this.GetValue(FontWeightProperty); }
			set { this.SetValue(FontWeightProperty, value); }
		}

		public static DependencyProperty FontWeightProperty { get; } =
			DependencyProperty.Register(
				"FontWeight",
				typeof(FontWeight),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					FontWeights.Normal,
					FrameworkPropertyMetadataOptions.Inherits,
					(s, e) => ((ContentPresenter)s)?.OnFontWeightChanged((FontWeight)e.OldValue, (FontWeight)e.NewValue)
				)
			);

		#endregion

		#region FontSize

		public double FontSize
		{
			get { return (double)this.GetValue(FontSizeProperty); }
			set { this.SetValue(FontSizeProperty, value); }
		}

		public static DependencyProperty FontSizeProperty { get; } =
			DependencyProperty.Register(
				"FontSize",
				typeof(double),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					14.0,
					FrameworkPropertyMetadataOptions.Inherits,
					(s, e) => ((ContentPresenter)s)?.OnFontSizeChanged((double)e.OldValue, (double)e.NewValue)
				)
			);

		#endregion

		#region FontFamily

		public FontFamily FontFamily
		{
			get { return (FontFamily)this.GetValue(FontFamilyProperty); }
			set { this.SetValue(FontFamilyProperty, value); }
		}

		public static DependencyProperty FontFamilyProperty { get; } =
			DependencyProperty.Register(
				"FontFamily",
				typeof(FontFamily),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					FontFamily.Default,
					FrameworkPropertyMetadataOptions.Inherits,
					(s, e) => ((ContentPresenter)s)?.OnFontFamilyChanged(e.OldValue as FontFamily, e.NewValue as FontFamily)
				)
			);
		#endregion

		#region FontStyle

		public FontStyle FontStyle
		{
			get { return (FontStyle)this.GetValue(FontStyleProperty); }
			set { this.SetValue(FontStyleProperty, value); }
		}

		public static DependencyProperty FontStyleProperty { get; } =
			DependencyProperty.Register(
				"FontStyle",
				typeof(FontStyle),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					FontStyle.Normal,
					FrameworkPropertyMetadataOptions.Inherits,
					(s, e) => ((ContentPresenter)s)?.OnFontStyleChanged((FontStyle)e.OldValue, (FontStyle)e.NewValue)
				)
			);
		#endregion

		#region TextWrapping Dependency Property

		public TextWrapping TextWrapping
		{
			get { return (TextWrapping)this.GetValue(TextWrappingProperty); }
			set { this.SetValue(TextWrappingProperty, value); }
		}

		public static DependencyProperty TextWrappingProperty { get; } =
			DependencyProperty.Register(
				"TextWrapping",
				typeof(TextWrapping),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					defaultValue: TextWrapping.NoWrap,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s).OnTextWrappingChanged()
				)
			);

		private void OnTextWrappingChanged()
		{
			OnTextWrappingChangedPartial();
		}

		partial void OnTextWrappingChangedPartial();

		#endregion

		#region MaxLines Dependency Property

		public int MaxLines
		{
			get { return (int)this.GetValue(MaxLinesProperty); }
			set { this.SetValue(MaxLinesProperty, value); }
		}

		public static DependencyProperty MaxLinesProperty { get; } =
			DependencyProperty.Register(
				"MaxLines",
				typeof(int),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					defaultValue: 0,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s).OnMaxLinesChanged()
				)
			);

		private void OnMaxLinesChanged()
		{
			OnMaxLinesChangedPartial();
		}

		partial void OnMaxLinesChangedPartial();

		#endregion

		#region TextTrimming Dependency Property

		public TextTrimming TextTrimming
		{
			get { return (TextTrimming)this.GetValue(TextTrimmingProperty); }
			set { this.SetValue(TextTrimmingProperty, value); }
		}

		public static DependencyProperty TextTrimmingProperty { get; } =
			DependencyProperty.Register(
				"TextTrimming",
				typeof(TextTrimming),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					defaultValue: TextTrimming.None,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s).OnTextTrimmingChanged()
				)
			);

		private void OnTextTrimmingChanged()
		{
			OnTextTrimmingChangedPartial();
		}

		partial void OnTextTrimmingChangedPartial();

		#endregion

		#region TextAlignment Dependency Property

		public
#if __ANDROID__
			new
#endif
			TextAlignment TextAlignment
		{
			get { return (TextAlignment)this.GetValue(TextAlignmentProperty); }
			set { this.SetValue(TextAlignmentProperty, value); }
		}

		public static DependencyProperty TextAlignmentProperty { get; } =
			DependencyProperty.Register(
				"TextAlignment",
				typeof(TextAlignment),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					defaultValue: TextAlignment.Left,
					propertyChangedCallback: (s, e) => ((ContentPresenter)s).OnTextAlignmentChanged()
				)
			);

		private void OnTextAlignmentChanged()
		{
			OnTextAlignmentChangedPartial();
		}

		partial void OnTextAlignmentChangedPartial();

		#endregion

		#region HorizontalContentAlignment DependencyProperty

		public HorizontalAlignment HorizontalContentAlignment
		{
			get => (HorizontalAlignment)this.GetValue(HorizontalContentAlignmentProperty);
			set => this.SetValue(HorizontalContentAlignmentProperty, value);
		}

		public static DependencyProperty HorizontalContentAlignmentProperty { get; } =
			DependencyProperty.Register(
				nameof(HorizontalContentAlignment),
				typeof(HorizontalAlignment),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					HorizontalAlignment.Stretch,
					FrameworkPropertyMetadataOptions.AffectsArrange,
					(s, e) => ((ContentPresenter)s)?.OnHorizontalContentAlignmentChanged((HorizontalAlignment)e.OldValue, (HorizontalAlignment)e.NewValue)
				)
			);

		protected virtual void OnHorizontalContentAlignmentChanged(HorizontalAlignment oldHorizontalContentAlignment, HorizontalAlignment newHorizontalContentAlignment)
		{
			OnHorizontalContentAlignmentChangedPartial(oldHorizontalContentAlignment, newHorizontalContentAlignment);
		}

		partial void OnHorizontalContentAlignmentChangedPartial(HorizontalAlignment oldHorizontalContentAlignment, HorizontalAlignment newHorizontalContentAlignment);

		#endregion

		#region VerticalContentAlignment DependencyProperty

		public VerticalAlignment VerticalContentAlignment
		{
			get => (VerticalAlignment)this.GetValue(VerticalContentAlignmentProperty);
			set => this.SetValue(VerticalContentAlignmentProperty, value);
		}

		public static DependencyProperty VerticalContentAlignmentProperty { get; } =
			DependencyProperty.Register(
				nameof(VerticalContentAlignment),
				typeof(VerticalAlignment),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					VerticalAlignment.Stretch,
					FrameworkPropertyMetadataOptions.AffectsArrange,
					(s, e) => ((ContentPresenter)s)?.OnVerticalContentAlignmentChanged((VerticalAlignment)e.OldValue, (VerticalAlignment)e.NewValue)
				)
			);

		protected virtual void OnVerticalContentAlignmentChanged(VerticalAlignment oldVerticalContentAlignment, VerticalAlignment newVerticalContentAlignment)
		{
			OnVerticalContentAlignmentChangedPartial(oldVerticalContentAlignment, newVerticalContentAlignment);
		}

		partial void OnVerticalContentAlignmentChangedPartial(VerticalAlignment oldVerticalContentAlignment, VerticalAlignment newVerticalContentAlignment);

		#endregion

		#region Padding DependencyProperty

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingProperty); }
			set { SetValue(PaddingProperty, value); }
		}

		public static DependencyProperty PaddingProperty { get; } =
			DependencyProperty.Register(
				"Padding",
				typeof(Thickness),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					(Thickness)Thickness.Empty,
					FrameworkPropertyMetadataOptions.AffectsMeasure,
					(s, e) => ((ContentPresenter)s)?.OnPaddingChanged((Thickness)e.OldValue, (Thickness)e.NewValue)
				)
			);

		private void OnPaddingChanged(Thickness oldValue, Thickness newValue)
		{
			OnPaddingChangedPartial(oldValue, newValue);
		}

		partial void OnPaddingChangedPartial(Thickness oldValue, Thickness newValue);

		#endregion

		#region BorderThickness DependencyProperty

		public Thickness BorderThickness
		{
			get { return (Thickness)GetValue(BorderThicknessProperty); }
			set { SetValue(BorderThicknessProperty, value); }
		}

		public static DependencyProperty BorderThicknessProperty { get; } =
			DependencyProperty.Register(
				"BorderThickness",
				typeof(Thickness),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					(Thickness)Thickness.Empty,
					FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
					(s, e) => ((ContentPresenter)s)?.OnBorderThicknessChanged((Thickness)e.OldValue, (Thickness)e.NewValue)
				)
			);

		private void OnBorderThicknessChanged(Thickness oldValue, Thickness newValue)
		{
			UpdateBorder();
		}

		#endregion

		#region BorderBrush DependencyProperty

		public Brush BorderBrush
		{
			get { return (Brush)GetValue(BorderBrushProperty); }
			set { SetValue(BorderBrushProperty, value); }
		}

		public static DependencyProperty BorderBrushProperty { get; } =
			DependencyProperty.Register(
				"BorderBrush",
				typeof(Brush),
				typeof(ContentPresenter),
				new FrameworkPropertyMetadata(
					null,
					(s, e) => ((ContentPresenter)s)?.OnBorderBrushChanged((Brush)e.OldValue, (Brush)e.NewValue)
				)
			);

		private void OnBorderBrushChanged(Brush oldValue, Brush newValue)
		{
#if __WASM__
			if (((oldValue is null) ^ (newValue is null)) && BorderThickness != default)
			{
				// The transition from null to non-null (and vice-versa) affects child arrange on Wasm when non-zero BorderThickness is specified.
				(Content as UIElement)?.InvalidateArrange();
			}
#endif
			UpdateBorder();
		}


		#endregion

		#region CornerRadius DependencyProperty
		private static CornerRadius GetCornerRadiusDefaultValue() => CornerRadius.None;

		[GeneratedDependencyProperty(ChangedCallback = true)]
		public static DependencyProperty CornerRadiusProperty { get; } = CreateCornerRadiusProperty();

		public CornerRadius CornerRadius
		{
			get => GetCornerRadiusValue();
			set => SetCornerRadiusValue(value);
		}

		private void OnCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
		{
			UpdateCornerRadius(newValue);
		}

		#endregion

		protected virtual void OnForegroundColorChanged(Brush oldValue, Brush newValue)
		{
			OnForegroundColorChangedPartial(oldValue, newValue);
		}

		partial void OnForegroundColorChangedPartial(Brush oldValue, Brush newValue);

		protected virtual void OnFontWeightChanged(FontWeight oldValue, FontWeight newValue)
		{
			OnFontWeightChangedPartial(oldValue, newValue);
		}

		partial void OnFontWeightChangedPartial(FontWeight oldValue, FontWeight newValue);

		protected virtual void OnFontFamilyChanged(FontFamily oldValue, FontFamily newValue)
		{
			OnFontFamilyChangedPartial(oldValue, newValue);
		}

		partial void OnFontFamilyChangedPartial(FontFamily oldValue, FontFamily newValue);

		protected virtual void OnFontSizeChanged(double oldValue, double newValue)
		{
			OnFontSizeChangedPartial(oldValue, newValue);
		}

		partial void OnFontSizeChangedPartial(double oldValue, double newValue);

		protected virtual void OnFontStyleChanged(FontStyle oldValue, FontStyle newValue)
		{
			OnFontStyleChangedPartial(oldValue, newValue);
		}

		partial void OnFontStyleChangedPartial(FontStyle oldValue, FontStyle newValue);


		protected virtual void OnContentChanged(object oldValue, object newValue)
		{
			if (oldValue != null && newValue == null)
			{
				// The content is being reset, remove the existing content properly.
				ContentTemplateRoot = null;
			}
			else if (oldValue is View || newValue is View)
			{
				// Make sure not to reuse the previous Content as a ContentTemplateRoot (i.e., in case there's no data template)
				// If setting Content to a new View, recreate the template
				ContentTemplateRoot = null;
			}

			if (newValue is not null)
			{
				TryRegisterNativeElement(newValue);

				TrySetDataContextFromContent(newValue);

				SetUpdateTemplate();
			}
			else
			{
				// Restore the inherited data context as it may have been overridden by TrySetDataContextFromContent
				this.ClearValue(DataContextProperty, DependencyPropertyValuePrecedences.Local);
			}
		}

		private void TrySetDataContextFromContent(object value)
		{
			if (value != null)
			{
				if (!(value is View))
				{
					// If the content is not a view, we apply the content as the
					// DataContext of the materialized content.
					DataContext = value;
				}
				else
				{
					// Restore DataContext propagation if the content is a view
					this.ClearValue(DataContextProperty, DependencyPropertyValuePrecedences.Local);
				}
			}
		}

		protected internal override void OnTemplatedParentChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnTemplatedParentChanged(e);

			SetImplicitContent();
		}

		protected virtual void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
		{
			if (ContentTemplateRoot != null)
			{
				ContentTemplateRoot = null;
			}

			SetUpdateTemplate();
		}

		protected virtual void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
		{
		}

		partial void UnregisterContentTemplateRoot();

		public View ContentTemplateRoot
		{
			get
			{
				return _contentTemplateRoot;
			}

			protected set
			{
				var previousValue = _contentTemplateRoot;

				if (previousValue != null)
				{
					CleanupView(previousValue);

					UnregisterContentTemplateRoot();

					UpdateContentTransitions(this.ContentTransitions, null);
				}

				_contentTemplateRoot = value;

				SynchronizeContentTemplatedParent();

				if (_contentTemplateRoot != null)
				{
					RegisterContentTemplateRoot();

					UpdateContentTransitions(null, this.ContentTransitions);
				}
			}
		}

		private void SynchronizeContentTemplatedParent()
		{
			if (IsNativeHost)
			{
				// In this case, the ContentPresenter is not used as part of the child of a
				// templated control, and we must not take the outer templated parent, but rather
				// the immediate template parent (as if the native view was not wrapped).
				// Needs to be reevaluated with https://github.com/unoplatform/uno/issues/1621
				if (_contentTemplateRoot is IFrameworkElement binder)
				{
					binder.TemplatedParent = this.TemplatedParent;
				}
			}
			else
			{
				if (_contentTemplateRoot is IFrameworkElement binder)
				{
					binder.TemplatedParent = FindTemplatedParent();

					DependencyObject FindTemplatedParent()
					{
						// ImplicitTextBlock is a special case that requires its TemplatedParent to be the ContentPresenter
						if (_contentTemplateRoot is ImplicitTextBlock) return this;

						// Sometimes when content is a child view defined in the xaml, the direct TemplatedParent should be used,
						// but only if the content hasnt been overwritten yet. If the content has been overwritten,
						// either ImplicitTextBlock or the DataTemplate (requiring the outter TemplatedParent) would has been used.
						if (!SynchronizeContentWithOuterTemplatedParent && _dataTemplateUsedLastUpdate == null)
						{
							return this.TemplatedParent;
						}

						return (this.TemplatedParent as IFrameworkElement)?.TemplatedParent;
					}
				}
				else if (_contentTemplateRoot is DependencyObject dependencyObject)
				{
					// Propagate binding context correctly
					dependencyObject.SetParent(this);
				}
			}
		}

		private void UpdateContentTransitions(TransitionCollection oldValue, TransitionCollection newValue)
		{
			var contentRoot = this.ContentTemplateRoot as IFrameworkElement;

			if (contentRoot == null)
			{
				return;
			}

			if (oldValue != null)
			{
				foreach (var item in oldValue)
				{
					item.DetachFromElement(contentRoot);
				}
			}

			if (newValue != null)
			{
				foreach (var item in newValue)
				{
					item.AttachToElement(contentRoot);
				}
			}
		}

		/// <summary>
		/// Cleanup the view from its binding references
		/// </summary>
		/// <param name="previousValue"></param>
		private void CleanupView(View previousValue)
		{
			if (!(previousValue is IFrameworkElement) && previousValue is DependencyObject dependencyObject)
			{
				dependencyObject.SetParent(null);
			}
		}

		private protected override void OnLoaded()
		{
			base.OnLoaded();

			if (ResetDataContextOnFirstLoad() || ContentTemplateRoot == null)
			{
				SetUpdateTemplate();
			}

			// When the control is loaded, set the TemplatedParent
			// as it may have been reset during the last unload.
			SynchronizeContentTemplatedParent();

			UpdateBorder();

			TryAttachNativeElement();
		}

		private protected override void OnUnloaded()
		{
			base.OnUnloaded();

			ClearBorder();

			TryDetachNativeElement();
		}

		private bool ResetDataContextOnFirstLoad()
		{
			if (!_firstLoadResetDone)
			{
				_firstLoadResetDone = true;

				// This test avoids the ContentPresenter from resetting
				// the DataContext to null (or the inherited value) and then back to
				// the content and have two-way bindings propagating the null value
				// back to the source.
				if (!ReferenceEquals(DataContext, Content))
				{
					// On first load UWP clears the local value of a ContentPresenter.
					// The reason for this behavior is unknown.
					this.ClearValue(DataContextProperty, DependencyPropertyValuePrecedences.Local);

					TrySetDataContextFromContent(Content);
				}

				return true;
			}

			return false;
		}

		void IFrameworkTemplatePoolAware.OnTemplateRecycled()
		{
			// This needs to be cleared on recycle, to prevent
			// SetUpdateTemplate from being skipped in OnLoaded.
			_firstLoadResetDone = false;
		}

		protected override void OnVisibilityChanged(Visibility oldValue, Visibility newValue)
		{
			base.OnVisibilityChanged(oldValue, newValue);

			if (oldValue == Visibility.Collapsed && newValue == Visibility.Visible)
			{
				SetUpdateTemplate();
			}
		}

		public void UpdateContentTemplateRoot()
		{
			if (Visibility == Visibility.Collapsed)
			{
				return;
			}

			//If ContentTemplateRoot is null, it must be updated even if the templates haven't changed
			if (ContentTemplateRoot == null)
			{
				_dataTemplateUsedLastUpdate = null;
			}

			//ContentTemplate/ContentTemplateSelector will only be applied to a control with no Template, normally the innermost element
			var dataTemplate = this.ResolveContentTemplate();

			//Only apply template if it has changed
			if (!object.Equals(dataTemplate, _dataTemplateUsedLastUpdate))
			{
				_dataTemplateUsedLastUpdate = dataTemplate;
				ContentTemplateRoot = dataTemplate?.LoadContentCached() ?? Content as View;
				if (ContentTemplateRoot != null)
				{
					IsUsingDefaultTemplate = false;
				}
			}

			if (Content != null
				&& !(Content is View)
				&& ContentTemplateRoot == null
			)
			{
				// Use basic default root for non-View Content if no template is supplied
				SetContentTemplateRootToPlaceholder();
			}

			if (ContentTemplateRoot == null && Content is View contentView && dataTemplate == null)
			{
				// No template and Content is a View, set it directly as root
				ContentTemplateRoot = contentView as View;
			}

			IsUsingDefaultTemplate = ContentTemplateRoot is ImplicitTextBlock;
		}

		private void SetContentTemplateRootToPlaceholder()
		{
			if (this.Log().IsEnabled(Uno.Foundation.Logging.LogLevel.Debug))
			{
				this.Log().DebugFormat("No ContentTemplate was specified for {0} and content is not a UIView, defaulting to TextBlock.", GetType().Name);
			}

			var textBlock = new ImplicitTextBlock(this);

			void setBinding(DependencyProperty property, string path)
				=> textBlock.SetBinding(
					property,
					new Binding
					{
						Path = new PropertyPath(path),
						Source = this,
						Mode = BindingMode.OneWay
					}
				);

			if (!IsNativeHost)
			{
				setBinding(TextBlock.TextProperty, nameof(Content));
				setBinding(TextBlock.HorizontalAlignmentProperty, nameof(HorizontalContentAlignment));
				setBinding(TextBlock.VerticalAlignmentProperty, nameof(VerticalContentAlignment));
				setBinding(TextBlock.TextWrappingProperty, nameof(TextWrapping));
				setBinding(TextBlock.MaxLinesProperty, nameof(MaxLines));
				setBinding(TextBlock.TextAlignmentProperty, nameof(TextAlignment));
			}

			ContentTemplateRoot = textBlock;
			IsUsingDefaultTemplate = true;
		}

		private bool _isBoundImplicitelyToContent;

		private void SetImplicitContent()
		{
			if (!FeatureConfiguration.ContentPresenter.UseImplicitContentFromTemplatedParent)
			{
				return;
			}

			if (!(TemplatedParent is ContentControl))
			{
				ClearImplicitBindinds();
				return; // Not applicable: no TemplatedParent or it's not a ContentControl
			}

			// Check if the Content is set to something
			var v = this.GetValueUnderPrecedence(ContentProperty, DependencyPropertyValuePrecedences.DefaultValue);
			if (v.precedence != DependencyPropertyValuePrecedences.DefaultValue)
			{
				ClearImplicitBindinds();
				return; // Nope, there's a value somewhere
			}

			// Check if the Content property is bound to something
			var b = GetBindingExpression(ContentProperty);
			if (b != null)
			{
				ClearImplicitBindinds();
				return; // Yep, there's a binding: a value "will" come eventually
			}

			// Create an implicit binding of Content to Content property of the TemplatedParent (which is a ContentControl)
			var binding =
				new Binding(new PropertyPath("Content"), null)
				{
					RelativeSource = RelativeSource.TemplatedParent,
				};
			SetBinding(ContentProperty, binding);
			_isBoundImplicitelyToContent = true;

			void ClearImplicitBindinds()
			{
				if (_isBoundImplicitelyToContent)
				{
					SetBinding(ContentProperty, new Binding());
				}
			}
		}

		partial void RegisterContentTemplateRoot();

		protected override void OnBackgroundChanged(DependencyPropertyChangedEventArgs e)
		{
			// Don't call base, the UpdateBorder() method handles drawing the background.
			// base.OnBackgroundChanged(e);

			UpdateBorder();
#if __WASM__
			SetAndObserveBackgroundBrush(e.OldValue as Brush, e.NewValue as Brush);
#endif
		}

		internal override void UpdateThemeBindings(ResourceUpdateReason updateReason)
		{
			base.UpdateThemeBindings(updateReason);
			SetDefaultForeground(ForegroundProperty);
		}

#if __ANDROID__
		// Support for the C# collection initializer style.
		public void Add(View view)
		{
			Content = view;
		}

		public IEnumerator GetEnumerator()
		{
			if (Content != null)
			{
				return new[] { Content }.GetEnumerator();
			}
			else
			{
				return Enumerable.Empty<object>().GetEnumerator();
			}
		}
#endif

		protected override Size ArrangeOverride(Size finalSize)
		{
			var child = this.FindFirstChild();

			if (child != null)
			{
				var padding = Padding;
				var borderThickness = BorderThickness;

				var innerRect = new Windows.Foundation.Rect(
					padding.Left + borderThickness.Left,
					padding.Top + borderThickness.Top,
					finalSize.Width - padding.Left - padding.Right - borderThickness.Left - borderThickness.Right,
					finalSize.Height - padding.Top - padding.Bottom - borderThickness.Top - borderThickness.Bottom
				);

				var availableSize = new Size(innerRect.Width, innerRect.Height);

				// Using GetElementDesiredSize to properly handle native controls.
				var desiredSize = GetElementDesiredSize(child);

				var contentWidth = HorizontalContentAlignment == HorizontalAlignment.Stretch ?
					availableSize.Width : desiredSize.Width;
				var contentHeight = VerticalContentAlignment == VerticalAlignment.Stretch ?
					availableSize.Height : desiredSize.Height;
				var contentSize = new Size(contentWidth, contentHeight);

				var offset = CalculateContentOffset(availableSize, contentSize);

				var arrangeRect = new Windows.Foundation.Rect(
					innerRect.X + offset.X,
					innerRect.Y + offset.Y,
					contentSize.Width,
					contentSize.Height);

				ArrangeElement(child, arrangeRect);

				ArrangeNativeElement(arrangeRect);
			}

			return finalSize;
		}

		private Windows.Foundation.Point CalculateContentOffset(Size availableSize, Size contentSize)
		{
			var horizontalAlignment = HorizontalContentAlignment;
			var verticalAlignment = VerticalContentAlignment;

			if (horizontalAlignment == HorizontalAlignment.Stretch &&
				contentSize.Width > availableSize.Width)
			{
				horizontalAlignment = HorizontalAlignment.Left;
			}

			if (verticalAlignment == VerticalAlignment.Stretch &&
				contentSize.Height > availableSize.Height)
			{
				verticalAlignment = VerticalAlignment.Top;
			}

			double offsetX;
			if (horizontalAlignment == HorizontalAlignment.Center ||
				horizontalAlignment == HorizontalAlignment.Stretch)
			{
				offsetX = (availableSize.Width - contentSize.Width) / 2;
			}
			else if (horizontalAlignment == HorizontalAlignment.Right)
			{
				offsetX = availableSize.Width - contentSize.Width;
			}
			else
			{
				offsetX = 0;
			}

			double offsetY;
			if (verticalAlignment == VerticalAlignment.Center ||
				verticalAlignment == VerticalAlignment.Stretch)
			{
				offsetY = (availableSize.Height - contentSize.Height) / 2;
			}
			else if (verticalAlignment == VerticalAlignment.Bottom)
			{
				offsetY = availableSize.Height - contentSize.Height;
			}
			else
			{
				offsetY = 0;
			}

			return new Windows.Foundation.Point(offsetX, offsetY);
		}

		protected override Size MeasureOverride(Size size)
		{
			var padding = Padding;
			var borderThickness = BorderThickness;

			var measuredSize = MeasureFirstChild(
				new Size(
					size.Width - padding.Left - padding.Right - borderThickness.Left - borderThickness.Right,
					size.Height - padding.Top - padding.Bottom - borderThickness.Top - borderThickness.Bottom
				)
			);

#if UNO_SUPPORTS_NATIVEHOST
			measuredSize = MeasureNativeElement(measuredSize);
#endif

			return new Size(
				measuredSize.Width + padding.Left + padding.Right + borderThickness.Left + borderThickness.Right,
				measuredSize.Height + padding.Top + padding.Bottom + borderThickness.Top + borderThickness.Bottom
			);
		}

		private protected override Thickness GetBorderThickness() => BorderThickness;

		internal override bool CanHaveChildren() => true;

		internal override bool IsViewHit() => Border.IsViewHitImpl(this);

		/// <summary>
		/// Registers the provided native element in the native shell
		/// </summary>
		partial void TryRegisterNativeElement(object newValue);

		/// <summary>
		/// Attaches the current native element in the native shell
		/// </summary>
		partial void TryAttachNativeElement();

		/// <summary>
		/// Detaches the current native element from the native shell
		/// </summary>
		partial void TryDetachNativeElement();

		/// <summary>
		/// Arranges the native element in the native shell
		/// </summary>
		partial void ArrangeNativeElement(Windows.Foundation.Rect arrangeRect);
	}
}

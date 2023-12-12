﻿using System;
using System.Drawing;
using Uno.Extensions;
using Uno.UI;
using Uno.UI.DataBinding;
using System.Linq;
using Microsoft.UI.Xaml.Input;

#if __IOS__
using View = UIKit.UIView;
using Color = UIKit.UIColor;
using Font = UIKit.UIFont;
using UIKit;
using CoreGraphics;
#elif __MACOS__
using View = AppKit.NSView;
using Color = AppKit.NSColor;
using Font = AppKit.NSFont;
using AppKit;
using CoreGraphics;
#endif


namespace Microsoft.UI.Xaml.Controls
{
	public partial class Control
	{
		public Control()
		{
			InitializeControl();
			Initialize();
		}

		void Initialize()
		{
		}

		/// <summary>
		/// Gets the first sub-view of this control or null if there is none
		/// </summary>
		internal IFrameworkElement GetTemplateRoot()
		{
			return Subviews.FirstOrDefault() as IFrameworkElement;
		}

		partial void UnregisterSubView()
		{
			if (Subviews.Length > 0)
			{
				Subviews[0].RemoveFromSuperview();
			}
		}

		partial void RegisterSubView(View child)
		{
			if (Subviews.Length != 0)
			{
				throw new Exception("A Xaml control may not contain more than one child.");
			}

			AddSubview(child);
		}
	}
}


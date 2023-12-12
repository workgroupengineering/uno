﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Markup;

namespace UITests.Shared.Windows_UI_Xaml.MarkupExtension
{
	[MarkupExtensionReturnType(ReturnType = typeof(string))]
	public class Simple : Microsoft.UI.Xaml.Markup.MarkupExtension
	{
		public string TextValue { get; set; }

		protected override object ProvideValue()
		{
			return TextValue + " markup extension";
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uno.UI.RuntimeTests.Helpers;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using static Private.Infrastructure.TestServices;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Markup
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_MarkupExtension
	{
#if HAS_UNO || !WINDOWS_UWP // the signatures are present from winui, uno\uwp and uno\winui, just not uwp
		[TestMethod]
		public void When_MarkupExtension_Default()
		{
			var page = new MarkupExtension_ParserContext();

			var sut = (page.SimpleMarkupExtension as TextBlock);
			var context = (IXamlServiceProvider)sut.Tag;
			var pvt = (IProvideValueTarget)context.GetService(typeof(IProvideValueTarget));
			var property = (ProvideValueTargetProperty)pvt.TargetProperty;

			Assert.AreEqual(pvt.TargetObject, sut);
			Assert.AreEqual(property.Name, nameof(TextBlock.Tag));
		}

		[TestMethod]
		public void When_MarkupExtension_Nested()
		{
			var page = new MarkupExtension_ParserContext();

			var sut = (page.NestedMarkupExtension as TextBlock);
			var context = (IXamlServiceProvider)sut.Tag;
			var pvt = (IProvideValueTarget)context.GetService(typeof(IProvideValueTarget));
			var property = (ProvideValueTargetProperty)pvt.TargetProperty;

			Assert.IsInstanceOfType(pvt.TargetObject, typeof(Binding));
			Assert.AreEqual(property.Name, nameof(Binding.Source));
		}

		[TestMethod]
		public void When_MarkupExtension_Enum()
		{
			var page = new MarkupExtension_ParserContext();

			Assert.AreEqual(Orientation.Horizontal, page.EnumMarkupExtension_Horizontal.Orientation);
			Assert.AreEqual(Orientation.Vertical, page.EnumMarkupExtension_Vertical.Orientation);
		}
#endif
	}
}

﻿using Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MUXControlsTestApp.Utilities;
using Private.Infrastructure;
using Uno.UI.RuntimeTests;
using Uno.UI.RuntimeTests.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.UI.Xaml.Tests.MUXControls.ApiTests
{
	[TestClass]
	public class DropDownButtonTests
	{
#if !WINDOWS_UWP // GetTemplateChild is protected in UWP while public in Uno.
		[TestMethod]
		[Description("Verifies that the TextBlock representing the Chevron glyph uses the correct font")]
		[Ignore("Fluent styles V2 use AnimatedIcon instead of FontIcon")]
		[RunsOnUIThread]
		public void VerifyFontFamilyForChevron()
		{
			DropDownButton dropDownButton = null;
			using (StyleHelper.UseFluentStyles())
			{
				dropDownButton = new DropDownButton();
				TestServices.WindowHelper.WindowContent = dropDownButton;

				var chevronTextBlock = dropDownButton.GetTemplateChild("ChevronTextBlock") as TextBlock;
				var font = chevronTextBlock.FontFamily;
				Verify.AreEqual((FontFamily)Application.Current.Resources["SymbolThemeFontFamily"], font);
			}
		}
#endif
	}
}

﻿using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Shapes
{
	[TestClass]
	public class Given_Path
	{
		[TestMethod]
		[RunsOnUIThread]
		public void Should_not_throw_if_Path_Data_is_set_to_null()
		{
			// This bug is an illustration of issue
			// https://github.com/unoplatform/uno/issues/6846

			// Set initial Data
			var SUT = new Path { Data = new RectangleGeometry() };

			// Switch back to null.  Should not throw an exception.
			SUT.Data = null;
		}

		[TestMethod]
		[RunsOnUIThread]
		public void Should_Not_Include_Control_Points_Bounds()
		{
#if WINDOWS_UWP
			var SUT = new Path { Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), "M 0 0 C 0 0 25 25 0 50") };
#else
			var SUT = new Path { Data = "M 0 0 C 0 0 25 25 0 50" };
#endif

			SUT.Measure(new Size(300, 300));

#if WINDOWS_UWP
			Assert.AreEqual(new Size(11, 50), SUT.DesiredSize);
#else
			Assert.IsTrue(Math.Abs(11 - SUT.DesiredSize.Width) <= 1, $"Actual size: {SUT.DesiredSize}");
			Assert.IsTrue(Math.Abs(50 - SUT.DesiredSize.Height) <= 1, $"Actual size: {SUT.DesiredSize}");
#endif
		}
	}
}

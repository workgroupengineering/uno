﻿using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using SamplesApp.UITests;
using Uno.UI.RuntimeTests.Helpers;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Shapes
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_Path
	{
		[TestMethod]
		[UnoWorkItem("https://github.com/unoplatform/uno/issues/6846")]
		public void Should_not_throw_if_Path_Data_is_set_to_null()
		{
			// Set initial Data
			var SUT = new Path { Data = new RectangleGeometry() };

			// Switch back to null.  Should not throw an exception.
			SUT.Data = null;
		}

		[TestMethod]
		public void Should_Not_Include_Control_Points_Bounds()
		{
			var SUT = new Path { Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), "M 0 0 C 0 0 25 25 0 50") };

			SUT.Measure(new Size(300, 300));

#if WINAPPSDK
			Assert.AreEqual(new Size(11, 50), SUT.DesiredSize);
#else
			Assert.IsTrue(Math.Abs(11 - SUT.DesiredSize.Width) <= 1, $"Actual size: {SUT.DesiredSize}");
			Assert.IsTrue(Math.Abs(50 - SUT.DesiredSize.Height) <= 1, $"Actual size: {SUT.DesiredSize}");
#endif
		}

		[TestMethod]
		[UnoWorkItem("https://github.com/unoplatform/uno/issues/18694")]
#if !__SKIA__
		[Ignore("PathFigure.IsFilled's interaction with Path is only implemented on Skia.")]
#endif
		public async Task When_PathGeometry_Figures_Not_Filled_ColorBrush()
		{
			var SUT = new Path
			{
				Fill = new SolidColorBrush(Windows.UI.Colors.Red),
				Data = new PathGeometry
				{
					Figures = new PathFigureCollection
					{
						new PathFigure
						{
							StartPoint = new Point(0, 0),
							IsFilled = true,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(100, 0) },
								new LineSegment { Point = new Point(100, 100) },
								new LineSegment { Point = new Point(0, 100) },
							}
						},
						new PathFigure // this is an unfilled rectangle without a stroke, should be useless
						{
							StartPoint = new Point(0, 0),
							IsFilled = false,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(50, 0) },
								new LineSegment { Point = new Point(50, 50) },
								new LineSegment { Point = new Point(0, 50) },
							}
						},
						new PathFigure
						{
							StartPoint = new Point(0, 100),
							IsFilled = false,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(100, 100) },
								new LineSegment { Point = new Point(100, 200) },
								new LineSegment { Point = new Point(0, 200) },
							}
						},
						new PathFigure
						{
							StartPoint = new Point(0, 200),
							IsFilled = true,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(100, 200) },
								new LineSegment { Point = new Point(100, 300) },
								new LineSegment { Point = new Point(0, 300) },
							}
						}
					}
				}
			};

			await UITestHelper.Load(SUT);

			var screenShot = await UITestHelper.ScreenShot(SUT);
			ImageAssert.HasColorAt(screenShot, new Point(25, 25), Windows.UI.Colors.Red);
			ImageAssert.HasColorAt(screenShot, new Point(50, 50), Windows.UI.Colors.Red);
			ImageAssert.DoesNotHaveColorAt(screenShot, new Point(50, 150), Windows.UI.Colors.Red);
			ImageAssert.HasColorAt(screenShot, new Point(50, 250), Windows.UI.Colors.Red);
		}

		[TestMethod]
		[UnoWorkItem("https://github.com/unoplatform/uno/issues/18694")]
#if !__SKIA__
		[Ignore("PathFigure.IsFilled's interaction with Path is only implemented on Skia.")]
#endif
		public async Task When_PathGeometry_Figures_Not_Filled_ImageBrush()
		{
			var SUT = new Path
			{
				Fill = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/rect.png")) },
				Data = new PathGeometry
				{
					Figures = new PathFigureCollection
					{
						new PathFigure
						{
							StartPoint = new Point(0, 0),
							IsFilled = true,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(100, 0) },
								new LineSegment { Point = new Point(100, 100) },
								new LineSegment { Point = new Point(0, 100) },
							}
						},
						new PathFigure // this is an unfilled rectangle, so the ImageBrush should fill the PathFigure above with the entire image (i.e. as if this PathFigure doesn't exist)
						{
							StartPoint = new Point(100, 0),
							IsFilled = false,
							Segments = new PathSegmentCollection
							{
								new LineSegment { Point = new Point(200, 0) },
								new LineSegment { Point = new Point(200, 100) },
								new LineSegment { Point = new Point(100, 100) },
							}
						}
					}
				}
			};

			await UITestHelper.Load(SUT);

			var screenShot = await UITestHelper.ScreenShot(SUT);
			ImageAssert.HasColorAt(screenShot, new Point(90, 50), "#FF38FF52");
		}
	}
}

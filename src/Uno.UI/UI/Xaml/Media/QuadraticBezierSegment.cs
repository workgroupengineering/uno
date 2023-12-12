﻿using Windows.Foundation;

namespace Microsoft.UI.Xaml.Media
{
	public partial class QuadraticBezierSegment : PathSegment
	{
		public QuadraticBezierSegment()
		{
		}

		#region Point1

		public Point Point1
		{
			get => (Point)this.GetValue(Point1Property);
			set => this.SetValue(Point1Property, value);
		}

		public static DependencyProperty Point1Property { get; } =
			DependencyProperty.Register(
				"Point1",
				typeof(Point),
				typeof(QuadraticBezierSegment),
				new FrameworkPropertyMetadata(
					defaultValue: new Point(),
					options: FrameworkPropertyMetadataOptions.AffectsMeasure
				)
			);

		#endregion

		#region Point2

		public Point Point2
		{
			get => (Point)this.GetValue(Point2Property);
			set => this.SetValue(Point2Property, value);
		}

		public static DependencyProperty Point2Property { get; } =
			DependencyProperty.Register(
				"Point2",
				typeof(Point),
				typeof(QuadraticBezierSegment),
				new FrameworkPropertyMetadata(
					defaultValue: new Point(),
					options: FrameworkPropertyMetadataOptions.AffectsMeasure
				)
			);

		#endregion
	}
}

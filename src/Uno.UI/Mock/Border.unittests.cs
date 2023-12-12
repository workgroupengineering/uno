﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.UI.Xaml.Controls
{
	public partial class Border
	{
		public override IEnumerable<UIElement> GetChildren()
			=> Child is FrameworkElement fe ? new[] { fe } : Array.Empty<FrameworkElement>();

		partial void OnChildChangedPartial(UIElement previousValue, UIElement newValue)
		{
			if (previousValue != null)
			{
				RemoveChild(previousValue);
			}

			if (newValue is not null)
			{
				AddChild(newValue);
			}
		}
	}
}

﻿using System.Threading.Tasks;
using Uno.UI.Samples.Controls;
using Uno.UI.Samples.Presentation.SamplePages;
using Microsoft.UI.Xaml.Controls;

namespace SamplesApp.Windows_UI_Xaml.Clipping
{
	[SampleControlInfo(category: "Clipping", viewModelType: typeof(ButtonTestsViewModel))]
	public sealed partial class XamlButtonWithClipping_Scrollable : UserControl
	{
		public XamlButtonWithClipping_Scrollable()
		{
			this.InitializeComponent();

			this.Loaded += async (s, e) =>
			{
				// Yield for the content to be materialized properly
				await Task.Yield();
				scrollView.ChangeView(null, 2000, null, true);
			};
		}
	}
}

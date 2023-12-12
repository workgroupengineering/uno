﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Uno.UI.Samples.Controls;
using Uno.UI;

namespace UITests.Windows_UI_Xaml_Controls.LoopingSelectorTests
{
	[Sample("LoopingSelector")]
	public sealed partial class LoopingSelector_Short : Page
	{
		private static readonly IList<object> _items = new[] {
				"Ga (0)",
				"Bu (1)",
				"Zo (2)",
				"Meu (3)",
			}
			.Select(x => new LoopingSelector_Items_Item { PrimaryText = x } as object)
			.ToList();

		public LoopingSelector_Short()
		{
			this.InitializeComponent();

#if !NETFX_CORE
			var loopingSelector = new LoopingSelector
			{
				ItemHeight = 30,
				ShouldLoop = false,
				SelectedIndex = 5,
				Items = _items
			};

			loopingSelector.SelectionChanged += OnSelectionChanged;

			void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
			{
				selection.Text =
					$"SelectedIdex={loopingSelector.SelectedIndex} SelectedItem={loopingSelector.SelectedItem}";
			}

			loopingSelectorContainer.Child = loopingSelector;

#if DEBUG
			async void Do()
			{
				await Task.Delay(100);
				var tree = this.ShowLocalVisualTree(0);
				global::System.Diagnostics.Debug.WriteLine(tree);
			}
			Do();
#endif
#else
			loopingSelectorContainer.Child = new TextBlock { Text = "Not supported on Windows." };
#endif
		}
	}
}

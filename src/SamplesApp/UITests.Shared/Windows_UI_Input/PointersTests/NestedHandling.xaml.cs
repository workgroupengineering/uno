﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Uno.UI.Samples.Controls;

namespace UITests.Windows_UI_Input.PointersTests
{
	[Sample(
		"Pointers",
		Description = "Press the pink square, move a bit and release while remaining on the pink square.",
		IgnoreInSnapshotTests = true)]
	public sealed partial class NestedHandling : Page
	{
		public NestedHandling()
		{
			this.InitializeComponent();

			int containerMoveCount = 0, nestedMoveCount = 0;
			var even = true;

			_nested.PointerPressed += (snd, e) =>
			{
				e.Handled = true;
				_result.Text = "";
				containerMoveCount = 0;
				nestedMoveCount = 0;
			};
			_nested.PointerMoved += (snd, e) =>
			{
				// We filter out half of the events to validate that handled events are not always invalidly bubbled.
				if (even)
				{
					containerMoveCount++;
				}
				else
				{
					e.Handled = true;
				}

				even = !even;
			};

			_container.AddHandler(PointerPressedEvent, new PointerEventHandler((snd, e) => _result.Text += "Pressed SUCCESS"), handledEventsToo: true);
			_container.PointerPressed += (snd, e) => _result.Text = "Pressed FAIL";
			_container.PointerMoved += (snd, e) => nestedMoveCount++;
			_container.PointerReleased += (snd, e) => _result.Text += $" | Released {(nestedMoveCount == containerMoveCount ? "SUCCESS" : "FAIL")}";


			_sample2_container.AddHandler(
				PointerEnteredEvent,
				new PointerEventHandler((snd, e) =>
				{
					if (e.OriginalSource == _sample2_nested
						|| e.OriginalSource == _sample2_intermediate2
						|| e.OriginalSource == _sample2_intermediate)
					{
						_enterResult.Text += "FAILED (container)";
					}
				}),
				handledEventsToo: true);
			_sample2_intermediate.AddHandler(
				PointerEnteredEvent,
				new PointerEventHandler((snd, e) => _enterResult.Text += "SUCCESS"),
				handledEventsToo: true);
			_sample2_intermediate2.AddHandler(
				PointerExitedEvent,
				new PointerEventHandler((snd, e) => _exitResult.Text += "FAILED (intermediate 2)"),
				handledEventsToo: false);
			_sample2_nested.PointerEntered += (snd, e) =>
			{
				e.Handled = true;
				_enterResult.Text = "ENTERED ";
			};

			_sample2_container.AddHandler(
				PointerExitedEvent,
				new PointerEventHandler((snd, e) =>
				{
					if (e.OriginalSource == _sample2_nested
						|| e.OriginalSource == _sample2_intermediate2
						|| e.OriginalSource == _sample2_intermediate)
					{
						_exitResult.Text += "FAILED (container)";
					}
				}),
				handledEventsToo: true);
			_sample2_intermediate.AddHandler(
				PointerExitedEvent,
				new PointerEventHandler((snd, e) => _exitResult.Text += "SUCCESS"),
				handledEventsToo: true);
			_sample2_intermediate2.AddHandler(
				PointerExitedEvent,
				new PointerEventHandler((snd, e) => _exitResult.Text += "FAILED (intermediate 2)"),
				handledEventsToo: false);
			_sample2_nested.PointerExited += (snd, e) =>
			{
				e.Handled = true;
				_exitResult.Text = "EXITED ";
			};
		}
	}
}

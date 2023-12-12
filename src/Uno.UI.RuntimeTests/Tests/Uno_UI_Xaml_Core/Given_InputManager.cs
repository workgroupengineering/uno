﻿#if __SKIA__
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Uno.UI.RuntimeTests.Extensions;
using Windows.ApplicationModel.Appointments;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Preview.Injection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using FluentAssertions;
using Private.Infrastructure;
using Uno.Extensions;
using Uno.UI.RuntimeTests.Helpers;

namespace Uno.UI.RuntimeTests.Tests.Uno_UI_Xaml_Core;

[TestClass]
[RunsOnUIThread]
public class Given_InputManager
{
	[TestMethod]
#if !__SKIA__
	[Ignore("Pointer injection supported only on skia for now.")]
#endif
	public async Task When_VisibilityChangesWhileDispatching_Then_RecomputeOriginalSource()
	{
		Border col1, col2;
		var ui = new Grid
		{
			Width = 200,
			Height = 200,
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
				new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
			},
			Children =
			{
				(col1 = new Border { Background = new SolidColorBrush(Colors.DeepPink) }),
				(col2 = new Border { Background = new SolidColorBrush(Colors.DeepSkyBlue) }),
			}
		};

		Grid.SetColumn(col1, 0);
		Grid.SetColumn(col2, 1);

		var position = await UITestHelper.Load(ui);

		var injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("Failed to init the InputInjector");
		using var finger = injector.GetFinger();

		var failed = false;
		col1.PointerExited += (snd, args) => col2.Visibility = Visibility.Collapsed;
		col2.PointerEntered += (snd, args) => failed = true;

		injector.GetFinger().Drag(position.Location.Offset(10), position.Location.Offset(180, 10));

		Assert.AreEqual(Visibility.Collapsed, col2.Visibility, "The visibility should have been changed when the pointer left the col1.");
		Assert.IsFalse(failed, "The pointer should not have been dispatched to the col2 as it has been set to visibility collapsed.");
	}

	[TestMethod]
#if !__SKIA__
	[Ignore("Pointer injection supported only on skia for now.")]
#endif
	public async Task When_LeaveElementWhileManipulating_Then_CaptureNotLost()
	{
		Border sut;
		TranslateTransform transform;
		var ui = new Grid
		{
			Width = 128,
			Height = 512,
			Children =
			{
				(sut = new Border
				{
					Name = "SUT-Border",
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Width = 16,
					Height = Windows.UI.Input.GestureRecognizer.Manipulation.StartTouch.TranslateY * 3,
					Background = new SolidColorBrush(Colors.DeepPink),
					ManipulationMode = ManipulationModes.TranslateY,
					RenderTransform = (transform = new TranslateTransform())
				}),
			}
		};

		await UITestHelper.Load(ui);

		var exited = false;
		sut.ManipulationDelta += (snd, e) => transform.Y = e.Cumulative.Translation.Y;
		sut.PointerExited += (snd, e) => exited = true;

		var injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("Failed to init the InputInjector");
		using var finger = injector.GetFinger();

		finger.Press(sut.GetAbsoluteBounds().GetCenter());

		// Start manipulation
		finger.MoveBy(0, 50, steps: 50);
		transform.Y.Should().NotBe(0, "Manipulation should have started");

		// Cause a fast move that will trigger a pointer leave
		// Note: This might not be the WinUI behavior, should we receive a pointer leave when the element is capturing the pointer?
		exited.Should().BeFalse();
		finger.MoveBy(0, 50, steps: 0);
		exited.Should().BeTrue();

		// Confirm that even if we got a leave, pointer is still captured and we are still receiving manipulation events
		var intermediatePosition = transform.Y;
		finger.MoveBy(0, 50);
		transform.Y.Should().Be(intermediatePosition + 50);
	}

	[TestMethod]
#if !__SKIA__
	[Ignore("Pointer injection supported only on skia for now.")]
#endif
	public async Task When_Hover_No_Delay_For_VisualState_Update()
	{
		var comboxBoxItem = new ComboBoxItem()
		{
			Content = "ComboBoxItem Content",
		};

		var position = await UITestHelper.Load(comboxBoxItem);

		var injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("Failed to init the InputInjector");
		var mouse = injector.GetMouse();

		var oldState = VisualStateManager.GetCurrentState(comboxBoxItem, "CommonStates").Name;
		mouse.MoveTo(position.GetCenter().X, position.GetCenter().Y);
		var newState = VisualStateManager.GetCurrentState(comboxBoxItem, "CommonStates").Name;
		Assert.AreEqual("Normal", oldState);
		Assert.AreEqual("PointerOver", newState);
	}

	[TestMethod]
#if !__SKIA__
	[Ignore("Pointer injection supported only on skia for now.")]
#endif
	public async Task When_Scroll_No_Delay_For_VisualState_Update()
	{
		var stackPanel = new StackPanel();
		for (var i = 1; i <= 10; i++)
		{
			var button = new Button { Content = "Button " + i };
			stackPanel.Children.Add(button);
		}

		var scrollViewer = new ScrollViewer()
		{
			VerticalScrollMode = ScrollMode.Enabled,
			Content = stackPanel,
			MaxHeight = 50,
		};

		var button1 = stackPanel.Children[0] as Button;
		var button2 = stackPanel.Children[1] as Button;

		var border = new Border { scrollViewer };

		await UITestHelper.Load(border);

		var injector = InputInjector.TryCreate() ?? throw new InvalidOperationException("Failed to init the InputInjector");
		var mouse = injector.GetMouse();

		Assert.AreEqual("Normal", VisualStateManager.GetCurrentState(button1, "CommonStates").Name);

		var position = button1.GetAbsoluteBounds();
		mouse.MoveTo(position.GetCenter().X, position.GetCenter().Y);
		Assert.AreEqual("PointerOver", VisualStateManager.GetCurrentState(button1, "CommonStates").Name);

		mouse.Wheel(-50);

		await TestServices.WindowHelper.WaitForIdle();
		Assert.AreEqual("Normal", VisualStateManager.GetCurrentState(button1, "CommonStates").Name);
		Assert.AreEqual("PointerOver", VisualStateManager.GetCurrentState(button2, "CommonStates").Name);
	}
}
#endif

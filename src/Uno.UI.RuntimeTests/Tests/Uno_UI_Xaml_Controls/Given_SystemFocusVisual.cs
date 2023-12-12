﻿#if __WASM__ || __SKIA__
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Private.Infrastructure;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Uno.Extensions;
using Uno.UI.RuntimeTests.Helpers;

namespace Uno.UI.RuntimeTests.Tests.Uno_UI_Xaml_Controls;

[TestClass]
[RunsOnUIThread]
public class Given_SystemFocusVisual
{
	[TestMethod]
	[RequiresFullWindow]
	public async Task When_Focused_Element_Scrolled()
	{
		if (TestServices.WindowHelper.IsXamlIsland)
		{
			Assert.Inconclusive($"Not supported under XAML islands");
		}
		var scrollViewer = new ScrollViewer()
		{
			Height = 200,
			Margin = ThicknessHelper.FromUniformLength(30)
		};
		var button = new Button()
		{
			Content = "Test",
			FocusVisualPrimaryThickness = ThicknessHelper.FromUniformLength(10),
			FocusVisualSecondaryThickness = ThicknessHelper.FromUniformLength(10),
		};
		var topBorder = new Border() { Height = 150, Width = 300, Background = SolidColorBrushHelper.Blue };
		var bottomBorder = new Border() { Height = 300, Width = 300, Background = SolidColorBrushHelper.Red };
		var stackPanel = new StackPanel()
		{
			Children =
			{
				topBorder,
				button,
				bottomBorder,
			}
		};

		scrollViewer.Content = stackPanel;

		TestServices.WindowHelper.WindowContent = scrollViewer;
		await TestServices.WindowHelper.WaitForIdle();

		button.Focus(FocusState.Keyboard);
		await TestServices.WindowHelper.WaitForIdle();
		var visualTree = Uno.UI.Xaml.Core.CoreServices.Instance.ContentRootCoordinator.CoreWindowContentRoot?.VisualTree;
		var focusVisualLayer = visualTree?.FocusVisualRoot;

		Assert.IsNotNull(focusVisualLayer);
		Assert.AreEqual(1, focusVisualLayer.Children.Count);

		var focusVisual = focusVisualLayer.Children.First();

		var transform = focusVisual.TransformToVisual(Microsoft.UI.Xaml.Window.Current.RootElement);
		var initialPoint = transform.TransformPoint(default);

		scrollViewer.ChangeView(null, 100, null, true);

		await TestServices.WindowHelper.WaitFor(() =>
		{
			transform = focusVisual.TransformToVisual(Microsoft.UI.Xaml.Window.Current.RootElement);
			var currentPoint = transform.TransformPoint(default);

			return currentPoint.Y < initialPoint.Y;
		});

		await TestServices.WindowHelper.WaitForIdle();

		transform = focusVisual.TransformToVisual(Microsoft.UI.Xaml.Window.Current.RootElement);
		var scrolledPoint = transform.TransformPoint(default);
		Assert.AreEqual(initialPoint.Y - 100, scrolledPoint.Y, 0.5);
	}

	[TestMethod]
#if __WASM__
	[Ignore("RenderTargetBitmap is not implemented")]
#endif
	public async Task When_Focused_Element_Scrolled_Clipping()
	{
		if (TestServices.WindowHelper.IsXamlIsland)
		{
			Assert.Inconclusive($"Not supported under XAML islands");
		}
		var sp = new StackPanel();
		var sv = new ScrollViewer
		{
			Height = 70,
			Content = sp
		};
		var buttons = Enumerable.Range(0, 10).Select(i => new Button
		{
			Content = $"{i}"
		}).ToList();
		sp.Children.AddRange(buttons);

		var border = new Border
		{
			Height = 130,
			Padding = new Thickness(0, 30, 0, 0),
			Child = sv
		};

		TestServices.WindowHelper.WindowContent = border;
		await TestServices.WindowHelper.WaitForIdle();

		buttons[2].Focus(FocusState.Keyboard);
		await TestServices.WindowHelper.WaitForIdle();
		var visualTree = Uno.UI.Xaml.Core.CoreServices.Instance.ContentRootCoordinator.CoreWindowContentRoot?.VisualTree;
		var focusVisualLayer = visualTree?.FocusVisualRoot;

		Assert.IsNotNull(focusVisualLayer);
		Assert.AreEqual(1, focusVisualLayer.Children.Count);

		for (var i = 0; i < 15; i++)
		{
			sv.ChangeView(null, 10 * i, null, true);
			await TestServices.WindowHelper.WaitForIdle();

			var screenShot = await UITestHelper.ScreenShot(border, true);
			ImageAssert.DoesNotHaveColorInRectangle(screenShot, new Rectangle(0, 0, 5, 30), ((SolidColorBrush)buttons[1].FocusVisualPrimaryBrush).Color);
			ImageAssert.DoesNotHaveColorInRectangle(screenShot, new Rectangle(0, screenShot.Height - 30, 5, 30), ((SolidColorBrush)buttons[2].FocusVisualPrimaryBrush).Color);
		}
	}
}
#endif

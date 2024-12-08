﻿#nullable enable

using Uno.UI.Xaml.Controls;

namespace Windows.UI.Xaml;

partial class Window
{
	public bool IsStatusBarTranslucent() => NativeWindowWrapper.Instance.IsStatusBarTranslucent(); //TODO: Can remove as breaking change? #8339
}

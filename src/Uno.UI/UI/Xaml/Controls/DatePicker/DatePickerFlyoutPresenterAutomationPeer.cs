﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// MUX reference DatePickerFlyoutPresenterAutomationPeer_Partial.cpp, tag winui3/release/1.4.2

using Windows.UI.Xaml.Controls;

namespace Windows.UI.Xaml.Automation.Peers;

/// <summary>
/// Exposes DatePickerFlyoutPresenter types to Microsoft UI Automation.
/// </summary>
public partial class DatePickerFlyoutPresenterAutomationPeer
{
	public DatePickerFlyoutPresenterAutomationPeer()
	{

	}

	protected override string GetClassNameCore()
		=> nameof(DatePickerFlyoutPresenter);

	protected override AutomationControlType GetAutomationControlTypeCore()
		=> AutomationControlType.Pane;

	protected override string GetNameCore() => nameof(DatePicker);
}

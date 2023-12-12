﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// MUX Reference CheckBoxAutomationPeer_Partial.cpp, tag winui3/release/1.4.2

using Microsoft.UI.Xaml.Controls;

namespace Microsoft.UI.Xaml.Automation.Peers;

public partial class CheckBoxAutomationPeer : ToggleButtonAutomationPeer
{
	public CheckBoxAutomationPeer(CheckBox owner) : base(owner)
	{
	}

	protected override string GetClassNameCore() => nameof(CheckBox);

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.CheckBox;
}

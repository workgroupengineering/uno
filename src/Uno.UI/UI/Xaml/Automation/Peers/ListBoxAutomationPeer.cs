﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// MUX Reference ListBoxAutomationPeer_Partial.cpp, tag winui3/release/1.4.2
namespace Windows.UI.Xaml.Automation.Peers;

/// <summary>
/// Exposes ListBox types to UI Automation.
/// </summary>
public partial class ListBoxAutomationPeer : SelectorAutomationPeer
{
	public ListBoxAutomationPeer(Controls.ListBox owner) : base(owner)
	{

	}

	protected override string GetClassNameCore() => nameof(Controls.ListBox);

	protected override AutomationControlType GetAutomationControlTypeCore()
		=> AutomationControlType.List;
}

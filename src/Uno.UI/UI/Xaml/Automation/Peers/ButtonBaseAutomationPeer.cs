﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// MUX reference ButtonBaseAutomationPeer_Partial.cpp, tag winui3/release/1.4.2

using Windows.UI.Xaml.Controls.Primitives;


namespace Windows.UI.Xaml.Automation.Peers;

/// <summary>
/// Exposes ButtonBase types to UI Automation.
/// </summary>
public partial class ButtonBaseAutomationPeer : FrameworkElementAutomationPeer
{
	protected ButtonBaseAutomationPeer(ButtonBase owner) : base(owner)
	{
	}

	protected ButtonBaseAutomationPeer(ButtonBaseAutomationPeer buttonBase) : base(buttonBase)
	{
	}

	protected override bool IsControlElementCore() => true;
}

﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// MUX reference ButtonAutomationPeer_Partial.cpp, tag winui3/release/1.4.2

using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Windows.UI.Xaml.Automation.Peers;

/// <summary>
/// Exposes Button types to UI Automation.
/// </summary>
/// <param name="owner"></param>
public partial class ButtonAutomationPeer : ButtonBaseAutomationPeer, IInvokeProvider
{
	public ButtonAutomationPeer(Button owner) : base(owner)
	{

	}

	protected override object GetPatternCore(PatternInterface patternInterface)
	{
		if (patternInterface == PatternInterface.Invoke)
		{
			return this;
		}

		return base.GetPatternCore(patternInterface);
	}

	protected override string GetClassNameCore() => nameof(Button);

	protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

	/// <summary>
	/// Sends a request to click the button associated with the automation peer.
	/// </summary>
	public void Invoke()
	{
		if (IsEnabled())
		{
			(Owner as ButtonBase).ProgrammaticClick();
		}
	}
}

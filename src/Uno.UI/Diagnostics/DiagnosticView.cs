﻿#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Uno.Diagnostics.UI;

/// <summary>
/// A generic diagnostic view.
/// </summary>
public partial class DiagnosticView(
	string id,
	string name,
	Func<IDiagnosticViewContext, UIElement> factory,
	Func<IDiagnosticViewContext, CancellationToken, ValueTask<object?>>? details = null,
	DiagnosticViewRegistrationPosition position = default)
	: DiagnosticView<UIElement>(id, name, factory, details, position)
{
	public DiagnosticView(
		string id,
		string name,
		Func<UIElement> preview,
		Func<CancellationToken, ValueTask<object?>>? details = null,
		DiagnosticViewRegistrationPosition position = default)
		: this(id, name, _ => preview(), async (_, ct) => details is null ? null : await details(ct), position)
	{
	}
}

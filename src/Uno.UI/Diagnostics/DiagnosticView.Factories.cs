﻿#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Uno.Diagnostics.UI;

partial class DiagnosticView
{
	/// <summary>
	/// Registers a dedicated diagnostic view to be displayed by the diagnostic overlay.
	/// </summary>
	/// <remarks>
	/// This is a designed for a control dedicated to render the status of a specific diagnostic information.
	/// The control is expected to internally listen to an event to updates itself.
	/// </remarks>
	/// <remarks>
	/// This only registers the diagnostic, it does not open the overlay.
	/// </remarks>
	/// <typeparam name="TView">Type of the control.</typeparam>
	/// <param name="friendlyName">The user-friendly name of the diagnostics view.</param>
	/// <param name="mode">Defines when the registered diagnostic view should be displayed.</param>
	/// <param name="position">Defines where the item should be placed in the overlay.</param>
	public static DiagnosticView<TView> Register<TView>(
		string friendlyName,
		DiagnosticViewRegistrationMode mode = default,
		DiagnosticViewRegistrationPosition position = default)
		where TView : UIElement, new()
	{
		var provider = new DiagnosticView<TView>(typeof(TView).Name, friendlyName, () => new TView(), position: position);
		DiagnosticViewRegistry.Register(provider, mode);
		return provider;
	}

	/// <summary>
	/// Registers a dedicated diagnostic view.
	/// </summary>
	/// <remarks>
	/// This is a designed for a control dedicated to render the status of a specific diagnostic information.
	/// The control is expected to internally listen to an event to updates itself.
	/// </remarks>
	/// <remarks>
	/// This only registers the diagnostic, it does not open the overlay.
	/// </remarks>
	/// <typeparam name="TView">Type of the control.</typeparam>
	/// <param name="friendlyName">The user-friendly name of the diagnostics view.</param>
	/// <param name="factory">Factory to create an instance of the control.</param>
	/// <param name="mode">Defines when the registered diagnostic view should be displayed.</param>
	/// <param name="position">Defines where the item should be placed in the overlay.</param>
	public static DiagnosticView<TView> Register<TView>(
		string friendlyName,
		Func<TView> factory,
		DiagnosticViewRegistrationMode mode = default,
		DiagnosticViewRegistrationPosition position = default)
		where TView : UIElement
	{
		var provider = new DiagnosticView<TView>(typeof(TView).Name, friendlyName, factory, position: position);
		DiagnosticViewRegistry.Register(provider, mode);
		return provider;
	}

	/// <summary>
	/// Registers a generic FrameworkElement as diagnostic view.
	/// </summary>
	/// <remarks>
	/// This only registers the diagnostic, it does not open the overlay.
	/// </remarks>
	/// <typeparam name="TView">Type if the generic FrameworkElement to use.</typeparam>
	/// <typeparam name="TState">Type of the state used to update the <typeparamref name="TView"/>.</typeparam>
	/// <param name="friendlyName">The user-friendly name of the diagnostics view.</param>
	/// <param name="update">Delegate to use to update the <typeparamref name="TView"/> when the <typeparamref name="TState"/> is being updated.</param>
	/// <param name="details">Optional delegate used to show more details about the diagnostic info when user taps on the view.</param>
	/// <param name="mode">Defines when the registered diagnostic view should be displayed.</param>
	/// <param name="position">Defines where the item should be placed in the overlay.</param>
	/// <returns>A diagnostic view helper class which can be used to push updates of the state (cf. <see cref="DiagnosticView{TView,TState}.Update"/>).</returns>
	public static DiagnosticView<TView, TState> Register<TView, TState>(
		string friendlyName,
		Action<TView, TState> update,
		Func<TState, object?>? details = null,
		DiagnosticViewRegistrationMode mode = default,
		DiagnosticViewRegistrationPosition position = default)
		where TView : FrameworkElement, new()
	{
		var provider = details is null
			? new DiagnosticView<TView, TState>(typeof(TView).Name, friendlyName, _ => new TView(), update, position: position)
			: new DiagnosticView<TView, TState>(typeof(TView).Name, friendlyName, _ => new TView(), update, (ctx, state, ct) => new(details(state)), position: position);
		DiagnosticViewRegistry.Register(provider, mode);
		return provider;
	}

	/// <summary>
	/// Registers a generic FrameworkElement as diagnostic view.
	/// </summary>
	/// <remarks>
	/// This only registers the diagnostic, it does not open the overlay.
	/// </remarks>
	/// <typeparam name="TView">Type if the generic FrameworkElement to use.</typeparam>
	/// <typeparam name="TState">Type of the state used to update the <typeparamref name="TView"/>.</typeparam>
	/// <param name="friendlyName">The user-friendly name of the diagnostics view.</param>
	/// <param name="factory">Factory to create an instance of the generic element.</param>
	/// <param name="update">Delegate to use to update the <typeparamref name="TView"/> when the <typeparamref name="TState"/> is being updated.</param>
	/// <param name="details">Optional delegate used to show more details about the diagnostic info when user taps on the view.</param>
	/// <param name="mode">Defines when the registered diagnostic view should be displayed.</param>
	/// <param name="position">Defines where the item should be placed in the overlay.</param>
	/// <returns>A diagnostic view helper class which can be used to push updates of the state (cf. <see cref="DiagnosticView{TView,TState}.Update"/>).</returns>
	public static DiagnosticView<TView, TState> Register<TView, TState>(
		string friendlyName,
		Func<IDiagnosticViewContext, TView> factory,
		Action<TView, TState> update,
		Func<TState, object?>? details = null,
		DiagnosticViewRegistrationMode mode = default,
		DiagnosticViewRegistrationPosition position = default)
		where TView : FrameworkElement
	{
		var provider = details is null
			? new DiagnosticView<TView, TState>(typeof(TView).Name, friendlyName, factory, update, position: position)
			: new DiagnosticView<TView, TState>(typeof(TView).Name, friendlyName, factory, update, (ctx, state, ct) => new(details(state)), position: position);
		DiagnosticViewRegistry.Register(provider, mode);
		return provider;
	}
}

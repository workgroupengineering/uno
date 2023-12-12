﻿using Windows.Foundation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Data;
using NotImplementedException = System.NotImplementedException;

namespace Microsoft.UI.Xaml.Automation
{
	internal class AutomationHelper
	{
		internal enum AutomationPropertyEnum
		{
			EmptyProperty,
			NameProperty,
			IsSelectedProperty,
			NotSupported
		};

		internal static void RaiseEventIfListener(
			UIElement element,
			AutomationEvents eventId)
		{
			var automationPeer = CreatePeerForElement(element);
			automationPeer?.RaiseAutomationEvent(eventId);
		}

		internal static void SetAutomationFocusIfListener(UIElement element)
		{
			var anyListener = ListenerExistsHelper(AutomationEvents.AutomationFocusChanged);

			if (anyListener)
			{
				//CreatePeerForElement(element)?.SetAutomationFocus();
				CreatePeerForElement(element)?.SetFocus();
			}
		}

		internal static AutomationPropertyEnum ConvertPropertyToEnum(AutomationProperty property)
		{
			if (property == null)
			{
				return AutomationPropertyEnum.EmptyProperty;
			}

			if (property == AutomationElementIdentifiers.NameProperty)
			{
				return AutomationPropertyEnum.NameProperty;
			}

			if (property == SelectionItemPatternIdentifiers.IsSelectedProperty)
			{
				return AutomationPropertyEnum.IsSelectedProperty;
			}

			return AutomationPropertyEnum.NotSupported;
		}

		internal static void RaisePropertyChanged<T>(
			UIElement element,
			AutomationProperty automationProperty,
			T oldValue,
			T newValue,
			bool checkIfListenerExists = false)
		{
			if (checkIfListenerExists && !ListenerExistsHelper(AutomationEvents.PropertyChanged))
			{
				return;
			}

			CreatePeerForElement(element)?.RaisePropertyChangedEvent(automationProperty, oldValue, newValue);
		}

		internal static void RaisePropertyChangedIfListener<T>(
			UIElement element,
			AutomationProperty automationProperty,
			T oldValue,
			T newValue)
		{
			RaisePropertyChanged(element, automationProperty, oldValue, newValue, checkIfListenerExists: true);
		}

		internal static AutomationPeer CreatePeerForElement(UIElement element)
		{
			return FrameworkElementAutomationPeer.CreatePeerForElement(element);
		}

		private static bool ListenerExistsHelper(AutomationEvents eventId)
		{
			return AutomationPeer.ListenerExists(eventId);
		}

		public static string GetPlainText(DependencyObject obj)
		{
			if (obj is IStringable strignable)
			{
				return strignable.ToString();
			}

			if (obj is IPropertyValue ipv)
			{
				return ipv.GetString();
			}

			if (obj is ICustomPropertyProvider icpp)
			{
				return icpp.GetStringRepresentation();
			}

			return null;
		}
	}
}

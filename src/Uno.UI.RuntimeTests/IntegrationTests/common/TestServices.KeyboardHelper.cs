﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;

namespace Private.Infrastructure
{
	public partial class TestServices
	{
		public static class KeyboardHelper
		{
			private static Dictionary<string, VirtualKey> m_vKeyMapping = new Dictionary<string, VirtualKey>()
			{
				{"cancel",                      VirtualKey.Cancel},
				{"backspace",                   VirtualKey.Back},
				{"clear",                       VirtualKey.Clear},
				{"enter",                       VirtualKey.Enter},
				{"return",                      VirtualKey.Enter},
				{"numpadenter",                 VirtualKey.Enter},
				{"shift",                       VirtualKey.Shift},
				{"pause",                       VirtualKey.Pause},
				{"capslock",                    VirtualKey.CapitalLock},
				{"kanamode",                    VirtualKey.Kana},
				{"hangulmode",                  VirtualKey.Hangul},
				{"junjamode",                   VirtualKey.Junja},
				{"finalmode",                   VirtualKey.Final},
				{"hanjamode",                   VirtualKey.Hanja},
				{"kanjimode",                   VirtualKey.Kanji},
				{"imeconvert",                  VirtualKey.Convert},
				{"imenonconvert",               VirtualKey.NonConvert},
				{"imeaccept",                   VirtualKey.Accept},
				{"imemodechange",               VirtualKey.ModeChange},
				{"prior",                       VirtualKey.Back},
				{"pageup",                      VirtualKey.PageUp},
				{"pagedown",                    VirtualKey.PageDown},
				{"end",                         VirtualKey.End},
				{"home",                        VirtualKey.Home},
				{"select",                      VirtualKey.Select},
				{"print",                       VirtualKey.Print},
				{"execute",                     VirtualKey.Execute},
				{"printscreen",                 VirtualKey.Snapshot},
				{"snapshot",                    VirtualKey.Snapshot},
				{"insert",                      VirtualKey.Insert},
				{"delete",                      VirtualKey.Delete},
				{"del",                         VirtualKey.Delete},
				{"help",                        VirtualKey.Help},
				{"lwin",                        VirtualKey.LeftWindows},
				{"rwin",                        VirtualKey.RightWindows},
				{"apps",                        VirtualKey.Application},
				{"sleep",                       VirtualKey.Sleep},
				{"*",                           VirtualKey.Multiply},
				{"+",                           VirtualKey.Add},
				{"separator",                   VirtualKey.Separator},
				{"-",                           VirtualKey.Subtract},
				{"/",                           VirtualKey.Divide},
				{"f1",                          VirtualKey.F1},
				{"f2",                          VirtualKey.F2},
				{"f3",                          VirtualKey.F3},
				{"f4",                          VirtualKey.F4},
				{"f5",                          VirtualKey.F5},
				{"f6",                          VirtualKey.F6},
				{"f7",                          VirtualKey.F7},
				{"f8",                          VirtualKey.F8},
				{"f9",                          VirtualKey.F9},
				{"f10",                         VirtualKey.F10},
				{"f11",                         VirtualKey.F11},
				{"f12",                         VirtualKey.F12},
				{"f13",                         VirtualKey.F13},
				{"f14",                         VirtualKey.F14},
				{"f15",                         VirtualKey.F15},
				{"f16",                         VirtualKey.F16},
				{"f17",                         VirtualKey.F17},
				{"f18",                         VirtualKey.F18},
				{"f19",                         VirtualKey.F19},
				{"f20",                         VirtualKey.F20},
				{"f21",                         VirtualKey.F21},
				{"f22",                         VirtualKey.F22},
				{"f23",                         VirtualKey.F23},
				{"f24",                         VirtualKey.F24},
				{"numlock",                     VirtualKey.NumberKeyLock},
				{"scrolllock",                  VirtualKey.Scroll},
				{"lshift",                      VirtualKey.LeftShift},
				{"rshift",                      VirtualKey.RightShift},
				{"lctrl",                       VirtualKey.LeftControl},
				{"rctrl",                       VirtualKey.RightControl},
				{"lalt",                        VirtualKey.LeftMenu},
				{"ralt",                        VirtualKey.RightMenu},
				{"space",                       VirtualKey.Space},
				{" ",                           VirtualKey.Space},
				{"down",                        VirtualKey.Down},
				{"up",                          VirtualKey.Up},
				{"left",                        VirtualKey.Left},
				{"right",                       VirtualKey.Right},
				{"tab",                         VirtualKey.Tab},
				{"esc",                         VirtualKey.Escape},
				{"ctrl",                        VirtualKey.Control},
				{"GamepadA",                    VirtualKey.GamepadA},
				{"GamepadB",                    VirtualKey.GamepadB},
				{"GamepadDpadRight",            VirtualKey.GamepadDPadRight},
				{"GamepadDpadDown",             VirtualKey.GamepadDPadDown},
				{"GamepadDpadUp",               VirtualKey.GamepadDPadUp},
				{"GamepadDpadLeft",             VirtualKey.GamepadDPadLeft},
				{"GamepadLeftShoulder",         VirtualKey.GamepadLeftShoulder},
				{"GamepadLeftTrigger",          VirtualKey.GamepadLeftTrigger},
				{"GamepadRightShoulder",        VirtualKey.GamepadRightShoulder},
				{"GamepadRightTrigger",         VirtualKey.GamepadRightTrigger},
				{"GamePadLeftThumbStickRight",  VirtualKey.GamepadLeftThumbstickRight},
				{"GamePadLeftThumbStickDown",   VirtualKey.GamepadLeftThumbstickDown},
				{"GamePadLeftThumbStickUp",     VirtualKey.GamepadLeftThumbstickUp},
				{"GamePadLeftThumbStickLeft",   VirtualKey.GamepadLeftThumbstickLeft},
				{"GamePadRightThumbStickRight", VirtualKey.GamepadRightThumbstickRight},
				{"GamePadRightThumbStickDown",  VirtualKey.GamepadRightThumbstickDown},
				{"GamePadRightThumbStickUp",    VirtualKey.GamepadRightThumbstickUp},
				{"GamePadRightThumbStickLeft",  VirtualKey.GamepadRightThumbstickLeft},
				{"GamePadMenu",                 VirtualKey.GamepadMenu},
			};


			public static async void PressKeySequence(string keys, UIElement element = null)
			{
#if !NETFX_CORE
				if (string.IsNullOrEmpty(keys))
				{
					return;
				}

				element ??= FocusManager.GetFocusedElement(WindowHelper.XamlRoot) as UIElement;
				if (element is null)
				{
					return;
				}

				// In RS3, alt+shift is a hotkey and XAML will not be notified of the
				// 'shift' key. We have the same behavior for the opposite sequence: if
				// the sequence is shift+alt, XAML will not be notified of the 'alt' key.
				// To workaround this behavior, we will ignore failures when sending alt
				// and shift inputs if both are present in the sequence.
				bool areAltAndShiftUsed = keys.Contains("alt") || keys.Contains("shift");

				// This is a cute key sequence mini-language adopted from the previous test framework.
				// It walks through the string and if it's a normal string sends it, or if it's a string
				// specifying special keys, will press those keys. Here are some valid strings:
				// "$d$_ctrl#$d$_-#$u$_-#$u$_ctrl" - Down Ctrl, Down -, Up -, Up Ctrl
				// "Hello" - Down Shift, Down H, Up H, Up Shift, Down e, Up e... etc
				int posStart = 0;
				keys += "#";
				int posEnd = keys.IndexOf("#");
				while (posEnd != -1)
				{
					var keyInstruction = keys.Substring(posStart, posEnd - posStart);

					int keyDownCodePos = keyInstruction.IndexOf("$d$_");
					int keyUpCodePos = keyInstruction.IndexOf("$u$_");

					if (keyDownCodePos == 0 || keyUpCodePos == 0)
					{
						var key = keyInstruction.Substring(4, keyInstruction.Length - 4);
						if (m_vKeyMapping.TryGetValue(key, out var vKey))
						{
							await RaiseOnElementDispatcherAsync(element, keyDownCodePos == 0 ? UIElement.KeyDownEvent : UIElement.KeyUpEvent, new KeyRoutedEventArgs(element, vKey, VirtualKeyModifiers.None));
						}
					}
					else
					{
						// This is a normal string, just send a up/down of each key.
						for (int i = 0; i < keyInstruction.Length; i++)
						{
							var key = keyInstruction.Substring(i, 1);
							bool shouldShift = char.IsUpper(key[0]);
							key = char.ToLower(key[0], CultureInfo.InvariantCulture).ToString();

							if (shouldShift)
							{
								if (m_vKeyMapping.TryGetValue("shift", out var vShiftKey))
								{
									await RaiseOnElementDispatcherAsync(element, UIElement.KeyDownEvent, new KeyRoutedEventArgs(element, vShiftKey, VirtualKeyModifiers.None));
								}
							}

							if (m_vKeyMapping.TryGetValue(key, out var vKey))
							{
								var modifiers = shouldShift ? VirtualKeyModifiers.Shift : VirtualKeyModifiers.None;
								await RaiseOnElementDispatcherAsync(element, UIElement.KeyDownEvent, new KeyRoutedEventArgs(element, vKey, modifiers));
								await RaiseOnElementDispatcherAsync(element, UIElement.KeyUpEvent, new KeyRoutedEventArgs(element, vKey, modifiers));
							}

							if (shouldShift)
							{
								if (m_vKeyMapping.TryGetValue("shift", out var vShiftKey))
								{
									await RaiseOnElementDispatcherAsync(element, UIElement.KeyUpEvent, new KeyRoutedEventArgs(element, vShiftKey, VirtualKeyModifiers.None));
								}
							}
						}
					}

					posStart = posEnd + 1;
					posEnd = keys.IndexOf("#", posStart);
				}

				async Task RaiseOnElementDispatcherAsync(UIElement element, RoutedEvent routedEvent, RoutedEventArgs args)
				{
					bool raiseSynchronously = element.Dispatcher.HasThreadAccess;
#if __WASM__
					if (!Uno.UI.Dispatching.NativeDispatcher.IsThreadingSupported)
					{
						raiseSynchronously = true;
					}
#endif

					if (raiseSynchronously)
					{
						element.SafeRaiseEvent(routedEvent, args);
					}
					else
					{
						await element.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => element.SafeRaiseEvent(routedEvent, args));
					}
				}
#endif
			}

			public static void Down(UIElement element = null)
			{
				PressKeySequence("$d$_down#$u$_down", element);
			}

			public static void Up(UIElement element = null)
			{
				PressKeySequence("$d$_up#$u$_up", element);

			}

			public static void Left(UIElement element = null)
			{
				PressKeySequence("$d$_left#$u$_left", element);

			}

			public static void Right(UIElement element = null)
			{
				PressKeySequence("$d$_right#$u$_right", element);

			}

			internal static void ShiftTab(UIElement element = null)
			{
				PressKeySequence("$d$_shift#$d$_tab#$u$_tab#$u$_shift", element);
			}

			public static void Tab(UIElement element = null)
			{
				PressKeySequence("$d$_tab#$u$_tab", element);

			}

			public static void PageDown(UIElement element = null)
			{
				PressKeySequence("$d$_pagedown#$u$_pagedown", element);

			}

			public static void Escape(UIElement element = null)
			{
				PressKeySequence("$d$_esc#$u$_esc", element);

			}

			public static void Enter(UIElement element = null)
			{
				PressKeySequence("$d$_enter#$u$_enter", element);

			}

			public static void Space(UIElement element = null)
			{
				PressKeySequence("$d$_space#$u$_space", element);

			}

			public static void GamepadA(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadA#$u$_GamepadA", element);
			}

			public static void GamepadB(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadB#$u$_GamepadB", element);
			}

			public static void GamepadDpadRight(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadDpadRight#$u$_GamepadDpadRight", element);
			}

			public static void GamepadDpadLeft(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadDpadLeft#$u$_GamepadDpadLeft", element);
			}
			public static void GamepadDpadUp(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadDpadUp#$u$_GamepadDpadUp", element);
			}
			public static void GamepadDpadDown(UIElement element = null)
			{
				PressKeySequence("$d$_GamepadDpadDown#$u$_GamepadDpadDown", element);
			}
		}
	}
}

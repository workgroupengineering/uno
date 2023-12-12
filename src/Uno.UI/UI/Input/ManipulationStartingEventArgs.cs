using System;
using System.Linq;
using Windows.Devices.Input;

#if HAS_UNO_WINUI && IS_UNO_UI_PROJECT
namespace Microsoft.UI.Input
#else
namespace Windows.UI.Input
#endif
{
	internal partial class ManipulationStartingEventArgs
	{
		// Be aware that this class is not part of the UWP contract

		internal ManipulationStartingEventArgs(PointerIdentifier pointer, GestureSettings settings)
		{
			Pointer = pointer;
			Settings = settings;
		}

		/// <summary>
		/// Gets identifier of the first pointer for which a manipulation is considered
		/// </summary>
		internal PointerIdentifier Pointer { get; }

		public GestureSettings Settings { get; set; }
	}
}

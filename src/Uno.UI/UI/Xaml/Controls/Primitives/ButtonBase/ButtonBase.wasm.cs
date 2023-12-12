﻿using Uno;
using Uno.Client;
using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno.UI;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Disposables;
using System.Text;
using System.Threading;
using Windows.System;
using Uno.Foundation;

using View = Microsoft.UI.Xaml.UIElement;

namespace Microsoft.UI.Xaml.Controls.Primitives
{
	public partial class ButtonBase : ContentControl
	{
		partial void PartialInitializeProperties()
		{
			// We need to ensure the "Tapped" event is registered
			// for the "Click" event to work properly
			Tapped += (snd, evt) => { };
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.UI.Xaml.Documents.TextFormatting
{
	internal readonly record struct GlyphInfo(ushort GlyphId, int Cluster, float AdvanceX, float OffsetX, float OffsetY);
}

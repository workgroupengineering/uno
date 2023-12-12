﻿
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Uno.UI.Samples.UITests.Helpers;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Controls;

namespace Uno.UI.Samples.Content.UITests.GridView
{
	internal class GridView_ComplexViewModel : ViewModelBase
	{
		public GridView_ComplexViewModel(CoreDispatcher coreDispatcher) : base(coreDispatcher)
		{
			SampleItems = GetSampleItems(coreDispatcher);
		}

		public object SampleItems { get; }

		private GridView_ComplexItemViewModel[] GetSampleItems(CoreDispatcher coreDispatcher)
		{
			return Enumerable
				.Range(0, 100)
				.Select(i => new GridView_ComplexItemViewModel(coreDispatcher)
				{
					Client_FirstName = $"FirstName {i}",
					Client_LastName = $"LastName {i}",
					Client_Title = $"Title {i}",
					PatientName = $"PatientName {i}",
					StartTime = $"Time {i}",
					HasWellnessMembership = (i % 2) == 0,
					Reasons = Enumerable
						.Range(0, i % 3)
						.Select(r => $"Reason {r}")
						.ToArray(),
					AppointmentTypes = Enumerable
						.Range(0, i % 3)
						.Select(r => $"Appointment {r}")
						.ToArray()
				})
				.ToArray();
		}
	}

	internal class GridView_ComplexItemViewModel : ViewModelBase
	{
		public GridView_ComplexItemViewModel(CoreDispatcher coreDispatcher) : base(coreDispatcher)
		{
		}

		public string PatientImage { get; set; }
		public string StartTime { get; set; }
		public string PatientName { get; set; }
		public bool HasWellnessMembership { get; set; }
		public string Client_Title { get; set; }

		public string Client_FirstName { get; set; }

		public string Client_LastName { get; set; }

		public string[] AppointmentTypes { get; set; }

		public string[] Reasons { get; set; }
	}

}

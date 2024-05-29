using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReport
	{
		private CarIsNotAtLineReport()
		{

		}

		public static CarIsNotAtLineReport Generate(DateTime date, int countDays, IEnumerable<int> includedEvents, IEnumerable<int> excludedEvents)
		{
			return new CarIsNotAtLineReport();
		}
	}
}

using System;
using System.Collections.Generic;

namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public class CarIsNotAtLineReport
	{
		private CarIsNotAtLineReport()
		{

		}

		public static CarIsNotAtLineReport Generate(DateTime date, int countDays, IEnumerable<string> includedElements, IEnumerable<string> excludedElements)
		{
			return new CarIsNotAtLineReport();
		}
	}
}

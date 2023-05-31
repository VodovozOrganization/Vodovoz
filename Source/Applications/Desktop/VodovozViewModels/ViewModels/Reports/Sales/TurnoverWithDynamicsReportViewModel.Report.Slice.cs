using System;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public class Slice
		{
			public Slice(string title, DateTime startDate, DateTime endDate)
			{
				Title = title;
				StartDate = startDate;
				EndDate = endDate;
			}

			public string Title { get; }
			public DateTime StartDate { get; }
			public DateTime EndDate { get; }
		}
	}
}

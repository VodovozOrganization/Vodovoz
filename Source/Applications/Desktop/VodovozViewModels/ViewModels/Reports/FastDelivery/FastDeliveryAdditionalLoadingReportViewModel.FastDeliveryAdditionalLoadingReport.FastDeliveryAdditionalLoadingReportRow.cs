using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		public partial class FastDeliveryAdditionalLoadingReport
		{
			public class FastDeliveryAdditionalLoadingReportRow
			{
				public DateTime RouteListDate { get; set; }
				public int RouteListId { get; set; }
				public int OwnOrdersCount { get; set; }
				public string AdditionaLoadingNomenclature { get; set; }
				public decimal AdditionaLoadingAmount { get; set; }
				public string RouteListDateString => RouteListDate.ToShortDateString();
			}
		}
	}
}

using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		public partial class FastDeliveryRemainingBottlesReport
		{
			public class Row
			{
				public string Shift { get; set; }

				public DateTime CreationDate { get; set; }

				public int RouteListId { get; set; }

				public string DriverFullName { get; set; }

				public decimal BottlesLoadedAdditionallyCount { get; set; }
				public decimal BottlesLoadedPlanCount { get; set; }
				public decimal BottlesLoadedFromOtherDriversCount { get; set; }

				public decimal BottlesShippedFastDeliveryCount { get; set; }
				public decimal BottlesShippedPlanCount { get; set; }
				public decimal BottlesTransferedToOtherDriversCount { get; set; }

				public decimal RemainingBottlesCount { get; set; }

				public int AddressesCount { get; set; }
			}
		}
	}
}

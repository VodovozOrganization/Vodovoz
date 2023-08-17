using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		public partial class FastDeliveryRemainingBottlesReport
		{
			public class Row
			{
				public string Route { get; set; }

				public DateTime CreationDate { get; set; }

				public int RouteListId { get; set; }

				public string DriverFullName { get; set; }

				public decimal BottlesLoadedCount { get; set; }

				public decimal BottlesShippedCount { get; set; }

				public decimal RemainingBottlesCount { get; set; }

				public int AddressesCount { get; set; }
			}
		}
	}
}

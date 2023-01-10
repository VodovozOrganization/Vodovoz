using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryFailsReport
	{
		public class FastDeliveryFailsReportRow
		{
			public int OrderId { get; set; }
			public DateTime VerificationDate { get; set; }
			public string District { get; set; }
			public bool? IsValidLastCoordinateTime { get; set; }
			public bool? IsValidDistanceByLine { get; set; }
			public bool? IsValidUnclosedFastDeliveries { get; set; }
			public bool? IsValidIsGoodsEnough { get; set; }
			public bool? IsValid { get; set; }
			public int Id { get; set; }
		}
	}
}

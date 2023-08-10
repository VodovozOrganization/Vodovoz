using System;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public partial class TurnoverWithDynamicsReport
		{
			public class TurnoverWithDynamicsReportLastSaleDetails
			{
				public DateTime LastSaleDate { get; set; }

				public double DaysFromLastShipment { get; set; }

				public decimal WarhouseResidue { get; set; }
			}
		}
	}
}

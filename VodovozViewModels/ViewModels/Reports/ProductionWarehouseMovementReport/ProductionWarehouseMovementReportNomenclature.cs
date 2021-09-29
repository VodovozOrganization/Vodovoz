using System;

namespace Vodovoz.ViewModels.Reports
{
	public partial class ProductionWarehouseMovementReport
	{
		public class ProductionWarehouseMovementReportNomenclature
		{
			public int Id { get; set; }
			public string NomenclatureName { get; set; }
			public decimal Amount { get; set; }
			public decimal PurchasePrice { get; set; }
			public decimal Sum { get; set; }
			public DateTime? PurchasePriceStartDate { get; set; }
			public DateTime? PurchasePriceEndDate { get; set; }
			public bool IsTotal { get; set; }

			public string DateRange
			{
				get
				{
					if(PurchasePriceStartDate == null)
					{
						return IsTotal ? "" : "Без цены в выбранный период";
					}
					else
					{
						return $"{PurchasePriceStartDate?.ToShortDateString()}  - {PurchasePriceEndDate?.ToShortDateString()}";
					}
				}
			}
		}
	}
}

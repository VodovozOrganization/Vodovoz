using System;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class TurnoverOfWarehouseBalancesReport
	{
		private class SalesGenerationNode
		{
			public DateTime? SaleDate { get; set; }
			public int WarehouseId { get; set; }
			public int NomenclatureId { get; set; }
			public string NomenclatureName { get; set; }
			public decimal? ActualCount { get; set; }
		}
	}
}

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class TurnoverOfWarehouseBalancesReport
	{
		private class WarehouseResidueNode
		{
			public int NomenclatureId { get; set; }
			public int WarehouseId { get; set; }
			public decimal StockAmount { get; set; }
		}
	}
}

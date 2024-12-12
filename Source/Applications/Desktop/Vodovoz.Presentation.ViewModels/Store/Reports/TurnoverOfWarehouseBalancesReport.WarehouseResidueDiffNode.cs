namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	public partial class TurnoverOfWarehouseBalancesReport
	{
		private class WarehouseResidueDiffNode
		{
			public int NomenclatureId { get; set; }
			public int WarehouseId { get; set; }
			public decimal StockAmountDiff { get; set; }
		}
	}
}

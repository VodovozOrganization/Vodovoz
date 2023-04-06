namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class ResidueDataNode
		{
			public int NomenclatureId { get; set; }

			public int ProductGroupId { get; set; }

			public int WarehouseId { get; set; }

			public decimal Residue { get; set; }
		}
	}
}

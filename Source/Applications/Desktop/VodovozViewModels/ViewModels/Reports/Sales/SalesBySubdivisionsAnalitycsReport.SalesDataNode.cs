namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		public class SalesDataNode
		{
			public int NomenclatureId { get; set; }

			public int ProductGroupId { get; set; }

			public int SubdivisionId { get; set; }

			public decimal Amount { get; set; }

			public decimal Price { get; set; }
		}
	}
}

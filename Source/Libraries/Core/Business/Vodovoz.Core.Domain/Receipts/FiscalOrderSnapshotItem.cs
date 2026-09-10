namespace Vodovoz.Core.Domain.Receipts
{
	public class FiscalOrderSnapshotItem
	{
		public int? NomenclatureId { get; set; }

		public string Name { get; set; }

		public decimal Quantity { get; set; }

		public decimal Price { get; set; }

		public decimal DiscountSum { get; set; }

		public decimal Sum { get; set; }
	}
}

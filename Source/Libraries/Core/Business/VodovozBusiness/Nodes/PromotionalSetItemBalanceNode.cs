namespace Vodovoz.Nodes
{
	public class PromotionalSetItemBalanceNode
	{
		public int PromotionalSetId { get; set; }
		public int NomenclatureId { get; set; }
		public int Count { get; set; }
		public decimal Discount { get; set; }
		public bool IsDiscountMoney { get; set; }
		public decimal Stock { get; set; }
	}
}

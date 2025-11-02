namespace Vodovoz.Nodes
{
	public class OnlineOrderItemNode
	{
		public int Position { get; set; }
		public string NomenclatureName { get; set; }
		public decimal OnlineOrderItemCount { get; set; }
		public decimal CountFromPromoSet { get; set; }
		public decimal OnlineOrderItemPrice { get; set; }
		public decimal NomenclaturePrice { get; set; }
		public decimal OnlineOrderItemSum { get; set; }
		public decimal OnlineOrderItemDiscount { get; set; }
		public decimal DiscountFromPromoSet { get; set; }
		public bool OnlineOrderItemDiscountType { get; set; }
		public bool DiscountTypeFromPromoSet { get; set; }
	}
}

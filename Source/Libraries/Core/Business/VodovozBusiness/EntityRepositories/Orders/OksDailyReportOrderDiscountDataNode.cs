namespace Vodovoz.EntityRepositories.Orders
{
	public class OksDailyReportOrderDiscountDataNode
	{
		public int OrderId { get; set; }
		public string ClientName { get; set; }
		public string NomenclatureName { get; set; }
		public decimal OrderItemPrice { get; set; }
		public decimal Amount { get; set; }
		public decimal Discount { get; set; }
		public decimal DiscountMoney { get; set; }
		public int DiscountResonId { get; set; }
		public string DiscountReasonName { get; set; }
	}
}

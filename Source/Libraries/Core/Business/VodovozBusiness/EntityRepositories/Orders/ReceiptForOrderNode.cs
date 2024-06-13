namespace Vodovoz.Infrastructure.Persistance.Orders
{
	public class ReceiptForOrderNode
	{
		public int OrderId { get; set; }
		public int TrueMarkCashReceiptOrderId { get; set; }
		public int? ReceiptId { get; set; }
		public bool? WasSent { get; set; }
	}
}

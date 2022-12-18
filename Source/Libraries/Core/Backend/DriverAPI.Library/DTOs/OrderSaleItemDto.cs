namespace DriverAPI.Library.DTOs
{
	public class OrderSaleItemDto
	{
		public int OrderSaleItemId { get; set; }
		public string Name { get; set; }
		public decimal Quantity { get; set; }
		public decimal TotalOrderItemPrice { get; set; }
		public bool NeedScanCode { get; set; }
	}
}

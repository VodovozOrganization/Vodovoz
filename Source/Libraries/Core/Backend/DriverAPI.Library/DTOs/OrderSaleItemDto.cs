namespace DriverAPI.Library.DTOs
{
	public class OrderSaleItemDto
	{
		public int OrderSaleItemId { get; set; }
		public string Name { get; set; }
		public decimal Quantity { get; set; }
		public decimal OrderItemPrice { get; set; }
		public decimal TotalOrderItemPrice { get; set; }
		public bool IsBottleStock { get; set; }
		public bool IsDiscountInMoney { get; set; }
		public string DiscountReason { get; set; }
		public decimal Discount { get;set; }
	}
}
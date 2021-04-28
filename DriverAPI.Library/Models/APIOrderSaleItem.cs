namespace DriverAPI.Library.Models
{
    public class APIOrderSaleItem
    {
        public int OrderSaleItemId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalOrderItemPrice { get; set; }
    }
}
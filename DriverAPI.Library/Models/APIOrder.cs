using System.Collections.Generic;

namespace DriverAPI.Library.Models
{
    public class APIOrder
    {
        public int OrderId { get; set; }
        public APISmsPaymentStatus? SmsPaymentStatus { get; set; }
        public string DeliveryTime { get; set; }
        public int FullBottleCount { get; set; }
        public string Counterparty { get; set; }
        public IEnumerable<string> CounterpartyPhoneNumbers { get; set; }
        public APIPaymentType PaymentType { get; set; }
        public APIAddress Address { get; set; }
        public string OrderComment { get; set; }
        public decimal OrderSum { get; set; }
        public IEnumerable<APIOrderSaleItem> OrderSaleItems { get; set; }
        public IEnumerable<APIOrderDeliveryItem> OrderDeliveryItems { get; set; }
        public IEnumerable<APIOrderReceptionItem> OrderReceptionItems { get; set; }
        public APIOrderAdditionalInfo OrderAdditionalInfo { get; set; }
    }
}
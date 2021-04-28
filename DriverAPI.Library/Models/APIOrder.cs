using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace DriverAPI.Library.Models
{
    public class APIOrder
    {
        public int OrderId { get; set; }
        public string SmsPaymentStatus => smsPaymentStatus;
        public string smsPaymentStatus;
        [XmlIgnore]
        [JsonIgnore]
        public APISmsPaymentStatus? SmsPaymentStatusEnum
        {
            get => smsPaymentStatusEnum;
            set {
                smsPaymentStatus = value.ToString();
                smsPaymentStatusEnum = value;
            }
        }
        private APISmsPaymentStatus? smsPaymentStatusEnum;
        public string DeliveryTime { get; set; }
        public int FullBottleCount { get; set; }
        public string Counterparty { get; set; }
        public IEnumerable<string> CounterpartyPhoneNumbers { get; set; }
        public string PaymentType => paymentType;
        private string paymentType;
        [XmlIgnore]
        [JsonIgnore]
        public APIPaymentType PaymentTypeEnum
        {
            get => paymentTypeEnum;
            set
            {
                paymentType = value.ToString();
                paymentTypeEnum = value;
            }
        }
        private APIPaymentType paymentTypeEnum;
        public APIAddress Address { get; set; }
        public string OrderComment { get; set; }
        public decimal OrderSum { get; set; }
        public IEnumerable<APIOrderSaleItem> OrderSaleItems { get; set; }
        public IEnumerable<APIOrderDeliveryItem> OrderDeliveryItems { get; set; }
        public IEnumerable<APIOrderReceptionItem> OrderReceptionItems { get; set; }
        public APIOrderAdditionalInfo OrderAdditionalInfo { get; set; }
    }
}
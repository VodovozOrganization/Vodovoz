using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Orders
{
    public class ExtendedReceiptForOrderNode
    {
        public int OrderId { get; set; }
        public int? ReceiptId { get; set; }
        public bool? WasSent { get; set; }
        public decimal? OrderSum { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public PaymentType PaymentType { get; set; }
    }
}

using System;
using Vodovoz.Domain.FastPayments;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsApi.Contracts
{
	public class FastPaymentDTO
	{
		public int OrderId { get; set; }
		public DateTime CreationDate { get; set; }
		public string Ticket { get; set; }
		public string QRPngBase64 { get; set; }
		public int ExternalId { get; set; }
		public Guid FastPaymentGuid { get; set; }
		public FastPaymentPayType FastPaymentPayType { get; set; }
		public Organization Organization { get; set; }
		public string PhoneNumber { get; set; }
		public PaymentFrom PaymentByCardFrom { get; set; }
	}
}

using System;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts
{
	[XmlRoot(ElementName = "order_info")]
	public class PaidOrderInfoDTO
	{
		[XmlElement(ElementName = "id")]
		public long Id { get; set; }

		[XmlElement(ElementName = "ticket")]
		public string Ticket { get; set; }

		[XmlElement(ElementName = "method_name")]
		public string MethodName { get; set; }

		[XmlElement(ElementName = "auth_code")]
		public string AuthCode { get; set; }

		[XmlElement(ElementName = "status_code")]
		public FastPaymentDTOStatus Status { get; set; }

		[XmlElement(ElementName = "status_desc")]
		public string StatusDescription { get; set; }

		[XmlElement(ElementName = "status_date")]
		public DateTime StatusDate { get; set; }

		[XmlElement(ElementName = "shop_id")]
		public long ShopId { get; set; }

		[XmlElement(ElementName = "order_number")]
		public string OrderNumber { get; set; }

		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }

		[XmlElement(ElementName = "refund_amount")]
		public int RefundAmount { get; set; }

		[XmlElement(ElementName = "card_num")]
		public string CardNum { get; set; }

		[XmlElement(ElementName = "exp_mm")]
		public string ExpMM { get; set; }

		[XmlElement(ElementName = "exp_yy")]
		public string ExpYY { get; set; }

		[XmlElement(ElementName = "signature")]
		public string Signature { get; set; }
	}
}

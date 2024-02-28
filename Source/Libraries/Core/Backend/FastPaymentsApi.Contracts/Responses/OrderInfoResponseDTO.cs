using FastPaymentsApi.Contracts;
using System;
using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Responses
{
	[XmlRoot(ElementName = "order_info")]
	public class OrderInfoResponseDTO : IAvangardResponseDetails
	{
		[XmlElement(ElementName = "id")]
		public long Id { get; set; }

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

		[XmlElement(ElementName = "response_code")]
		public int ResponseCode { get; set; }

		[XmlElement(ElementName = "response_message")]
		public string ResponseMessage { get; set; }

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

		[XmlElement(ElementName = "rrn")]
		public string Rrn { get; set; }

		[XmlElement(ElementName = "txn")]
		public string Transaction { get; set; }

		[XmlElement(ElementName = "is_auth_only")]
		public string IsAuthOnly { get; set; }

		[XmlElement(ElementName = "last_error")]
		public string LastError { get; set; }
	}
}

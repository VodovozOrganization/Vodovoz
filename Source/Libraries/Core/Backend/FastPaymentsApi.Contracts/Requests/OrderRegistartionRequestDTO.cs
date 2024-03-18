using System.Xml.Serialization;

namespace FastPaymentsApi.Contracts.Requests
{
	[XmlRoot(ElementName = "new_order")]
	public class OrderRegistrationRequestDTO : AbstractShopRequestDTO
	{
		[XmlElement(ElementName = "amount")]
		public int Amount { get; set; }

		[XmlElement(ElementName = "is_auth_only")]
		public int is_auth_only { get; set; }

		[XmlElement(ElementName = "back_url")]
		public string BackUrl { get; set; }

		[XmlElement(ElementName = "back_url_fail")]
		public string BackUrlFail { get; set; }

		[XmlElement(ElementName = "back_url_ok")]
		public string BackUrlOk { get; set; }

		[XmlElement(ElementName = "card_num")]
		public string CardNum { get; set; }

		[XmlElement(ElementName = "client_address")]
		public string ClientAddress { get; set; }

		[XmlElement(ElementName = "client_email")]
		public string ClientEmail { get; set; }

		[XmlElement(ElementName = "client_ip")]
		public string ClientIp { get; set; }

		[XmlElement(ElementName = "client_name")]
		public string ClientName { get; set; }

		[XmlElement(ElementName = "client_phone")]
		public string ClientPhone { get; set; }

		[XmlElement(ElementName = "exp_month")]
		public string ExpMonth { get; set; }

		[XmlElement(ElementName = "exp_year")]
		public string ExpYear { get; set; }

		[XmlElement(ElementName = "is_qr")]
		public int IsQR { get; set; }

		[XmlElement(ElementName = "language")]
		public string Language { get; set; }

		[XmlElement(ElementName = "order_description")]
		public string OrderDescription { get; set; }

		[XmlElement(ElementName = "order_number")]
		public string OrderNumber { get; set; }

		[XmlElement(ElementName = "qr_ttl")]
		public int? QRTtl { get; set; }

		[XmlElement(ElementName = "return_qr_image")]
		public int ReturnQRImage { get; set; }

		[XmlElement(ElementName = "return_qr_url")]
		public int ReturnQRUrl { get; set; }

		[XmlElement(ElementName = "txn")]
		public string Transaction { get; set; }

		[XmlElement(ElementName = "signature")]
		public string Signature { get; set; }

		public bool ShouldSerializeQRTtl() => QRTtl.HasValue;
	}
}

using System.Xml.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Responses;

[XmlRoot(ElementName = "cancel_order_response")]
public class CancelPaymentResponseDTO
{
	[XmlElement(ElementName = "response_code")]
	public int ResponseCode { get; set; }

	[XmlElement(ElementName = "response_message")]
	public string ResponseMessage { get; set; }
}

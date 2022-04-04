using System.Xml.Serialization;

namespace FastPaymentsAPI.Library.DTO_s.Requests;

[XmlRoot(ElementName = "cancel_order")]
public class CancelPaymentRequestDTO : AbstractTicketRequestDTO
{
	
}

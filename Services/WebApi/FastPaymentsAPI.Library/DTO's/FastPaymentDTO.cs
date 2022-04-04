using System;

namespace FastPaymentsAPI.Library.DTO_s;

public class FastPaymentDTO
{
	public int OrderId { get; set; }
	public DateTime CreationDate { get; set; }
	public string Ticket { get; set; }
	public string QRPngBase64 { get; set; }
	public int ExternalId { get; set; }
}

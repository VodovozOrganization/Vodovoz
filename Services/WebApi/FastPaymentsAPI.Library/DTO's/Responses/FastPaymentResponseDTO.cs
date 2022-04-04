using Vodovoz.Domain.FastPayments;

namespace FastPaymentsAPI.Library.DTO_s.Responses;

public class FastPaymentResponseDTO
{
	public FastPaymentResponseDTO() { }
	public FastPaymentResponseDTO(string errorMessage)
	{
		ErrorMessage = errorMessage;
	}
	
	public string Ticket { get; set; }
	public string PhoneNumber { get; set; }
	public string ErrorMessage { get; set; }
	public FastPaymentStatus? FastPaymentStatus { get; set; }
}

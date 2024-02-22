using Vodovoz.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses
{
	public class QRResponseDTO : IErrorResponse
	{
		public string QRCode { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus FastPaymentStatus { get; set; }
	}
}

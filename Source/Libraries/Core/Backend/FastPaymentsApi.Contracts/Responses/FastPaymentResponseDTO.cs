using System;
using Vodovoz.Domain.FastPayments;

namespace FastPaymentsApi.Contracts.Responses
{
	public class FastPaymentResponseDTO : IErrorResponse
	{
		public FastPaymentResponseDTO()
		{
		}

		public FastPaymentResponseDTO(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}

		public string Ticket { get; set; }
		public Guid FastPaymentGuid { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}
}

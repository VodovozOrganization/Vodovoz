using System;
using Vodovoz.Domain.FastPayments;

namespace Vodovoz.Models
{
	public class FastPaymentResponseDTO
	{
		public string Ticket { get; set; }
		public Guid FastPaymentGuid { get; set; }
		public string ErrorMessage { get; set; }
		public FastPaymentStatus? FastPaymentStatus { get; set; }
	}
}

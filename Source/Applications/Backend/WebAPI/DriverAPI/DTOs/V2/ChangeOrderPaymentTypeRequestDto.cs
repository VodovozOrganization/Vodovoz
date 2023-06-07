using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.DTOs.V2
{
	public class ChangeOrderPaymentTypeRequestDto
	{
		public int OrderId { get; set; }
		public PaymentDtoType NewPaymentType { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}

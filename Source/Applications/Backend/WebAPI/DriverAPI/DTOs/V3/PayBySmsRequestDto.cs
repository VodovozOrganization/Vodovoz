using System;

namespace DriverAPI.DTOs.V3
{
	public class PayBySmsRequestDto
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}

using System;

namespace DriverAPI.DTOs
{
	public class PayBySmsRequestDto
	{
		public int OrderId { get; set; }
		public string PhoneNumber { get; set; }
		public DateTime ActionTime { get; set; }
	}
}

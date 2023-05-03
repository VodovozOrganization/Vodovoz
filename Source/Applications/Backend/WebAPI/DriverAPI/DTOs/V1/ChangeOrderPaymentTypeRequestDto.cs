using DriverAPI.Library.Deprecated.DTOs;
using System;

namespace DriverAPI.DTOs.V1
{
	public class ChangeOrderPaymentTypeRequestDto : IActionTimeTrackable
	{
		public int OrderId { get; set; }
		public Library.Deprecated.DTOs.PaymentDtoType NewPaymentType { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}

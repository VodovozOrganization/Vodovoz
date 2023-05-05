using DriverAPI.Library.DTOs;
using System;

namespace DriverAPI.DTOs
{
	public class PayByQRRequestDTO : IActionTimeTrackable
	{
		public int OrderId { get; set; }
		public int? BottlesByStockActualCount { get; set; }
		public DateTime? ActionTime { get; set; }
		public DateTime? ActionTimeUtc { get; set; }
	}
}

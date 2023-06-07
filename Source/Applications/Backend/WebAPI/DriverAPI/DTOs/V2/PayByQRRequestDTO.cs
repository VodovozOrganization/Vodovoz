using System;

namespace DriverAPI.DTOs.V2
{
	public class PayByQRRequestDTO
	{
		public int OrderId { get; set; }
		public int? BottlesByStockActualCount { get; set; }
		public DateTime ActionTimeUtc { get; set; }
	}
}

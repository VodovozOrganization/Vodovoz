using System;

namespace DriverAPI.DTOs
{
	public class PayByQRRequestDTO
	{
		public int OrderId { get; set; }
		public DateTime ActionTime { get; set; }
		public int? BottlesByStockActualCount { get; set; }
	}
}

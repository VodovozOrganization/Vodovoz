using System;
using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V3
{
	public class PayByQRRequestDTO
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }
		public int? BottlesByStockActualCount { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }
	}
}

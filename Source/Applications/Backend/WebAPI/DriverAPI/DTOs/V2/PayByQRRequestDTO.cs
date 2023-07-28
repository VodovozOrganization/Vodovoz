﻿using System;
using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V2
{
	/// <summary>
	/// Запрос на оплату по QR-коду
	/// </summary>
	public class PayByQRRequestDTO
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Фактическое количество бутылей по акции
		/// </summary>
		public int? BottlesByStockActualCount { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]
		public DateTime ActionTimeUtc { get; set; }
	}
}

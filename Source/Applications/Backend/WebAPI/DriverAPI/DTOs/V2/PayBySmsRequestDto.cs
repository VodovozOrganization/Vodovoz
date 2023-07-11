using System;
using System.ComponentModel.DataAnnotations;

namespace DriverAPI.DTOs.V2
{
	public class PayBySmsRequestDto
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Телефонный номер для отправки смс
		/// </summary>
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Время операции на стороне мобильного приложения водителя
		/// </summary>
		[Required]		
		public DateTime ActionTimeUtc { get; set; }
	}
}

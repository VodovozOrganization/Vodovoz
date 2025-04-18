using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на отправку кодов ЧЗ
	/// </summary>
	public class SendOrderCodesRequest
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Отсканированные коды ЧЗ бутылок для строк заказа
		/// </summary>
		public IEnumerable<OrderItemScannedBottlesDto> ScannedBottles { get; set; }

		/// <summary>
		/// Причина несканирования бутылок
		/// </summary>
		public string UnscannedBottlesReason { get; set; }
	}
}

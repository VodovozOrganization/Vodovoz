using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на добавление кода ЧЗ
	/// </summary>
	public class AddOrderCodeRequest
	{
		/// <summary>
		/// Номер заказа
		/// </summary>
		[Required]
		public int OrderId { get; set; }

		/// <summary>
		/// Номер строки заказа
		/// </summary>
		[Required]
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Отсканированный код ЧЗ
		/// </summary>
		[Required]
		public string Code { get; set; }
	}
}

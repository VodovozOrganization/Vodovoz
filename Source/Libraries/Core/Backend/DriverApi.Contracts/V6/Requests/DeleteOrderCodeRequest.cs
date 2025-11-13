using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на удаление кода ЧЗ
	/// </summary>
	public class DeleteOrderCodeRequest
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
		/// Отсканированный код ЧЗ для удаления
		/// </summary>
		[Required]
		public string DeletedCode { get; set; }
	}
}

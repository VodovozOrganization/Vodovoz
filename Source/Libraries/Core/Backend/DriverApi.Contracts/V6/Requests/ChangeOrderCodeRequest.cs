using System.ComponentModel.DataAnnotations;

namespace DriverApi.Contracts.V6.Requests
{
	/// <summary>
	/// Запрос на изменение кода ЧЗ
	/// </summary>
	public class ChangeOrderCodeRequest
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
		/// Новый отсканированный код ЧЗ
		/// </summary>
		[Required]
		public string OldCode { get; set; }

		/// <summary>
		/// Старый отсканированный код ЧЗ
		/// </summary>
		[Required]
		public string NewCode { get; set; }
	}
}

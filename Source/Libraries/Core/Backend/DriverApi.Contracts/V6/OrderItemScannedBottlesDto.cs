using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// DTO для отсканированных кодов ЧЗ бутылок
	/// </summary>
	public class OrderItemScannedBottlesDto
	{
		/// <summary>
		/// Номер строки заказа
		/// </summary>
		public int OrderSaleItemId { get; set; }

		/// <summary>
		/// Коды ЧЗ бутылок
		/// </summary>
		public IEnumerable<string> BottleCodes { get; set; }
	}
}

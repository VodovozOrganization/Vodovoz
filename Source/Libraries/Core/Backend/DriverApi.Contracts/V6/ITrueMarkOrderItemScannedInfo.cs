using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Интерфейс, представляющий информацию о сканировании товаров ЧЗ строк заказа
	/// </summary>
	public interface ITrueMarkOrderItemScannedInfo
	{
		/// <summary>
		/// Коды ЧЗ бутылок.
		/// </summary>
		IEnumerable<string> BottleCodes { get; set; }

		/// <summary>
		/// Коды ЧЗ дефектных бутылок.
		/// </summary>
		IEnumerable<string> DefectiveBottleCodes { get; set; }

		/// <summary>
		/// Номер строки заказа
		/// </summary>
		int OrderSaleItemId { get; set; }
	}
}

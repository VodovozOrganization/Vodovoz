using System.Collections.Generic;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Интерфейс, представляющий информацию о сканировании товаров ЧЗ заказа
	/// </summary>
	public interface ITrueMarkOrderScannedInfo
	{
		/// <summary>
		/// Получает список отсканированных элементов заказа
		/// </summary>
		IEnumerable<ITrueMarkOrderItemScannedInfo> ScannedItems { get; }

		/// <summary>
		/// Получает причину, по которой коды не были отсканированы
		/// </summary>
		string UnscannedCodesReason { get; }
	}
}

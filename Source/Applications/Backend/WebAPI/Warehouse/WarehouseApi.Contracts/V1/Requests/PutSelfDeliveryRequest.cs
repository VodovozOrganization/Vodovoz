using System.Collections.Generic;

namespace WarehouseApi.Contracts.Requests.V1
{
	/// <summary>
	/// Запрос на создание документа отпуска самовывоза
	/// </summary>
	public class PutSelfDeliveryRequest
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; set; }
		/// <summary>
		/// Идентификатор склада
		/// </summary>
		public int WarehouseId { get; set; }
		/// <summary>
		/// Коды маркировки честного знака, которые нужно добавить
		/// </summary>
		public IEnumerable<string> CodesToAdd { get; set; }
	}
}

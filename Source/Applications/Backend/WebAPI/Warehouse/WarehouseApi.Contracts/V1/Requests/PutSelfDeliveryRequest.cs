using System.Collections.Generic;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// Запрос на создание документа отпуска самовывоза
	/// </summary>
	public class PutSelfDeliveryRequest
	{
		/// <summary>
		/// Идентификатор заказа
		/// </summary>
		public int OrderId { get; internal set; }
		/// <summary>
		/// Идентификатор склада
		/// </summary>
		public int WarehouseId { get; internal set; }
		/// <summary>
		/// Коды маркировки честного знака, которые нужно добавить
		/// </summary>
		public IEnumerable<string> CodesToAdd { get; set; }
		/// <summary>
		/// Требуется ли завершить отгрузку
		/// </summary>
		public bool EndLoad { get; internal set; }
	}
}

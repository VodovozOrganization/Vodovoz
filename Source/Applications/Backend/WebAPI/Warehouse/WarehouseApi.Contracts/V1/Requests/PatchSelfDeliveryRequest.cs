using System.Collections.Generic;

namespace WarehouseApi.Contracts.V1.Requests
{
	/// <summary>
	/// Запрос на изменение документа отпуска самовывоза
	/// </summary>
	public class PatchSelfDeliveryRequest
	{
		/// <summary>
		/// Идентификатор документа отпуска самовывоза
		/// </summary>
		public int SelfDeliveryDocumentId { get; set; }
		/// <summary>
		/// Коды маркировки честного знака, которые нужно удалить
		/// </summary>
		public IEnumerable<string> CodesToDelete { get; set; }
		/// <summary>
		/// Коды маркировки честного знака, которые нужно изменить
		/// </summary>
		public IDictionary<string, string> CodesToChange { get; set; }
		/// <summary>
		/// Коды маркировки честного знака, которые нужно добавить
		/// </summary>
		public IEnumerable<string> CodesToAdd { get; set; }
		/// <summary>
		/// Требуется ли завершить отгрузку
		/// </summary>
		public bool EndLoad { get; set; }
	}
}

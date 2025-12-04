using System.Collections.Generic;

namespace WarehouseApi.Contracts.V1.Dto
{
	/// <summary>
	/// Заказ в документе погрузки авто
	/// </summary>
	public class OrderDto
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Id документа погрузки авто
		/// </summary>
		public int CarLoadDocument { get; set; }

		/// <summary>
		/// Состояние процесса погрузки номенклатур по заказу
		/// </summary>
		public LoadOperationStateEnumDto State { get; set; }

		/// <summary>
		/// Строки заказа
		/// </summary>
		public IEnumerable<OrderItemDto> Items { get; set; }
	}
}

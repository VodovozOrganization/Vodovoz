using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Contracts.Responses.V1
{
	/// <summary>
	/// Ответ на запрос получения информации о заказе самовывоза
	/// </summary>
	public class GetSelfDeliveryResponse
	{
		/// <summary>
		/// Данные по заказу
		/// </summary>
		public OrderDto Order { get; set; }

		/// <summary>
		/// Существует ли самовывоз
		/// </summary>
		public int? SelfDeliveryDocumentId { get; set; }
	}
}

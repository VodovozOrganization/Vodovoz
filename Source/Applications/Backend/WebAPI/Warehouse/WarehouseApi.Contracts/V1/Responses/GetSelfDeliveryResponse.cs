using WarehouseApi.Contracts.V1.Dto;

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
		/// Номер документа отпуска самовывоза
		/// </summary>
		public int? SelfDeliveryDocumentId { get; set; }
	}
}

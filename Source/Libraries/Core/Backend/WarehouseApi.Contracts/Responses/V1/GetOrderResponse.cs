using WarehouseApi.Contracts.Dto.V1;

namespace WarehouseApi.Contracts.Responses.V1
{
	/// <summary>
	/// DTO ответа на запрос получения информации о заказе
	/// </summary>
	public class GetOrderResponse : WarehouseApiResponseBase
	{
		/// <summary>
		/// Данные по заказу
		/// </summary>
		public OrderDto Order { get; set; }
	}
}

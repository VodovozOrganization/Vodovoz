using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Данные для получения детализированной информации о заказе
	/// </summary>
	public class GetDetailedOrderInfoDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Номер заказа в ДВ
		/// </summary>
		public int? OrderId { get; set; }
		/// <summary>
		/// Номер онлайн заказа в ДВ
		/// </summary>
		public int? OnlineOrderId { get; set; }
	}
}

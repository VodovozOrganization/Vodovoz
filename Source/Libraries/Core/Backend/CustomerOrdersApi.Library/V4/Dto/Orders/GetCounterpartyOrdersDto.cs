using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Базовая информация для получения заказов клиента
	/// </summary>
	public class GetCounterpartyOrdersDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }

		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int CounterpartyErpId { get; set; }

		/// <summary>
		/// Номер страницы
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// Количество заказов для отображения на странице
		/// </summary>
		public int OrdersCountOnPage { get; set; }
	}
}

using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Информация для получения списка заказов
	/// </summary>
	public class GetOrdersDto
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

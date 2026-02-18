using System;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
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
		
		/// <summary>
		/// Статус заказа(фильтр)
		/// </summary>
		public ExternalOrderStatus? OrderStatus { get; set; }
		
		/// <summary>
		/// Начальная дата доставки(фильтр)
		/// </summary>
		public DateTime? StartDateTime { get; set; }
		
		/// <summary>
		/// Окончательная дата доставки(фильтр)
		/// </summary>
		public DateTime? EndDateTime { get; set; }
		
		/// <summary>
		/// Массив Id точек доставки(фильтр)
		/// </summary>
		public int[] DeliveryPointsIds { get; set; }
	}
}

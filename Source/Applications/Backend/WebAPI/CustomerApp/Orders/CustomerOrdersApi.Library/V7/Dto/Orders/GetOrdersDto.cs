using System;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Информация для получения списка заказов
	/// </summary>
	public class GetOrdersDto : GetCounterpartyOrdersDto
	{
		/// <summary>
		/// Контрольная сумма запроса
		/// </summary>
		public string Signature { get; set; }

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

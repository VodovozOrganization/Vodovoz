using System;
using Vodovoz.Domain.Orders;
using Vodovoz.RobotMia.Contracts.Responses.V1;

namespace Vodovoz.RobotMia.Api.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="Order"/>
	/// </summary>
	public static class OrderExtensions
	{
		/// <summary>
		/// Маппинг заказа в <see cref="LastOrderResponse"/>
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static LastOrderResponse MapToLastOrderResponseV1(this Order order)
		{
			return new LastOrderResponse
			{
				id = order.Id,
				DeliveryDate = order.DeliveryDate ?? throw new ArgumentException("В заказе отсутствует дата доставки", nameof(order)),
				DeliveryPointId = order.DeliveryPoint.Id,
				OrderItems = order.OrderItems.MapToOrderSaleItemsDtoV1()
			};
		}
	}
}

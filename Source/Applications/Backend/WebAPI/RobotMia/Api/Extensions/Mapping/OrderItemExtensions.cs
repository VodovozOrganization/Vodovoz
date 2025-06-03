using RobotMiaApi.Contracts.Responses.V1;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Orders;

namespace RobotMiaApi.Extensions.Mapping
{
	/// <summary>
	/// Расширение функционала <see cref="OrderItem"/>
	/// </summary>
	public static class OrderItemExtensions
	{
		/// <summary>
		/// Маппинг строки заказа в <see cref="OrderSaleItemDto"/>
		/// </summary>
		/// <param name="orderItem"></param>
		/// <returns></returns>
		public static OrderSaleItemDto MapToOrderSaleItemsDtoV1(this OrderItem orderItem)
			=> new OrderSaleItemDto
			{
				NomenclatureId = orderItem.Nomenclature.Id,
				Name = orderItem.Nomenclature.Name,
				Count = orderItem.ActualCount ?? orderItem.Count,
				ActualPrice = orderItem.ActualSum,
				PromoSetId = orderItem.PromoSet?.Id
			};

		/// <summary>
		/// Маппинг строк заказа в <see cref="OrderSaleItemDto"/>
		/// </summary>
		/// <param name="orderItems"></param>
		/// <returns></returns>
		public static IEnumerable<OrderSaleItemDto> MapToOrderSaleItemsDtoV1(this IEnumerable<OrderItem> orderItems)
			=> orderItems.Select(MapToOrderSaleItemsDtoV1);
	}
}

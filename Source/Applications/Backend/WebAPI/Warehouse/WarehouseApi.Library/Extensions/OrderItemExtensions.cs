using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Library.Extensions
{
	internal static class OrderItemExtensions
	{
		public static OrderItemDto ToApiDtoV1(this OrderItem orderItem, Nomenclature nomenclature)
		{
			if(orderItem is null)
			{
				throw new ArgumentNullException(nameof(orderItem));
			}

			return new OrderItemDto
			{
				NomenclatureId = nomenclature.Id,
				Name = nomenclature.Name,
				Gtin = nomenclature.Gtins.Select(x => x.GtinNumber),
				GroupGtins = nomenclature.GroupGtins
					.Select(gg => new GroupGtinDto
					{
						Gtin = gg.GtinNumber,
						Count = gg.CodesCount
					}),
				Quantity = (int)orderItem.ActualCount,
			};
		}

		public static IEnumerable<OrderItemDto> ToApiDtoV1(this IEnumerable<OrderItem> orderItems, IEnumerable<Nomenclature> nomenclatures)
		{
			if(orderItems is null)
			{
				throw new ArgumentNullException(nameof(orderItems));
			}

			return orderItems
				.Select(x => x.ToApiDtoV1(nomenclatures
					.FirstOrDefault(n => n.Id == x.Nomenclature.Id)));
		}
	}
}

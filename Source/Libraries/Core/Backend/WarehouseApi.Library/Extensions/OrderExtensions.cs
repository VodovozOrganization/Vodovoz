using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using WarehouseApi.Contracts.Dto;

namespace WarehouseApi.Library.Extensions
{
	internal static class OrderExtensions
	{
		public static OrderDto ToApiDtoV1(this Order order, IEnumerable<Nomenclature> nomenclatures)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			return new OrderDto
			{
				Id = order.Id,
				DocNumber = order.Id,
				State = LoadOperationStateEnumDto.NotStarted,
				Items = order.OrderItems.ToApiDto(nomenclatures)
			};
		}
	}
}

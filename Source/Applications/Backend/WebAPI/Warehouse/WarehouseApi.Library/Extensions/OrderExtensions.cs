using System;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using WarehouseApi.Contracts.V1.Dto;

namespace WarehouseApi.Library.Extensions
{
	public static class OrderExtensions
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
				State = LoadOperationStateEnumDto.NotStarted,
				Items = order.OrderItems.ToApiDtoV1(nomenclatures)
			};
		}
	}
}

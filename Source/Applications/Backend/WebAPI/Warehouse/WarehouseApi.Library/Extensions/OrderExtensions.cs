using System;
using System.Collections.Generic;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using WarehouseApi.Contracts.V1.Dto;
using WarehouseApi.Contracts.V1.Responses;

namespace WarehouseApi.Library.Extensions
{
	public static class OrderExtensions
	{
		public static OrderDto ToApiDtoV1(this Order order, IEnumerable<Nomenclature> nomenclatures, SelfDeliveryDocument selfDeliveryDocument)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			return new OrderDto
			{
				Id = order.Id,
				State = LoadOperationStateEnumDto.NotStarted,
				Items = order.OrderItems.ToApiDtoV1(nomenclatures, selfDeliveryDocument)
			};
		}

		public static GetSelfDeliveryOrderResponse ToGetSelfDeliveryOrderResponseDto(this Order order)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			return new GetSelfDeliveryOrderResponse
			{
				OrderId = order.Id,
				Client = order.Client?.FullName ?? string.Empty,
				Sum = order.OrderSum
			};
		}
	}
}

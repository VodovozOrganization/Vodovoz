using System;
using TaxcomEdo.Contracts.Orders;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Converters
{
	public class OrderItemConverter : IOrderItemConverter
	{
		private readonly INomenclatureConverter _nomenclatureConverter;

		public OrderItemConverter(INomenclatureConverter nomenclatureConverter)
		{
			_nomenclatureConverter = nomenclatureConverter ?? throw new ArgumentNullException(nameof(nomenclatureConverter));
		}
		
		public OrderItemInfoForEdo ConvertOrderItemToOrderItemInfoForEdo(OrderItem orderItem)
		{
			var nomenclatureInfo = _nomenclatureConverter.ConvertNomenclatureToNomenclatureInfoForEdo(orderItem.Nomenclature);
			
			var orderItemInfo = new OrderItemInfoForEdo
			{
				Id = orderItem.Id,
				OrderId = orderItem.Order.Id,
				Count = orderItem.Count,
				Price = orderItem.Price,
				DiscountMoney = orderItem.DiscountMoney,
				ActualCount = orderItem.ActualCount,
				IncludeNDS = orderItem.IncludeNDS,
				ValueAddedTax = orderItem.ValueAddedTax,
				NomenclatureInfoForEdo = nomenclatureInfo
			};

			return orderItemInfo;
		}
	}
}

using System;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class SaleItemFactory : ISaleItemFactory
	{
		private readonly IOnlineOrderTemplateSaleItemFactory _templateSaleItemFactory;
		private readonly IOrderSaleItemFactory _orderSaleItemFactory;

		public SaleItemFactory(
			IOnlineOrderTemplateSaleItemFactory templateSaleItemFactory,
			IOrderSaleItemFactory orderSaleItemFactory
			)
		{
			_templateSaleItemFactory = templateSaleItemFactory ?? throw new ArgumentNullException(nameof(templateSaleItemFactory));
			_orderSaleItemFactory = orderSaleItemFactory ?? throw new ArgumentNullException(nameof(orderSaleItemFactory));
		}
		
		public ISaleItem Create(object source, NewOrderSaleItem newSaleItem)
		{
			switch(source)
			{
				case OnlineOrderTemplate template:
					return _templateSaleItemFactory.Create(source, newSaleItem);
				case Order order:
					return _orderSaleItemFactory.Create(source, newSaleItem);
				default:
					throw new ArgumentException("Неизвестный тип источника продажи", nameof(source));
			}
		}
	}
}

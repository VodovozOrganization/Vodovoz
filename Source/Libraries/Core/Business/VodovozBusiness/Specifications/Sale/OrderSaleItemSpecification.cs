using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Specifications.Sale
{
	public class OrderSaleItemSpecification : ExpressionSpecification<IOrderSaleItem, (SaleItemPriceType PriceType, decimal Price)>
	{
		public OrderSaleItemSpecification(Expression<Func<IOrderSaleItem, (SaleItemPriceType PriceType, decimal Price), bool>> expression) : base(expression)
		{
		}
		
		/// <summary>
		/// Создание спецификации пользовательской цены продаваемой позиции заказа
		/// </summary>
		/// <param name="calculatedPriceData">Рассчитанные данные стоимости</param>
		/// <returns></returns>
		public static OrderSaleItemSpecification Create((SaleItemPriceType PriceType, decimal Price) calculatedPriceData)
			=> new OrderSaleItemSpecification((item, priceData) =>
				(priceData.Price != calculatedPriceData.Price
					&& priceData.Price != 0
					&& priceData.PriceType != SaleItemPriceType.Fixed)
				|| item.CopiedFromUndelivery);
	}
}

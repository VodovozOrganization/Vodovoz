using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Specifications.Sale
{
	public class SaleItemSpecification : ExpressionSpecification<ISaleItem, (SaleItemPriceType PriceType, decimal Price)>
	{
		public SaleItemSpecification(Expression<Func<ISaleItem, (SaleItemPriceType PriceType, decimal Price), bool>> expression) : base(expression)
		{
		}
		
		/// <summary>
		/// Создание спецификации пользовательской цены продаваемой позиции
		/// </summary>
		/// <param name="calculatedPriceData">Рассчитанные данные стоимости</param>
		/// <returns></returns>
		public static SaleItemSpecification Create((SaleItemPriceType PriceType, decimal Price) calculatedPriceData)
			=> new SaleItemSpecification((item, priceData) =>
				priceData.Price != calculatedPriceData.Price
				&& priceData.Price != 0
				&& priceData.PriceType != SaleItemPriceType.Fixed);
	}
}

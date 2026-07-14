using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Specifications;

namespace Vodovoz.Specifications
{
	public class OnlineShopGoodsSumFreeDeliverySpecification : ExpressionSpecification<decimal>
	{
		public OnlineShopGoodsSumFreeDeliverySpecification(Expression<Func<decimal, bool>> expression) : base(expression)
		{
		}

		/// <summary>
		/// Создание спецификации для определения бесплатной доставки по сумме товаров ИМ
		/// </summary>
		/// <param name="currentOnlineShopGoodsSum">Текущая сумма товаров интернет магазина</param>
		/// <returns></returns>
		public static OnlineShopGoodsSumFreeDeliverySpecification Create(decimal currentOnlineShopGoodsSum)
			=> new OnlineShopGoodsSumFreeDeliverySpecification(x => currentOnlineShopGoodsSum >= x && x != 0);
	}
}

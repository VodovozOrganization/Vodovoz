using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Specifications
{
	/// <summary>
	/// 
	/// </summary>
	public class CartFreeDeliverySpecification : ExpressionSpecification<IDeliveryRulesRequestContext>
	{
		public CartFreeDeliverySpecification(Expression<Func<IDeliveryRulesRequestContext, bool>> expression) : base(expression)
		{
		}
		
		/// <summary>
		/// Создание спецификации для онлайн заказа из корзины на стадии запроса правил доставки
		/// </summary>
		/// <returns></returns>
		public static CartFreeDeliverySpecification Create()
			=> new CartFreeDeliverySpecification(x =>
				x.IsSelfDelivery
				|| x.CartItems.Any(i => i.ItemType == SaleItemType.Service)
				|| (x.DeliveryPoint != null && x.DeliveryPoint.AlwaysFreeDelivery));
	}
}

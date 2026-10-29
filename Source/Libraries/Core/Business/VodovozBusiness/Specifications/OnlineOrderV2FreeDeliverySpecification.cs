using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders.Delivery;

namespace VodovozBusiness.Specifications.Sale
{
	public class OnlineOrderV2FreeDeliverySpecification : ExpressionSpecification<IOnlineOrderV2FreeDeliveryPrice>
	{
		public OnlineOrderV2FreeDeliverySpecification(Expression<Func<IOnlineOrderV2FreeDeliveryPrice, bool>> expression) : base(expression)
		{
		}

		/// <summary>
		/// Создание спецификации для онлайн заказа
		/// </summary>
		/// <param name="paidDeliveryId">Идентификатор платной доставки</param>
		/// <returns></returns>
		public static OnlineOrderV2FreeDeliverySpecification CreateForOnlineOrder(int paidDeliveryId)
		{
			return new OnlineOrderV2FreeDeliverySpecification(x =>
				x.IsSelfDelivery
				|| x.SaleItems.Any(g => g.Nomenclature.Category == NomenclatureCategory.master)
				|| x.OnlinePromoSets.Any(onlinePromoSet =>
					onlinePromoSet.PromoSet.PromotionalSetItems.Any(item =>
						item.Nomenclature.Category == NomenclatureCategory.master))
				|| (x.DeliveryPoint != null && x.DeliveryPoint.AlwaysFreeDelivery)
				|| !x.SaleItems.Any(g => g.Nomenclature != null && g.Nomenclature.Id != paidDeliveryId));
		}
	}
}

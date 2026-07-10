using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Specifications
{
	/// <summary>
	/// Спецификации для бесплатной доставки
	/// </summary>
	public class FreeDeliverySpecification : ExpressionSpecification<IFreeDeliveryPrice>
	{
		public FreeDeliverySpecification(Expression<Func<IFreeDeliveryPrice, bool>> expression) : base(expression)
		{
		}
		
		/// <summary>
		/// Создание спецификации для онлайн заказа
		/// </summary>
		/// <param name="paidDeliveryId">Идентификатор платной доставки</param>
		/// <returns></returns>
		public static FreeDeliverySpecification CreateForOnlineOrder(int paidDeliveryId)
			=> new FreeDeliverySpecification(x =>
				x.IsSelfDelivery
				//TODO проверить тип сервис
				|| x.Goods.Any(g => g.Nomenclature.Category == NomenclatureCategory.master)
				|| (x.DeliveryPoint != null && x.DeliveryPoint.AlwaysFreeDelivery)
				|| !x.Goods.Any(g => g.Nomenclature != null && g.Nomenclature.Id != paidDeliveryId));
	}
}

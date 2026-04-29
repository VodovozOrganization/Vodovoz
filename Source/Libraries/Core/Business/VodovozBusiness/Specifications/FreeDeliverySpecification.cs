using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Specifications;
using VodovozBusiness.Domain.Orders;

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
				|| (x.DeliveryPoint != null && x.DeliveryPoint.AlwaysFreeDelivery)
				|| !x.Goods.Any(g => g.Nomenclature != null && g.Nomenclature.Id != paidDeliveryId));
	}
}

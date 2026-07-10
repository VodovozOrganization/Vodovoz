using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders.Delivery;

namespace Vodovoz.Specifications
{
	/// <summary>
	/// Спецификации для бесплатной доставки
	/// </summary>
	public class OrderFreeDeliverySpecification : ExpressionSpecification<IOrderFreeDeliveryPrice>
	{
		public OrderFreeDeliverySpecification(Expression<Func<IOrderFreeDeliveryPrice, bool>> expression) : base(expression)
		{
		}
		
		/// <summary>
		/// Создание спецификации для заказа
		/// </summary>
		/// <param name="paidDeliveryId">Идентификатор платной доставки</param>
		/// <returns></returns>
		public static OrderFreeDeliverySpecification Create(int paidDeliveryId)
			=> new OrderFreeDeliverySpecification(x =>
				x.IsSelfDelivery
				|| x.OrderAddressType == OrderAddressType.Service
				|| x.DeliveryPoint.AlwaysFreeDelivery
				|| x.Goods
					.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
				|| !x.Goods.Any(n => n.Nomenclature.Id != paidDeliveryId)
				&& (x.BottlesReturn > 0
					|| x.ObservableOrderEquipments.Any()
					|| x.ObservableOrderDepositItems.Any()));
	}
}

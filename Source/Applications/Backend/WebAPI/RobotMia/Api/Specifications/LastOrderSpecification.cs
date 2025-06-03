using System;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Orders;

namespace RobotMiaApi.Specifications
{
	/// <summary>
	/// Спецификации робота Мия для последнего заказа
	/// </summary>
	public class LastOrderSpecification : ExpressionSpecification<Order>
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="expression"></param>
		public LastOrderSpecification(Expression<Func<Order, bool>> expression) : base(expression)
		{
		}

		/// <summary>
		/// Спецификация валидации заказов для выборки последних заказов для робота Мия
		/// </summary>
		/// <param name="orderCompletedStatuses"></param>
		/// <param name="deliveryDateStartsAt"></param>
		/// <param name="paidDeliveryNomenclatureId"></param>
		/// <param name="fastDeliveryNomenclatureId"></param>
		/// <returns></returns>
		public static LastOrderSpecification CreateForValidRobotMiaLastOrders(
			OrderStatus[] orderCompletedStatuses,
			DateTime deliveryDateStartsAt,
			int paidDeliveryNomenclatureId,
			int fastDeliveryNomenclatureId)
			=> new LastOrderSpecification(order => orderCompletedStatuses.Contains(order.OrderStatus)
				&& !order.IsBottleStock
				&& order.DeliveryDate >= deliveryDateStartsAt
				&& !order.PromotionalSets.Any()
				&& !order.OrderItems
					.Where(x => x.Nomenclature.Id != paidDeliveryNomenclatureId)
					.Where(x => x.Nomenclature.Id != fastDeliveryNomenclatureId)
					.Any(x => x.Nomenclature.Category != NomenclatureCategory.water)
				&& order.OrderItems.OrderBy(x =>x.Nomenclature.Id).Distinct().Count() == order.OrderItems.OrderBy(x => x.Nomenclature.Id).Count()
				&& order.Client.ReasonForLeaving != ReasonForLeaving.Unknown
				&& order.DeliveryPoint.IsActive);
	}
}

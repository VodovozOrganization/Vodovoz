using System;
using System.Linq.Expressions;
using Vodovoz.Core.Domain.Specifications;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Specifications.Orders
{
	/// <summary>
	/// Специфиакация заказа
	/// </summary>
	public class OrderSpecification : ExpressionSpecification<Order>
	{
		public OrderSpecification(Expression<Func<Order, bool>> expression) : base(expression)
		{
		}

		/// <summary>
		/// Создание спецификацию для идентификатора контрагента
		/// </summary>
		/// <param name="counterpartyId">идентификатор контрагента</param>
		/// <returns></returns>
		public static OrderSpecification CreateForCounterpartyId(int counterpartyId)
			=> new OrderSpecification(o => o.Client.Id == counterpartyId);

		/// <summary>
		/// Создание спецификацию для идентификатора точки доставки
		/// </summary>
		/// <param name="deliveryPointId">идентификатор точки доставки</param>
		/// <returns></returns>
		public static OrderSpecification CreateForDeliveryPointId(int deliveryPointId)
			=> new OrderSpecification(o => o.DeliveryPoint.Id == deliveryPointId);
	}
}

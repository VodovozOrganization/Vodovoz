using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repositories.Orders
{
	public static class PromotionalSetRepository
	{
		/// <summary>
		/// Возврат словаря, у которого ключ это <see cref="PromotionalSet.Id"/>,
		/// а значение - массив с <see cref="VodovozOrder.Id"/>, для всех точек доставок
		/// похожих по полям <see cref="DeliveryPoint.City"/>,
		/// <see cref="DeliveryPoint.Street"/>, <see cref="DeliveryPoint.Building"/>,
		/// <see cref="DeliveryPoint.Room"/>
		/// </summary>
		/// <returns>Словарь</returns>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="currOrder">Заказ, из которого берётся точка доставки</param>
		/// <param name="ignoreCurrentOrder">Если <c>true</c>, то в выборке будет
		/// игнорироваться заказ передаваемы в качестве параметра <paramref name="currOrder"/></param>
		public static Dictionary<int, int[]> GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(IUnitOfWork uow, VodovozOrder currOrder, bool ignoreCurrentOrder = false)
		{
			VodovozOrder ordersAlias = null;
			PromotionalSet promotionalSetAlias = null;
			DeliveryPoint deliveryPointAlias = null;

			var dp = currOrder.DeliveryPoint;
			var oId = !ignoreCurrentOrder ? -1 : currOrder.Id;
			var acceptableStatuses = new[] {
												OrderStatus.Accepted,
												OrderStatus.InTravelList,
												OrderStatus.WaitForPayment,
												OrderStatus.OnLoading,
												OrderStatus.OnTheWay,
												OrderStatus.Shipped,
												OrderStatus.UnloadingOnStock,
												OrderStatus.Closed
											};

			var subQuerySimilarDP = QueryOver.Of(() => deliveryPointAlias)
											   .Where(p => p.City == dp.City)
											   .Where(p => p.Street == dp.Street)
											   .Where(p => p.Building == dp.Building)
											   .Where(p => p.Room == dp.Room)
											   .Select(Projections.Property(() => deliveryPointAlias.Id))
											   ;

			var result = uow.Session.QueryOver(() => promotionalSetAlias)
									.JoinAlias(() => promotionalSetAlias.Orders, () => ordersAlias)
									.JoinAlias(() => ordersAlias.DeliveryPoint, () => deliveryPointAlias)
									.Where(() => ordersAlias.Id != oId)
									.Where(() => ordersAlias.OrderStatus.IsIn(acceptableStatuses))
									.WithSubquery.WhereProperty(() => deliveryPointAlias.Id).In(subQuerySimilarDP)
									.SelectList(list => list.Select(() => promotionalSetAlias.Id)
															.Select(() => ordersAlias.Id))
									.List<object[]>()
									.GroupBy(x => (int)x[0])
									.ToDictionary(g => g.Key, g => g.Select(x => (int)x[1]).ToArray());
			return result;
		}
	}
}
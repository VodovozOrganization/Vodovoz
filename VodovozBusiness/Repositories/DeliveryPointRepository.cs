using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository
{
	public static class DeliveryPointRepository
	{
		public static QueryOver<DeliveryPoint> DeliveryPointsForCounterpartyQuery (Counterparty counterparty)
		{
			return QueryOver.Of<DeliveryPoint> ()
				.Where (dp => dp.Counterparty.Id == counterparty.Id);
		}

		public static int GetBottlesOrderedForPeriod(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime start, DateTime end)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var notConfirmedQueryResult = uow.Session.QueryOver<Order>(() => orderAlias)
				.Where(()=>orderAlias.DeliveryPoint.Id==deliveryPoint.Id)
				.Where(() => start < orderAlias.DeliveryDate && orderAlias.DeliveryDate < end)
				.Where(() => orderAlias.OrderStatus != OrderStatus.Canceled)
				.JoinAlias(()=>orderAlias.OrderItems,()=>orderItemAlias)
				.JoinAlias(()=>orderItemAlias.Nomenclature,()=>nomenclatureAlias)
				.Where(()=>nomenclatureAlias.Category==NomenclatureCategory.water)
				.Select(Projections.Sum(()=>orderItemAlias.Count)).List<int?>();
			
			var confirmedQueryResult = uow.Session.QueryOver<Order>(() => orderAlias)
				.Where(()=>orderAlias.DeliveryPoint.Id==deliveryPoint.Id)
				.Where(() => start < orderAlias.DeliveryDate && orderAlias.DeliveryDate < end)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.JoinAlias(()=>orderAlias.OrderItems,()=>orderItemAlias)
				.JoinAlias(()=>orderItemAlias.Nomenclature,()=>nomenclatureAlias)
				.Where(()=>nomenclatureAlias.Category==NomenclatureCategory.water)
				.Select(Projections.Sum(()=>orderItemAlias.ActualCount)).List<int?>();
			
			var bottlesOrdered = notConfirmedQueryResult.FirstOrDefault().GetValueOrDefault() 
				+ confirmedQueryResult.FirstOrDefault().GetValueOrDefault();
			return bottlesOrdered;
		}

		public static double GetAvgBottlesOrdered(IUnitOfWork uow, DeliveryPoint deliveryPoint, int? countLastOrders)
		{
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			var confirmedQueryResult = uow.Session.QueryOver<Order>(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Closed)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water)
				.OrderByAlias(() => orderAlias.DeliveryDate).Desc;
			if (countLastOrders.HasValue)
				confirmedQueryResult.Take(countLastOrders.Value);
			
			var list = confirmedQueryResult.Select(Projections.Group<Order>(x => x.Id),
				Projections.Sum(()=>orderItemAlias.Count)).List<object[]>();

			return list.Count > 0 ? list.Average (x => (int)x[1]) : 0;
		}
	}
}


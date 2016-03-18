using NHibernate.Criterion;
using Vodovoz.Domain;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using NHibernate.Transform;
using System.Linq;
using Vodovoz.Domain.Orders;
using System;
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

		public static int GetBottlesAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesAtDeliveryPointQueryResult result;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.SelectList(list => list
					.SelectSum(()=>operationAlias.Delivered).WithAlias(()=>result.Delivered)
					.SelectSum(()=>operationAlias.Returned).WithAlias(()=>result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesAtDeliveryPointQueryResult>()).List<BottlesAtDeliveryPointQueryResult>();
			var bottleCount = queryResult.FirstOrDefault()?.Total ?? 0;
			return bottleCount;
		}

		class BottlesAtDeliveryPointQueryResult
		{
			public int Delivered{ get; set; }
			public int Returned{get;set;}
			public int Total
			{
				get{ return Delivered - Returned; }
			}
		}
	}
}


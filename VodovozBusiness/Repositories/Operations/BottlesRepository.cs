using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
{
	public static class BottlesRepository
	{
		public static int GetBottlesAtCounterparty(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			
			var bottles =  queryResult.SelectList(list => list
					.SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
					.SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				).TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?.BottlesDebt ?? 0;
			return bottles;
		}

		public static int GetBottlesAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, DateTime? before = null)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);			
			
			var bottles =  queryResult.SelectList(list => list
					.SelectSum(()=>operationAlias.Delivered).WithAlias(()=>result.Delivered)
					.SelectSum(()=>operationAlias.Returned).WithAlias(()=>result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?.BottlesDebt ?? 0;
			return bottles;
		}

		public static int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, Order order)
		{
			var routeListItems = uow.Session.QueryOver<RouteListItem>()
			                        .Where(rli => rli.Order == order)
			                        .List();
			if(routeListItems.Any())
				return routeListItems.Sum(q => q.BottlesReturned);

			var defBottle = NomenclatureRepository.GetDefaultBottle(uow);
			SelfDeliveryDocument selfDeliveryDocumentAlias = null;
			var bttls = uow.Session.QueryOver<SelfDeliveryDocumentReturned>()
			               .Left.JoinAlias(d => d.Document, () => selfDeliveryDocumentAlias)
			               .Where(() => selfDeliveryDocumentAlias.Order == order)
			               .Where(r => r.Nomenclature == defBottle)
			               .Select(Projections.Sum<SelfDeliveryDocumentReturned>(s => s.Amount))
			               .SingleOrDefault<Decimal>();
			return (int)bttls;
		}

		class BottlesBalanceQueryResult
		{
			public int Delivered{get;set;}
			public int Returned{get;set;}
			public int BottlesDebt => Delivered - Returned;
		}
	}
}
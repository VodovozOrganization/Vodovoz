using System;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Goods;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Operations
{
	public partial class BottlesRepository : IBottlesRepository
	{
		public int GetBottlesDebtAtCounterparty(IUnitOfWork uow, Counterparty counterparty, DateTime? before = null)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			
			var queryResult = uow.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty == counterparty);
			
			if(before.HasValue)
			{
				queryResult.Where(() => operationAlias.OperationTime < before);
			}

			var bottles = queryResult.SelectList(list => list
				   .SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
				   .SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				).TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?.BottlesDebt ?? 0;
			return bottles;
		}

		public int GetBottlesDebtAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint, DateTime? before = null)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			
			var queryResult = uow.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint == deliveryPoint);
			
			if(before.HasValue)
			{
				queryResult.Where(() => operationAlias.OperationTime < before);
			}

			var bottles = queryResult.SelectList(list => list
				   .SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
				   .SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?.BottlesDebt ?? 0;
			return bottles;
		}

		public int GetBottlesDebtAtCounterpartyAndDeliveryPoint(
			IUnitOfWork uow,
			Counterparty counterparty,
			DeliveryPoint deliveryPoint,
			DateTime? before = null)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			
			var queryResult = uow.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				 .Where(() => operationAlias.Counterparty == counterparty)
				 .Where(() => operationAlias.DeliveryPoint == deliveryPoint);
			
			if(before.HasValue)
			{
				queryResult.Where(() => operationAlias.OperationTime < before);
			}

			var bottles = queryResult.SelectList(list => list
				   .SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
				   .SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?
				.BottlesDebt ?? 0;
			return bottles;
		}

		public int GetBottleDebtBySelfDelivery(IUnitOfWork uow, Counterparty counterparty)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			Order orderAlias = null;

			var queryResult = uow.Session.QueryOver(() => operationAlias)
				 .JoinAlias(() => operationAlias.Order, () => orderAlias, NHibernate.SqlCommand.JoinType.RightOuterJoin)
				 .Where(() => operationAlias.Counterparty == counterparty)
				 .And(() => orderAlias.SelfDelivery);

			var bottles = queryResult.SelectList(list => list
				   .SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
				   .SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>()
				.FirstOrDefault()?
				.BottlesDebt ?? 0;
			return bottles;
		}

		public int GetEmptyBottlesFromClientByOrder(IUnitOfWork uow, INomenclatureRepository nomenclatureRepository, Order order, int? excludeDocument = null)
		{
			if(nomenclatureRepository == null)
				throw new ArgumentNullException(nameof(nomenclatureRepository));

			var routeListItems = uow.Session.QueryOver<RouteListItem>()
									.Where(rli => rli.Order == order)
									.List();
			if(routeListItems.Any())
				return routeListItems.Sum(q => q.BottlesReturned);

			var defBottle = nomenclatureRepository.GetDefaultBottleNomenclature(uow);
			SelfDeliveryDocument selfDeliveryDocumentAlias = null;

			var query = uow.Session.QueryOver<SelfDeliveryDocumentReturned>()
								   .Left.JoinAlias(d => d.Document, () => selfDeliveryDocumentAlias)
								   .Where(() => selfDeliveryDocumentAlias.Order == order)
								   .Where(r => r.Nomenclature == defBottle);

			if(excludeDocument.HasValue && excludeDocument.Value > 0)
				query.Where(() => selfDeliveryDocumentAlias.Id != excludeDocument.Value);

			var bttls = query.Select(Projections.Sum<SelfDeliveryDocumentReturned>(s => s.Amount))
							 .SingleOrDefault<decimal>();
			return (int)bttls;
		}
	}
}

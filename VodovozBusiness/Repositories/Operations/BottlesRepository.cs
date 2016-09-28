using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
{
	public static class BottlesRepository
	{
		public static int GetBottlesAtCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
					.SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				).TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>();
			var bottles = queryResult.FirstOrDefault()?.BottlesDebt ?? 0;
			return bottles;
		}

		public static int GetBottlesAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.SelectList(list => list
					.SelectSum(()=>operationAlias.Delivered).WithAlias(()=>result.Delivered)
					.SelectSum(()=>operationAlias.Returned).WithAlias(()=>result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>();
			return queryResult.FirstOrDefault()?.BottlesDebt ?? 0;
		}

		class BottlesBalanceQueryResult
		{
			public int Delivered{get;set;}
			public int Returned{get;set;}
			public int BottlesDebt{
				get{
					return Delivered - Returned;
				}
			}
		}
	}


}
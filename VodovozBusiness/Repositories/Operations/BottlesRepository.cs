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
		public static decimal GetBottlesAtCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesBalanceQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.Delivered).WithAlias(() => result.Delivered)
					.SelectSum(() => operationAlias.Returned).WithAlias(() => result.Returned)
				).TransformUsing(Transformers.AliasToBean<BottlesBalanceQueryResult>()).List<BottlesBalanceQueryResult>();
			var deposits = queryResult.FirstOrDefault()?.BottlesDebt ?? 0;
			return deposits;
		}

		class BottlesBalanceQueryResult
		{
			public decimal Delivered{get;set;}
			public decimal Returned{get;set;}
			public decimal BottlesDebt{
				get{
					return Delivered - Returned;
				}
			}
		}
	}


}
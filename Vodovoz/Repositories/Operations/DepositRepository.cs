using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
{
	public static class DepositRepository
	{
		public static decimal GetBottleDepositsAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<DepositOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>();
			var deposits = queryResult.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		public static decimal GetDepositsAtCounterparty(IUnitOfWork UoW, Counterparty counterparty)
		{
			DepositOperation operationAlias = null;
			DepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<DepositOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<DepositsQueryResult>()).List<DepositsQueryResult>();
			var deposits = queryResult.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}

		class DepositsQueryResult
		{
			public decimal Received{get;set;}
			public decimal Refund{get;set;}
			public decimal Deposits{
				get{
					return Received - Refund;
				}
			}
		}
	}


}
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
			BottleDepositsQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<DepositOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.ReceivedDeposit).WithAlias(() => result.Received)
					.SelectSum(() => operationAlias.RefundDeposit).WithAlias(() => result.Refund)
				).TransformUsing(Transformers.AliasToBean<BottleDepositsQueryResult>()).List<BottleDepositsQueryResult>();
			var deposits = queryResult.FirstOrDefault()?.Deposits ?? 0;
			return deposits;
		}



		class BottleDepositsQueryResult
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
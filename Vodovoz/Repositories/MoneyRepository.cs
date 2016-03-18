using System;
using Vodovoz.Domain.Operations;
using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Transform;
using System.Linq;
using Vodovoz.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;
using NHibernate.Criterion;

namespace Vodovoz.Repository
{
	public static class MoneyRepository
	{
		public static decimal GetCounterpartyDebt(IUnitOfWork UoW, Counterparty counterparty)
		{
			MoneyMovementOperation operationAlias = null;
			CounterpartyDebtQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<MoneyMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id)
				.SelectList(list => list
					.SelectSum(() => operationAlias.Debt).WithAlias(() => result.Charged)
					.SelectSum(() => operationAlias.Money).WithAlias(() => result.Payed)
					.SelectSum(() => operationAlias.Deposit).WithAlias(() => result.Deposit)
				).TransformUsing(Transformers.AliasToBean<CounterpartyDebtQueryResult>()).List<CounterpartyDebtQueryResult>();
			var debt = queryResult
				.FirstOrDefault()?.Debt ?? 0;
			return debt;
		}			

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

		class CounterpartyDebtQueryResult
		{
			public decimal Charged{ get; set; }
			public decimal Payed{ get; set; }
			public decimal Deposit{ get; set; }
			public decimal Debt{
				get{
					return Charged-(Payed - Deposit);
				}
			}
		}

	}


}
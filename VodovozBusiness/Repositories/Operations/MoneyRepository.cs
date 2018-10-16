using System;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
{
	public static class MoneyRepository
	{
		public static CounterpartyDebtQueryResult GetCounterpartyBalanceResult(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null)
		{
			MoneyMovementOperation operationAlias = null;
			CounterpartyDebtQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<MoneyMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.Counterparty.Id == counterparty.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);			
			var debt = queryResult
			.SelectList(list => list
					.SelectSum(() => operationAlias.Debt).WithAlias(() => result.Charged)
					.SelectSum(() => operationAlias.Money).WithAlias(() => result.Payed)
					.SelectSum(() => operationAlias.Deposit).WithAlias(() => result.Deposit)
				).TransformUsing(Transformers.AliasToBean<CounterpartyDebtQueryResult>()).List<CounterpartyDebtQueryResult>();
			return debt.FirstOrDefault();
		}

		public static decimal GetCounterpartyDebt(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null)
		{
			return (GetCounterpartyBalanceResult(UoW, counterparty, before))?.Debt ?? 0;
		}

		public class CounterpartyDebtQueryResult
		{
			public decimal Charged{ get; set; }
			public decimal Payed{ get; set; }
			public decimal Deposit{ get; set; }
			public decimal Debt{
				get{
					return Charged-(Payed - Deposit);
				}
			}

			public decimal Balance{
				get{
					return Payed - Deposit - Charged;
				}
			}
		}

	}


}
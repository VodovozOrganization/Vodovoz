using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Repository.Operations
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
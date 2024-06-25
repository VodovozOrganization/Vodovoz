using System;
using System.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Operations;

namespace Vodovoz.Infrastructure.Persistance.Operations
{
	internal sealed class MoneyRepository : IMoneyRepository
	{
		public CounterpartyDebtQueryResult GetCounterpartyBalanceResult(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null)
		{
			MoneyMovementOperation operationAlias = null;
			CounterpartyDebtQueryResult result = null;
			var queryResult = UoW.Session.QueryOver(() => operationAlias)
										 .Where(() => operationAlias.Counterparty.Id == counterparty.Id);

			if(before.HasValue)
			{
				queryResult.Where(() => operationAlias.OperationTime < before);
			}

			var debt = queryResult
			.SelectList(list => list
					.SelectSum(() => operationAlias.Debt).WithAlias(() => result.Charged)
					.SelectSum(() => operationAlias.Money).WithAlias(() => result.Payed)
					.SelectSum(() => operationAlias.Deposit).WithAlias(() => result.Deposit)
				).TransformUsing(Transformers.AliasToBean<CounterpartyDebtQueryResult>()).List<CounterpartyDebtQueryResult>();

			return debt.FirstOrDefault();
		}

		public decimal GetCounterpartyDebt(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null)
		{
			return GetCounterpartyBalanceResult(UoW, counterparty, before)?.Debt ?? 0;
		}
	}
}

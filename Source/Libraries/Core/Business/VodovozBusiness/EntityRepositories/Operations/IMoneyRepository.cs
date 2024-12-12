using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Operations
{
	public interface IMoneyRepository
	{
		CounterpartyDebtQueryResult GetCounterpartyBalanceResult(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null);
		decimal GetCounterpartyDebt(IUnitOfWork UoW, Counterparty counterparty, DateTime? before = null);
	}
}

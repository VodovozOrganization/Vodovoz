using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Accounting;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Accounting
{
	public class AccountIncomeRepository : IAccountIncomeRepository
	{
		public bool AccountIncomeExists (IUnitOfWork uow, int year, int number, string counterpartyInn, string accountNumber)
		{
			Account accountAlias = null;
			Counterparty counterpartyAlias = null;

			var income = uow.Session.QueryOver<AccountIncome> ()
				.JoinAlias (ai => ai.CounterpartyAccount, () => accountAlias)
				.JoinAlias (ai => ai.Counterparty, () => counterpartyAlias)
				.Where (ai => ai.Date.Year == year &&
			             ai.Number == number &&
			             accountAlias.Number == accountNumber &&
			             counterpartyAlias.INN == counterpartyInn)
				.SingleOrDefault<AccountIncome> ();
			return income != null;
		}
	}
}


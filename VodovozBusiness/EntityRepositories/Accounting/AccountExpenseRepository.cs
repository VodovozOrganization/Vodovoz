using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Accounting;

namespace Vodovoz.EntityRepositories.Accounting
{
	public class AccountExpenseRepository : IAccountExpenseRepository
	{
		public bool AccountExpenseExists(IUnitOfWork uow, int year, int number, string accountNumber)
		{
			Account accountAlias = null;

			var expense = uow.Session.QueryOver<AccountExpense> ()
				.JoinAlias (ae => ae.OrganizationAccount, () => accountAlias)
				.Where (ae => ae.Date.Year == year &&
			              ae.Number == number &&
			              accountAlias.Number == accountNumber)
				.SingleOrDefault<AccountExpense> ();
			return expense != null;
		}
	}
}


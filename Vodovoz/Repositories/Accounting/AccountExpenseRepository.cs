using QSOrmProject;
using Vodovoz.Domain.Accounting;
using QSBanks;

namespace Vodovoz.Repository
{
	public static class AccountExpenseRepository
	{
		public static bool AccountExpenseExists (IUnitOfWork uow, int year, int number, string accountNumber)
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


using System.Collections.Generic;
using NHibernate.Criterion;
using QSBanks;
using QSOrmProject;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repository
{
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery ()
		{
			return QueryOver.Of<Counterparty> ()
				.Where (c => c.CooperationCustomer)
				.Where (c => !c.IsArchive);
		}

		public static IList<Counterparty> All (IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty> ().List<Counterparty> ();
		}

		public static Counterparty GetCounterpartyByINN (IUnitOfWork uow, string inn)
		{
			if (string.IsNullOrWhiteSpace (inn))
				return null;
			return uow.Session.QueryOver<Counterparty> ()
				.Where (c => c.INN == inn)
				.Take (1)
				.SingleOrDefault ();
		}

		public static Counterparty GetCounterpartyByAccount (IUnitOfWork uow, string accountNumber)
		{
			if (string.IsNullOrWhiteSpace (accountNumber))
				return null;
			Account accountAlias = null;

			return uow.Session.QueryOver<Counterparty> ()
				.JoinAlias(x => x.Accounts, () => accountAlias)
				.Where (() => accountAlias.Number == accountNumber)
				.Take (1)
				.SingleOrDefault ();
		}

	}
}


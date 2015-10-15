using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery ()
		{
			return QueryOver.Of<Counterparty> ()
				.Where (c => c.CounterpartyType == CounterpartyType.customer);
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
	}
}


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

		public static IList<Counterparty> GetCounterpartiesByCode1c (IUnitOfWork uow, string[] codes1c)
		{
			return uow.Session.QueryOver<Counterparty> ()
				.Where(c => c.Code1c.IsIn(codes1c))
				.List<Counterparty> ();
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

		public static IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn) 
		{
			if (string.IsNullOrWhiteSpace (inn))
				return null;
			return uow.Session.QueryOver<Counterparty>()
				.Where(c => c.INN == inn).List<Counterparty>();
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

		public static IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryPost)))
				.List<string>();
		}

		public static IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Counterparty>()
				.Select(Projections.Distinct(Projections.Property<Counterparty>(x => x.SignatoryBaseOf)))
				.List<string>();
		}

	}
}


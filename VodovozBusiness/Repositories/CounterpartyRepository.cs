using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using RepositoryInstance = Vodovoz.EntityRepositories.Counterparties.CounterpartyRepository;

namespace Vodovoz.Repositories
{
	[Obsolete("Используйте EntityRepositories.Counterparties.CounterpartyRepository")]
	public static class CounterpartyRepository
	{
		public static QueryOver<Counterparty> ActiveClientsQuery()
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.ActiveClientsQuery();
		}

		public static IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetCounterpartiesByCode1c(uow, codes1c);
		}

		public static IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetPlacesClientCameFrom(uow, doNotShowArchive, orderByDescending);
		}

		public static Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetCounterpartyByINN(uow, inn);
		}

		public static IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetCounterpartiesByINN(uow, inn);
		}

		public static Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetCounterpartyByAccount(uow, accountNumber);
		}

		public static IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetUniqueSignatoryPosts(uow);
		}

		public static IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetUniqueSignatoryBaseOf(uow);
		}

		public static IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow)
		{
			ICounterpartyRepository repository = new RepositoryInstance();
			return repository.GetCounterpartiesWithInnAndAnyContact(uow);
		}
	}
}


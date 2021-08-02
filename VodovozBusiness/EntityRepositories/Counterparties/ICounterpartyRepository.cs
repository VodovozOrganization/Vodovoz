using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface ICounterpartyRepository
	{
		QueryOver<Counterparty> ActiveClientsQuery();
		IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c);
		IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn);
		IList<Counterparty> GetCounterpartiesByNameAndPhone(IUnitOfWork uow, string partOfName, string phoneDigitNumber);
		IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow);
		Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber);
		Counterparty GetCounterpartyByBitrixId(IUnitOfWork uow, uint bitrixId);
		Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn);
		IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false);
		IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow);
		IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow);
	}
}

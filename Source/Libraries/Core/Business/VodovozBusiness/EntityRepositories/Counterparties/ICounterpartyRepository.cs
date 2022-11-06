using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public interface ICounterpartyRepository
	{
		QueryOver<Counterparty> ActiveClientsQuery();
		IList<Counterparty> GetCounterpartiesByCode1c(IUnitOfWork uow, string[] codes1c);
		IList<ClientCameFrom> GetPlacesClientCameFrom(IUnitOfWork uow, bool doNotShowArchive, bool orderByDescending = false);
		Counterparty GetCounterpartyByINN(IUnitOfWork uow, string inn);
		IList<Counterparty> GetCounterpartiesByINN(IUnitOfWork uow, string inn);
		Counterparty GetCounterpartyByAccount(IUnitOfWork uow, string accountNumber);
		IList<string> GetUniqueSignatoryPosts(IUnitOfWork uow);
		IList<string> GetUniqueSignatoryBaseOf(IUnitOfWork uow);
		PaymentType[] GetPaymentTypesForCash();
		PaymentType[] GetPaymentTypesForCashless();
		bool IsCashPayment(PaymentType payment);
		bool IsCashlessPayment(PaymentType payment);
		IList<CounterpartyTo1CNode> GetCounterpartiesWithInnAndAnyContact(IUnitOfWork uow);
		IList<Counterparty> GetDealers();
		Task<IList<Counterparty>> GetCounterpartiesByInnAndKpp(IUnitOfWork uow, string inn, string kpp, CancellationToken stoppingToken);
		Counterparty GetCounterpartyByPersonalAccountIdInEdo(IUnitOfWork uow, string edxClientId);
	}
}

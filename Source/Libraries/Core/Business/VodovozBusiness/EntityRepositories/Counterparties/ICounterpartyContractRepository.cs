using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Counterparties
{
    public interface ICounterpartyContractRepository
    {
        CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order, IErrorReporter errorReporter);
		CounterpartyContract GetCounterpartyContractByOrganization(IUnitOfWork uow, Order order, Organization organization);
		ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType);
    }
}

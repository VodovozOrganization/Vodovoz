using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Counterparties
{
    public interface ICounterpartyContractRepository
    {
        CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order);
        ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType);
    }
}

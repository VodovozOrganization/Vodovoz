using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories
{
    public interface ICounterpartyContractRepository
    {
        CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order);
        ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType);
    }
}
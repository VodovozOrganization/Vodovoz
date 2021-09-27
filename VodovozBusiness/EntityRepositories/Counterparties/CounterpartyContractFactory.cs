using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Models;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class CounterpartyContractFactory
	{
		private readonly IOrganizationProvider _organizationProvider;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;

		public CounterpartyContractFactory(IOrganizationProvider organizationProvider, ICounterpartyContractRepository counterpartyContractRepository)
		{
			this._organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
			this._counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
		}
		
		public CounterpartyContract CreateContract(IUnitOfWork uow, Order order, DateTime? issueDate)
		{
			var contractType = _counterpartyContractRepository.GetContractTypeForPaymentType(order.Client.PersonType, order.PaymentType);
			var org = _organizationProvider.GetOrganization(uow, order);
			var contractSubNumber = CounterpartyContract.GenerateSubNumber(order.Client);
			
			CounterpartyContract contract = new CounterpartyContract {
				 Counterparty = order.Client,
				 ContractSubNumber = contractSubNumber,
				 Organization = org,
				 IsArchive = false,
				 ContractType = contractType
			};
			if(issueDate.HasValue) {
				contract.IssueDate = issueDate.Value;
			}
			return contract;
		}
	}
}

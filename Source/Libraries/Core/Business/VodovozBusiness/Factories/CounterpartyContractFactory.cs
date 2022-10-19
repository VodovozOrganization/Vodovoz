using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Models;

namespace Vodovoz.Factories
{
	public class CounterpartyContractFactory
	{
		private readonly IOrganizationProvider _organizationProvider;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;

		public CounterpartyContractFactory(IOrganizationProvider organizationProvider, ICounterpartyContractRepository counterpartyContractRepository)
		{
			_organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
			_counterpartyContractRepository = counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
		}
		
		public CounterpartyContract CreateContract(IUnitOfWork uow, Order order, DateTime? issueDate, Organization organization = null)
		{
			var contractType = _counterpartyContractRepository.GetContractTypeForPaymentType(order.Client.PersonType, order.PaymentType);
			var org = organization ?? _organizationProvider.GetOrganization(uow, order);
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

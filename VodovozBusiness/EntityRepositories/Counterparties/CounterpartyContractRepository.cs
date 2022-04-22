using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Models;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class CounterpartyContractRepository : ICounterpartyContractRepository
	{
		private readonly IOrganizationProvider organizationProvider;

		public CounterpartyContractRepository(IOrganizationProvider organizationProvider)
		{
			this.organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
		}
		
		public CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order, IErrorReporter errorReporter = null)
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));
			if(order == null) throw new ArgumentNullException(nameof(order));
			if(order.Client == null) {
				return null;
			}

			var personType = order.Client.PersonType;
			var paymentType = order.PaymentType;
			var contractType = GetContractTypeForPaymentType(personType, paymentType);
			var organization = organizationProvider.GetOrganization(uow, order);
			if(organization == null)
				return null;

			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			var result =
			uow.Session.QueryOver<CounterpartyContract>()
				.JoinAlias(co => co.Counterparty, () => counterpartyAlias)
				.JoinAlias(co => co.Organization, () => organizationAlias)
				.Where(
					co => (
						   counterpartyAlias.Id == order.Client.Id &&
					       !co.IsArchive &&
					       !co.OnCancellation &&
					       organizationAlias.Id == organization.Id &&
					       co.ContractType == contractType
					)
				)
				.OrderBy(x => x.IssueDate).Desc.List();
			
			if(result.Count > 1 && errorReporter != null)
			{
				Exception ex = new ArgumentException("Query returned >1 CounterpartyContract");
				errorReporter.AutomaticSendErrorReport($"Ошибка в {nameof(CounterpartyContractRepository)}, GetCounterpartyContract() вернул больше 1 контракта", ex);
			}
			return result.FirstOrDefault();
		}
 
		public IList<CounterpartyContract> GetActiveContractsWithOrganization(IUnitOfWork uow, Counterparty counterparty, Organization org, ContractType type)
		{
			return uow.Session.QueryOver<CounterpartyContract>()
				.Where(co => (co.Counterparty.Id == counterparty.Id &&
				   !co.IsArchive &&
				   !co.OnCancellation &&
				   co.Organization.Id == org.Id)
				   && co.ContractType == type)
				.List();
		}
		
		public ContractType GetContractTypeForPaymentType(PersonType clientType, PaymentType paymentType)
		{
			switch(paymentType) {
				case PaymentType.cash:
				case PaymentType.ByCard:
				case PaymentType.Terminal:
					if(clientType == PersonType.legal) {
						return ContractType.CashUL;
					}else {
						return ContractType.CashFL;
					}
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					return ContractType.Cashless;
				case PaymentType.barter:
					return ContractType.Barter;
				default:
					return ContractType.Cashless;
			}
		}
	}
}


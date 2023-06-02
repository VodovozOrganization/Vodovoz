using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Models;
using Vodovoz.Tools;

namespace Vodovoz.EntityRepositories.Counterparties
{
	public class CounterpartyContractRepository : ICounterpartyContractRepository
	{
		private readonly IOrganizationProvider _organizationProvider;
		private readonly ICashReceiptRepository _cashReceiptRepository;

		public CounterpartyContractRepository(IOrganizationProvider organizationProvider, ICashReceiptRepository cashReceiptRepository)
		{
			this._organizationProvider = organizationProvider ?? throw new ArgumentNullException(nameof(organizationProvider));
			_cashReceiptRepository = cashReceiptRepository ?? throw new ArgumentNullException(nameof(cashReceiptRepository));
		}

		public CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order, IErrorReporter errorReporter = null)
		{
			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(order == null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(order.Client == null)
			{
				return null;
			}

			if(order.Contract != null && order.Id != 0 && _cashReceiptRepository.HasReceipt(order.Id))
			{
				return order.Contract;
			}

			var personType = order.Client.PersonType;
			var paymentType = order.PaymentType;
			var contractType = GetContractTypeForPaymentType(personType, paymentType);

			try
			{
				var organization = _organizationProvider.GetOrganization(uow, order);

				if(organization == null)
				{
					return null;
				}

				var result = GetCounterpartyContractsOrderByIssueDateDesc(uow, order, organization, contractType);

				if(result.Count > 1 && errorReporter != null)
				{
					Exception ex = new ArgumentException("Query returned >1 CounterpartyContract");
					errorReporter.AutomaticSendErrorReport($"Ошибка в {nameof(CounterpartyContractRepository)}, GetCounterpartyContract() вернул больше 1 контракта", ex);
				}

				return result.FirstOrDefault();

			}
			catch(NotSupportedException)
			{
				return null;
			}
		}

		public CounterpartyContract GetCounterpartyContract(IUnitOfWork uow, Order order, Organization organization)
		{
			if(order.Contract != null && order.Id != 0 && _cashReceiptRepository.HasReceipt(order.Id))
			{
				return order.Contract;
			}

			var personType = order.Client.PersonType;
			var paymentType = order.PaymentType;
			var contractType = GetContractTypeForPaymentType(personType, paymentType);

			IList<CounterpartyContract> result = GetCounterpartyContractsOrderByIssueDateDesc(uow, order, organization, contractType);
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
			switch(paymentType)
			{
				case PaymentType.Cash:
				case PaymentType.PaidOnline:
				case PaymentType.Terminal:
					if(clientType == PersonType.legal)
					{
						return ContractType.CashUL;
					}
					else
					{
						return ContractType.CashFL;
					}
				case PaymentType.Cashless:
				case PaymentType.ContractDocumentation:
					return ContractType.Cashless;
				case PaymentType.Barter:
					return ContractType.Barter;
				default:
					return ContractType.Cashless;
			}
		}

		private IList<CounterpartyContract> GetCounterpartyContractsOrderByIssueDateDesc(
			IUnitOfWork uow, Order order, Organization organization, ContractType contractType)
		{
			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			var result = uow.Session.QueryOver<CounterpartyContract>()
				.JoinAlias(co => co.Counterparty, () => counterpartyAlias)
				.JoinAlias(co => co.Organization, () => organizationAlias)
				.Where(co => counterpartyAlias.Id == order.Client.Id
					&& !co.IsArchive
					&& !co.OnCancellation
					&& organizationAlias.Id == organization.Id
					&& co.ContractType == contractType)
				.OrderBy(x => x.IssueDate)
				.Desc
				.List();
			return result;
		}
	}
}


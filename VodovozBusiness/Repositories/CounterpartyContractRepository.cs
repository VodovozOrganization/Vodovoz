using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Repository.Client;

namespace Vodovoz.Repository
{
	public class CounterpartyContractRepository
	{
		public static CounterpartyContract GetCounterpartyContractByPaymentType (IUnitOfWork uow, Counterparty counterparty, PersonType personType, PaymentType paymentType)
		{
			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(personType, paymentType);
			Organization organization = OrganizationRepository.GetOrganizationByPaymentType (uow, personType, paymentType);
			if(organization == null)
				return null;

			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			var result = 
			uow.Session.QueryOver<CounterpartyContract> ()
				.JoinAlias (co => co.Counterparty, () => counterpartyAlias)
				.JoinAlias (co => co.Organization, () => organizationAlias)
				.Where (co => (counterpartyAlias.Id == counterparty.Id &&
			!co.IsArchive &&
			!co.OnCancellation &&
			organizationAlias.Id == organization.Id &&
			co.ContractType == contractType))
				.OrderBy(x => x.IssueDate).Desc.List();
			return result.FirstOrDefault();
		}

		public static IList<CounterpartyContract> GetActiveContractsWithOrganization (IUnitOfWork uow, Counterparty counterparty, Organization org, ContractType type)
		{
			return uow.Session.QueryOver<CounterpartyContract> ()
				.Where (co => (co.Counterparty.Id == counterparty.Id &&
					!co.IsArchive &&
					!co.OnCancellation &&
					co.Organization.Id == org.Id)
				    && co.ContractType == type)
				.List();
		}

	}
}


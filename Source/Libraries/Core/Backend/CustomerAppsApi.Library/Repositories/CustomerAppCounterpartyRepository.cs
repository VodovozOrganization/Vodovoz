using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using NHibernate;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace CustomerAppsApi.Library.Repositories
{
	public class CustomerAppCounterpartyRepository : ICustomerAppCounterpartyRepository
	{
		public CompanyInfoResponse GetLinkedCompanyInfo(IUnitOfWork uow, Source source, Guid externalCounterpartyId, int legalCounterpartyId)
		{
			ExternalLegalCounterpartyAccount externalLegalAccountAlias = null;
			ExternalLegalCounterpartyAccountActivation externalLegalAccountActivationAlias = null;
			Counterparty counterpartyAlias = null;
			Account accountAlias = null;
			CompanyInfoResponse resultAlias = null;
			
			var linkedCompany = uow.Session.QueryOver(() => externalLegalAccountAlias)
				.JoinEntityAlias(() => counterpartyAlias, () => externalLegalAccountAlias.LegalCounterpartyId == counterpartyAlias.Id)
				.Left.JoinAlias(
					() => counterpartyAlias.Accounts,
					() => accountAlias,
					() => accountAlias.IsDefault)
				.Left.JoinAlias(() => externalLegalAccountAlias.AccountActivation, () => externalLegalAccountActivationAlias)
				.Where(() => externalLegalAccountAlias.ExternalUserId == externalCounterpartyId)
				.And(() => externalLegalAccountAlias.Source == source)
				.And(() => externalLegalAccountAlias.LegalCounterpartyId == legalCounterpartyId)
				.SelectList(list => list
					.Select(() => counterpartyAlias.Address).WithAlias(() => resultAlias.Address)
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.KPP).WithAlias(() => resultAlias.Kpp)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.AccountNumber)
					.Select(() => counterpartyAlias.DelayDaysForBuyers).WithAlias(() => resultAlias.DelayOfPayment)
					.Select(() => externalLegalAccountActivationAlias.AddingPhoneNumberState)
						.WithAlias(() => resultAlias.ActivationCompanyAccountInfo.AddingPhoneNumberState)
					.Select(() => externalLegalAccountActivationAlias.AddingReasonForLeavingState)
						.WithAlias(() => resultAlias.ActivationCompanyAccountInfo.AddingReasonForLeavingState)
					.Select(() => externalLegalAccountActivationAlias.AddingEdoAccountState)
						.WithAlias(() => resultAlias.ActivationCompanyAccountInfo.AddingEdoAccountState)
					.Select(() => externalLegalAccountActivationAlias.TaxServiceCheckState)
						.WithAlias(() => resultAlias.ActivationCompanyAccountInfo.TaxServiceCheckState)
					.Select(() => externalLegalAccountActivationAlias.TrueMarkCheckState)
						.WithAlias(() => resultAlias.ActivationCompanyAccountInfo.TrueMarkCheckState)
				)
				.TransformUsing(Transformers.AliasToBean<CompanyInfoResponse>())
				.List<CompanyInfoResponse>()
				.FirstOrDefault();
			
			return linkedCompany;
		}

		public IEnumerable<LegalCustomersByInnResponse> GetLegalCustomersByInn(IUnitOfWork uow, string inn, string emailAddress)
		{
			return (
				from counterparty in uow.Session.Query<Counterparty>()
				where counterparty.INN == inn

				let emailId =
					(int?)(from email in uow.Session.Query<Email>()
						where email.Counterparty.Id == counterparty.Id
							&& email.Address == emailAddress
						select email.Id)
					.FirstOrDefault()

				let activeEmailId =
					(int?)(from email in uow.Session.Query<Email>()
					join linkedData in uow.Session.Query<ExternalLegalCounterpartyAccount>()
						on email.Id equals linkedData.LegalCounterpartyEmailId
					where linkedData.LegalCounterpartyId == counterparty.Id
					select email.Id)
					.FirstOrDefault()

				select LegalCustomersByInnResponse.Create(
					counterparty.Id,
					counterparty.Name,
					counterparty.JurAddress,
					counterparty.INN,
					counterparty.KPP,
					counterparty.FullName,
					counterparty.TypeOfOwnership,
					counterparty.FirstName,
					counterparty.Surname,
					counterparty.Patronymic,
					emailId,
					activeEmailId)
				).ToList();
		}
	}
}

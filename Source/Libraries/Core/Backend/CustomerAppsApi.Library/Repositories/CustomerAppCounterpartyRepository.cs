using System;
using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Accounts;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Client;

namespace CustomerAppsApi.Library.Repositories
{
	public class CustomerAppCounterpartyRepository : ICustomerAppCounterpartyRepository
	{
		private readonly IOrganizationSettings _organizationSettings;

		public CustomerAppCounterpartyRepository(IOrganizationSettings organizationSettings)
		{
			_organizationSettings = organizationSettings ?? throw new ArgumentNullException(nameof(organizationSettings));
		}
		
		public CompanyInfoResponse GetLinkedCompanyInfo(IUnitOfWork uow, int legalCounterpartyId)
		{
			ExternalLegalCounterpartyAccount externalLegalAccountAlias = null;
			Counterparty counterpartyAlias = null;
			Account accountAlias = null;
			CompanyInfoResponse resultAlias = null;

			var edoAccountSubQuery = QueryOver.Of<CounterpartyEdoAccount>()
				.Where(a => a.Counterparty.Id == counterpartyAlias.Id)
				.And(a => a.OrganizationId == _organizationSettings.VodovozOrganizationId)
				.Select(a => a.Id)
				.Take(1);
			
			var linkedCompany = uow.Session.QueryOver(() => externalLegalAccountAlias)
				.JoinEntityAlias(() => counterpartyAlias, () => externalLegalAccountAlias.LegalCounterpartyId == counterpartyAlias.Id)
				.Left.JoinAlias(
					() => counterpartyAlias.Accounts,
					() => accountAlias,
					() => accountAlias.IsDefault)
				.Where(() => externalLegalAccountAlias.LegalCounterpartyId == legalCounterpartyId)
				.SelectList(list => list
					.Select(() => counterpartyAlias.JurAddress).WithAlias(() => resultAlias.Address)
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.KPP).WithAlias(() => resultAlias.Kpp)
					.Select(() => counterpartyAlias.FullName).WithAlias(() => resultAlias.Name)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.AccountNumber)
					.Select(() => counterpartyAlias.DelayDaysForBuyers).WithAlias(() => resultAlias.DelayOfPayment)
					.Select(
						Projections.Cast(
							NHibernateUtil.String,
							Projections.Property(() => externalLegalAccountAlias.TaxServiceCheckState)))
						.WithAlias(() => resultAlias.TaxServiceCheckState)
					.Select(Projections.Conditional(
							Restrictions.IsNotNull(Projections.SubQuery(edoAccountSubQuery)),
							Projections.Cast(NHibernateUtil.String, Projections.Constant(AddingEdoAccountState.Done)),
							Projections.Cast(NHibernateUtil.String, Projections.Constant(AddingEdoAccountState.NeedAdd))))
						.WithAlias(() => resultAlias.AddingEdoAccountState)
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
					counterparty.FullName,
					counterparty.FirstName,
					counterparty.Surname,
					counterparty.Patronymic,
					counterparty.INN,
					counterparty.KPP,
					counterparty.JurAddress,
					counterparty.TypeOfOwnership,
					emailId,
					activeEmailId)
				).ToList();
		}
	}
}

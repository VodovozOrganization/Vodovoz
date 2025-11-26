using System.Collections.Generic;
using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using NHibernate;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;

namespace CustomerAppsApi.Library.Repositories
{
	public class CounterpartyRepository : ICounterpartyRepository
	{
		public CompanyInfoResponse GetLinkedCompany(IUnitOfWork uow, int externalCounterpartyId)
		{
			LinkedLegalCounterpartyEmailToExternalUser linkedDataAlias = null;
			Counterparty counterpartyAlias = null;
			Account accountAlias = null;
			CompanyInfoResponse resultAlias = null;
			
			var linkedCompany = uow.Session.QueryOver(() => linkedDataAlias)
				.JoinEntityAlias(() => counterpartyAlias, () => linkedDataAlias.LegalCounterpartyId == counterpartyAlias.Id)
				.Left.JoinAlias(
					() => counterpartyAlias.Accounts,
					() => accountAlias,
					() => accountAlias.IsDefault)
				.Where(() => linkedDataAlias.ExternalCounterpartyId == externalCounterpartyId)
				.SelectList(list => list
					//.SelectGroup(() => counterpartyAlias.Id).WithAlias(() => resultAlias.)
					.Select(() => counterpartyAlias.Address).WithAlias(() => resultAlias.Address)
					.Select(() => counterpartyAlias.INN).WithAlias(() => resultAlias.Inn)
					.Select(() => counterpartyAlias.KPP).WithAlias(() => resultAlias.Kpp)
					.Select(() => counterpartyAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.AccountNumber)
					.Select(() => counterpartyAlias.DelayDaysForBuyers).WithAlias(() => resultAlias.DelayOfPayment)
				)
				.TransformUsing(Transformers.AliasToBean<CompanyInfoResponse>())
				.List<CompanyInfoResponse>()
				.FirstOrDefault();

			if(linkedCompany is null)
			{
				return null;
			}
			
			linkedCompany.ActivationLinkCompany = ActivationLinkCompany.Create();
			
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
					join linkedData in uow.Session.Query<LinkedLegalCounterpartyEmailToExternalUser>()
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
				/*select new LegalCustomersByInnResponse
				{
					ErpCounterpartyId = counterparty.Id,
					Name = counterparty.Name,
					JurAddress = counterparty.JurAddress,
					Inn = counterparty.INN,
					Kpp = counterparty.KPP,
					FullName = counterparty.FullName,
					ShortTypeOfOwnership = counterparty.TypeOfOwnership,
					FirstName = counterparty.FirstName,
					Surname = counterparty.Surname,
					Patronymic = counterparty.Patronymic
				}*/
				).ToList();
		}
	}
}

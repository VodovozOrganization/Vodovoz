using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;

namespace CustomerAppsApi.Library.Repositories
{
	public class CounterpartyRepository : ICounterpartyRepository
	{
		public CompanyInfoResponse GetLinkedCompany(IUnitOfWork uow, int externalCounterpartyId)
		{
			return (
				from linkedData in uow.Session.Query<LinkedLegalCounterpartyEmailToExternalUser>()
				join legalCounterparty in uow.Session.Query<Counterparty>()
					on linkedData.LegalCounterpartyId equals legalCounterparty.Id
				where linkedData.ExternalCounterpartyId == externalCounterpartyId
				select new CompanyInfoResponse
				{
					Address = legalCounterparty.Address,
					Inn = legalCounterparty.INN,
					Kpp = legalCounterparty.KPP,
					Name = legalCounterparty.Name,
					AccountNumber = ,
					DelayOfPayment = legalCounterparty.DelayDaysForBuyers,
					ActivationLinkCompany = new ActivationLinkCompany
					{
						PhoneNumber = ,
						PurposeOfPurchase = ,
						EDocumentSystem = ,
						MarkingSystemCheck = ,
						TaxServiceCheck = 
					}
				}
				)
				.FirstOrDefault();
		}
	}
}

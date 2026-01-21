using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICustomerAppCounterpartyRepository
	{
		CompanyInfoResponse GetLinkedCompanyInfo(IUnitOfWork uow, int legalCounterpartyId);
		IEnumerable<LegalCustomersByInnResponse> GetLegalCustomersByInn(IUnitOfWork uow, string inn, string emailAddress);
	}
}

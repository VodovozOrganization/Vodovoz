using System;
using System.Collections.Generic;
using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICustomerAppCounterpartyRepository
	{
		CompanyInfoResponse GetLinkedCompanyInfo(IUnitOfWork uow, Source source, Guid externalCounterpartyId, int legalCounterpartyId);
		IEnumerable<LegalCustomersByInnResponse> GetLegalCustomersByInn(IUnitOfWork uow, string inn, string emailAddress);
	}
}

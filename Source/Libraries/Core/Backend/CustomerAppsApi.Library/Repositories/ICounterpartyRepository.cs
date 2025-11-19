using CustomerAppsApi.Library.Dto.Counterparties;
using QS.DomainModel.UoW;

namespace CustomerAppsApi.Library.Repositories
{
	public interface ICounterpartyRepository
	{
		CompanyInfoResponse GetLinkedCompany(IUnitOfWork uow, int externalCounterpartyId);
	}
}

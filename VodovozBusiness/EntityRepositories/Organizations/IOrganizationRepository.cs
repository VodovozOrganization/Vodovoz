using QS.DomainModel.UoW;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Organizations
{
	public interface IOrganizationRepository
	{
		Organization GetOrganizationByInn(IUnitOfWork uow, string inn);
		Organization GetOrganizationByAccountNumber(IUnitOfWork uow, string accountNumber);
	}
}
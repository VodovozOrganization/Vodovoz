using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.EntityRepositories.Organizations
{
	public interface IOrganizationRepository
	{
		Organization GetOrganizationByInn(IUnitOfWork uow, string inn);
		Organization GetOrganizationByAccountNumber(IUnitOfWork uow, string accountNumber);
		Organization GetOrganizationById(IUnitOfWork uow, int organizationId);
		Organization GetOrganizationByTaxcomEdoAccountId(IUnitOfWork uow, string edoAccountId);
		IList<OrganizationOwnershipType> GetOrganizationOwnershipTypeByAbbreviation(IUnitOfWork uow, string abbreviation);
		IList<OrganizationOwnershipType> GetAllOrganizationOwnershipTypes(IUnitOfWork uow);
		Organization GetCommonOrganisation(IUnitOfWork uow);
		Task<IList<Organization>> GetOrganizationsByTaxcomEdoAccountIds(IUnitOfWork uow, string[] edoAccountIds, CancellationToken cancellationToken);

		/// <summary>
		/// Получение списка всех организаций
		/// </summary>
		/// <param name="uow"></param>
		/// <returns></returns>
		IList<Organization> GetOrganizations(IUnitOfWork uow);
	}
}

using System;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QS.Banks.Domain;
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
		OrganizationOwnershipType GetOrganizationOwnershipTypeByCode(IUnitOfWork uow, string code);
		Organization GetCommonOrganisation(IUnitOfWork uow);
		Task<IList<Organization>> GetOrganizationsByTaxcomEdoAccountIds(IUnitOfWork uow, string[] edoAccountIds, CancellationToken cancellationToken);

		/// <summary>
		/// Получение списка всех организаций
		/// </summary>
		/// <param name="uow"></param>
		/// <returns></returns>
		IList<Organization> GetOrganizations(IUnitOfWork uow);
		/// <summary>
		/// Получение активных р/сч для точки контроля безналичных передвижений 
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="startDate">Начальная дата</param>
		/// <param name="endDate">Конечная дата</param>
		/// <param name="bankId">Идентификатор банка</param>
		/// <param name="accountId">Идентификатор аккаунта</param>
		/// <returns></returns>
		IEnumerable<(string OrganizationName, Account Account)> GetActiveAccountsOrganizationsWithCashlessControl(
			IUnitOfWork uow, DateTime startDate, DateTime endDate, int? bankId, int? accountId);
	}
}

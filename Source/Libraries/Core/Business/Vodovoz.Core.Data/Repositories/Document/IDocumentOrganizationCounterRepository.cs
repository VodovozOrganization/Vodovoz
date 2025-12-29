using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Core.Data.Repositories.Document
{
	public interface IDocumentOrganizationCounterRepository
	{
		/// <summary>
		/// Получить максимальный счетчик в выбранном году
		/// </summary>
		/// <param name="unitOfWork">UoW</param>
		/// <param name="date">Выбранная дата</param>
		/// <param name="organizationEntity">Организация</param>
		/// <returns>Сущность (null если записи нет)</returns>
		DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(
			IUnitOfWork unitOfWork,
			DateTime date, 
			OrganizationEntity organizationEntity);

		/// <summary>
		/// Получить максимальный счетчик в выбранном году (async)
		/// </summary>
		/// <param name="unitOfWork">UoW</param>
		/// <param name="date">Выбранная дата</param>
		/// <param name="organizationEntity">Организация</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Сущность (null если записи нет)</returns>
		Task<DocumentOrganizationCounter> GetMaxDocumentOrganizationCounterOnYearAsync(
			IUnitOfWork unitOfWork,
			DateTime date,
			OrganizationEntity organizationEntity,
			CancellationToken cancellationToken);
	}
}

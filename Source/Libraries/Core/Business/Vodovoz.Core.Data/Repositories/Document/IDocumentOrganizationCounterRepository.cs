using System;
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
		/// <param name="organizationEntity"></param>
		/// <param name="documentType"></param>
		/// <returns>Сущность (null если записи нет)</returns>
		DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(IUnitOfWork unitOfWork, DateTime date, OrganizationEntity organizationEntity);
		
		/// <summary>
		/// Получить максимальный счетчик в выбранном году
		/// </summary>
		/// <param name="unitOfWork">UoW</param>
		/// <param name="date">Выбранная дата</param>
		/// <returns>Счетчик (null если записи нет)</returns>
		int? GetMaxCounterOnYear(IUnitOfWork unitOfWork, DateTime date, OrganizationEntity organizationEntity);
	}
}

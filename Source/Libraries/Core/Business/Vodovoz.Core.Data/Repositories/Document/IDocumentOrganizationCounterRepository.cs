using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.Repositories.Document
{
	public interface IDocumentOrganizationCounterRepository
	{
		/// <summary>
		/// Получить максимальный счетчик в выбранном году
		/// </summary>
		/// <param name="unitOfWork">UoW</param>
		/// <param name="date">Выбранная дата</param>
		/// <returns>Сущность (null если записи нет)</returns>
		DocumentOrganizationCounter GetMaxDocumentOrganizationCounterOnYear(IUnitOfWork unitOfWork, DateTime date);
		
		/// <summary>
		/// Получить максимальный счетчик в выбранном году
		/// </summary>
		/// <param name="unitOfWork">UoW</param>
		/// <param name="date">Выбранная дата</param>
		/// <returns>Счетчик (null если записи нет)</returns>
		int? GetMaxCounterOnYear(IUnitOfWork unitOfWork, DateTime date);
	}
}

using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.Repositories.Cash
{
	public interface IVatRateVersionRepository
	{
		/// <summary>
		/// Получить актуальную версию НДС для номенклатуры
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="id">ID номенклатуры</param>
		/// <returns>Версия НДС</returns>
		VatRateVersion GetActualVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id);
		
		/// <summary>
		/// Получить актуальную версию НДС для организации
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="id">ID организации</param>
		/// <returns>Версия НДС</returns>
		VatRateVersion GetActualVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id);

		/// <summary>
		/// Получить версию НДС для номенклатуры на выбранную дату
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="id">ID номенклатуры</param>
		/// <param name="date">Дата</param>
		/// <returns>Версия НДС</returns>
		VatRateVersion GetVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id, DateTime date);

		/// <summary>
		/// Получить версию НДС для организации на выбранную дату
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="id">ID организации</param>
		/// <param name="date">Дата</param>
		/// <returns>Версия НДС</returns>
		VatRateVersion GetVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id, DateTime date);
		
		/// <summary>
		/// Получить список всех версий НДС для организацией с определенной НДС
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="targetVatRate">Ставка НДС</param>
		/// <returns>Список версий НДС</returns>
		IEnumerable<VatRateVersion> GetVatRateVersionsForOrganization(IUnitOfWork unitOfWork, decimal targetVatRate);
		
		/// <summary>
		/// Получить список всех версий НДС для номенклатур с определенной НДС
		/// </summary>
		/// <param name="unitOfWork">uow</param>
		/// <param name="targetVatRate">Ставка НДС</param>
		/// <returns>Список версий НДС</returns>
		IEnumerable<VatRateVersion> GetVatRateVersionsForNomenclature(IUnitOfWork unitOfWork, decimal targetVatRate);
		
	}
}

using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.Repositories.Cash
{
	public interface IVatRateRepository
	{
		/// <summary>
		/// Получить ставку НДС по значению
		/// </summary>
		/// <returns>Ставка НДС</returns>
		VatRate GetVatRateByValue(IUnitOfWork unitOfWork, decimal vatRateValue);
		VatRate GetActualVatRateFromOrganization(IUnitOfWork uow, int organizationId, DateTime? date);
		VatRate GetActualVatRateFromNomenclature(IUnitOfWork uow, int nomenclatureId, DateTime? date);
	}
}

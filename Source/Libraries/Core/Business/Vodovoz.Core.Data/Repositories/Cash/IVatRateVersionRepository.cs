using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Cash;

namespace Vodovoz.Core.Data.Repositories.Cash
{
	public interface IVatRateVersionRepository
	{
		VatRateVersion GetActualVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id);
		VatRateVersion GetActualVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id);
		VatRateVersion GetVatRateVersionForNomenclature(IUnitOfWork unitOfWork, int id, DateTime date);
		VatRateVersion GetVatRateVersionForOrganization(IUnitOfWork unitOfWork, int id, DateTime date);
		IEnumerable<VatRateVersion> GetVatRateVersionsForOrganization(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate, decimal targetVatRate);
		IEnumerable<VatRateVersion> GetVatRateVersionsForNomenclature(IUnitOfWork unitOfWork, DateTime startDate, DateTime endDate, decimal targetVatRate);
		
	}
}

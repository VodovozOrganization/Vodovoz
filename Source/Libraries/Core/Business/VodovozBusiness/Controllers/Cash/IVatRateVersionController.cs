using System;
using Vodovoz.Core.Domain.Cash;

namespace VodovozBusiness.Controllers.Cash
{
	public interface IVatRateVersionController
	{
		VatRateVersion CreateAndAddVersion(VatRateVersionType vatRateVersionType, DateTime? startDate = null);
		
		void ChangeVersionStartDate(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType);
		
		bool IsValidDateForNewVatRateVersion(DateTime dateTime, VatRateVersionType vatRateVersionType);
		
		bool IsValidDateForVersionStartDateChange(VatRateVersion version, DateTime newStartDate, VatRateVersionType vatRateVersionType);
	}
}

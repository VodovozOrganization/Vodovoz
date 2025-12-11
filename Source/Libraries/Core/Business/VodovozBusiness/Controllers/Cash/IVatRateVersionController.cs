using System;
using Vodovoz.Core.Domain.Cash;

namespace VodovozBusiness.Controllers.Cash
{
	public interface IVatRateVersionController
	{
		VatRateVersion CreateAndAddVersion(DateTime? startDate = null);
		
		void ChangeVersionStartDate(VatRateVersion version, DateTime newStartDate);
		
		bool IsValidDateForNewVatRateVersion(DateTime dateTime);
		
		bool IsValidDateForVersionStartDateChange(VatRateVersion version, DateTime newStartDate);
	}
}

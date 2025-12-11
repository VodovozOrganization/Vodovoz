using System;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Controllers.Cash
{
	public class VatRateVersionController : IVatRateVersionController
	{
		private readonly Nomenclature _nomenclature;
		private readonly Organization _organization;

		public VatRateVersionController(Nomenclature nomenclature, Organization organization)
		{
			_nomenclature = nomenclature;
			_organization = organization;
		}
		
		public VatRateVersion CreateAndAddVersion(DateTime? startDate = null)
		{
			throw new NotImplementedException();
		}

		public void ChangeVersionStartDate(VatRateVersion version, DateTime newStartDate)
		{
			throw new NotImplementedException();
		}

		public bool IsValidDateForNewVatRateVersion(DateTime dateTime)
		{
			throw new NotImplementedException();
		}

		public bool IsValidDateForVersionStartDateChange(VatRateVersion version, DateTime newStartDate)
		{
			throw new NotImplementedException();
		}
	}
}

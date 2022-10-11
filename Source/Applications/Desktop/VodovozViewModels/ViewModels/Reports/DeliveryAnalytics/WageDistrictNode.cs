using System;
using QS.DomainModel.Entity;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.ViewModels.ViewModels.Reports.DeliveryAnalytics
{
	public class WageDistrictNode: PropertyChangedBase
	{
		private bool selected;
		public virtual bool Selected {
			get => selected;
			set => SetField(ref selected, value);
		}

		private WageDistrict _wageDistrict;
		public virtual WageDistrict WageDistrict {
			get => _wageDistrict;
			set => SetField(ref _wageDistrict, value);
		}

		public WageDistrictNode(WageDistrict wageDistrict)
		{
			WageDistrict = wageDistrict ?? throw new ArgumentNullException(nameof(wageDistrict));
		}

		public override string ToString()
		{
			return _wageDistrict.Name;
		}
	}
}
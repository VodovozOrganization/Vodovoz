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

		private WageSector _wageSector;
		public virtual WageSector WageSector {
			get => _wageSector;
			set => SetField(ref _wageSector, value);
		}

		public WageDistrictNode(WageSector wageSector)
		{
			WageSector = wageSector ?? throw new ArgumentNullException(nameof(wageSector));
		}

		public override string ToString()
		{
			return _wageSector.Name;
		}
	}
}
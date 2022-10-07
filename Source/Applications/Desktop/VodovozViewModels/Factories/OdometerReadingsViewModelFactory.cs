using QS.Services;
using System;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public class OdometerReadingsViewModelFactory : IOdometerReadingsViewModelFactory
	{
		private readonly ICommonServices _commonServices;

		public OdometerReadingsViewModelFactory(ICommonServices commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public OdometerReadingsViewModel CreateOdometerReadingsViewModel(Car car)
		{
			return new OdometerReadingsViewModel(car, _commonServices, new OdometerReadingsController(car));
		}
	}
}

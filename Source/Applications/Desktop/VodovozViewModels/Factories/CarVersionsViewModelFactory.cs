using QS.Services;
using System;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public class CarVersionsViewModelFactory : ICarVersionsViewModelFactory
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly ICommonServices _commonServices;

		public CarVersionsViewModelFactory(IRouteListRepository routeListRepository, ICommonServices commonServices)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public CarVersionsViewModel CreateCarVersionsViewModel(Car car)
		{
			return new CarVersionsViewModel(car, _commonServices, new CarVersionsController(car, _routeListRepository));
		}
	}
}

using System;
using QS.Services;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public class CarVersionsViewModelFactory : ICarVersionsViewModelFactory
	{
		private readonly ICommonServices _commonServices;

		public CarVersionsViewModelFactory(ICommonServices commonServices)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
		}

		public CarVersionsViewModel CreateCarVersionsViewModel(Car car)
		{
			var routeListRepository = new RouteListRepository(new StockRepository(), new BaseParametersProvider(new ParametersProvider()));
			return new CarVersionsViewModel(car, _commonServices, new CarVersionsController(car, routeListRepository));
		}
	}
}

using Autofac;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Services.Fuel;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public class FuelCardVersionViewModelFactory : IFuelCardVersionViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public FuelCardVersionViewModelFactory(
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public FuelCardVersionViewModel CreateFuelCardVersionViewModel(Car car, IUnitOfWork unitOfWork)
		{
			var fuelCardVersionService = _lifetimeScope.Resolve<IFuelCardVersionService>(
				new TypedParameter(typeof(Car), car));

			var fuelCardVersionViewModel = _lifetimeScope.Resolve<FuelCardVersionViewModel>(
				 new TypedParameter(typeof(Car), car),
				 new TypedParameter(typeof(IFuelCardVersionService), fuelCardVersionService));

			return fuelCardVersionViewModel;
		}
	}
}

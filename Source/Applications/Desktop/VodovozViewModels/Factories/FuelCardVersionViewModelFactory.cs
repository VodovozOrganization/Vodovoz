using Autofac;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Controllers;
using Vodovoz.Domain.Logistic.Cars;
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
			var fuelCardVersionController = _lifetimeScope.Resolve<IFuelCardVersionController>(
				new TypedParameter(typeof(Car), car));

			var fuelCardVersionViewModel = _lifetimeScope.Resolve<FuelCardVersionViewModel>(
				 new TypedParameter(typeof(Car), car),
				 new TypedParameter(typeof(IFuelCardVersionController), fuelCardVersionController));

			return fuelCardVersionViewModel;
		}
	}
}

using Autofac;
using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Application.Logistics.Fuel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.ViewModels.Factories
{
	public class AdditionalFuelTypeManagementViewModelFactory : IAdditionalFuelTypeManagementViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public AdditionalFuelTypeManagementViewModelFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public AdditionalFuelTypeManagementViewModel CreateAdditionalFuelTypeManagementViewModel(Car car, IUnitOfWork uow)
		{
			var additionalFuelTypeManagementService = new AdditionalFuelTypeManagementService(car);

			var additionalFuelTypeManagementViewModel = _lifetimeScope.Resolve<AdditionalFuelTypeManagementViewModel>(
				 new TypedParameter(typeof(Car), car),
				 new TypedParameter(typeof(IUnitOfWork), uow),
				 new TypedParameter(typeof(AdditionalFuelTypeManagementService), additionalFuelTypeManagementService)
				 );

			return additionalFuelTypeManagementViewModel;
		}
	}
}

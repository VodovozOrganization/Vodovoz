using Autofac;
using System;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;

namespace Vodovoz.ViewModels.Factories
{
	public class CarInsuranceVersionViewModelFactory : ICarInsuranceVersionViewModelFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public CarInsuranceVersionViewModelFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public CarInsuranceVersionViewModel CreateOsagoCarInsuranceVersionViewModel(Car car)
		{
			var viewModel = _lifetimeScope.Resolve<CarInsuranceVersionViewModel>(
				 new TypedParameter(typeof(Car), car));

			viewModel.InsuranceType = CarInsuranceType.Osago;

			return viewModel;
		}

		public CarInsuranceVersionViewModel CreateKaskoCarInsuranceVersionViewModel(Car car)
		{
			var viewModel = _lifetimeScope.Resolve<CarInsuranceVersionViewModel>(
				 new TypedParameter(typeof(Car), car));

			viewModel.InsuranceType = CarInsuranceType.Kasko;

			return viewModel;
		}
	}
}

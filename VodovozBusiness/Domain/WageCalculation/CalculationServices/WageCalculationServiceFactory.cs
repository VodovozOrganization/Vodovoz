using System;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageCalculationServiceFactory
	{
		private readonly Employee employee;
		private readonly IWageCalculationRepository wageCalculationRepository;

		public WageCalculationServiceFactory(Employee employee, IWageCalculationRepository wageCalculationRepository)
		{
			this.employee = employee;
			this.wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
		}

		public IRouteListWageCalculationService GetRouteListWageCalculationService(IRouteListWageCalculationSource source)
		{
			WageParameter actualWageParameter;

			if(source.DriverOfOurCar && !source.IsTruck) {
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
					actualWageParameter = wageCalculationRepository.GetActualParameterForOurCars(uow, source.RouteListDate);
				}
			} else {
				actualWageParameter = employee.GetActualWageParameter(source.RouteListDate);
			}

			if(actualWageParameter == null) {
				return new DefaultRouteListWageCalculationService();
			}

			switch(actualWageParameter.WageParameterType) {
				case WageParameterTypes.Manual:
					return new RouteListManualWageCalculationService((ManualWageParameter)actualWageParameter, source);
				case WageParameterTypes.Fixed:
					return new RouteListFixedWageCalculationService((FixedWageParameter)actualWageParameter, source);
				case WageParameterTypes.Percent:
					return new RouteListPercentWageCalculationService((PercentWageParameter)actualWageParameter, source);
				case WageParameterTypes.RatesLevel:
					return new RouteListRatesLevelWageCalculationService((RatesLevelWageParameter)actualWageParameter, source);
				case WageParameterTypes.OldRates:
					return new RouteListOldRatesWageCalculationService((OldRatesWageParameter)actualWageParameter, source);
				case WageParameterTypes.SalesPlan:
				default:
					return new DefaultRouteListWageCalculationService();
			}
		}
	}
}

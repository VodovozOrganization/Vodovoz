using System;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using QS.DomainModel.UoW;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.Services;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageCalculationServiceFactory
	{
		private readonly Employee employee;
		private readonly IWageCalculationRepository wageCalculationRepository;
		private readonly IWageParametersProvider wageParametersProvider;

		public WageCalculationServiceFactory(Employee employee, IWageCalculationRepository wageCalculationRepository, IWageParametersProvider wageParametersProvider)
		{
			this.employee = employee;
			this.wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			this.wageParametersProvider = wageParametersProvider ?? throw new ArgumentNullException(nameof(wageParametersProvider));
		}

		public IRouteListWageCalculationService GetRouteListWageCalculationService(IUnitOfWork uow, IRouteListWageCalculationSource source)
		{
			ChangeWageParameter(uow, source.RouteListId);

			WageParameter actualWageParameter = employee.GetActualWageParameter(source.RouteListDate);
			if(source.IsLargus && actualWageParameter is RatesLevelWageParameter) {
				actualWageParameter = wageCalculationRepository.GetActualParameterForOurCars(uow, source.RouteListDate);
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

		private void ChangeWageParameter(IUnitOfWork uow, int currentRouteListId)
		{
			if(employee.WageParameters.Count != 1) {
				return;
			}

			var startedWageParameter = employee.WageParameters.FirstOrDefault();
			if(startedWageParameter == null) {
				return;
			}

			IEnumerable<DateTime> workedDays = wageCalculationRepository.GetDaysWorkedWithRouteLists(uow, employee, currentRouteListId);
			DateTime lastWorkedDay = workedDays.Max();
			int daysWorkedNeeded = wageParametersProvider.GetDaysWorkedForMinRatesLevel();

			if(workedDays.Count() >= daysWorkedNeeded && startedWageParameter.IsStartedWageParameter && lastWorkedDay < DateTime.Today) {
				employee.ChangeWageParameter(
					new RatesLevelWageParameter {
						WageDistrictLevelRates = wageCalculationRepository.DefaultLevelForNewEmployees(uow),
						WageParameterTarget = WageParameterTargets.ForMercenariesCars
					}, 
					lastWorkedDay.AddDays(1)
				);
			}
		}
	}
}

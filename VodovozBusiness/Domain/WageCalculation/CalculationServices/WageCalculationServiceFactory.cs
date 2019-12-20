using System;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using QS.DomainModel.UoW;
using System.Linq;
using System.Collections.Generic;
using Vodovoz.Services;
using QS.Services;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageCalculationServiceFactory
	{
		private readonly Employee employee;
		private readonly IWageCalculationRepository wageCalculationRepository;
		private readonly IWageParametersProvider wageParametersProvider;
		private readonly IInteractiveService interactiveService;

		public WageCalculationServiceFactory(Employee employee, IWageCalculationRepository wageCalculationRepository, IWageParametersProvider wageParametersProvider, IInteractiveService interactiveService)
		{
			this.employee = employee;
			this.wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			this.wageParametersProvider = wageParametersProvider ?? throw new ArgumentNullException(nameof(wageParametersProvider));
			this.interactiveService = interactiveService;
		}

		public IRouteListWageCalculationService GetRouteListWageCalculationService(IUnitOfWork uow, IRouteListWageCalculationSource source)
		{
			//Нет необходимости пересчитывать зарплату для МЛ до этой даты
			//FIXME Возможно стоит эту дату вынести как параметр
			if(source.RouteListDate <= new DateTime(2019, 09, 30)) {
				return new WageCalculationServiceForOldRouteLists(source);
			}

			ChangeWageParameter(uow, source.RouteListId);

			WageParameter actualWageParameter = employee.GetActualWageParameter(source.RouteListDate);
			if(source.IsLargusOrGazelle && actualWageParameter is RatesLevelWageParameter) {
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
			//Проверка на то, что сотрудник имеет только один стартовый расчет зарплаты
			if(employee.WageParameters.Count != 1) {
				return;
			}
			var startedWageParameter = employee.WageParameters.FirstOrDefault();
			if(startedWageParameter == null || !startedWageParameter.IsStartedWageParameter) {
				return;
			}

			IEnumerable<DateTime> workedDays = wageCalculationRepository.GetDaysWorkedWithRouteLists(uow, employee).OrderBy(x => x);
			int daysWorkedNeeded = wageParametersProvider.GetDaysWorkedForMinRatesLevel();
			if(workedDays.Count() < daysWorkedNeeded || daysWorkedNeeded < 1) {
				return;
			}
			DateTime wageChangeDate = workedDays.ToArray()[daysWorkedNeeded-1].AddDays(1);

			employee.ChangeWageParameter(
				new RatesLevelWageParameter {
					WageDistrictLevelRates = wageCalculationRepository.DefaultLevelForNewEmployees(uow),
					WageParameterTarget = WageParameterTargets.ForMercenariesCars
				},
				wageChangeDate,
				interactiveService
			);
		}
	}
}

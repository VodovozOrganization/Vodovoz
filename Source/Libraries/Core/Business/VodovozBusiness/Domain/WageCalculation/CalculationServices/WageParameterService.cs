using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Services;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageParameterService : IWageParameterService
	{
		private readonly IWageCalculationRepository wageCalculationRepository;
		private readonly IWageSettings wageSettings;

		public WageParameterService(IWageCalculationRepository wageCalculationRepository, IWageSettings wageSettings)
		{
			this.wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			this.wageSettings = wageSettings ?? throw new ArgumentNullException(nameof(wageSettings));
		}

		public IRouteListWageCalculationService ActualizeWageParameterAndGetCalculationService(IUnitOfWork uow, Employee employee, IRouteListWageCalculationSource source)
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));
			if(employee == null) throw new ArgumentNullException(nameof(employee));
			if(source == null) throw new ArgumentNullException(nameof(source));

			//Не пересчитывать зарплату для МЛ до этой даты
			if(source.RouteListDate <= wageSettings.DontRecalculateWagesForRouteListsBefore)
			{
				return new WageCalculationServiceForOldRouteLists(source);
			}

			ActualizeWageParameter(uow, employee);

			EmployeeWageParameter actualWageParameter = employee.GetActualWageParameter(source.RouteListDate);

			return new RouteListWageCalculationService(actualWageParameter, source);
		}

		private void ActualizeWageParameter(IUnitOfWork uow, Employee employee)
		{
			//Проверка на то, что сотрудник имеет только один стартовый расчет зарплаты
			if(employee.WageParameters.Count != 1) return;

			var startedWageParameter = employee.WageParameters.FirstOrDefault();

			if(startedWageParameter == null || !startedWageParameter.IsStartedWageParameter) return;

			IEnumerable<DateTime> workedDays = wageCalculationRepository.GetDaysWorkedWithRouteLists(uow, employee).OrderBy(x => x);
			int daysWorkedNeeded = wageSettings.DaysWorkedForMinRatesLevel;

			if(workedDays.Count() < daysWorkedNeeded || daysWorkedNeeded < 1) return;

			DateTime wageChangeDate = workedDays.ToArray()[daysWorkedNeeded - 1].AddDays(1);

			var ratesLevelWageParameter = new EmployeeWageParameter
			{
				WageParameterItem = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = wageCalculationRepository.DefaultLevelForNewEmployees(uow)
				},
				WageParameterItemForOurCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = wageCalculationRepository.DefaultLevelForNewEmployeesOnOurCars(uow)
				},
				WageParameterItemForRaskatCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = wageCalculationRepository.DefaultLevelForNewEmployeesOnRaskatCars(uow)
				}
			};

			employee.ChangeWageParameter(
				ratesLevelWageParameter,
				wageChangeDate
			);
		}
	}
}

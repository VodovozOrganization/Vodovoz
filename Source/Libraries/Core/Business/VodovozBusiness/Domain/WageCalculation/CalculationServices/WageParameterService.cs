using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Services;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class WageParameterService : IWageParameterService
	{
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IWageSettings _wageSettings;

		public WageParameterService(IWageCalculationRepository wageCalculationRepository, IWageSettings wageSettings)
		{
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_wageSettings = wageSettings ?? throw new ArgumentNullException(nameof(wageSettings));
		}

		public IRouteListWageCalculationService ActualizeWageParameterAndGetCalculationService(IUnitOfWork uow, Employee employee, IRouteListWageCalculationSource source)
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));
			if(employee == null) throw new ArgumentNullException(nameof(employee));
			if(source == null) throw new ArgumentNullException(nameof(source));

			//Не пересчитывать зарплату для МЛ до этой даты
			if(source.RouteListDate <= _wageSettings.DontRecalculateWagesForRouteListsBefore)
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

			IEnumerable<DateTime> workedDays = _wageCalculationRepository.GetDaysWorkedWithRouteLists(uow, employee).OrderBy(x => x);
			int daysWorkedNeeded = _wageSettings.DaysWorkedForMinRatesLevel;

			if(workedDays.Count() < daysWorkedNeeded || daysWorkedNeeded < 1) return;

			DateTime wageChangeDate = workedDays.ToArray()[daysWorkedNeeded - 1].AddDays(1);

			var ratesLevelWageParameter = new EmployeeWageParameter
			{
				WageParameterItem = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = _wageCalculationRepository.DefaultLevelForNewEmployees(uow)
				},
				WageParameterItemForOurCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = _wageCalculationRepository.DefaultLevelForNewEmployeesOnOurCars(uow)
				},
				WageParameterItemForRaskatCars = new RatesLevelWageParameterItem
				{
					WageDistrictLevelRates = _wageCalculationRepository.DefaultLevelForNewEmployeesOnRaskatCars(uow)
				}
			};

			employee.ChangeWageParameter(
				ratesLevelWageParameter,
				wageChangeDate
			);
		}
		
		public Result TryCreateDefaultWageParameterForNewEmployee(IUnitOfWork uow, Employee employee)
		{
			if(employee.Id != 0)
			{
				return Result.Success();
			}

			var defaultLevel = _wageCalculationRepository.DefaultLevelForNewEmployees(uow);
			if(defaultLevel == null)
			{
				return Result.Failure(new Error(
					"500",
					"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Найм)!\""));
			}

			var defaultLevelForOurCar = _wageCalculationRepository.DefaultLevelForNewEmployeesOnOurCars(uow);
			if(defaultLevelForOurCar == null)
			{
				return Result.Failure(new Error(
					"500",
					"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для наших авто)!\""));
			}

			var defaultLevelForRaskatCar = _wageCalculationRepository.DefaultLevelForNewEmployeesOnRaskatCars(uow);
			if(defaultLevelForRaskatCar == null)
			{
				return Result.Failure(new Error(
					"500",
					"В журнале ставок по уровням не отмечен \"Уровень по умолчанию для новых сотрудников (Для авто в раскате)!\""));
			}

			employee.CreateDefaultWageParameterForNewEmployee(defaultLevel, defaultLevelForOurCar, defaultLevelForRaskatCar);
			return Result.Success();
		}
	}
}

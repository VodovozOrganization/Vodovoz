using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.EntityRepositories.WageCalculation
{
	public interface IWageCalculationRepository
	{
		IEnumerable<SalesPlan> AllSalesPlans(IUnitOfWork uow, bool hideArchive = true);
		IEnumerable<WageDistrict> AllWageDistricts(IUnitOfWork uow, bool hideArchive = true);
		IEnumerable<WageDistrictLevelRates> AllLevelRates(IUnitOfWork uow, bool hideArchive = true);
		WageDistrictLevelRates DefaultLevelForNewEmployees(IUnitOfWork uow);
		WageDistrictLevelRates DefaultLevelForNewEmployeesOnOurCars(IUnitOfWork uow);
		WageDistrictLevelRates DefaultLevelForNewEmployeesOnRaskatCars(IUnitOfWork uow);
		IEnumerable<DateTime> GetDaysWorkedWithRouteLists(IUnitOfWork uow, Employee employee);

		/// <summary>
		/// Все ставки по умолчанию для новых сотрудников (найм)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <returns></returns>
		IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployees(IUnitOfWork uow);
		/// <summary>
		/// Все ставки по умолчанию для новых сотрудников (наши авто)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <returns></returns>
		IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployeesOnOurCars(IUnitOfWork uow);
		/// <summary>
		/// Все ставки по умолчанию для новых сотрудников (раскат)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <returns></returns>
		IEnumerable<WageDistrictLevelRates> AllDefaultLevelForNewEmployeesOnRaskatCars(IUnitOfWork uow);
		/// <summary>
		/// Сброс флага "По умолчанию для новых сотрудников" (найм)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		void ResetExistinDefaultLevelsForNewEmployees(IUnitOfWork uow);
		/// <summary>
		/// Сброс флага "По умолчанию для новых сотрудников" (наши авто)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		void ResetExistinDefaultLevelsForNewEmployeesOnOurCars(IUnitOfWork uow);
		/// <summary>
		/// Сброс флага "По умолчанию для новых сотрудников" (раскат)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		void ResetExistinDefaultLevelsForNewEmployeesOnRaskatCars(IUnitOfWork uow);
	}
}

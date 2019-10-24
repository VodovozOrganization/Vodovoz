using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.WageCalculation;
using System;

namespace Vodovoz.EntityRepositories.WageCalculation
{
	public interface IWageCalculationRepository
	{
		IEnumerable<SalesPlan> AllSalesPlans(IUnitOfWork uow, bool hideArchive = true);
		IEnumerable<WageDistrict> AllWageDistricts(IUnitOfWork uow, bool hideArchive = true);
		IEnumerable<WageDistrictLevelRates> AllLevelRates(IUnitOfWork uow, bool hideArchive = true);
		IList<WageParameter> GetWageParameters(IUnitOfWork uow, WageParameterTargets wageParameterTarget);
		WageParameter GetActualParameterForOurCars(IUnitOfWork uow, DateTime dateTime);
		WageDistrictLevelRates DefaultLevelForNewEmployees(IUnitOfWork uow);
		IEnumerable<DateTime> GetDaysWorkedWithRouteLists(IUnitOfWork uow, Employee employee, int currentRouteListId);
	}
}
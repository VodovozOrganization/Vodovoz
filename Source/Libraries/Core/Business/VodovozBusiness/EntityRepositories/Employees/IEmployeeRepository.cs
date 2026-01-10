using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.Employees
{
	public interface IEmployeeRepository
	{
		QueryOver<Employee> ActiveEmployeeOrderedQuery();
		Employee GetEmployeeByAndroidLogin(
			IUnitOfWork uow,
			string login,
			ExternalApplicationType externalApplicationType = ExternalApplicationType.DriverApp);
		Employee GetEmployeeByAuthKey(IUnitOfWork uow, string authKey);
		Employee GetEmployeeByINNAndAccount(IUnitOfWork uow, string inn, string account);
		Employee GetEmployeeForCurrentUser(IUnitOfWork uow);
		IList<Employee> GetWorkingDriversAtDay(IUnitOfWork uow, DateTime date);
		IList<Employee> GetEmployeesForUser(IUnitOfWork uow, int userId);
		IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date);
		string GetEmployeePushTokenByOrderId(
			IUnitOfWork uow,
			int orderId,
			ExternalApplicationType externalApplicationType = ExternalApplicationType.DriverApp);
		EmployeeRegistration EmployeeRegistrationDuplicateExists(IUnitOfWorkFactory uowFactory, EmployeeRegistration registration);
		IEnumerable<Employee> GetSubscribedToPushNotificationsDrivers(IUnitOfWork uow);
		string GetDriverPushTokenById(IUnitOfWork unitOfWork, int notifyableEmployeeId);
		int? GetEmployeeCounterpartyFromDatabase(IUnitOfWorkFactory uowFactory, int employeeId);
		NamedDomainObjectNode GetOtherEmployeeInfoWithSameCounterparty(
			IUnitOfWorkFactory uowFactory, int employeeId, int counterpartyId);
		IEnumerable<int> GetControlledByEmployeeSubdivisionIds(IUnitOfWork uow, int employeeId);

		/// <summary>
		/// Возвращает список сотрудников с датами начала действия их последних параметров оплаты
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="employeeIds">Список Id сотрудников</param>
		/// <returns></returns>
		IEnumerable<EmployeeLastWageParameterStartDateNode> GetSelectedEmployeesWageParametersStartDate(IUnitOfWork uow, IEnumerable<int> employeeIds);

		/// <summary>
		/// Возвращает список водителей и экспедиторов, у которых есть параметры оплаты по уровню районов з/п
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="category">Категория сотрудника</param>
		/// <param name="wageDistrictLevelRatesIdFilter">Id ставки по зарплатным районам и уровням</param>
		/// <param name="isExcludeSelectedInFilterWageDistrictLevelRates">Исключить сотрудников с указанной ставкой по зарплатным районам и уровням</param>
		/// <returns></returns>
		IList<EmployeeNode> GetDriverForwarderEmployeesHavingWageDistrictLevelRates(IUnitOfWork uow, EmployeeCategory? category, int? wageDistrictLevelRatesIdFilter, bool isExcludeSelectedInFilterWageDistrictLevelRates);
	}
}

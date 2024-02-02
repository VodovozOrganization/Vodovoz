using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;

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
		EmployeeRegistration EmployeeRegistrationDuplicateExists(EmployeeRegistration registration);
		IEnumerable<Employee> GetSubscribedToPushNotificationsDrivers(IUnitOfWork uow);
	}
}

using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Employees
{
	public interface IEmployeeRepository
	{
		QueryOver<Employee> ActiveEmployeeOrderedQuery();
		Employee GetDriverByAndroidLogin(IUnitOfWork uow, string login);
		Employee GetDriverByAuthKey(IUnitOfWork uow, string authKey);
		Employee GetEmployeeByINNAndAccount(IUnitOfWork uow, string inn, string account);
		Employee GetEmployeeForCurrentUser(IUnitOfWork uow);
		IList<Employee> GetWorkingDriversAtDay(IUnitOfWork uow, DateTime date);
		IList<Employee> GetEmployeesForUser(IUnitOfWork uow, int userId);
		IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date);
		string GetEmployeePushTokenByOrderId(IUnitOfWork uow, int orderId);
		EmployeeRegistration EmployeeRegistrationDuplicateExists(IUnitOfWorkFactory uowFactory, EmployeeRegistration registration);
		IEnumerable<Employee> GetSubscribedToPushNotificationsDrivers(IUnitOfWork uow);
	}
}

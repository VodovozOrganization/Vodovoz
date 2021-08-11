using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Employees
{
	public interface IEmployeeRepository
	{
		QueryOver<Employee> ActiveDriversOrderedQuery();
		QueryOver<Employee> ActiveEmployeeOrderedQuery();
		QueryOver<Employee> ActiveEmployeeQuery();
		QueryOver<Employee> ActiveForwarderOrderedQuery();
		QueryOver<Employee> DriversQuery();
		QueryOver<Employee> ForwarderQuery();
		Employee GetDriverByAndroidLogin(IUnitOfWork uow, string login);
		Employee GetDriverByAuthKey(IUnitOfWork uow, string authKey);
		Employee GetEmployeeByINNAndAccount(IUnitOfWork uow, string inn, string account);
		Employee GetEmployeeForCurrentUser(IUnitOfWork uow);
		IList<Employee> GetWorkingDriversAtDay(IUnitOfWork uow, DateTime date);
		IList<Employee> GetEmployeesForUser(IUnitOfWork uow, int userId);
		IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date);
		QueryOver<Employee> OfficeWorkersQuery();
		string GetEmployeePushTokenByOrderId(IUnitOfWork uow, int orderId);
	}
}

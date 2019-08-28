using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Employees
{
	public class EmployeeRepository : IEmployeeRepository
	{
		private static EmployeeRepository instance;

		public static EmployeeRepository GetInstance()
		{
			if(instance == null)
				instance = new EmployeeRepository();
			return instance;
		}

		protected EmployeeRepository() { }

		public Employee GetEmployeeForCurrentUser(IUnitOfWork uow)
		{
			User userAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == QSProjectsLib.QSMain.User.Id)
				.SingleOrDefault();
		}

		public Employee GetDriverByAuthKey(IUnitOfWork uow, string authKey)
		{
			Employee employeeAlias = null;

			return uow.Session.QueryOver<Employee>(() => employeeAlias)
				.Where(() => employeeAlias.AndroidSessionKey == authKey)
				.Where(() => employeeAlias.IsFired == false)
				.SingleOrDefault();
		}

		public Employee GetDriverByAndroidLogin(IUnitOfWork uow, string login)
		{
			return uow.Session.QueryOver<Employee>()
				.Where(e => e.AndroidLogin == login)
				.SingleOrDefault();
		}

		public IList<Employee> GetEmployeesForUser(IUnitOfWork uow, int userId)
		{
			User userAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == userId)
				.List();
		}

		public Employee GetEmployeeByINNAndAccount(IUnitOfWork uow, string inn, string account)
		{
			IList<Account> accountsAlias = null;
			var employees = uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.Accounts, () => accountsAlias)
				.Where(e => e.INN == inn)
				.List();
			return employees.FirstOrDefault(e => e.Accounts.Any(acc => acc.Number == account));
		}

		public IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date)
		{
			EmployeeWorkChart ewcAlias = null;

			return uow.Session.QueryOver<EmployeeWorkChart>(() => ewcAlias)
				.Where(() => ewcAlias.Employee.Id == employee.Id)
				.Where(() => ewcAlias.Date.Month == date.Month)
				.Where(() => ewcAlias.Date.Year == date.Year)
				.List();
		}

		public QueryOver<Employee> DriversQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.driver);
		}

		public QueryOver<Employee> OfficeWorkersQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.office);
		}

		public QueryOver<Employee> ForwarderQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.forwarder);
		}

		public QueryOver<Employee> ActiveEmployeeQuery()
		{
			return QueryOver.Of<Employee>().Where(e => !e.IsFired);
		}

		public QueryOver<Employee> ActiveDriversOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.driver && !e.IsFired)
							.OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}

		public QueryOver<Employee> ActiveForwarderOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.forwarder && !e.IsFired)
							.OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}

		public QueryOver<Employee> ActiveEmployeeOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => !e.IsFired).OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}
	}
}

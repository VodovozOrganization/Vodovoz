using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using QSBanks;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repositories.HumanResources
{
	public static class EmployeeRepository
	{
		public static Employee GetEmployeeForCurrentUser (IUnitOfWork uow)
		{
			User userAlias = null;

			return uow.Session.QueryOver<Employee> ()
				.JoinAlias (e => e.User, () => userAlias)
				.Where (() => userAlias.Id == QSProjectsLib.QSMain.User.Id)
				.SingleOrDefault ();
		}

		public static Employee GetDriverByAuthKey(IUnitOfWork uow, string authKey) 
		{
			Employee employeeAlias = null;

			return uow.Session.QueryOver<Employee> (() => employeeAlias)
				.Where (() => employeeAlias.AndroidSessionKey == authKey)
				.Where (() => employeeAlias.IsFired == false)
				.SingleOrDefault ();
		}

		public static Employee GetDriverByAndroidLogin(IUnitOfWork uow, string login)
		{
			return uow.Session.QueryOver<Employee>()
				.Where(e => e.AndroidLogin == login)
				//.Where (() => employeeAlias.IsFired == false) - отключено, т.к. поле android_login в БД уникально!
				.SingleOrDefault();
		}

		public static IList<Employee> GetEmployeesForUser (IUnitOfWork uow, int userId)
		{
			User userAlias = null;

			return uow.Session.QueryOver<Employee> ()
				.JoinAlias (e => e.User, () => userAlias)
				.Where (() => userAlias.Id == userId)
				.List ();
		}

		public static Employee GetEmployeeByINNAndAccount (IUnitOfWork uow, string inn, string account)
		{
			IList<Account> accountsAlias = null;
			var employees = uow.Session.QueryOver<Employee> ()
				.JoinAlias (e => e.Accounts, () => accountsAlias)
				.Where (e => e.INN == inn)
				.List ();
			return employees.FirstOrDefault (e => e.Accounts.Any (acc => acc.Number == account));
		}

		public static IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date)
		{
			EmployeeWorkChart ewcAlias = null;

			return uow.Session.QueryOver<EmployeeWorkChart>(() => ewcAlias)
				.Where(() => ewcAlias.Employee.Id == employee.Id)
				.Where(() => ewcAlias.Date.Month == date.Month)
				.Where(() => ewcAlias.Date.Year == date.Year)
				.List();
		}

		public static QueryOver<Employee> DriversQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.driver);
		}

		public static QueryOver<Employee> OfficeWorkersQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.office);
		}

		public static QueryOver<Employee> ForwarderQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.forwarder);
		}

		public static QueryOver<Employee> ActiveEmployeeQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => !e.IsFired);
		}

		public static QueryOver<Employee> ActiveDriversOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.driver && !e.IsFired)
				            .OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}

		public static QueryOver<Employee> ActiveForwarderOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Category == EmployeeCategory.forwarder && !e.IsFired)
				            .OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}

		public static QueryOver<Employee> ActiveEmployeeOrderedQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => !e.IsFired).OrderBy (e => e.LastName).Asc.ThenBy (e => e.Name).Asc.ThenBy (e => e.Patronymic).Asc;
		}

		public static IList<Subdivision> Subdivisions(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<Subdivision>().OrderBy(s => s.Name).Asc().List();
		}
	}
}


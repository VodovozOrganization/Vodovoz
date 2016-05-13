using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSBanks;
using QSOrmProject;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Repository
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

		public static QueryOver<Employee> DriversQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.driver);
		}

		public static QueryOver<Employee> ForwarderQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.forwarder);
		}

		public static QueryOver<Employee> ActiveEmployeeQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => !e.IsFired);
		}

		public static QueryOver<Employee> ActiveEmployeeOrderedQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => !e.IsFired).OrderBy (e => e.LastName).Asc.ThenBy (e => e.Name).Asc.ThenBy (e => e.Patronymic).Asc;
		}
	}
}


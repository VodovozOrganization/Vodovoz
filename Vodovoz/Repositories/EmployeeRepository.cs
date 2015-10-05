using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain;
using NHibernate.Criterion;

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
			return QueryOver.Of<Employee> ().Where (e => !e.IsFired );
		}

	}
}


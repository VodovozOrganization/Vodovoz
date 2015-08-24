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

		public static QueryOver<Employee> DriversQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.driver);
		}

		public static QueryOver<Employee> ForwarderQuery ()
		{
			return QueryOver.Of<Employee> ().Where (e => e.Category == EmployeeCategory.forwarder);
		}

	}
}


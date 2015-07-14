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
				.Where (() => userAlias.Id == QSProjectsLib.QSMain.User.id)
				.SingleOrDefault ();
		}
	}
}


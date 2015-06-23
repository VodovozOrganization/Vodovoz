using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain;

namespace Vodovoz.Repository
{
	public static class EmployeeRepository
	{
		public static Employee GetEmployeeForCurrentUser(IUnitOfWork uow)
		{
			User userAlians = null;

			return uow.Session.QueryOver<Employee> ()
				.JoinAlias (e => e.User, () => userAlians)
				.Where (() => userAlians.Id == QSProjectsLib.QSMain.User.id)
				.SingleOrDefault ();
		}
	}
}


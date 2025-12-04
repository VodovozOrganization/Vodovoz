using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Employees
{
	public class EmployeeWithLoginRepository : IEmployeeWithLoginRepository
	{
		public EmployeeWithLogin GetEmployeeWithLogin(
			IUnitOfWork uow,
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp)
		{
			var query = from applicationUser in uow.Session.Query<ExternalApplicationUserForApi>()
						join employee in uow.Session.Query<EmployeeWithLogin>()
							on applicationUser.Employee.Id equals employee.Id
						where applicationUser.ExternalApplicationType == applicationType
							&& applicationUser.Login == userLogin
						select employee;

			return query.FirstOrDefault();
		}

		public EmployeeWithLogin GetEmployeeWithLoginById(IUnitOfWork uow, int id)
		{
			var query = from employee in uow.Session.Query<EmployeeWithLogin>()
						where employee.Id == id
						select employee;

			return query.FirstOrDefault();
		}
	}
}

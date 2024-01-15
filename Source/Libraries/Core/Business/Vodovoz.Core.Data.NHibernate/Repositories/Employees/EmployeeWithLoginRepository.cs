using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Data.Interfaces.Employees;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Employees
{
	public class EmployeeWithLoginRepository : IEmployeeWithLoginRepository
	{
		public EmployeeWithLogin GetEmployeeWithLogin(IUnitOfWork uow, string userLogin)
		{
			return uow.Session
				.Query<EmployeeWithLogin>()
				.FirstOrDefault(x => x.UserLogin == userLogin);
		}
	}
}

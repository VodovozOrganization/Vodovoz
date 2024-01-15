using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Employees;

namespace Vodovoz.Core.Data.Interfaces.Employees
{
	public interface IEmployeeWithLoginRepository
	{
		EmployeeWithLogin GetEmployeeWithLogin(IUnitOfWork uow, string userLogin);
	}
}

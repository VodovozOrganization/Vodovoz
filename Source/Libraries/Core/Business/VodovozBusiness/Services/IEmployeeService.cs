using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
		Employee GetEmployeeForCurrentUser();
		Employee GetEmployeeForCurrentUser(IUnitOfWork uow);
	}
}

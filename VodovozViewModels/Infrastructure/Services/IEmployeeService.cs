using System;
using Vodovoz.Domain.Employees;
using QS.DomainModel.UoW;
namespace Vodovoz.Infrastructure.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
	}
}

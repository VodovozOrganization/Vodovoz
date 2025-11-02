using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Settings.Employee;

namespace Vodovoz.Tools.CallTasks
{
	public interface ICallTaskFactory
	{
		CallTask CreateTask(IUnitOfWork uow, IEmployeeRepository employeeRepository, IEmployeeSettings employeeSettings, CallTask newTask = null, object source = null, string creationComment = null);

		CallTask FillNewTask(IUnitOfWork uow, CallTask callTask, IEmployeeRepository employeeRepository);
	}
}

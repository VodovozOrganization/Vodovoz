using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Data.Employees;
using Vodovoz.Core.Domain.Employees;

namespace Vodovoz.Core.Data.Interfaces.Employees
{
	public interface IEmployeeWithLoginRepository
	{
		EmployeeWithLogin GetEmployeeWithLogin(
			IUnitOfWork uow,
			string userLogin,
			ExternalApplicationType applicationType = ExternalApplicationType.WarehouseApp);
		EmployeeWithLogin GetEmployeeWithLoginById(IUnitOfWork uow, int id);
	}
}

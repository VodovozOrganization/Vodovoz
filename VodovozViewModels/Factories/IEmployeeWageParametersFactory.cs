using QS.DomainModel.UoW;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Factories
{
	public interface IEmployeeWageParametersFactory
	{
		EmployeeWageParametersViewModel CreateEmployeeWageParametersViewModel(Employee employee, ITdiTab tab, IUnitOfWork uow);
	}
}
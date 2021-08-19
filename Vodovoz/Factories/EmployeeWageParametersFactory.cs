using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Factories
{
	public class EmployeeWageParametersFactory : IEmployeeWageParametersFactory
	{
		public EmployeeWageParametersViewModel CreateEmployeeWageParametersViewModel(Employee employee, ITdiTab tab, IUnitOfWork uow)
		{
			var validator = new HierarchicalPresetPermissionValidator(
				EmployeeSingletonRepository.GetInstance(),
				new PermissionRepository());

			return new EmployeeWageParametersViewModel(employee, tab, uow, validator, UserSingletonRepository.GetInstance(),
				ServicesConfig.CommonServices, NavigationManagerProvider.NavigationManager, EmployeeSingletonRepository.GetInstance());
		}
	}
}
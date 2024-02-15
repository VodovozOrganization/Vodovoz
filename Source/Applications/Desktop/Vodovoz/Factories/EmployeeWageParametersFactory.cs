﻿using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Permissions;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Permissions;
using Vodovoz.EntityRepositories.WageCalculation;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.WageCalculation;

namespace Vodovoz.Factories
{
	public class EmployeeWageParametersFactory : IEmployeeWageParametersFactory
	{
		public EmployeeWageParametersViewModel CreateEmployeeWageParametersViewModel(Employee employee, ITdiTab tab, IUnitOfWork uow)
		{
			var employeeRepository = new EmployeeRepository();
			var userRepository = new UserRepository();
			var wageCalculationRepository = new WageCalculationRepository();
			
			var validator = new HierarchicalPresetPermissionValidator(
				employeeRepository,
				new PermissionRepository());

			return new EmployeeWageParametersViewModel(employee, tab, uow, validator, userRepository, ServicesConfig.CommonServices,
				NavigationManagerProvider.NavigationManager, employeeRepository, wageCalculationRepository);
		}
	}
}
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using System;
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
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IUserRepository _userRepository;
		private readonly ICommonServices _commonServices;
		private readonly IWageCalculationRepository _wageCalculationRepository;
		private readonly IPermissionRepository _permissionRepository;

		public EmployeeWageParametersFactory(
			IUnitOfWorkFactory uowFactory,
			IEmployeeRepository employeeRepository,
			IUserRepository userRepository,
			ICommonServices commonServices,
			IWageCalculationRepository wageCalculationRepository,
			IPermissionRepository permissionRepository)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_wageCalculationRepository = wageCalculationRepository ?? throw new ArgumentNullException(nameof(wageCalculationRepository));
			_permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
		}

		public EmployeeWageParametersViewModel CreateEmployeeWageParametersViewModel(Employee employee, ITdiTab tab, IUnitOfWork uow)
		{
			var validator = new HierarchicalPresetPermissionValidator(
				_uowFactory,
				_employeeRepository,
				_permissionRepository);

			return new EmployeeWageParametersViewModel(employee, tab, uow, validator, _userRepository, _commonServices,
				NavigationManagerProvider.NavigationManager, _employeeRepository, _wageCalculationRepository);
		}
	}
}

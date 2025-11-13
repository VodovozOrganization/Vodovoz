using Pacs.Core;
using QS.Services;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Application.Pacs
{
	public class PacsEmployeeProvider : IPacsEmployeeProvider, IPacsOperatorProvider, IPacsAdministratorProvider
	{
		private readonly IEmployeeService _employeeService;
		private readonly IPacsRepository _pacsRepository;
		private readonly IPermissionService _permissionService;

		private Employee _employee;

		public PacsEmployeeProvider(IEmployeeService employeeService, IPacsRepository pacsRepository, IPermissionService permissionService)
		{
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));

			LoadEmployee();
		}

		public int? EmployeeId
		{
			get
			{
				if(_employee == null)
				{
					LoadEmployee();
				}
				return _employee?.Id;
			}
		}

		public bool IsOperator { get; private set; }
		public bool IsAdministrator { get; private set; }

		int? IPacsOperatorProvider.OperatorId => IsOperator ? EmployeeId : null;

		int? IPacsAdministratorProvider.AdministratorId => IsAdministrator ? EmployeeId : null;

		private void LoadEmployee()
		{
			_employee = _employeeService.GetEmployeeForCurrentUser();
			if(_employee != null)
			{
				IsOperator = _pacsRepository.PacsEnabledFor(_employee.Id);
				IsAdministrator = _permissionService.ValidateUserPresetPermission(Vodovoz.Core.Domain.Permissions.PacsPermissions.IsAdministrator, _employee.User.Id);
			}
		}
	}
}

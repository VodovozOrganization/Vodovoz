using QS.Navigation;
using QS.Services;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IPacsViewModelFactory _pacsViewModelFactory;
		private readonly IPacsEmployeeProvider _pacsEmployeeProvider;
		private readonly IPacsRepository _pacsRepository;
		private readonly IPermissionService _permissionService;
		//private readonly Employee _employee;
		private bool _isOperator;
		private bool _isAdmin;

		public PacsViewModel(
			IPacsViewModelFactory pacsViewModelFactory,
			IPacsEmployeeProvider pacsEmployeeProvider,
			//IEmployeeService employeeService,
			//IPacsRepository pacsRepository,
			//IPermissionService permissionService,
			INavigationManager navigation) 
			: base(navigation)
		{
			/*if(employeeService is null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}*/
			_pacsViewModelFactory = pacsViewModelFactory ?? throw new ArgumentNullException(nameof(pacsViewModelFactory));
			_pacsEmployeeProvider = pacsEmployeeProvider ?? throw new ArgumentNullException(nameof(pacsEmployeeProvider));
			//_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			//_permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));

			Title = "СКУД";

			var pacsEmployeeId = _pacsEmployeeProvider.EmployeeId;
			if(pacsEmployeeId == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.", 
					"Не настроен пользователь");
			}

			try
			{
				if(_pacsEmployeeProvider.IsOperator)
				{
					OperatorViewModel = _pacsViewModelFactory.CreateOperatorViewModel();
				}

				if(_pacsEmployeeProvider.IsAdministrator)
				{
					DashboardViewModel = _pacsViewModelFactory.CreateDashboardViewModel();
					SettingsViewModel = _pacsViewModelFactory.CreateSettingsViewModel();
					ReportsViewModel = _pacsViewModelFactory.CreateReportsViewModel();
				}
			}
			catch(Exception ex)
			{
				Dispose();
				throw;
			}
		}

		public PacsOperatorViewModel OperatorViewModel { get; private set; }
		public PacsDashboardViewModel DashboardViewModel { get; private set; }
		public PacsSettingsViewModel SettingsViewModel { get; private set; }
		public PacsReportsViewModel ReportsViewModel { get; private set; }

		public void Dispose()
		{
			DashboardViewModel?.Dispose();
		}
	}
}

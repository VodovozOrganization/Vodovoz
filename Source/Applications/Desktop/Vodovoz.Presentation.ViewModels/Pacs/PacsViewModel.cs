using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsViewModel : DialogViewModelBase
	{
		private readonly IPacsViewModelFactory _pacsViewModelFactory;
		private readonly Employee _employee;
		private bool _isOperator;
		private bool _isAdmin;

		public PacsViewModel(IPacsViewModelFactory pacsViewModelFactory, IEmployeeService employeeService, INavigationManager navigation) 
			: base(navigation)
		{
			if(employeeService is null)
			{
				throw new ArgumentNullException(nameof(employeeService));
			}
			_pacsViewModelFactory = pacsViewModelFactory ?? throw new ArgumentNullException(nameof(pacsViewModelFactory));

			_employee = employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				throw new AbortCreatingPageException(
					"Должен быть привязан сотрудник к пользователю. Обратитесь в отдел кадров.", 
					"На настроен пользователь");
			}

			if(_isOperator)
			{
				OperatorViewModel = _pacsViewModelFactory.CreateOperatorViewModel();
			}

			if(_isAdmin)
			{
				DashboardViewModel = _pacsViewModelFactory.CreateDashboardViewModel();
				SettingsViewModel = _pacsViewModelFactory.CreateSettingsViewModel();
				ReportsViewModel = _pacsViewModelFactory.CreateReportsViewModel();
			}
		}

		public PacsOperatorViewModel OperatorViewModel { get; private set; }
		public PacsDashboardViewModel DashboardViewModel { get; private set; }
		public PacsSettingsViewModel SettingsViewModel { get; private set; }
		public PacsReportsViewModel ReportsViewModel { get; private set; }
		public bool CanEdit { get; private set; }
	}
}

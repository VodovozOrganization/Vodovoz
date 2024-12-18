using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.Application.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsViewModel : DialogViewModelBase, IDisposable
	{
		private readonly IPacsViewModelFactory _pacsViewModelFactory;
		private readonly IPacsEmployeeProvider _pacsEmployeeProvider;

		public PacsViewModel(
			IPacsViewModelFactory pacsViewModelFactory,
			IPacsEmployeeProvider pacsEmployeeProvider,
			INavigationManager navigation) 
			: base(navigation)
		{
			_pacsViewModelFactory = pacsViewModelFactory ?? throw new ArgumentNullException(nameof(pacsViewModelFactory));
			_pacsEmployeeProvider = pacsEmployeeProvider ?? throw new ArgumentNullException(nameof(pacsEmployeeProvider));

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
			catch(Exception)
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
			OperatorViewModel?.Dispose();
		}
	}
}

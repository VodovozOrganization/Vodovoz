using Gtk;
using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Pacs;

namespace Vodovoz.Views.Pacs
{
	[ToolboxItem(true)]
	public partial class PacsView : DialogViewBase<PacsViewModel>
	{
		private PacsOperatorView _operatorView;
		private PacsDashboardView _dashboardView;
		private PacsSettingsView _settingsView;
		private PacsReportsView _reportsView;

		public PacsView(PacsViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ViewModel.PropertyChanged += ViewModelPropertyChanged;

			buttonOperator.Clicked += OperatorTabSelected;
			buttonDashboard.Clicked += DashboardTabSelected;
			buttonSettings.Clicked += SettingsTabSelected;
			buttonReports.Clicked += ReportsTabSelected;

			ConfigOperatorView();
			ConfigDashboardView();
			ConfigSettingsView();
			ConfigReportsView();
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(PacsViewModel.OperatorViewModel):
					ConfigOperatorView();
					break;
				case nameof(PacsViewModel.DashboardViewModel):
					ConfigDashboardView();
					break;
				case nameof(PacsViewModel.SettingsViewModel):
					ConfigSettingsView();
					break;
				case nameof(PacsViewModel.ReportsViewModel):
					ConfigReportsView();
					break;
				default:
					break;
			}
		}

		private void ConfigOperatorView()
		{
			if(ViewModel.OperatorViewModel == null)
			{
				notebookPacs.RemovePage(0);
				_operatorView = null;
				buttonOperator.Visible = false;
			}
			else
			{
				if(_operatorView == null)
				{
					_operatorView = new PacsOperatorView();
					notebookPacs.InsertPage(_operatorView, new Label("Оператор"), 0);
					_operatorView.Show();
					buttonOperator.Visible = true;
				}
				_operatorView.ViewModel = ViewModel.OperatorViewModel;
			}
		}

		private void ConfigDashboardView()
		{
			if(ViewModel.DashboardViewModel == null)
			{
				notebookPacs.RemovePage(1);
				_dashboardView = null;
				buttonDashboard.Visible = false;
			}
			else
			{
				if(_dashboardView == null)
				{
					_dashboardView = new PacsDashboardView();
					notebookPacs.InsertPage(_dashboardView, new Label("Сводка"), 1);
					_dashboardView.Show();
					buttonDashboard.Visible = true;
				}
				_dashboardView.ViewModel = ViewModel.DashboardViewModel;
			}
		}


		private void ConfigSettingsView()
		{
			if(ViewModel.SettingsViewModel == null)
			{
				notebookPacs.RemovePage(2);
				_settingsView = null;
				buttonSettings.Visible = false;
			}
			else
			{
				if(_settingsView == null)
				{
					_settingsView = new PacsSettingsView();
					notebookPacs.InsertPage(_settingsView, new Label("Настройки"), 2);
					_settingsView.Show();
					buttonSettings.Visible = true;
				}
				_settingsView.ViewModel = ViewModel.SettingsViewModel;
			}
		}

		private void ConfigReportsView()
		{
			if(ViewModel.ReportsViewModel == null)
			{
				notebookPacs.RemovePage(3);
				_reportsView = null;
				buttonReports.Visible = false;
			}
			else
			{
				if(_reportsView == null)
				{
					_reportsView = new PacsReportsView();
					notebookPacs.InsertPage(_reportsView, new Label("Отчеты"), 3);
					_reportsView.Show();
					buttonReports.Visible = true;
				}
				_reportsView.ViewModel = ViewModel.ReportsViewModel;
			}
		}

		private void OperatorTabSelected(object sender, System.EventArgs e)
		{
			if(buttonOperator.Active)
			{
				notebookPacs.CurrentPage = notebookPacs.PageNum(_operatorView);
			}
		}

		private void DashboardTabSelected(object sender, System.EventArgs e)
		{
			if(buttonDashboard.Active)
			{
				notebookPacs.CurrentPage = notebookPacs.PageNum(_dashboardView);
			}
		}

		private void SettingsTabSelected(object sender, System.EventArgs e)
		{
			if(buttonSettings.Active)
			{
				notebookPacs.CurrentPage = notebookPacs.PageNum(_settingsView);
			}
		}

		private void ReportsTabSelected(object sender, System.EventArgs e)
		{
			if(buttonReports.Active)
			{
				notebookPacs.CurrentPage = notebookPacs.PageNum(_reportsView);
			}
		}
	}
}

using QS.Dialog;
using QS.Views;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.ReportsParameters;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.Reports
{
	public partial class ProfitabilitySalesReportView : ViewBase<ProfitabilitySalesReportViewModel>
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IInteractiveService _interactiveService;
		private SelectableParameterReportFilterView _filterView;

		public ProfitabilitySalesReportView(ProfitabilitySalesReportViewModel viewModel) : base(viewModel)
		{
			this.Build();

			ConfigureDlg();
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ConfigureDlg()
		{
			buttonInfo.Clicked += (s, e) => ViewModel.ShowInfoCommand.Execute();
			ViewModel.ShowInfoCommand.CanExecuteChanged += (s, e) => buttonInfo.Sensitive = ViewModel.ShowInfoCommand.CanExecute();

			buttonCreateReport.Clicked += (s, e) => ViewModel.LoadReportCommand.Execute();
			ViewModel.LoadReportCommand.CanExecuteChanged += (s, e) => buttonCreateReport.Sensitive = ViewModel.LoadReportCommand.CanExecute();

			dateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonPhones.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanEditShowPhones, w => w.Sensitive)
				.AddBinding(vm => vm.ShowPhones, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonDetail.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsDetailed, w => w.Active)
				.InitializeFromSource();

			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			ShowFilter();
		}

		private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(ViewModel.FilterViewModel):
					ShowFilter();
					break;
				default:
					break;
			}
		}

		private void ShowFilter()
		{
			_filterView?.Destroy();
			_filterView = new SelectableParameterReportFilterView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}
	}
}

using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Profitability;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class ProfitabilitySalesReportView : ViewBase<ProfitabilitySalesReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;

		public ProfitabilitySalesReportView(ProfitabilitySalesReportViewModel viewModel) : base(viewModel)
		{
			Build();

			ConfigureDlg();
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

			ycheckbuttonDetail.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsDetailed, w => w.Active)
				.InitializeFromSource();

			orderdatefilterview2.ViewModel = ViewModel.OrderDateFilterViewModel;
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			ShowFilter();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
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
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			vboxParameters.Add(_filterView);
			_filterView.Show();
		}
	}
}

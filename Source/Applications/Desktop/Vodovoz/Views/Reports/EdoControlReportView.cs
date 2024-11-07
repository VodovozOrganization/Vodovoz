using QS.Views.GtkUI;
using Vodovoz.ViewModels.Bookkeepping.Reports.EdoControl;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Views.Reports
{
	public partial class EdoControlReportView : TabViewBase<EdoControlReportViewModel>
	{
		private IncludeExludeFiltersView _filterView;

		public EdoControlReportView(EdoControlReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = 600;
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ShowIncludeExludeFilter();
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;
		}

		private void ShowIncludeExludeFilter()
		{
			_filterView?.Destroy();
			_filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);
			yvboxParameters.Add(_filterView);
			_filterView.HeightRequest = ViewModel.FilterViewModel.Filters.Count * 21 + 70;
			_filterView.Show();
		}
	}
}

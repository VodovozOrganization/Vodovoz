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
			hpanedMain.Position = 500;
			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;

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

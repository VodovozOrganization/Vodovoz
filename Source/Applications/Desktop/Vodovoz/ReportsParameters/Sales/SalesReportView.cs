using QS.Views;
using QSReport;
using System;
using Vodovoz.ViewModels.ReportsParameters;
using Vodovoz.ViewWidgets.Reports;

namespace Vodovoz.Reports
{
	public partial class SalesReportView : ViewBase<SalesReportViewModel>
	{
		public event EventHandler<LoadReportEventArgs> LoadReport;

		public SalesReportView(SalesReportViewModel viewModel)
			:base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			dateperiodpicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			buttonInfo.Clicked += (sender, args) => ViewModel.ShowInfoCommand.Execute();

			ycheckbuttonDetail.Binding
				.AddBinding(ViewModel, vm => vm.IsDetailed, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanAccessSalesReports, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonPhones.Binding
				.AddBinding(ViewModel, vm => vm.CanShowPhones, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.ShowPhones, w => w.Active)
				.InitializeFromSource();

			var filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);

			vboxParameters.Add(filterView);
			filterView.Show();

			leftrightlistview.ViewModel = ViewModel.GroupingSelectViewModel;
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			ViewModel.GenerateReportCommand.Execute();
		}
	}
}

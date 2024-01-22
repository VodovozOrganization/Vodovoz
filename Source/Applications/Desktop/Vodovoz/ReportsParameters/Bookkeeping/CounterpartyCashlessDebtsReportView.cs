using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Bookkeeping;
using Vodovoz.ViewWidgets.Reports;
namespace Vodovoz.ReportsParameters.Bookkeeping
{
	public partial class CounterpartyCashlessDebtsReportView : ViewBase<CounterpartyCashlessDebtsReportViewModel>
	{
		public CounterpartyCashlessDebtsReportView(CounterpartyCashlessDebtsReportViewModel viewModel) : base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			periodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ybuttonInfo.Clicked += (sender, args) => ViewModel.ShowInfoMessageCommand.Execute();

			ycheckExcludeClosingDocuments.Binding
				.AddBinding(ViewModel, vm => vm.IsExcludeClosingDocuments, w => w.Active)
				.InitializeFromSource();

			ycheckExpiredOnly.Binding
				.AddBinding(ViewModel, vm => vm.IsExpiredOnly, w => w.Active)
				.InitializeFromSource();

			ycheckOrderByDate.Binding
				.AddBinding(ViewModel, vm => vm.IsOrderByDate, w => w.Active)
				.InitializeFromSource();

			var filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);

			yvboxParameters.Add(filterView);
			filterView.Show();
		}
	}
}

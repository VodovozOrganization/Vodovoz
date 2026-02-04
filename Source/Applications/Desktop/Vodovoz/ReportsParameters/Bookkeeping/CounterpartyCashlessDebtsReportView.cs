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

			ycheckOrderByDate.Binding
				.AddBinding(ViewModel, vm => vm.IsOrderByDate, w => w.Active)
				.InitializeFromSource();

			ybuttonCounterpartyDebtDetails.Binding
				.AddBinding(ViewModel, vm => vm.IsCanCreateCounterpartyDebtDetailsReport, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonShowPhones.Binding
				.AddBinding(ViewModel, vm => vm.ShowPhones, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CanShowPhones, w => w.Sensitive)
				.InitializeFromSource();

			var filterView = new IncludeExludeFiltersView(ViewModel.FilterViewModel);

			yvboxFilter.Add(filterView);
			filterView.Show();

			ybuttonInfo.Clicked += (s, e) => ViewModel.ShowInfoMessageCommand.Execute();
			ybuttonCounterpartyDebtBalance.Clicked += (s, e) => ViewModel.GenerateCompanyDebtBalanceReportCommand.Execute();
			ybuttonNotPaidOrders.Clicked += (s, e) => ViewModel.GenerateNotPaidOrdersReportCommand.Execute();
			ybuttonCounterpartyDebtDetails.Clicked += (s, e) => ViewModel.GenerateCounterpartyDebtDetailsReportCommand.Execute();
		}
	}
}

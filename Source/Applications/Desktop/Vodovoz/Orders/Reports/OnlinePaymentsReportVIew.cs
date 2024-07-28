using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.ViewModels.Orders.Reports;

namespace Vodovoz.Orders.Reports
{
	[ToolboxItem(true)]
	public partial class OnlinePaymentsReportView : DialogViewBase<OnlinePaymentsReportViewModel>
	{
		public OnlinePaymentsReportView(OnlinePaymentsReportViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			daterangepicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsDateTimeRangeCustomPeriod, w => w.Sensitive)
				.AddBinding(vm => vm.StartDate, w => w.StartDate)
				.AddBinding(vm => vm.EndDate, w => w.EndDate)
				.InitializeFromSource();

			yradiobuttonYesterday.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeYesterday, w => w.Active)
				.InitializeFromSource();

			yradiobuttonLast3Days.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeLast3Days, w => w.Active)
				.InitializeFromSource();

			yradiobuttonCustomPeriod.Binding
				.AddBinding(ViewModel, vm => vm.IsDateTimeRangeCustomPeriod, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxShop.ShowSpecialStateAll = true;
			speciallistcomboboxShop.SetRenderTextFunc<string>(o =>
				string.IsNullOrWhiteSpace(o) ? "{ нет названия }" : o);

			speciallistcomboboxShop.ItemsList = ViewModel.Shops;
			speciallistcomboboxShop.Binding.AddBinding(ViewModel, vm => vm.SelectedShop, w => w.SelectedItem)
				.InitializeFromSource();
		}
	}
}

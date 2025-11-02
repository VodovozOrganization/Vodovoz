using QS.Views;
using System.ComponentModel;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters
{
	[ToolboxItem(true)]
	public partial class CardPaymentsOrdersReport : ViewBase<CardPaymentsOrdersReportViewModel>
	{
		public CardPaymentsOrdersReport(CardPaymentsOrdersReportViewModel viewModel) : base(viewModel)
		{
			Build();

			ydateperiodpicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			comboPaymentFrom.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PaymentsFrom, w => w.ItemsList)
				.AddBinding(vm => vm.PaymentFrom, w => w.SelectedItem)
				.InitializeFromSource();
			comboPaymentFrom.Changed += (sender, e) => ViewModel.AllPaymentsFromSelected = comboPaymentFrom.IsSelectedAll;

			comboGeoGroup.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.GeoGroups, w => w.ItemsList)
				.AddBinding(vm => vm.GeoGroup, w => w.SelectedItem)
				.InitializeFromSource();
			comboGeoGroup.Changed += (sender, e) => ViewModel.AllGeoGroupsSelected = comboGeoGroup.IsSelectedAll;

			ycheckbuttonShowArchive.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.AllPaymentsFromSelected, w => w.Sensitive)
				.AddBinding(vm => vm.ShowArchive, w => w.Active)
				.InitializeFromSource();

			buttonCreateRepot.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}

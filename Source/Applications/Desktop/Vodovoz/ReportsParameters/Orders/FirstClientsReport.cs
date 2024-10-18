using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Views;
using Vodovoz.ViewModels.ReportsParameters.Orders;

namespace Vodovoz.ReportsParameters.Orders
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FirstClientsReport : ViewBase<FirstClientsReportViewModel>, ISingleUoWDialog
	{
		public FirstClientsReport(FirstClientsReportViewModel viewModel) : base(viewModel)
		{
			Build();

			datePeriodPicker.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			yCpecCmbDiscountReason.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DiscountReasons, w => w.ItemsList)
				.AddBinding(vm => vm.DiscountReason, w => w.SelectedItem)
				.InitializeFromSource();

			yChooseOrderStatus.ItemsEnum = ViewModel.OrderStatusType;
			yChooseOrderStatus.ShowSpecialStateAll = true;
			yChooseOrderStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.OrderStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yChooseThePaymentTypeForTheOrder.ItemsEnum = ViewModel.PaymentTypeType;
			yChooseThePaymentTypeForTheOrder.ShowSpecialStateAll = true;
			yChooseThePaymentTypeForTheOrder.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.PaymentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryDistrict.SetEntityAutocompleteSelectorFactory(ViewModel.DistrictSelectorFactory);
			entryDistrict.CanEditReference = false;
			entryDistrict.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.District, w => w.Subject)
				.InitializeFromSource();

			chkBtnWithPromotionalSets.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.WithPromosets, w => w.Active)
				.InitializeFromSource();

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);
		}

		public IUnitOfWork UoW => ViewModel.UoW;
	}
}

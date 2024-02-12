using QS.Views.GtkUI;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Filters.GtkViews
{
	public partial class UndeliveredOrdersFilterView : FilterViewBase<UndeliveredOrdersFilterViewModel>
	{
		public UndeliveredOrdersFilterView(UndeliveredOrdersFilterViewModel filterViewModel) : base(filterViewModel)
		{
			Build();
			Initialize();
		}
		private void Initialize()
		{
			chkProblematicCases.Binding.AddBinding(ViewModel, vm => vm.RestrictIsProblematicCases, w => w.Active).InitializeFromSource();

			yEnumCMBUndeliveryStatus.ItemsEnum = typeof(UndeliveryStatus);
			yEnumCMBUndeliveryStatus.Binding.AddBinding(ViewModel, vm => vm.RestrictUndeliveryStatus, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumCMBActionWithInvoice.ItemsEnum = typeof(ActionsWithInvoice);
			yEnumCMBActionWithInvoice.Binding.AddBinding(ViewModel, vm => vm.RestrictActionsWithInvoice, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumStatusOnOldOrderCancels.ItemsEnum = typeof(OrderStatus);
			yEnumStatusOnOldOrderCancels.Binding.AddBinding(ViewModel, vm => vm.OldOrderStatus, w => w.SelectedItemOrNull).InitializeFromSource();

			entryInProcessAtSubdivision.ViewModel = ViewModel.InProcessAtSubdivisionViewModel;

			ySpecCMBGuiltyDep.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Subdivisions, w => w.ItemsList)
				.AddBinding(vm => vm.RestrictGuiltyDepartment, w => w.SelectedItem)
				.AddBinding(vm => vm.RestrictGuiltyDepartmentVisible, w => w.Visible)
				.InitializeFromSource();

			ylabelGuiltyDep.Binding.AddBinding(ViewModel, vm => vm.RestrictGuiltyDepartmentVisible, w => w.Visible).InitializeFromSource();

			yEnumCMBGuilty.ItemsEnum = typeof(GuiltyTypes);
			yEnumCMBGuilty.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RestrictGuiltySide, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.RestrictNotIsProblematicCases, w => w.Sensitive)
				.InitializeFromSource();

			entryOldOrder.SetEntityAutocompleteSelectorFactory(ViewModel.OrderSelectorFactory);
			entryOldOrder.Binding.AddBinding(ViewModel, vm => vm.RestrictOldOrder, w => w.Subject).InitializeFromSource();

			entryDriver.SetEntityAutocompleteSelectorFactory(ViewModel.DriverEmployeeSelectorFactory);
			entryDriver.Binding.AddBinding(ViewModel, vm => vm.RestrictDriver, w => w.Subject).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.RestrictClient, w => w.Subject).InitializeFromSource();

			entryDeliveryPoint.SetEntityAutocompleteSelectorFactory(ViewModel.DeliveryPointSelectorFactory);
			entryDeliveryPoint.Binding.AddBinding(ViewModel, vm => vm.RestrictAddress, w => w.Subject).InitializeFromSource();

			entryOldOrderAuthor.ViewModel = ViewModel.OldOrderAuthorViewModel;

			entryUndeliveryAuthor.ViewModel = ViewModel.UndeliveryAuthorViewModel;

			dateperiodOldOrderDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RestrictOldOrderStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.RestrictOldOrderEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			dateperiodNewOrderDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RestrictNewOrderStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.RestrictNewOrderEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryAuthorSubdivision.ViewModel = ViewModel.AuthorSubdivisionViewModel;
		}
	}
}

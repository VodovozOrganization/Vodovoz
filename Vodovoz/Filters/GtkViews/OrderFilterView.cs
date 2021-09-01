using System;
using System.Linq;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderFilterView : FilterViewBase<OrderJournalFilterViewModel>
	{
		public OrderFilterView(OrderJournalFilterViewModel orderJournalFilterViewModel) : base(orderJournalFilterViewModel)
		{
			this.Build();
			Configure();
			InitializeRestrictions();
		}

		private void Configure()
		{
			enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			enumcomboStatus.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumcomboPaymentType.ItemsEnum = typeof(PaymentType);
			enumcomboPaymentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangePaymentType, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictPaymentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryCounterparty.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CounterpartySelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.AddBinding(vm => vm.CanChangeCounterparty, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictCounterparty, w => w.Subject)
				.InitializeFromSource();

			entryDeliveryPoint.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryPoint, w => w.Subject)
				.AddFuncBinding(vm => vm.CanChangeDeliveryPoint && vm.RestrictCounterparty != null, w => w.Sensitive)
				.AddBinding(vm => vm.DeliveryPointSelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.InitializeFromSource();

			dateperiodOrders.StartDateOrNull = DateTime.Today.AddDays(ViewModel.DaysToBack);
			dateperiodOrders.EndDateOrNull = DateTime.Today.AddDays(ViewModel.DaysToForward);
			dateperiodOrders.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RestrictStartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.RestrictEndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckOnlySelfdelivery.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeOnlySelfDelivery, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictOnlySelfDelivery, w => w.Active, new NullableBooleanToBooleanConverter())
				.InitializeFromSource();
			ycheckWithoutSelfdelivery.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeWithoutSelfDelivery, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictWithoutSelfDelivery, w => w.Active, new NullableBooleanToBooleanConverter())
				.InitializeFromSource();
			ycheckOnlyServices.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeOnlyService, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictOnlyService, w => w.Active, new NullableBooleanToBooleanConverter())
				.InitializeFromSource();
			ycheckHideServices.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeHideService, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictHideService, w => w.Active, new NullableBooleanToBooleanConverter())
				.InitializeFromSource();
			ycheckLessThreeHours.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeLessThreeHours, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictLessThreeHours, w => w.Active, new NullableBooleanToBooleanConverter())
				.InitializeFromSource();
			ycheckSortDeliveryDate.Binding
				.AddBinding(ViewModel, vm => vm.SortDeliveryDate, w => w.Active, new NullableBooleanToBooleanConverter())
				.AddBinding(ViewModel, vm => vm.SortDeliveryDateVisibility, w => w.Visible)
				.InitializeFromSource();

			yenumcomboboxPaymentOrder.ItemsEnum = typeof(PaymentOrder);
			yenumcomboboxPaymentOrder.Binding.AddBinding(ViewModel, vm => vm.PaymentOrder, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yenumcomboboxViewTypes.ItemsEnum = typeof(ViewTypes);
			yenumcomboboxViewTypes.Binding.AddBinding(ViewModel, vm => vm.ViewTypes, w => w.SelectedItem).InitializeFromSource();
			yenumСmbboxOrderPaymentStatus.ItemsEnum = typeof(OrderPaymentStatus);
			yenumСmbboxOrderPaymentStatus.Binding.AddBinding(ViewModel, vm => vm.OrderPaymentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yenumcomboboxDateType.ItemsEnum = typeof(OrdersDateFilterType);
			yenumcomboboxDateType.Binding.AddBinding(ViewModel, x => x.FilterDateType, w => w.SelectedItem).InitializeFromSource();


			speciallistCmbOrganisations.ItemsList = ViewModel.Organisations;
			speciallistCmbOrganisations.Binding.AddBinding(ViewModel, vm => vm.Organisation, w => w.SelectedItem).InitializeFromSource();
			speciallistCmbPaymentsFrom.ItemsList = ViewModel.PaymentsFrom;
			speciallistCmbPaymentsFrom.Binding.AddBinding(ViewModel, vm => vm.PaymentByCardFrom, w => w.SelectedItem)
				.InitializeFromSource();
			speciallistCmbPaymentsFrom.Binding.AddBinding(ViewModel, vm => vm.PaymentsFromVisibility, w => w.Visible)
				.InitializeFromSource();
			ylblPaymentFrom.Binding.AddBinding(ViewModel, vm => vm.PaymentsFromVisibility, w => w.Visible).InitializeFromSource();
			
			ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
			ySpecCmbGeographicGroup.Binding.AddBinding(ViewModel, vm => vm.GeographicGroup, w => w.SelectedItem).InitializeFromSource();
		}

		private void InitializeRestrictions()
		{
			#region OrderStatusRestriction

			if(ViewModel.AllowStatuses != null)
			{
				enumcomboStatus.ClearEnumHideList();
				enumcomboStatus.AddEnumToHideList(Enum.GetValues(typeof(OrderStatus)).Cast<OrderStatus>()
					.Where(item => !ViewModel.AllowStatuses.Contains(item)).Cast<object>().ToArray());
			}

			if(ViewModel.HideStatuses != null)
			{
				enumcomboStatus.AddEnumToHideList(ViewModel.HideStatuses);
			}

			#endregion OrderStatusRestriction

			#region PaymentTypeRestriction

			if(ViewModel.AllowPaymentTypes != null)
			{
				enumcomboPaymentType.ClearEnumHideList();
				enumcomboPaymentType.AddEnumToHideList(
					Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>()
					.Where(item => !ViewModel.AllowPaymentTypes.Contains(item)).Cast<object>().ToArray());
			}

			#endregion PaymentTypeRestriction
		}
	}
}

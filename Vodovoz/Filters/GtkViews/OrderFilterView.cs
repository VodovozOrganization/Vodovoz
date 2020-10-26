using System;
using System.Collections.Generic;
using System.Linq;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;

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

		void Configure()
		{
			enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			enumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.RestrictStatus, w => w.SelectedItemOrNull).InitializeFromSource();

			enumcomboPaymentType.ItemsEnum = typeof(PaymentType);
			enumcomboPaymentType.Binding.AddBinding(ViewModel, vm => vm.RestrictPaymentType, w => w.SelectedItemOrNull).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices));
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.RestrictCounterparty, w => w.Subject).InitializeFromSource();

			representationentryDeliveryPoint.Binding.AddBinding(ViewModel, vm => vm.DeliveryPointRepresentationModel, w => w.RepresentationModel).InitializeFromSource();
			representationentryDeliveryPoint.Binding.AddBinding(ViewModel, vm => vm.DeliveryPointRepresentationModel, w => w.Sensitive, new NullToBooleanConverter()).InitializeFromSource();

			dateperiodOrders.StartDateOrNull = DateTime.Today.AddDays(ViewModel.DaysToBack);
			dateperiodOrders.EndDateOrNull = DateTime.Today.AddDays(ViewModel.DaysToForward);
			dateperiodOrders.Binding.AddBinding(ViewModel, vm => vm.RestrictStartDate, w => w.StartDateOrNull).InitializeFromSource();
			dateperiodOrders.Binding.AddBinding(ViewModel, vm => vm.RestrictEndDate, w => w.EndDateOrNull).InitializeFromSource();

			ycheckOnlySelfdelivery.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlySelfDelivery, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
			ycheckWithoutSelfdelivery.Binding.AddBinding(ViewModel, vm => vm.RestrictWithoutSelfDelivery, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
			ycheckOnlyServices.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyService, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
			ycheckHideServices.Binding.AddBinding(ViewModel, vm => vm.HideService, w => w.Active).InitializeFromSource();
			ycheckLessThreeHours.Binding.AddBinding(ViewModel, vm => vm.RestrictLessThreeHours, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();

			yenumcomboboxPaymentOrder.ItemsEnum = typeof(PaymentOrder);
			yenumcomboboxPaymentOrder.Binding.AddBinding(ViewModel, vm => vm.PaymentOrder, w => w.SelectedItemOrNull).InitializeFromSource();
			yenumcomboboxViewTypes.ItemsEnum = typeof(ViewTypes);
			yenumcomboboxViewTypes.Binding.AddBinding(ViewModel, vm => vm.ViewTypes, w => w.SelectedItem).InitializeFromSource();
			yenumСmbboxOrderPaymentStatus.ItemsEnum = typeof(OrderPaymentStatus);
			yenumСmbboxOrderPaymentStatus.Binding.AddBinding(ViewModel, vm => vm.OrderPaymentStatus, w => w.SelectedItemOrNull).InitializeFromSource();
		}

		void InitializeRestrictions()
		{
			enumcomboStatus.Sensitive = ViewModel.CanChangeStatus;
			enumcomboPaymentType.Sensitive = ViewModel.CanChangePaymentType;
			entryCounterparty.Sensitive = ViewModel.CanChangeCounterparty;
			representationentryDeliveryPoint.Sensitive = ViewModel.CanChangeDeliveryPoint && ViewModel.RestrictCounterparty != null;
			dateperiodOrders.Sensitive = ViewModel.CanChangeStartDate && ViewModel.CanChangeEndDate;
			ycheckOnlySelfdelivery.Sensitive = ViewModel.CanChangeOnlySelfDelivery;
			ycheckWithoutSelfdelivery.Sensitive = ViewModel.CanChangeWithoutSelfDelivery;
			ycheckOnlyServices.Sensitive = ViewModel.CanChangeOnlyService;
			ycheckHideServices.Sensitive = ViewModel.CanChangeHideService;
			ycheckLessThreeHours.Sensitive = ViewModel.CanChangeLessThreeHours;

			#region OrderStatusRestriction
			if(ViewModel.AllowStatuses != null) {
				List<object> hideStatuses = new List<object>();
				foreach(OrderStatus item in Enum.GetValues(typeof(OrderStatus))) {
					if(!ViewModel.AllowStatuses.Contains(item))
						hideStatuses.Add(item);
				}
				enumcomboStatus.ClearEnumHideList();
				enumcomboStatus.AddEnumToHideList(hideStatuses.ToArray());
			}
			if(ViewModel.HideStatuses != null)
				enumcomboStatus.AddEnumToHideList(ViewModel.HideStatuses);
			#endregion OrderStatusRestriction

			#region PaymentTypeRestriction
			if(ViewModel.AllowPaymentTypes != null) {
				List<object> hidePayments = new List<object>();
				foreach(PaymentType item in Enum.GetValues(typeof(PaymentType))) {
					if(!ViewModel.AllowPaymentTypes.Contains(item))
						hidePayments.Add(item);
				}
				enumcomboPaymentType.ClearEnumHideList();
				enumcomboPaymentType.AddEnumToHideList(hidePayments.ToArray());
			}
			#endregion PaymentTypeRestriction
		}
	}
}
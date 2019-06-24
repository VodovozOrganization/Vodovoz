using System;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
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
		}

		private void Configure()
		{
			enumcomboStatus.ItemsEnum = typeof(OrderStatus);
			enumcomboStatus.Binding.AddBinding(ViewModel, vm => vm.RestrictStatus, w => w.SelectedItemOrNull).InitializeFromSource();

			enumcomboPaymentType.ItemsEnum = typeof(PaymentType);
			enumcomboPaymentType.Binding.AddBinding(ViewModel, vm => vm.RestrictPaymentType, w => w.SelectedItemOrNull).InitializeFromSource();

			entryCounterparty.SetEntitySelectorFactory(new DefaultEntitySelectorFactory<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices));
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
			ycheckHideServices.Binding.AddBinding(ViewModel, vm => vm.RestrictHideService, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
			ycheckOnlyWithoutCoordinates.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyWithoutCoodinates, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
			ycheckLessThreeHours.Binding.AddBinding(ViewModel, vm => vm.RestrictLessThreeHours, w => w.Active, new NullableBooleanToBooleanConverter()).InitializeFromSource();
		}
	}
}

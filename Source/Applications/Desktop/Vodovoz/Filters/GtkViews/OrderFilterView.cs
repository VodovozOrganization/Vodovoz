using System;
using System.Linq;
using Gtk;
using NLog;
using QS.ViewModels.Dialog;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewWidgets.Search;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderFilterView : FilterViewBase<OrderJournalFilterViewModel>
	{
		private static readonly Logger _logger =  LogManager.GetCurrentClassLogger();
		private DialogViewModelBase _journal;

		public OrderFilterView(OrderJournalFilterViewModel orderJournalFilterViewModel) : base(orderJournalFilterViewModel)
		{
			Build();
			Configure();
			InitializeRestrictions();
		}

		private void Configure()
		{
			entryOrderId.ValidationMode = ValidationType.Numeric;
			entryOrderId.KeyReleaseEvent += OnKeyReleased;
			entryOrderId.Binding.AddBinding(ViewModel, vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

			eOnlineOrderId.ValidationMode = ValidationType.Numeric;
			eOnlineOrderId.KeyReleaseEvent += OnKeyReleased;
			eOnlineOrderId.Binding.AddBinding(ViewModel, vm => vm.OnlineOrderId, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

			entryCounterpartyPhone.ValidationMode = ValidationType.Numeric;
			entryCounterpartyPhone.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyPhone.Binding.AddBinding(ViewModel, vm => vm.CounterpartyPhone, w => w.Text).InitializeFromSource();

			entryDeliveryPointPhone.ValidationMode = ValidationType.Numeric;
			entryDeliveryPointPhone.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointPhone.Binding.AddBinding(ViewModel, vm => vm.DeliveryPointPhone, w => w.Text).InitializeFromSource();
			
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
				.AddBinding(vm => vm.Counterparty, w => w.Subject)
				.InitializeFromSource();

			entryDeliveryPoint.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DeliveryPoint, w => w.Subject)
				.AddFuncBinding(vm => vm.CanChangeDeliveryPoint && vm.Counterparty != null, w => w.Sensitive)
				.AddBinding(vm => vm.DeliveryPointSelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.InitializeFromSource();

			entrySalesManager.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ManagerSelectorFactory, w => w.EntitySelectorAutocompleteFactory)
				.AddBinding(vm => vm.CanChangeSalesManager, w => w.Sensitive)
				.AddBinding(vm => vm.SalesManager, w => w.Subject)
				.InitializeFromSource();
			
			entryAuthor.ViewModel = ViewModel.AuthorViewModel;

			yenumcomboboxDateType.ItemsEnum = typeof(OrdersDateFilterType);
			yenumcomboboxDateType.Binding
				.AddBinding(ViewModel, x => x.FilterDateType, w => w.SelectedItem)
				.AddBinding(ViewModel, x => x.CanChangeFilterDateType, w => w.Sensitive)
				.InitializeFromSource();

			dateperiodOrders.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.AddFuncBinding(vm => vm.CanChangeStartDate && vm.CanChangeEndDate, w => w.Sensitive)
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
			chkBtnClosingDocumentDeliverySchedule.RenderMode = RenderMode.Symbol;
			chkBtnClosingDocumentDeliverySchedule.Binding
				.AddBinding(ViewModel, vm => vm.FilterClosingDocumentDeliverySchedule, w => w.Active)
				.InitializeFromSource();

			yenumcomboboxPaymentOrder.ItemsEnum = typeof(PaymentOrder);
			yenumcomboboxPaymentOrder.Binding.AddBinding(ViewModel, vm => vm.PaymentOrder, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumcomboboxViewTypes.ItemsEnum = typeof(ViewTypes);
			yenumcomboboxViewTypes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ViewTypes, w => w.SelectedItem)
				.AddBinding(vm => vm.CanChangeViewTypes, w => w.Sensitive)
				.InitializeFromSource();

			yenumСmbboxOrderPaymentStatus.ItemsEnum = typeof(OrderPaymentStatus);
			yenumСmbboxOrderPaymentStatus.Binding.AddBinding(ViewModel, vm => vm.OrderPaymentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

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

			entryCounteragentNameLike.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyNameLike, w => w.Text)
				.InitializeFromSource();
			entryCounteragentNameLike.KeyReleaseEvent += OnKeyReleased;
			
			entryUpdNumber.Binding
				.AddBinding(ViewModel, vm => vm.UpdDocumentNumber, w => w.Text)
				.InitializeFromSource();
			entryUpdNumber.KeyReleaseEvent += OnKeyReleased;

			entryInn.Binding.AddBinding(ViewModel, vm => vm.CounterpartyInn, w => w.Text).InitializeFromSource();
			entryInn.KeyReleaseEvent += OnKeyReleased;

			var searchByAddressView = new CompositeSearchView(ViewModel.SearchByAddressViewModel);
			yhboxSearchByAddress.Add(searchByAddressView);
			searchByAddressView.Show();

			enumCmbEdoDocFlowStatus.ItemsEnum = typeof(EdoDocFlowStatus);
			enumCmbEdoDocFlowStatus.Binding.AddBinding(ViewModel, vm => vm.EdoDocFlowStatus, w => w.SelectedItem).InitializeFromSource();
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

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Destroy()
		{
			_logger.Info($"Вызван Destroy() у {typeof(OrderFilterView)}");
			entryOrderId.KeyReleaseEvent -= OnKeyReleased;
			entryCounterpartyPhone.KeyReleaseEvent -= OnKeyReleased;
			entryDeliveryPointPhone.KeyReleaseEvent -= OnKeyReleased;
			entryCounterparty.DestroyEntry();
			entryDeliveryPoint.DestroyEntry();
			base.Destroy();
		}
	}
}

using System;
using System.Linq;
using Gtk;
using QS.Tdi;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewWidgets.Search;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OnlineOrdersJournalFilterView : FilterViewBase<OnlineOrdersJournalFilterViewModel>
	{
		public OnlineOrdersJournalFilterView(OnlineOrdersJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			enumCmbEntityType.ShowSpecialStateAll = true;
			enumCmbEntityType.ItemsEnum = typeof(OnlineRequestsType);
			enumCmbEntityType.Binding
				.AddBinding(ViewModel, vm => vm.OnlineRequestsType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			entryOrderId.ValidationMode = ValidationType.Numeric;
			entryOrderId.KeyReleaseEvent += OnKeyReleased;
			entryOrderId.Binding
				.AddBinding(ViewModel, vm => vm.OrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			eOnlineOrderId.ValidationMode = ValidationType.Numeric;
			eOnlineOrderId.KeyReleaseEvent += OnKeyReleased;
			eOnlineOrderId.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyPhone.ValidationMode = ValidationType.Numeric;
			entryCounterpartyPhone.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyPhone.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyPhone, w => w.Text)
				.InitializeFromSource();

			enumCmbSource.ShowSpecialStateAll = true;
			enumCmbSource.ItemsEnum = typeof(Source);
			enumCmbSource.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeSource, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			enumcomboStatus.ItemsEnum = typeof(OnlineOrderStatus);
			enumcomboStatus.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeStatus, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			enumcomboPaymentType.ItemsEnum = typeof(OnlineOrderPaymentType);
			enumcomboPaymentType.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangePaymentType, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictPaymentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			ylblPaymentFrom.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.IsVisibleOnlinePaymentSource, w => w.Visible)
				.InitializeFromSource();
			
			//Чтобы не лезть в UI делаю костыльно с существующим виджетом
			speciallistCmbPaymentsFrom.ItemsList = Enum.GetValues(typeof(OnlinePaymentSource));
			speciallistCmbPaymentsFrom.RenderTextFunc = x => ((OnlinePaymentSource)x).GetEnumDisplayName();
			speciallistCmbPaymentsFrom.ShowSpecialStateAll = true;
			speciallistCmbPaymentsFrom.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.IsVisibleOnlinePaymentSource, w => w.Visible)
				.AddBinding(vm => vm.CanChangeOnlinePaymentSource, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictOnlinePaymentSource, w => w.SelectedItem)
				.InitializeFromSource();

			entryCounterparty.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeCounterparty, w => w.Sensitive)
				.InitializeFromSource();

			entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<OnlineOrdersJournalFilterViewModel>(
					ViewModel.Journal as ITdiTab, ViewModel, ViewModel.UoW, ViewModel.Journal.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.RestrictCounterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entryDeliveryPoint.Binding.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.CanChangeDeliveryPoint && vm.RestrictCounterparty != null, w => w.Sensitive)
				.InitializeFromSource();

			entryEmployeeWorkWith.ViewModel = ViewModel.EmployeeWorkWithViewModel;

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

			selfDeliveryBtn.RenderMode = RenderMode.Icon;
			selfDeliveryBtn.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeOnlySelfDelivery, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictSelfDelivery, w => w.Active)
				.InitializeFromSource();
			needConfirmationByCallBtn.RenderMode = RenderMode.Icon;
			needConfirmationByCallBtn.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeNeedConfirmationByCall, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictNeedConfirmationByCall, w => w.Active)
				.InitializeFromSource();
			fastDeliveryBtn.RenderMode = RenderMode.Icon;
			fastDeliveryBtn.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeFastDelivery, w => w.Sensitive)
				.AddBinding(vm => vm.RestrictFastDelivery, w => w.Active)
				.InitializeFromSource();
			
			chkOnlyWithoutDeliverySchedule.Binding
				.AddBinding(ViewModel, vm => vm.WithoutDeliverySchedule, w => w.Active)
				.InitializeFromSource();
			
			yenumСmbboxOrderPaymentStatus.ShowSpecialStateAll = true;
			yenumСmbboxOrderPaymentStatus.ItemsEnum = typeof(OnlineOrderPaymentStatus);
			yenumСmbboxOrderPaymentStatus.Binding
				.AddBinding(ViewModel, vm => vm.OnlineOrderPaymentStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			ySpecCmbGeographicGroup.ItemsList = ViewModel.GeographicGroups;
			ySpecCmbGeographicGroup.Binding
				.AddBinding(ViewModel, vm => vm.GeographicGroup, w => w.SelectedItem)
				.InitializeFromSource();

			entryCounteragentNameLike.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyNameLike, w => w.Text)
				.InitializeFromSource();
			entryCounteragentNameLike.KeyReleaseEvent += OnKeyReleased;

			entryInn.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyInn, w => w.Text)
				.InitializeFromSource();
			entryInn.KeyReleaseEvent += OnKeyReleased;

			var searchByAddressView = new CompositeSearchView(ViewModel.SearchByAddressViewModel);
			yhboxSearchByAddress.Add(searchByAddressView);
			searchByAddressView.Show();
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
			entryOrderId.KeyReleaseEvent -= OnKeyReleased;
			entryCounterpartyPhone.KeyReleaseEvent -= OnKeyReleased;
			eOnlineOrderId.KeyReleaseEvent += OnKeyReleased;
			entryCounteragentNameLike.KeyReleaseEvent -= OnKeyReleased;
			entryInn.KeyReleaseEvent -= OnKeyReleased;
			base.Destroy();
		}
	}
}

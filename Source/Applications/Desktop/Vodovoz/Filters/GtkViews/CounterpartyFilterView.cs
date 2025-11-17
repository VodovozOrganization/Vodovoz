using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using System;
using System.ComponentModel;
using Gamma.Widgets.Additions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewWidgets.Search;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[ToolboxItem(true)]
	public partial class CounterpartyFilterView : FilterViewBase<CounterpartyJournalFilterViewModel>
	{
		public CounterpartyFilterView(CounterpartyJournalFilterViewModel counterpartyJournalFilterViewModel) : base(counterpartyJournalFilterViewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryName.KeyReleaseEvent += OnKeyReleased;
			entryName.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyName, w => w.Text)
				.InitializeFromSource();

			entryCounterpartyPhone.ValidationMode = ValidationType.Numeric;
			entryCounterpartyPhone.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyPhone.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyPhone, w => w.Text)
				.InitializeFromSource();

			entryDeliveryPointPhone.ValidationMode = ValidationType.Numeric;
			entryDeliveryPointPhone.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointPhone.Binding
				.AddBinding(ViewModel, vm => vm.DeliveryPointPhone, w => w.Text)
				.InitializeFromSource();

			entryTag.ViewModel = ViewModel.TagViewModel;

			yenumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yenumCounterpartyType.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CounterpartyType, w => w.SelectedItemOrNull)
				.AddBinding(vm => vm.CanChangeCounterpartyType, w => w.Sensitive)
				.InitializeFromSource();

			yenumReasonForLeaving.ItemsEnum = typeof(ReasonForLeaving);
			yenumReasonForLeaving.Binding
				.AddBinding(ViewModel, vm => vm.ReasonForLeaving, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumClassification.ItemsEnum = typeof(CounterpartyCompositeClassification);
			yenumClassification.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyClassification, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			checkIncludeArhive.Binding
				.AddBinding(ViewModel, vm => vm.RestrictIncludeArchive, w => w.Active)
				.InitializeFromSource();

			enumcheckRevenueStatus.EnumType = typeof(RevenueStatus);
			enumcheckRevenueStatus.Binding
				.AddBinding(ViewModel, vm => vm.RestrictedRevenueStatuses, w => w.SelectedValuesList, new EnumsListConverter<RevenueStatus>())
				.InitializeFromSource();
			enumcheckRevenueStatus.OnlySelectValue(RevenueStatus.Active);

			checkNeedSendEdo.Binding
				.AddBinding(ViewModel, vm => vm.IsNeedToSendBillByEdo, w => w.Active)
				.InitializeFromSource();

			entryCounterpartyId.ValidationMode = ValidationType.Numeric;
			entryCounterpartyId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyId.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			ylabelCounterpartyVodovozInternalId.LabelProp = "Номер договора";
			entryCounterpartyVodovozInternalId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyVodovozInternalId.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyContractNumber, w => w.Text)
				.InitializeFromSource();

			entryCounterpartyInn.ValidationMode = ValidationType.Numeric;
			entryCounterpartyInn.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyInn.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyInn, w => w.Text)
				.InitializeFromSource();

			if(ViewModel?.IsForRetail ?? false)
			{
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelSelectableNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				ytreeviewSalesChannels.ItemsDataSource = ViewModel.SalesChannels;

				yenumClassification.Visible = false;
				labelClassification.Visible = false;
			}
			else
			{
				frame2.Visible = false;
			}

			speciallistcomboboxCounterpartySource.ItemsList = ViewModel.ClientCameFromCache;
			speciallistcomboboxCounterpartySource.ShowSpecialStateNot = true;
			speciallistcomboboxCounterpartySource.ShowSpecialStateAll = true;

			speciallistcomboboxCounterpartySource.Binding
				.AddBinding(ViewModel, vm => vm.ClientCameFrom, w => w.SelectedItem)
				.InitializeFromSource();

			speciallistcomboboxCounterpartySource.Changed += OnSpeciallistcomboboxCounterpartySourceChanged;

			var searchByAddressView = new CompositeSearchView(ViewModel.SearchByAddressViewModel);
			yhboxSearchByAddress.Add(searchByAddressView);
			searchByAddressView.Show();
		}

		private void OnSpeciallistcomboboxCounterpartySourceChanged(object sender, EventArgs e)
		{
			if(speciallistcomboboxCounterpartySource.IsSelectedNot != ViewModel.ClientCameFromIsEmpty)
			{
				ViewModel.ClientCameFromIsEmpty = speciallistcomboboxCounterpartySource.IsSelectedNot;
			}
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}

		public override void Dispose()
		{
			entryName.KeyReleaseEvent -= OnKeyReleased;
			entryCounterpartyPhone.KeyReleaseEvent -= OnKeyReleased;
			entryDeliveryPointPhone.KeyReleaseEvent -= OnKeyReleased;
			base.Dispose();
		}
	}
}

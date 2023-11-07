using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using System.ComponentModel;
using Vodovoz.Domain.Client;
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

			yentryTag.RepresentationModel = ViewModel.TagVM;
			yentryTag.Binding
				.AddBinding(ViewModel, vm => vm.Tag, w => w.Subject)
				.InitializeFromSource();

			yenumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yenumCounterpartyType.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			yenumReasonForLeaving.ItemsEnum = typeof(ReasonForLeaving);
			yenumReasonForLeaving.Binding
				.AddBinding(ViewModel, vm => vm.ReasonForLeaving, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			checkIncludeArhive.Binding
				.AddBinding(ViewModel, vm => vm.RestrictIncludeArchive, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonShowLiquidated.Binding
				.AddBinding(ViewModel, vm => vm.ShowLiquidating, w => w.Active)
				.InitializeFromSource();

			checkNeedSendEdo.Binding
				.AddBinding(ViewModel, vm => vm.IsNeedToSendBillByEdo, w => w.Active)
				.InitializeFromSource();

			entryCounterpartyId.ValidationMode = ValidationType.Numeric;
			entryCounterpartyId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyId.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyVodovozInternalId.ValidationMode = ValidationType.Numeric;
			entryCounterpartyVodovozInternalId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyVodovozInternalId.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyVodovozInternalId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyInn.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyInn.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyInn, w => w.Text)
				.InitializeFromSource();

			yenumClassification.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsForRetail.HasValue || !vm.IsForRetail.Value, w => w.Visible)
				.InitializeFromSource();

			ylabelClassification.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsForRetail.HasValue || !vm.IsForRetail.Value, w => w.Visible)
				.InitializeFromSource();

			if(ViewModel?.IsForRetail ?? false)
			{
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelSelectableNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				ytreeviewSalesChannels.ItemsDataSource = ViewModel.SalesChannels;
			}
			else
			{
				frame2.Visible = false;
			}

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

		public override void Dispose()
		{
			entryName.KeyReleaseEvent -= OnKeyReleased;
			entryCounterpartyPhone.KeyReleaseEvent -= OnKeyReleased;
			entryDeliveryPointPhone.KeyReleaseEvent -= OnKeyReleased;
			base.Dispose();
		}
	}
}

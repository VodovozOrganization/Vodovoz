using Gamma.GtkWidgets;
using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Domain.Client;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CounterpartyFilterView : FilterViewBase<CounterpartyJournalFilterViewModel>
	{
		public CounterpartyFilterView(CounterpartyJournalFilterViewModel counterpartyJournalFilterViewModel) : base(counterpartyJournalFilterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			entryName.KeyReleaseEvent += OnKeyReleased;
			entryName.Binding.AddBinding(ViewModel, vm => vm.CounterpartyName, w => w.Text).InitializeFromSource();

			entryCounterpartyPhone.ValidationMode = ValidationType.Numeric;
			entryCounterpartyPhone.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyPhone.Binding.AddBinding(ViewModel, vm => vm.CounterpartyPhone, w => w.Text).InitializeFromSource();
			
			entryDeliveryPointPhone.ValidationMode = ValidationType.Numeric;
			entryDeliveryPointPhone.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointPhone.Binding.AddBinding(ViewModel, vm => vm.DeliveryPointPhone, w => w.Text).InitializeFromSource();

			yentryTag.RepresentationModel = ViewModel.TagVM;
			yentryTag.Binding.AddBinding(ViewModel, vm => vm.Tag, w => w.Subject).InitializeFromSource();
			
			yenumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yenumCounterpartyType.Binding.AddBinding(ViewModel, vm => vm.CounterpartyType, w => w.SelectedItemOrNull).InitializeFromSource();
			
			yenumReasonForLeaving.ItemsEnum = typeof(ReasonForLeaving);
			yenumReasonForLeaving.Binding.AddBinding(ViewModel, vm => vm.ReasonForLeaving, w => w.SelectedItemOrNull).InitializeFromSource();
			
			checkIncludeArhive.Binding.AddBinding(ViewModel, vm => vm.RestrictIncludeArchive, w => w.Active).InitializeFromSource();

			checkNeedSendEdo.Binding.AddBinding(ViewModel, vm => vm.IsNeedToSendBillByEdo, w => w.Active).InitializeFromSource();

			entryCounterpartyId.ValidationMode = ValidationType.Numeric;
			entryCounterpartyId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyId.Binding
				.AddBinding(ViewModel,
				vm => vm.CounterpartyId,
				w => w.Text,
				new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyVodovozInternalId.ValidationMode = ValidationType.Numeric;
			entryCounterpartyVodovozInternalId.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyVodovozInternalId.Binding
				.AddBinding(ViewModel,
				vm => vm.CounterpartyVodovozInternalId,
				w => w.Text,
				new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyInn.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyInn.Binding.AddBinding(ViewModel, vm => vm.CounterpartyInn, w => w.Text).InitializeFromSource();

			entryDeliveryPointAddressLike.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointAddressLike.Binding.AddBinding(ViewModel, vm => vm.DeliveryPointAddressLike, w => w.Text).InitializeFromSource();

			if (ViewModel?.IsForRetail ?? false)
			{
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelSelectableNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				ytreeviewSalesChannels.ItemsDataSource = ViewModel.SalesChannels;
			} else
			{
				frame2.Visible = false;
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

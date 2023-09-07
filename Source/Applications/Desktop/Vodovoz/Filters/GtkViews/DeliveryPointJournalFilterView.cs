using Gtk;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DeliveryPointJournalFilterView : FilterViewBase<DeliveryPointJournalFilterViewModel>
	{
		public DeliveryPointJournalFilterView(DeliveryPointJournalFilterViewModel filterViewModel) : base(filterViewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ycheckWithoutStreet.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyWithoutStreet, w => w.Active).InitializeFromSource();
			ycheckOnlyNotFoundOsm.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyNotFoundOsm, w => w.Active).InitializeFromSource();
			ycheckRestrictActive.Binding.AddBinding(ViewModel, vm => vm.RestrictOnlyActive, w => w.Active).InitializeFromSource();
			entityVMentryCounterparty.Binding.AddBinding(ViewModel, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();

			entryDeliveryPointId.ValidationMode = ValidationType.Numeric;
			entryDeliveryPointId.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointId.Binding
				.AddBinding(ViewModel, vm => vm.RestrictDeliveryPointId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			entryCounterpartyNameLike.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyNameLike.Binding
				.AddSource(ViewModel)
				.AddBinding( vm => vm.RestrictCounterpartyNameLike, w => w.Text)
				.AddFuncBinding(vm => vm.Counterparty == null, w => w.Sensitive)
				.InitializeFromSource();

			entryDeliveryPointCompiledAddressLike.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointCompiledAddressLike.Binding
				.AddBinding(ViewModel, vm => vm.RestrictDeliveryPointCompiledAddressLike, w => w.Text)
				.InitializeFromSource();

			entryDeliveryPointAddress1cLike.KeyReleaseEvent += OnKeyReleased;
			entryDeliveryPointAddress1cLike.Binding
				.AddBinding(ViewModel, vm => vm.RestrictDeliveryPointAddress1cLike, w => w.Text)
				.InitializeFromSource();
		}

		private void OnKeyReleased(object sender, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}

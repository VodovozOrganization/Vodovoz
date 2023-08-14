using Gtk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Counterparties;

namespace Vodovoz.Filters.Views
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExternalCounterpartiesMatchingJournalFilterView
		: FilterViewBase<ExternalCounterpartiesMatchingJournalFilterViewModel>
	{
		public ExternalCounterpartiesMatchingJournalFilterView(
			ExternalCounterpartiesMatchingJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			enumCmbStatus.ItemsEnum = typeof(ExternalCounterpartyMatchingStatus);
			enumCmbStatus.Binding
				.AddBinding(ViewModel, vm => vm.MatchingStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			dateRangePicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();
			
			entryPhoneNumber.Binding
				.AddBinding(ViewModel, vm => vm.PhoneNumber, w => w.Text)
				.InitializeFromSource();
			
			entryCounterpartyId.Binding
				.AddBinding(ViewModel, vm => vm.CounterpartyId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			
			entryPhoneNumber.KeyReleaseEvent += OnEntryKeyReleaseEvent;
			entryCounterpartyId.KeyReleaseEvent += OnEntryKeyReleaseEvent;
		}

		private void OnEntryKeyReleaseEvent(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key != Gdk.Key.Return)
			{
				return;
			}
			ViewModel.Update();
		}

		public override void Destroy()
		{
			base.Destroy();
			enumCmbStatus.Destroy();
		}
	}
}

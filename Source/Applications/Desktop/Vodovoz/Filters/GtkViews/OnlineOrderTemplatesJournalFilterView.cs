using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using QS.Widgets;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Key = Gdk.Key;

namespace Vodovoz.Filters.GtkViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OnlineOrderTemplatesJournalFilterView : FilterViewBase<OnlineOrderTemplatesJournalFilterViewModel>
	{
		public OnlineOrderTemplatesJournalFilterView(OnlineOrderTemplatesJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			entryTemplateId.ValidationMode = ValidationType.Numeric;
			entryTemplateId.KeyReleaseEvent += OnKeyReleased;
			entryTemplateId.Binding
				.AddBinding(ViewModel, vm => vm.TemplateId, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			idArchiveChkBtn.RenderMode = RenderMode.Icon;
			idArchiveChkBtn.Binding
				.AddBinding(ViewModel, vm => vm.Archive, w => w.Active)
				.InitializeFromSource();
			
			entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<OnlineOrderTemplatesJournalFilterViewModel>(
					ViewModel.Journal, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			
			entryDeliveryPoint.ViewModel = ViewModel.DeliveryPointViewModel;
			
			entryCounterpartyPhone.ValidationMode = ValidationType.Numeric;
			entryCounterpartyPhone.KeyReleaseEvent += OnKeyReleased;
			entryCounterpartyPhone.Binding
				.AddBinding(ViewModel, vm => vm.ContactPhone, w => w.Text)
				.InitializeFromSource();

			paymentTypeCmb.ShowSpecialStateAll = true;
			paymentTypeCmb.ItemsEnum = typeof(OnlineOrderPaymentType);
			paymentTypeCmb.Binding
				.AddBinding(ViewModel, vm => vm.PaymentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			
			templateStateCmb.ShowSpecialStateAll = true;
			templateStateCmb.ItemsEnum = typeof(OnlineOrderTemplateStatus);
			templateStateCmb.Binding
				.AddBinding(ViewModel, vm => vm.TemplateStatus, w => w.SelectedItemOrNull)
				.InitializeFromSource();
		}

		private void OnKeyReleased(object o, KeyReleaseEventArgs args)
		{
			if(args.Event.Key == Key.Return)
			{
				ViewModel.Update();
			}
		}
	}
}

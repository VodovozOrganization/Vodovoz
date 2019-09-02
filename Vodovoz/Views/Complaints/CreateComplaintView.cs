using System;
using Gamma.Widgets;
using QS.DomainModel.Config;
using QS.Project.Journal.EntitySelector;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CreateComplaintView : TabViewBase<CreateComplaintViewModel>
	{
		public CreateComplaintView(CreateComplaintViewModel viewModel) : base(viewModel)
		{
			this.Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.ComplainantName, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			EntryCounterparty_ChangedByUser(this, new EventArgs());
			entryCounterparty.ChangedByUser += EntryCounterparty_ChangedByUser;

			spLstAddress.Binding.AddBinding(ViewModel, s => s.CanSelectDeliveryPoint, w => w.Sensitive).InitializeFromSource();

			var orderSelectorFactory = new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(typeof(Order), () => {
				var filter = new OrderJournalFilterViewModel(ServicesConfig.InteractiveService);
				if(ViewModel.Entity.Counterparty != null) {
					filter.RestrictCounterparty = ViewModel.Entity.Counterparty;
				}
				return new OrderJournalViewModel(filter, new DefaultEntityConfigurationProvider(), ServicesConfig.CommonServices);
			});

			entryOrder.SetEntitySelectorFactory(orderSelectorFactory);
			entryOrder.Binding.AddBinding(ViewModel.Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			yentryPhone.Binding.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			yentryPhone.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			comboboxComplaintSource.SetRenderTextFunc<ComplaintSource>(x => x.Name);
			comboboxComplaintSource.ItemsList = ViewModel.ComplaintSources;
			comboboxComplaintSource.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;

			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(false); };
		}

		void EntryCounterparty_ChangedByUser(object sender, System.EventArgs e)
		{
			if(ViewModel.Entity.Counterparty != null) {
				spLstAddress.NameForSpecialStateNot = "Самовывоз";
				spLstAddress.SetRenderTextFunc<DeliveryPoint>(d => string.Format("{0}: {1}", d.Id, d.ShortAddress));
				spLstAddress.Binding.AddBinding(ViewModel.Entity.Counterparty, s => s.DeliveryPoints, w => w.ItemsList).InitializeFromSource();
				spLstAddress.Binding.AddBinding(ViewModel.Entity, t => t.DeliveryPoint, w => w.SelectedItem).InitializeFromSource();
				return;
			}
			spLstAddress.NameForSpecialStateNot = null;
			spLstAddress.SelectedItem = SpecialComboState.Not;
			spLstAddress.ItemsList = null;
		}
	}
}

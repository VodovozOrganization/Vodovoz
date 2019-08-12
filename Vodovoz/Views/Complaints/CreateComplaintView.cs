using System;
using QS.Views.GtkUI;
using Vodovoz.Domain.Complaints;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.Filters.ViewModels;
using QS.DomainModel.Config;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Orders;

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
	}
}

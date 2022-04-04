using System;
using Gamma.Widgets;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.TempAdapters;
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

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyJournalFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			EntryCounterparty_ChangedByUser(this, new EventArgs());
			entryCounterparty.ChangedByUser += EntryCounterparty_ChangedByUser;

			spLstComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			spLstComplaintKind.Binding.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList).InitializeFromSource();
			spLstComplaintKind.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintKind, w => w.SelectedItem).InitializeFromSource();

			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.ComplaintObject, w => w.SelectedItem).InitializeFromSource();

			spLstAddress.Binding.AddBinding(ViewModel, s => s.CanSelectDeliveryPoint, w => w.Sensitive).InitializeFromSource();

			var orderSelectorFactory = new EntityAutocompleteSelectorFactory<OrderJournalViewModel>(typeof(Order), () => {
				var filter = new OrderJournalFilterViewModel(ViewModel.CounterpartyJournalFactory, ViewModel.DeliveryPointJournalFactory);
				if(ViewModel.Entity.Counterparty != null) {
					filter.RestrictCounterparty = ViewModel.Entity.Counterparty;
				}
				return new OrderJournalViewModel(filter,
												UnitOfWorkFactory.GetDefaultFactory,
												ServicesConfig.CommonServices,
												ViewModel.EmployeeService,
												ViewModel.NomenclatureRepository,
												ViewModel.UserRepository,
												ViewModel.OrderSelectorFactory,
												ViewModel.EmployeeJournalFactory,
												ViewModel.CounterpartyJournalFactory,
												ViewModel.DeliveryPointJournalFactory,
												ViewModel.SubdivisionJournalFactory,
												ViewModel.GtkDialogsOpener,
												ViewModel.UndeliveredOrdersJournalOpener,
												ViewModel.NomenclatureSelector,
												ViewModel.UndeliveredOrdersRepository,
												ViewModel.SubdivisionRepository,
												ViewModel.FileDialogService);
			});

			entryOrder.SetEntitySelectorFactory(orderSelectorFactory);
			entryOrder.Binding.AddBinding(ViewModel.Entity, e => e.Order, w => w.Subject).InitializeFromSource();
			entryOrder.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			entryOrder.ChangedByUser += (sender, e) => ViewModel.ChangeDeliveryPointCommand.Execute();
			
			if(ViewModel.UserHasOnlyAccessToWarehouseAndComplaints)
			{
				entryCounterparty.CanEditReference = entryOrder.CanEditReference = false;
			}

			yentryPhone.Binding.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			yentryPhone.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            complaintfilesview.ViewModel = ViewModel.FilesViewModel;
            complaintfilesview.Sensitive = ViewModel.CanEdit;

			comboboxComplaintSource.SetRenderTextFunc<ComplaintSource>(x => x.Name);
			comboboxComplaintSource.ItemsList = ViewModel.ComplaintSources;
			comboboxComplaintSource.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;

			buttonSave.Clicked += (sender, e) => { ViewModel.CheckAndSave(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
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

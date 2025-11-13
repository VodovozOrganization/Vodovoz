using System;
using System.ComponentModel;
using Gamma.Widgets;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Complaints;

namespace Vodovoz.Views.Complaints
{
	[ToolboxItem(true)]
	public partial class CreateComplaintView : TabViewBase<CreateComplaintViewModel>
	{
		public CreateComplaintView(CreateComplaintViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			yentryName.Binding.AddBinding(ViewModel.Entity, e => e.ComplainantName, w => w.Text).InitializeFromSource();
			yentryName.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			InitializeEntryViewModels();

			spLstComplaintKind.SetRenderTextFunc<ComplaintKind>(k => k.GetFullName);
			spLstComplaintKind.Binding
				.AddBinding(ViewModel, vm => vm.ComplaintKindSource, w => w.ItemsList)
				.AddBinding(ViewModel.Entity, e => e.ComplaintKind, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanEditComplaintClassification, w => w.Sensitive)
				.InitializeFromSource();


			yspeccomboboxComplaintObject.ShowSpecialStateAll = true;
			yspeccomboboxComplaintObject.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ComplaintObjectSource, w => w.ItemsList)
				.AddBinding(vm => vm.ComplaintObject, w => w.SelectedItem)
				.AddBinding(vm => vm.CanEditComplaintClassification, w => w.Sensitive)
				.InitializeFromSource();

			spLstAddress.Binding.AddBinding(ViewModel, s => s.CanSelectDeliveryPoint, w => w.Sensitive).InitializeFromSource();

			yentryPhone.Binding.AddBinding(ViewModel.Entity, e => e.Phone, w => w.Text).InitializeFromSource();
			yentryPhone.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

            smallfileinformationsview.ViewModel = ViewModel.AttachedFileInformationsViewModel;
            smallfileinformationsview.Sensitive = ViewModel.CanEdit;

			comboboxComplaintSource.SetRenderTextFunc<ComplaintSource>(x => x.Name);
			comboboxComplaintSource.ItemsList = ViewModel.ComplaintSources;
			comboboxComplaintSource.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintSource, w => w.SelectedItem).InitializeFromSource();
			comboboxComplaintSource.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			ytextviewComplaintText.Binding.AddBinding(ViewModel.Entity, e => e.ComplaintText, w => w.Buffer.Text).InitializeFromSource();
			ytextviewComplaintText.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			guiltyitemsview.ViewModel = ViewModel.GuiltyItemsViewModel;
			orderRatingEntry.ViewModel = ViewModel.OrderRatingEntryViewModel;
			
			buttonSave.Clicked += (sender, e) => { ViewModel.SaveAndClose(); };
			buttonCancel.Clicked += (sender, e) => { ViewModel.Close(true, QS.Navigation.CloseSource.Cancel); };
			
			ViewModel.Entity.PropertyChanged += OnViewModelEntityPropertyChanged;
		}

		private void InitializeEntryViewModels()
		{
			var builder = new LegacyEEVMBuilderFactory<Complaint>(
				Tab, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope);

			var counterpartyEntryViewModel = 
				builder
					.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();

			counterpartyEntry.ViewModel = counterpartyEntryViewModel;
			counterpartyEntry.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();

			var orderEntryViewModel =
				builder
					.ForProperty(x => x.Order)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<OrderJournalViewModel, OrderJournalFilterViewModel>(
						f => f.RestrictCounterparty = ViewModel.Entity.Counterparty
					)
					.Finish();
			orderEntryViewModel.BeforeChangeByUser += OnBeforeChangeOrderByUser;

			orderEntry.ViewModel = orderEntryViewModel;
			orderEntry.Binding.AddBinding(ViewModel, vm => vm.CanEdit, w => w.Sensitive).InitializeFromSource();
			orderEntry.ViewModel.ChangedByUser += (sender, e) => ViewModel.ChangeDeliveryPointCommand.Execute();

			if(ViewModel.UserHasOnlyAccessToWarehouseAndComplaints)
			{
				counterpartyEntryViewModel.CanViewEntity = orderEntryViewModel.CanViewEntity = false;
			}
		}

		private void OnViewModelEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Entity.Counterparty))
			{
				OnCounterpartyChanged();
			}
		}
		
		private void OnBeforeChangeOrderByUser(object sender, BeforeChangeEventArgs e)
		{
			var result = ViewModel.Entity.CanChangeOrder();

			if(!result.CanChange)
			{
				e.CanChange = false;
				ViewModel.ShowMessage(result.Message);
				return;
			}
			
			e.CanChange = true;
		}

		private void OnCounterpartyChanged()
		{
			if(ViewModel.Entity.Counterparty != null)
			{
				spLstAddress.NameForSpecialStateNot = "Самовывоз";
				spLstAddress.SetRenderTextFunc<DeliveryPoint>(d => $"{d.Id}: {d.ShortAddress}");
				spLstAddress.Binding
					.AddBinding(ViewModel.Entity.Counterparty, s => s.DeliveryPoints, w => w.ItemsList)
					.AddBinding(ViewModel.Entity, c => c.DeliveryPoint, w => w.SelectedItem)
					.InitializeFromSource();
				
				return;
			}
			
			spLstAddress.NameForSpecialStateNot = null;
			spLstAddress.SelectedItem = SpecialComboState.Not;
			spLstAddress.ItemsList = null;
		}

		public override void Destroy()
		{
			ViewModel.Entity.PropertyChanged -= OnViewModelEntityPropertyChanged;
			base.Destroy();
		}
	}
}

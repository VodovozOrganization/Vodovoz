using System;
using Gtk;
using QS.ViewModels.Control.EEVM;
using QS.Views.Dialog;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.Presentation.ViewModels.Organisations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.Views.Organization
{
	public partial class BusinessAccountView : DialogViewBase<BusinessAccountViewModel>
	{
		public BusinessAccountView(BusinessAccountViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CancelCommand);

			lblIdTitle.Binding
				.AddBinding(ViewModel, e => e.CanShowId, w => w.Visible)
				.InitializeFromSource();

			lblId.Binding
				.AddBinding(ViewModel, e => e.IdString, w => w.LabelProp)
				.AddBinding(ViewModel, vm => vm.CanShowId, w => w.Visible)
				.InitializeFromSource();

			entryName.Binding
				.AddBinding(ViewModel.Entity, e => e.Name, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			entryNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.Number, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();
			entryNumber.Changed += OnNumericEntryChanged;

			entryBank.Binding
				.AddBinding(ViewModel.Entity, e => e.Bank, w => w.Text)
				.AddBinding(ViewModel, vm => vm.CanEdit, w => w.IsEditable)
				.InitializeFromSource();

			lblAccountFillTypeTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowAccountFillType, w => w.Visible)
				.InitializeFromSource();

			enumСmbAccountFillType.ItemsEnum = typeof(AccountFillType);
			enumСmbAccountFillType.Binding
				.AddBinding(ViewModel.Entity, e => e.AccountFillType, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanShowAccountFillType, w => w.Visible)
				.InitializeFromSource();

			entryBusinessActivity.ViewModel = ViewModel.BusinessActivityViewModel;
			entryFunds.ViewModel = ViewModel.FundsViewModel;

			lblSubdivisionTitle.Binding
				.AddBinding(ViewModel, vm => vm.CanShowSubdivision, w => w.Visible)
				.InitializeFromSource();
			
			var subdivisionViewModel =
				new CommonEEVMBuilderFactory<BusinessAccountViewModel>(
					ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.LifetimeScope)
					.ForProperty(x => x.Subdivision)
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
					.UseViewModelDialog<SubdivisionViewModel>()
					.Finish();
			subdivisionViewModel.IsEditable = ViewModel.CanEdit;

			entrySubdivision.ViewModel = subdivisionViewModel;
			entrySubdivision.Binding
				.AddBinding(ViewModel, vm => vm.CanShowSubdivision, w => w.Visible)
				.InitializeFromSource();

			chkIsArchive.Binding
				.AddBinding(ViewModel.Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
		}
		
		private void OnNumericEntryChanged(object sender, EventArgs e)
		{
			var entry = sender as Entry;
			var chars = entry.Text.ToCharArray();
			
			var text = ViewModel.StringHandler.ConvertCharsArrayToNumericString(chars);
			entry.Text = string.IsNullOrWhiteSpace(text) ? string.Empty : text;
		}

		public override void Destroy()
		{
			entryNumber.Changed -= OnNumericEntryChanged;
			base.Destroy();
		}
	}
}

using Gamma.Utilities;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashlessRequestView : TabViewBase<CashlessRequestViewModel>
	{
		public CashlessRequestView(CashlessRequestViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			comboRoleChooser.SetRenderTextFunc<PayoutRequestUserRole>(ur => ur.GetEnumTitle());
			comboRoleChooser.ItemsList = ViewModel.UserRoles;
			comboRoleChooser.Binding.AddBinding(ViewModel, vm => vm.UserRole, w => w.SelectedItem).InitializeFromSource();
			comboRoleChooser.Sensitive = ViewModel.IsRoleChooserSensitive;

			labelStatus.Binding.AddFuncBinding(ViewModel.Entity, e => e.PayoutRequestState.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			evmeAuthor.Sensitive = false;
			evmeAuthor.Binding.AddBinding(ViewModel.Entity, e => e.Author, w => w.Subject).InitializeFromSource();
			evmeSubdivision.Sensitive = false;
			evmeSubdivision.Binding.AddBinding(ViewModel.Entity, e => e.Subdivision, w => w.Subject).InitializeFromSource();

			evmeCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelector);
			evmeCounterparty.Binding.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject).InitializeFromSource();
			evmeCounterparty.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();

			spinSum.Binding.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal).InitializeFromSource();
			spinSum.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();

			checkNotToReconcile.Binding.AddBinding(ViewModel, vm => vm.CanSeeNotToReconcile, w => w.Visible).InitializeFromSource();
			checkNotToReconcile.Binding.AddBinding(ViewModel.Entity, e => e.PossibilityNotToReconcilePayments, w => w.Active)
				.InitializeFromSource();

			eventBoxOrganisationSeparator.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSeeOrganisation || vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();
			labelComboOrganization.Binding.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible).InitializeFromSource();
			comboOrganisation.SetRenderTextFunc<Domain.Organizations.Organization>(org => org.Name);
			comboOrganisation.ItemsList = ViewModel.OurOrganisations;
			comboOrganisation.ShowSpecialStateNot = true;
			comboOrganisation.Binding.AddBinding(ViewModel.Entity, e => e.Organization, w => w.SelectedItem).InitializeFromSource();
			comboOrganisation.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSeeOrganisation, w => w.Visible)
				.AddBinding(vm => vm.CanSetOrganisaton, w => w.Sensitive)
				.InitializeFromSource();

			labelExpenceCategory.Binding.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible).InitializeFromSource();
			evmeExpenceCategory.CanEditReference = true;
			evmeExpenceCategory.SetEntityAutocompleteSelectorFactory(ViewModel.ExpenceCategoryAutocompleteSelector);
			evmeExpenceCategory.Binding.AddBinding(ViewModel.Entity, vm => vm.ExpenseCategory, w => w.Subject).InitializeFromSource();
			evmeExpenceCategory.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.AddBinding(vm => vm.CanSetExpenseCategory, w => w.Sensitive)
				.InitializeFromSource();

			entryBasis.Binding.AddBinding(ViewModel.Entity, e => e.Basis, w => w.Buffer.Text).InitializeFromSource();
			entryBasis.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();
			entryExplanation.Binding.AddBinding(ViewModel.Entity, e => e.Explanation, w => w.Buffer.Text).InitializeFromSource();
			entryExplanation.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();

			eventBoxReasonsSeparator.Binding.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible).InitializeFromSource();
			eventBoxCancelReason.Binding.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible).InitializeFromSource();
			labelCancelReason.Binding.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible).InitializeFromSource();
			entryCancelReason.Binding.AddBinding(ViewModel.Entity, e => e.CancelReason, w => w.Buffer.Text).InitializeFromSource();
			entryCancelReason.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();

			eventBoxWhySentToReapproval.Binding.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible).InitializeFromSource();
			labelWhySentToReapproval.Binding.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible).InitializeFromSource();
			entryWhySentToReapproval.Binding.AddBinding(ViewModel, vm => vm.IsNotClosed, w => w.Sensitive).InitializeFromSource();
			entryWhySentToReapproval.Binding.AddBinding(ViewModel.Entity, e => e.ReasonForSendToReappropriate, w => w.Buffer.Text)
				.InitializeFromSource();

			filesView.ViewModel = ViewModel.CashlessRequestFilesViewModel;

			buttonSave.Clicked += (s, a) => ViewModel.Save(true);
			buttonCancel.Clicked += (s, a) => ViewModel.Close(true, CloseSource.Cancel);

			buttonPayout.Binding.AddBinding(ViewModel, vm => vm.CanPayout, w => w.Visible).InitializeFromSource();
			buttonPayout.Clicked += (s, a) => ViewModel.Payout();

			btnAccept.Binding.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible).InitializeFromSource();
			btnAccept.Clicked += (s, a) => ViewModel.Accept();

			btnApprove.Binding.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible).InitializeFromSource();
			btnApprove.Clicked += (s, a) => ViewModel.Approve();

			btnCancel.Binding.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible).InitializeFromSource();
			btnCancel.Clicked += (s, a) => ViewModel.Cancel();

			btnReapprove.Binding.AddBinding(ViewModel, vm => vm.CanReapprove, w => w.Visible).InitializeFromSource();
			btnReapprove.Clicked += (s, a) => ViewModel.Reapprove();

			btnConveyForPayout.Binding.AddBinding(ViewModel, vm => vm.CanConveyForPayout, w => w.Visible).InitializeFromSource();
			btnConveyForPayout.Clicked += (s, a) => ViewModel.ConveyForPayout();
		}
	}
}

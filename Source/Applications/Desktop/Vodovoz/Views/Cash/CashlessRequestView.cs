using System.ComponentModel;
using Gamma.Utilities;
using QS.Navigation;
using QS.Views.GtkUI;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.Views.Cash
{
	public partial class CashlessRequestView : TabViewBase<CashlessRequestViewModel>
	{
		public CashlessRequestView(CashlessRequestViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			comboRoleChooser.SetRenderTextFunc<PayoutRequestUserRole>(ur => ur.GetEnumTitle());
			comboRoleChooser.ItemsList = ViewModel.UserRoles;
			comboRoleChooser.Binding
				.AddBinding(ViewModel, vm => vm.UserRole, w => w.SelectedItem)
				.InitializeFromSource();
			comboRoleChooser.Sensitive = ViewModel.IsRoleChooserSensitive;

			labelStatus.Binding
				.AddFuncBinding(ViewModel.Entity, e => e.PayoutRequestState.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			evmeAuthor.Sensitive = false;
			evmeAuthor.Binding
				.AddBinding(ViewModel.Entity, e => e.Author, w => w.Subject)
				.InitializeFromSource();

			entrySubdivision.ViewModel = ViewModel.SubdivisionViewModel;

			evmeCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartyAutocompleteSelector);
			evmeCounterparty.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Counterparty, w => w.Subject)
				.InitializeFromSource();

			spinSum.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			checkNotToReconcile.Binding
				.AddBinding(ViewModel, vm => vm.CanSeeNotToReconcile, w => w.Visible)
				.AddBinding(ViewModel.Entity, e => e.PossibilityNotToReconcilePayments, w => w.Active)
				.InitializeFromSource();

			eventBoxOrganisationSeparator.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSeeOrganisation || vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();
			labelComboOrganization.Binding
				.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible)
				.InitializeFromSource();
			comboOrganisation.SetRenderTextFunc<Domain.Organizations.Organization>(org => org.Name);
			comboOrganisation.ItemsList = ViewModel.OurOrganisations;
			comboOrganisation.ShowSpecialStateNot = true;
			comboOrganisation.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetOrganisaton && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Organization, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CanSeeOrganisation, w => w.Visible)
				.InitializeFromSource();

			labelExpenceCategory.Binding.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible).InitializeFromSource();

			entryExpenseFinancialCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryExpenseFinancialCategory.Binding
				.AddFuncBinding(ViewModel, vm => vm.CanSetExpenseCategory && !vm.IsSecurityServiceRole, w => w.ViewModel.IsEditable)
				.AddBinding(ViewModel, vm => vm.CanSeeExpenseCategory, w => w.Visible)
				.InitializeFromSource();

			entryBasis.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Basis, w => w.Buffer.Text)
				.InitializeFromSource();
			entryExplanation.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.Explanation, w => w.Buffer.Text)
				.InitializeFromSource();

			eventBoxReasonsSeparator.Binding
				.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
				.InitializeFromSource();
			eventBoxCancelReason.Binding
				.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
				.InitializeFromSource();
			labelCancelReason.Binding
				.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
				.InitializeFromSource();
			entryCancelReason.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.CancelReason, w => w.Buffer.Text)
				.InitializeFromSource();

			eventBoxWhySentToReapproval.Binding
				.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
				.InitializeFromSource();
			labelWhySentToReapproval.Binding
				.AddBinding(ViewModel, vm => vm.IsNotNew, w => w.Visible)
				.InitializeFromSource();
			entryWhySentToReapproval.Binding
				.AddFuncBinding(ViewModel, vm => vm.IsNotClosed && !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel.Entity, e => e.ReasonForSendToReappropriate, w => w.Buffer.Text)
				.InitializeFromSource();

			filesView.ViewModel = ViewModel.CashlessRequestFilesViewModel;

			buttonSave.Clicked += (s, a) => ViewModel.Save(true);
			buttonSave.Sensitive = !ViewModel.IsSecurityServiceRole;
			buttonCancel.Clicked += (s, a) => ViewModel.Close(ViewModel.AskSaveOnClose, CloseSource.Cancel);

			buttonPayout.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanPayout, w => w.Visible)
				.InitializeFromSource();
			buttonPayout.Clicked += (s, a) => ViewModel.Payout();

			btnAccept.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanAccept, w => w.Visible)
				.InitializeFromSource();
			btnAccept.Clicked += (s, a) => ViewModel.Accept();

			btnApprove.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanApprove, w => w.Visible)
				.InitializeFromSource();
			btnApprove.Clicked += (s, a) => ViewModel.Approve();

			btnCancel.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanCancel, w => w.Visible)
				.InitializeFromSource();
			btnCancel.Clicked += (s, a) => ViewModel.Cancel();

			btnReapprove.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanReapprove, w => w.Visible)
				.InitializeFromSource();
			btnReapprove.Clicked += (s, a) => ViewModel.Reapprove();

			btnConveyForPayout.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsSecurityServiceRole, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.CanConveyForPayout, w => w.Visible)
				.InitializeFromSource();
			btnConveyForPayout.Clicked += (s, a) => ViewModel.ConveyForPayout();
		}
	}
}

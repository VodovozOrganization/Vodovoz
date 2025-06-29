using System;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CreateManualPaymentFromBankClientView : TabViewBase<CreateManualPaymentFromBankClientViewModel>
	{
		public CreateManualPaymentFromBankClientView(CreateManualPaymentFromBankClientViewModel viewModel) : base(viewModel)
		{
			Build();
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			btnSave.BindCommand(ViewModel.SaveCommand);
			btnCancel.BindCommand(ViewModel.CloseCommand);
			btnGoToManualPaymentMatching.BindCommand(ViewModel.SaveAndOpenManualPaymentMatchingCommand);

			paymentDatePicker.IsEditable = true;
			paymentDatePicker.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			chkBtnUpdateBalance.Toggled += UpdateBalanceToggled;
			chkBtnUpdateBalance.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentForUpdateBalance, w => w.Active)
				.InitializeFromSource();

			yspinPaymentNum.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentNum, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinPaymentNum.Adjustment.Lower = 1;
			yspinPaymentTotal.Binding
				.AddBinding(ViewModel.Entity, e => e.Total, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ConfigureEntityEntries();
			
			textViewPaymentPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			specListCmbCategoryProfit.ItemsList = ViewModel.ProfitCategories;
			specListCmbCategoryProfit.SetRenderTextFunc<ProfitCategory>(pc => pc.Name);
			specListCmbCategoryProfit.Binding
				.AddBinding(ViewModel.Entity, e => e.ProfitCategory, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void UpdateBalanceToggled(object sender, EventArgs e)
		{
			ViewModel.ChangePaymentNumAndPaymentPurposeCommand.Execute();
		}

		private void ConfigureEntityEntries()
		{
			var builder = new LegacyEEVMBuilderFactory<Payment>(
				ViewModel, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);
			counterpartyEntry.ViewModel = builder.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();

			organizationEntry.ViewModel = ViewModel.OrganizationsEntryViewModel;
		}
	}
}

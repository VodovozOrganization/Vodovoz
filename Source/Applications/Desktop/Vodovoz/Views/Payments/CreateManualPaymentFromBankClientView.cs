using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
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
			btnSave.Clicked += (sender, args) => ViewModel.SaveAndClose();
			btnCancel.Clicked += (sender, args) => ViewModel.Close(false, CloseSource.Cancel);
			btnGoToManualPaymentMatching.Clicked += (sender, args) => ViewModel.SaveAndOpenManualPaymentMatchingCommand.Execute();

			paymentDatePicker.IsEditable = true;
			paymentDatePicker.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			chkBtnUpdateBalance.Toggled += (sender, args) => ViewModel.ChangePaymentNumAndPaymentPurposeCommand.Execute();
			chkBtnUpdateBalance.Binding
				.AddBinding(ViewModel, vm => vm.IsPaymentForUpdateBalance, w => w.Active)
				.InitializeFromSource();

			yspinPaymentNum.Binding
				.AddBinding(ViewModel.Entity, e => e.Number, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinPaymentNum.Adjustment.Lower = 1;
			yspinPaymentTotal.Binding
				.AddBinding(ViewModel.Entity, e => e.Total, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ConfigureCounterpartyEntry();
			
			textViewPaymentPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewComment.Binding
				.AddBinding(ViewModel, vm => vm.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			specListCmbCategoryProfit.ItemsList = ViewModel.ProfitCategories;
			specListCmbCategoryProfit.SetRenderTextFunc<ProfitCategory>(pc => pc.Name);
			specListCmbCategoryProfit.Binding
				.AddBinding(ViewModel, vm => vm.ProfitCategory, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void ConfigureCounterpartyEntry()
		{
			var builder = new LegacyEEVMBuilderFactory<CreateManualPaymentFromBankClientViewModel>(
				ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);
			counterpartyEntry.ViewModel = builder.ForProperty(x => x.SelectedCounterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();
		}
	}
}

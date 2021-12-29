using QS.Navigation;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using Vodovoz.Domain.Payments;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.Views.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentFromBankClientView : TabViewBase<PaymentFromBankClientViewModel>
	{
		public PaymentFromBankClientView(PaymentFromBankClientViewModel viewModel) : base(viewModel)
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
			
			yspinPaymentNum.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentNum, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinPaymentTotal.Binding
				.AddBinding(ViewModel.Entity, e => e.Total, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ConfigureCounterpartyEntry();
			
			textViewPaymentPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			specListCmbCategoryProfit.ItemsList = ViewModel.ProfitCategories;
			specListCmbCategoryProfit.SetRenderTextFunc<ProfitCategory>(cp => cp.Name);
			specListCmbCategoryProfit.Binding
				.AddBinding(ViewModel.Entity, e => e.ProfitCategory, w => w.SelectedItem)
				.InitializeFromSource();
		}

		private void ConfigureCounterpartyEntry()
		{
			var builder = new LegacyEEVMBuilderFactory<Payment>(
				ViewModel, ViewModel.Entity, ViewModel.UoW, ViewModel.NavigationManager, ViewModel.Scope);
			counterpartyEntry.ViewModel = builder.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();
		}
	}
}

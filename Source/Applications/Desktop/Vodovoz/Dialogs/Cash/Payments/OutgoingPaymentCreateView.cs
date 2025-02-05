using System;
namespace Vodovoz.Dialogs.Cash.Payments
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OutgoingPaymentCreateView : Gtk.Bin
	{
		public OutgoingPaymentCreateView()
		{
			this.Build();
		}

		private void Initialize()
		{
			dpPaymentDate.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentDate, w => w.Date)
				.InitializeFromSource();

			var counterpartyViewModel = new LegacyEEVMBuilderFactory<OutgoingPaymentCreateViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(filter =>
				{
					filter.CounterpartyType = Domain.Client.CounterpartyType.Supplier;
				})
				.Finish();

			ViewModel.CounterpartyViewModel = counterpartyViewModel;

			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			entryFinancialExpenseCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			entryOurOrganization.ViewModel = ViewModel.OurOrganizationViewModel;

			ytvComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ytvPaymentPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();

			ysbPaymentNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentNumber, w => w.ValueAsInt)
				.InitializeFromSource();

			ysbSum.Binding
				.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}

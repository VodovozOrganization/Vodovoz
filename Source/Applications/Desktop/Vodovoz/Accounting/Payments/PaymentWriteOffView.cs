using Autofac;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Accounting.Payments;

namespace Vodovoz.Accounting.Payments
{
	[ToolboxItem(true)]
	public partial class PaymentWriteOffView : TabViewBase<PaymentWriteOffViewModel>
	{
		private ILifetimeScope _lifetimeScope;

		public PaymentWriteOffView(
			PaymentWriteOffViewModel viewModel,
			ILifetimeScope lifetimeScope)
			: base(viewModel)
		{
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));
			Build();

			Initialize();
		}

		private void Initialize()
		{
			ybuttonSave.BindCommand(ViewModel.SaveCommand);
			ybuttonCancel.BindCommand(ViewModel.CancelCommand);

			datepickerDate.Binding
				.AddBinding(ViewModel.Entity, e => e.Date, w => w.Date)
				.InitializeFromSource();

			yspinbuttonNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentNumber, w => w.ValueAsInt)
				.InitializeFromSource();

			ytextviewResaon.Binding
				.AddBinding(ViewModel.Entity, e => e.Reason, w => w.Buffer.Text)
				.InitializeFromSource();

			yspinbuttonSum.Binding
				.AddBinding(ViewModel.Entity, e => e.Sum, w => w.ValueAsDecimal)
				.InitializeFromSource();

			ViewModel.CounterpartyViewModel = new LegacyEEVMBuilderFactory<PaymentWriteOffViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			entityentryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			ViewModel.OrganizationViewModel = new LegacyEEVMBuilderFactory<PaymentWriteOffViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Organization)
				.UseTdiDialog<OrganizationDlg>()
				.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
				.Finish();

			entityentryOrganization.ViewModel = ViewModel.OrganizationViewModel;

			entityentryFinancialExpenseCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			ytextviewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}

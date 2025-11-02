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
				.AddBinding(ViewModel, vm => vm.CanEditDate, w => w.IsEditable)
				.InitializeFromSource();

			yspinbuttonNumber.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentNumber, w => w.ValueAsInt)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			ytextviewResaon.Binding
				.AddBinding(ViewModel.Entity, e => e.Reason, w => w.Buffer.Text)
				.InitializeFromSource();

			yspinbuttonSum.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Sum, w => w.ValueAsDecimal)
				.AddBinding(ViewModel, vm => vm.IsNew, w => w.Sensitive)
				.InitializeFromSource();

			var counterpartyViewModel = new LegacyEEVMBuilderFactory<PaymentWriteOffViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			counterpartyViewModel.CanViewEntity = false;
			counterpartyViewModel.IsEditable = ViewModel.IsNew;

			ViewModel.CounterpartyViewModel = counterpartyViewModel;

			entityentryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			entityentryOrganization.ViewModel = ViewModel.OrganizationViewModel;

			entityentryFinancialExpenseCategory.ViewModel = ViewModel.FinancialExpenseCategoryViewModel;

			ytextviewComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
		}
	}
}

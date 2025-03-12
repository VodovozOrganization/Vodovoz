using Autofac;
using QS.ViewModels.Control.EEVM;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Cash.Payments;

namespace Vodovoz.Dialogs.Cash.Payments
{
	[ToolboxItem(true)]
	public partial class OutgoingPaymentCreateView : TabViewBase<OutgoingPaymentCreateViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;

		public OutgoingPaymentCreateView(
			OutgoingPaymentCreateViewModel viewModel,
			ILifetimeScope lifetimeScope)
			: base(viewModel)
		{
			_lifetimeScope = lifetimeScope
				?? throw new ArgumentNullException(nameof(lifetimeScope));

			Build();

			Initialize();
		}

		private void Initialize()
		{
			dpPaymentDate.Binding
				.AddBinding(ViewModel, vm => vm.PaymentDate, w => w.DateOrNull)
				.InitializeFromSource();

			dpPaymentDate.IsEditable = true;

			var counterpartyViewModel = new LegacyEEVMBuilderFactory<OutgoingPaymentCreateViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(filter =>
				{
					filter.CounterpartyType = CounterpartyType.Supplier;
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

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
	public partial class OutgoingPaymentEditView : TabViewBase<OutgoingPaymentEditViewModel>
	{
		private readonly ILifetimeScope _lifetimeScope;

		public OutgoingPaymentEditView(
			OutgoingPaymentEditViewModel viewModel,
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
			var counterpartyViewModel = new LegacyEEVMBuilderFactory<OutgoingPaymentEditViewModel>(ViewModel, ViewModel, ViewModel.UoW, ViewModel.NavigationManager, _lifetimeScope)
				.ForProperty(x => x.Counterparty)
				.UseTdiDialog<CounterpartyDlg>()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(filter =>
				{
					filter.CounterpartyType = CounterpartyType.Supplier;
				})
				.Finish();

			ViewModel.CounterpartyViewModel = counterpartyViewModel;

			entryCounterparty.ViewModel = ViewModel.CounterpartyViewModel;

			ytvComment.Binding
				.AddBinding(ViewModel.Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();

			ytvPaymentPurpose.Binding
				.AddBinding(ViewModel.Entity, e => e.PaymentPurpose, w => w.Buffer.Text)
				.InitializeFromSource();

			ylblCashlessRequestId.Binding
				.AddBinding(ViewModel, e => e.CashlessRequestId, w => w.Text)
				.InitializeFromSource();

			ylblOrganizationName.Binding
				.AddBinding(ViewModel, vm => vm.OrganizationName, w => w.Text)
				.InitializeFromSource();

			ylblPaymentDate.Binding
				.AddBinding(
					ViewModel,
					vm => vm.PaymentDate,
					w => w.Text)
				.InitializeFromSource();

			ylblPaymentNumber.Binding
				.AddBinding(ViewModel, vm => vm.PaymentNumber, w => w.Text)
				.InitializeFromSource();

			ylblSum.Binding
				.AddBinding(ViewModel, vm => vm.Sum, w => w.Text)
				.InitializeFromSource();

			buttonSave.BindCommand(ViewModel.SaveCommand);
			buttonCancel.BindCommand(ViewModel.CancelCommand);
		}
	}
}

using QS.ViewModels.Control.EEVM;
using QS.Views;
using Vodovoz.Domain.Contacts;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Organizations;
using Vodovoz.ViewModels.ReportsParameters;

namespace Vodovoz.Reports
{
	public partial class Revision : ViewBase<RevisionReportViewModel>
	{
		public Revision(RevisionReportViewModel viewModel)
			: base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<RevisionReportViewModel>(
					ViewModel.RdlViewerViewModel,
					ViewModel.TdiTab,
					ViewModel,
					ViewModel.UnitOfWork,
					ViewModel.NavigationManager,
					ViewModel.LifetimeScope)
			.ForProperty(x => x.Counterparty)
			.UseTdiDialog<CounterpartyDlg>()
			.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
			.Finish();

			entryOrganization.ViewModel = new CommonEEVMBuilderFactory<RevisionReportViewModel>(
				ViewModel.RdlViewerViewModel,
				ViewModel,
				ViewModel.UnitOfWork,
				ViewModel.NavigationManager,
				ViewModel.LifetimeScope)
			.ForProperty(x => x.Organization)
			.UseViewModelJournalAndAutocompleter<OrganizationJournalViewModel>()
			.Finish();

			speciallistcomboboxEmail.SetRenderTextFunc<Email>(s => s.Address);
			speciallistcomboboxEmail.Binding
				.AddBinding(ViewModel, vm => vm.Emails, w => w.ItemsList)
				.AddBinding(ViewModel, vm => vm.SelectedEmail, w => w.SelectedItem)
				.AddBinding(ViewModel, vm => vm.CounterpartyIsSelected, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonRevision.Binding
				.AddBinding(ViewModel, vm => vm.IsSendRevision, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CounterpartyIsSelected, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonBillsForNotPaidOrders.Binding
				.AddBinding(ViewModel, vm => vm.IsSendBillsForNotPaidOrder, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CounterpartyIsSelected, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonGeneralBill.Binding
				.AddBinding(ViewModel, vm => vm.IsSendGeneralBill, w => w.Active)
				.AddBinding(ViewModel, vm => vm.CounterpartyIsSelected, w => w.Sensitive)
				.InitializeFromSource();

			buttonInfo.BindCommand(ViewModel.ShowInfoCommand);
			ybuttonSendByEmail.BindCommand(ViewModel.SendByEmailCommand);
			buttonRun.BindCommand(ViewModel.RunCommand);
		}

		public override void Destroy()
		{
			ViewModel.TdiTab = null;
			base.Destroy();
		}
	}
}


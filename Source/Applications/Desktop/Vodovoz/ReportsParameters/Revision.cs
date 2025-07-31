using QS.ViewModels.Control.EEVM;
using QS.Views;
using QSReport;
using ReactiveUI;
using System;
using Vodovoz.JournalViewModels;
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

			if(ViewModel.Counterparty != null)
			{
				OnReferenceCounterpartyChanged(ViewModel, EventArgs.Empty);
			}
		}

		private void Configure()
		{
			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<RevisionReportViewModel>(ViewModel.RdlViewerViewModel, ViewModel.TdiTab, ViewModel, ViewModel.UnitOfWork, ViewModel.NavigationManager, ViewModel.LifetimeScope)
					.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();

			entryCounterparty.ViewModel.PropertyChanged += (s, e) =>
			{
				if(e.PropertyName == nameof(entryCounterparty.ViewModel.Entity))
				{
					OnReferenceCounterpartyChanged(s, EventArgs.Empty);
				}
			};

			entityentryEmail.Sensitive = false;
			ybuttonSendByEmail.Sensitive = false;

			ycheckbuttonRevision.Binding
				.AddBinding(ViewModel, vm => vm.SendRevision, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonBillsForNotPaidOrders.Binding
				.AddBinding(ViewModel, vm => vm.SendBillsForNotPaidOrder, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonGeneralBill.Binding
				.AddBinding(ViewModel, vm => vm.SendGeneralBill, w => w.Active)
				.InitializeFromSource();

			ybuttonSendByEmail.BindCommand(ViewModel.SendByEmailCommand);
			buttonRun.BindCommand(ViewModel.RunCommand);
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnDateperiodpicker1PeriodChanged(object sender, EventArgs e)
		{
			ValidateParameters();
		}

		private void ValidateParameters()
		{
			var datePeriodSelected = ViewModel.EndDate.HasValue && ViewModel.StartDate.HasValue;
			var counterpartySelected = ViewModel.Counterparty != null;
			buttonRun.Sensitive = datePeriodSelected && counterpartySelected;
		}

		protected void OnReferenceCounterpartyChanged(object sender, EventArgs e)
		{
			ValidateParameters();

			bool hasCounterparty = ViewModel.Counterparty != null;
			bool hasEmails = hasCounterparty && ViewModel.Counterparty.Emails != null && ViewModel.Counterparty.Emails.Count > 0;

			entityentryEmail.Sensitive = hasCounterparty;

			if(hasCounterparty && hasEmails)
			{
				entityentryEmail.ViewModel = new LegacyEEVMBuilderFactory<RevisionReportViewModel>(ViewModel.RdlViewerViewModel, ViewModel.TdiTab, ViewModel, ViewModel.UnitOfWork, ViewModel.NavigationManager, ViewModel.LifetimeScope)
					.ForProperty(x => x.Email)
					.UseTdiEntityDialog()
					.UseOrmReferenceJournalAndAutocompleter()
					.Finish();
				//ViewModel.Email = ViewModel.Counterparty.Emails
			}
		}

		public override void Destroy()
		{
			entryCounterparty.ViewModel.PropertyChanged -= OnReferenceCounterpartyChanged;
			ViewModel.TdiTab = null;
			base.Destroy();
		}
	}
}


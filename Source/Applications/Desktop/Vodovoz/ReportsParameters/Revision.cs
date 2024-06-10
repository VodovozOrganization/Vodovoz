using QS.ViewModels.Control.EEVM;
using QS.Views;
using QSReport;
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

			dateperiodpicker1.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.TdiTab))
			{
				entryCounterparty.ViewModel = new LegacyEEVMBuilderFactory<RevisionReportViewModel>(ViewModel.RdlViewerViewModel, ViewModel.TdiTab, ViewModel, ViewModel.UnitOfWork, ViewModel.NavigationManager, ViewModel.LifetimeScope)
					.ForProperty(x => x.Counterparty)
					.UseTdiEntityDialog()
					.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
					.Finish();

				entryCounterparty.ViewModel.PropertyChanged += OnReferenceCounterpartyChanged;
			}
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;

		protected void OnButtonRunClicked(object sender, EventArgs e)
		{
			ViewModel.LoadReport();
		}

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
		}

		public override void Destroy()
		{
			entryCounterparty.ViewModel.PropertyChanged -= OnReferenceCounterpartyChanged;
			ViewModel.TdiTab = null;
			base.Destroy();
		}
	}
}


using System;
using System.ComponentModel;
using QS.Views.GtkUI;
using Vodovoz.Filters.ViewModels;

namespace Vodovoz.Filters.Views
{
	[ToolboxItem(true)]
	public partial class BankAccountsMovementsJournalFilterView : FilterViewBase<BankAccountsMovementsJournalFilterViewModel>
	{
		public BankAccountsMovementsJournalFilterView(BankAccountsMovementsJournalFilterViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			datePeriodAccountMovement.Binding
				.AddBinding(ViewModel, vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(ViewModel, vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			chkOnlyWithDiscrepancies.Binding
				.AddBinding(ViewModel, vm => vm.OnlyWithDiscrepancies, w => w.Active)
				.InitializeFromSource();
		}
	}
}

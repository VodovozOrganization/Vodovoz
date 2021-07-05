using QS.Views;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DebtorsJournalActionsView : ViewBase<DebtorsJournalActionsViewModel>
	{
		public DebtorsJournalActionsView(DebtorsJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			ybtnCreateTasks.Clicked += (sender, args) => ViewModel.CreateTasksCommand.Execute();
			ybtnSummaryBottlesAndDepositsReport.Clicked += (sender, args) => ViewModel.OpenReportCommand.Execute();
			ybtnOpenPrintingForm.Clicked += (sender, args) => ViewModel.OpenPrintingFormCommand.Execute();
		}
	}
}

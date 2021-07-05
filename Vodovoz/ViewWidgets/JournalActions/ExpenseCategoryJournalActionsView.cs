using Gtk;
using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ExpenseCategoryJournalActionsView : ViewBase<ExpenseCategoryJournalActionsViewModel>
	{
		public ExpenseCategoryJournalActionsView(ExpenseCategoryJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnExportData.Clicked += (sender, args) => ViewModel.ExportDataCommand.Execute();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}

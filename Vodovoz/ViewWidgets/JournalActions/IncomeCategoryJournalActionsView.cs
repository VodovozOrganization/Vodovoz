using Gtk;
using QS.Views;
using Vodovoz.Journals.JournalActionsViewModels;
using QS.Views.GtkUI;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class IncomeCategoryJournalActionsView : ViewBase<IncomeCategoryJournalActionsViewModel>
	{
		public IncomeCategoryJournalActionsView(IncomeCategoryJournalActionsViewModel viewModel) : base(viewModel)
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

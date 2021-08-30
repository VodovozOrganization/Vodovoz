using Gtk;
using QS.Views;
using Vodovoz.Journals.JournalActionsViewModels;
using QS.Project.Journal.Actions.Views;

namespace Vodovoz.ViewWidgets.JournalActions
{
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

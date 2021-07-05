using Gtk;
using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ComplaintsJournalActionsView : ViewBase<ComplaintsJournalActionsViewModel>
	{
		public ComplaintsJournalActionsView(ComplaintsJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnOpenReport.Clicked += (sender, e) => ViewModel.OpenReportCommand.Execute();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}

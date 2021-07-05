using Gtk;
using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FinancialDistrictsSetJournalActionsView : ViewBase<FinancialDistrictsSetJournalActionsViewModel>
	{
		public FinancialDistrictsSetJournalActionsView(FinancialDistrictsSetJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnFinancialDistrictSetCopy.Clicked += (sender, args) => ViewModel.CopyDistrictSetCommand.Execute();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}

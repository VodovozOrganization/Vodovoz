using Gtk;
using QS.Views;
using QS.Views.GtkUI;
using Vodovoz.Journals.JournalActionsViewModels;

namespace Vodovoz.ViewWidgets.JournalActions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PaymentsJournalActionsView : ViewBase<PaymentsJournalActionsViewModel>
	{
		public PaymentsJournalActionsView(PaymentsJournalActionsViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			CreateDefaultButtons();

			btnCompleteAllocation.Clicked += (sender, e) => ViewModel.CompleteAllocationCommand.Execute();
		}

		private void CreateDefaultButtons()
		{
			Widget entitiesJournalActions = new EntitiesJournalActionsView(ViewModel);
			hboxDefaultBtns.Add(entitiesJournalActions);
			entitiesJournalActions.Show();
		}
	}
}

using QS.Project.Journal;
using System;
using Vodovoz.ViewModels.Journals.FilterViewModels.Proposal;
using Vodovoz.ViewModels.Journals.JournalViewModels.Proposal;

public partial class MainWindow
{
	/// <summary>
	/// Открыть журнал предложений
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void OnActionOpenProposalsJournalActivated(object sender, EventArgs e)
	{
		var page = NavigationManager.OpenViewModel<ApplicationDevelopmentProposalsJournalViewModel, Action<ApplicationDevelopmentProposalsJournalFilterViewModel>>(null, filter => filter.HidenByDefault = true);

		page.ViewModel.SelectionMode = JournalSelectionMode.Multiple;
	}
}

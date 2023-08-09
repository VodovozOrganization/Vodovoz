using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services;
using System;
using Vodovoz.Infrastructure.Services;
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
		var filter = new ApplicationDevelopmentProposalsJournalFilterViewModel { HidenByDefault = true };

		tdiMain.AddTab(
			new ApplicationDevelopmentProposalsJournalViewModel(
				filter,
				new EmployeeService(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices)
			{ SelectionMode = JournalSelectionMode.Multiple }
		);
	}
}

using System;
using System.Linq;
using QS.Commands;
using QS.Project.Journal;
using QS.ViewModels;
using Vodovoz.ViewModels.Journals.Nodes;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class DebtorsJournalActionsViewModel : JournalActionsViewModel
	{
		private DelegateCommand _createTaskCommand;
		private DelegateCommand _openReportCommand;
		private DelegateCommand _openPrintingFormCommand;

		private Action<int, int> OpenReportAction;
		private Action OpenPrintingFormAction;
		private Func<DebtorJournalNode[], int> CreateTasksFunc;
		
		public DelegateCommand CreateTasksCommand => _createTaskCommand ?? (_createTaskCommand = new DelegateCommand(
			() => CreateTasksFunc?.Invoke(SelectedItems.OfType<DebtorJournalNode>().ToArray()),
			() => true
			)
		);
		
		public DelegateCommand OpenReportCommand => _openReportCommand ?? (_openReportCommand = new DelegateCommand(
				() =>
				{
					var selectedNode = SelectedItems.OfType<DebtorJournalNode>().FirstOrDefault();

					if(selectedNode != null)
					{
						OpenReportAction?.Invoke(selectedNode.ClientId, selectedNode.AddressId);
					}
				},
				() => true
			)
		);
		
		public DelegateCommand OpenPrintingFormCommand => _openPrintingFormCommand ?? (_openPrintingFormCommand = new DelegateCommand(
				() => OpenPrintingFormAction?.Invoke(),
				() => true
			)
		);

		public void Initialize(
			JournalSelectionMode selectionMode,
			Action<int, int> openReportAction,
			Action openPrintingFormAction,
			Func<DebtorJournalNode[], int> createTaskFunc)
		{
			OpenReportAction = openReportAction;
			OpenPrintingFormAction = openPrintingFormAction;
			CreateTasksFunc = createTaskFunc;
			
			CreateDefaultSelectAction(selectionMode, null, false);
		}
	}
}
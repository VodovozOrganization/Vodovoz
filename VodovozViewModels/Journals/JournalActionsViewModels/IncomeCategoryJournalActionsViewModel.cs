using System;
using QS.Commands;
using QS.Services;
using QS.ViewModels;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class IncomeCategoryJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private DelegateCommand _exportDataCommand;
		private Action _exportDataAction;
		
		public IncomeCategoryJournalActionsViewModel(IInteractiveService interactiveService) : base(interactiveService)
		{
		}
		
		public DelegateCommand ExportDataCommand => _exportDataCommand ?? (_exportDataCommand = new DelegateCommand(
				() => _exportDataAction?.Invoke(),
				() => true
			)
		);

		public void SetExportDataAction(Action exportDataAction)
		{
			_exportDataAction = exportDataAction;
		}
	}
}
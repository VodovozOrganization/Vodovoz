using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.Project.Journal;
using QS.Project.Journal.Actions.ViewModels;
using QS.ViewModels;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;

namespace Vodovoz.Journals.JournalActionsViewModels
{
	public class UndeliveredOrdersJournalActionsViewModel : EntitiesJournalActionsViewModel
	{
		private readonly UndeliveredOrdersFilterViewModel _journalFilterViewModel;
		private readonly IGtkTabsOpener _gtkDlgOpener;
		private DelegateCommand _printCommand;
		
		public UndeliveredOrdersJournalActionsViewModel(
			UndeliveredOrdersFilterViewModel journalFilterViewModel,
			IInteractiveService interactiveService,
			IGtkTabsOpener gtkDlgOpener) : base(interactiveService)
		{
			_journalFilterViewModel = journalFilterViewModel ?? throw new ArgumentNullException(nameof(journalFilterViewModel));
			_gtkDlgOpener = gtkDlgOpener ?? throw new ArgumentNullException(nameof(gtkDlgOpener));
		}

		protected override string DefaultAddLabel() => "Создать";
		
		protected override void DefaultAddAction()
		{
			var config = EntityConfigs.First().Value;
			var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault();
			foundDocumentConfig?.GetCreateEntityDlgConfigs().FirstOrDefault()?.OpenEntityDialogFunction();
		}

		protected override void DefaultEditAction()
		{
			var selectedNodes = SelectedItems.OfType<JournalEntityNodeBase>().ToArray();
			var selectedNode = selectedNodes.First();
			var config = EntityConfigs[selectedNode.EntityType];
			var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

			foundDocumentConfig?.GetOpenEntityDlgFunction().Invoke(selectedNode);
		}

		public DelegateCommand PrintCommand => _printCommand ?? (_printCommand = new DelegateCommand(
				() => _gtkDlgOpener.OpenUndeliveriesWithCommentsPrintDlg(JournalTab, _journalFilterViewModel)
			)
		);
	}
}
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QS.Dialog;
using Vodovoz.Journal;
using Vodovoz.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace
{
    public class HistoryTraceObjectJournalViewModel : JournalViewModelBase, INodeAutocompleteSelector
    {
        public HistoryTraceObjectJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, IInteractiveService interactiveService, INavigationManager navigation = null)
            : base(unitOfWorkFactory, interactiveService, navigation)
        {
            TabName = "Журнал объектов изменений";

            DataLoader = new AnyDataLoader<HistoryTraceObjectNode>(GetItems);

            DataLoader.ItemsListUpdated += (s, e) => { ListUpdated?.Invoke(this, EventArgs.Empty); };
            OnSelectResult += (s, e) => { 
                OnEntitySelectedResult?.Invoke(this,
                    new JournalSelectedNodesNodesEventArgs(e.SelectedObjects.OfType<IJournalNode>().ToArray())
                );
            };
        }

        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultSelectAction();
        }

        public Func<CancellationToken, IList<HistoryTraceObjectNode>> GetItems => (token) => {

            var result = HistoryMain.TraceClasses.OrderBy(x => x.DisplayName)?
                .Select(x => 
                new HistoryTraceObjectNode(x.ObjectType, x.DisplayName)
                {
                    ObjectName = x.ObjectName
                });

            if (Search.SearchValues != null && Search.SearchValues.Any(x => !String.IsNullOrWhiteSpace(x)))
            {
                foreach (var searchValue in Search.SearchValues)
                {
	                const StringComparison caseInsensitive = StringComparison.CurrentCultureIgnoreCase;
	                result = result.Where(n => n.DisplayName.IndexOf(searchValue, caseInsensitive) > -1
	                                           || n.ObjectType.ToString().IndexOf(searchValue, caseInsensitive) > -1);
                }
            }

            return result.ToList();
        };

        public event EventHandler ListUpdated;
		public event EventHandler<JournalSelectedNodesNodesEventArgs> OnEntitySelectedResult;

		public void SearchValues(params string[] values)
        {
            Search.SearchValues = values;
        }
    }
}

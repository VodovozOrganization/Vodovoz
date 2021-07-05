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
using QS.ViewModels;
using Vodovoz.Journal;
using Vodovoz.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace
{
    public class HistoryTracePropertyJournalViewModel : JournalViewModelBase, INodeAutocompleteSelector
    {
        public HistoryTracePropertyJournalViewModel(
	        JournalActionsViewModel journalActionsViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        IInteractiveService interactiveService,
	        INavigationManager navigation = null) 
            : base(journalActionsViewModel, unitOfWorkFactory, interactiveService, navigation)
        {
            TabName = "Журнал полей объектов изменений";

            DataLoader = new AnyDataLoader<HistoryTracePropertyNode>(GetItems);

            DataLoader.ItemsListUpdated += (s, e) => { ListUpdated?.Invoke(this, EventArgs.Empty); };
            OnSelectResult += (s, e) => {
                OnEntitySelectedResult?.Invoke(this,
                    new JournalSelectedNodesNodesEventArgs(e.SelectedObjects.OfType<JournalNodeBase>().ToArray())
                );
            };
        }

        public Type ObjectType { get; set; }

        public Func<CancellationToken, IList<HistoryTracePropertyNode>> GetItems => (token) => {
            if (ObjectType != null)
            {
                var tracedClass = HistoryMain.TraceClasses.Where(x => x.ObjectType == ObjectType).FirstOrDefault();
                var result = tracedClass.TracedProperties.OrderBy(x => x.DisplayName)?
                    .Select(x =>
                    new HistoryTracePropertyNode(ObjectType, x.DisplayName)
                    {
                        PropertyPath = x.FieldName
                    });

                if (Search.SearchValues != null && Search.SearchValues.Any(x => !String.IsNullOrWhiteSpace(x)))
                {
                    foreach (var searchValue in Search.SearchValues)
                    {
                        result = result.Where(n => n.PropertyName.IndexOf(searchValue, StringComparison.CurrentCultureIgnoreCase) > -1);
                    }
                }

                return result.ToList();
            }
            return new List<HistoryTracePropertyNode>();
        };

        public event EventHandler ListUpdated;
        public event EventHandler<JournalSelectedNodesNodesEventArgs> OnEntitySelectedResult;

        public void SearchValues(params string[] values)
        {
            Search.SearchValues = values;
        }
    }
}

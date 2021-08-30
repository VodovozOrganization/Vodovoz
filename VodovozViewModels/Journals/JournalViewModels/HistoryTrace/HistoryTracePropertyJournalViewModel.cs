using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using QS.Dialog;
using QS.Project.Journal.Actions.ViewModels;
using QS.ViewModels;
using Vodovoz.Journal;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Journals.FilterViewModels.HistoryTrace;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.HistoryTrace
{
    public class HistoryTracePropertyJournalViewModel : JournalViewModelBase, INodeAutocompleteSelector
    {
	    private readonly HistoryTracePropertyJournalFilterViewModel _journalFilterViewModel;
	    
        public HistoryTracePropertyJournalViewModel(
	        HistoryTracePropertyJournalFilterViewModel journalFilterViewModel,
	        JournalActionsViewModel journalActionsViewModel,
	        IUnitOfWorkFactory unitOfWorkFactory,
	        IInteractiveService interactiveService,
	        INavigationManager navigation = null) 
            : base(journalActionsViewModel, unitOfWorkFactory, interactiveService, navigation)
        {
	        _journalFilterViewModel = journalFilterViewModel;
	        
	        TabName = "Журнал полей объектов изменений";

            DataLoader = new AnyDataLoader<HistoryTracePropertyNode>(GetItems);

            DataLoader.ItemsListUpdated += (s, e) => { ListUpdated?.Invoke(this, EventArgs.Empty); };
            OnSelectResult += (s, e) => {
                OnEntitySelectedResult?.Invoke(this,
                    new JournalSelectedNodesNodesEventArgs(e.SelectedObjects.OfType<JournalNodeBase>().ToArray())
                );
            };
            
            InitializeJournalActionsViewModel();
        }

        public Func<CancellationToken, IList<HistoryTracePropertyNode>> GetItems => token =>
        {
            if (_journalFilterViewModel?.ObjectType != null)
            {
                var tracedClass =
	                HistoryMain.TraceClasses.FirstOrDefault(x => x.ObjectType == _journalFilterViewModel.ObjectType);
                
                var result = tracedClass.TracedProperties.OrderBy(x => x.DisplayName)
                    .Select(x =>
                    new HistoryTracePropertyNode(_journalFilterViewModel.ObjectType, x.DisplayName)
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

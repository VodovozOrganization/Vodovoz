using System;
using QS.Project.Journal;

namespace Vodovoz.Journal
{
	public class NodeAutocompleteSelectorFactory<TJournal> : INodeAutocompleteSelectorFactory
		where TJournal : JournalViewModelBase, INodeAutocompleteSelector
	{
		private readonly Func<TJournal> selectorCtorFunc;

		public NodeAutocompleteSelectorFactory(Type entityType, Func<TJournal> selectorCtorFunc)
		{
			EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
			this.selectorCtorFunc = selectorCtorFunc ?? throw new ArgumentNullException(nameof(selectorCtorFunc));
		}

		public Type EntityType { get; }

		public INodeAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var journal = selectorCtorFunc.Invoke();
			journal.SelectionMode = multipleSelect ? JournalSelectionMode.Single : JournalSelectionMode.Multiple;
			return journal;
		}

		public INodeSelector CreateSelector(bool multipleSelect = false)
		{
			return CreateAutocompleteSelector(multipleSelect);
		}
	}
}

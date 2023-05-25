using System;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IWarehouseJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSelectorFactory(Action<WarehouseJournalFilterViewModel> filterParams = null);
	}
}

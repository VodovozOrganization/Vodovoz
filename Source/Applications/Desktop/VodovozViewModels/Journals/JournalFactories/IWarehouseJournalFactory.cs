using System;
using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IWarehouseJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateSelectorFactory(
			ILifetimeScope lifetimeScope, Action<WarehouseJournalFilterViewModel> filterParams = null);
	}
}

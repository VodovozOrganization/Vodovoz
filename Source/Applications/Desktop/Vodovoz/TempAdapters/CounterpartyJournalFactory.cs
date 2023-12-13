using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class CounterpartyJournalFactory : ICounterpartyJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateCounterpartyAutocompleteSelectorFactory(ILifetimeScope lifetimeScope) =>
			new EntityAutocompleteSelectorFactory<CounterpartyJournalViewModel>(
				typeof(Counterparty),
				() => CreateJournalViewModel(lifetimeScope));

		private CounterpartyJournalViewModel CreateJournalViewModel(ILifetimeScope lifetimeScope) =>
			lifetimeScope.Resolve<CounterpartyJournalViewModel>();
	}
}

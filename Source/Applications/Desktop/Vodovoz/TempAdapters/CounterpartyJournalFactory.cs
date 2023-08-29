using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Client;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class CounterpartyJournalFactory : ICounterpartyJournalFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public CounterpartyJournalFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public IEntityAutocompleteSelectorFactory CreateCounterpartyAutocompleteSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<CounterpartyJournalViewModel>(typeof(Counterparty), CreateJournalViewModel);
		}

		private CounterpartyJournalViewModel CreateJournalViewModel() => _lifetimeScope.Resolve<CounterpartyJournalViewModel>();
	}
}

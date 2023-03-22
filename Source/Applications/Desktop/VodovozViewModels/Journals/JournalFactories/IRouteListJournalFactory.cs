using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public interface IRouteListJournalFactory
	{
		IEntityAutocompleteSelectorFactory CreateRouteListJournalAutocompleteSelectorFactory(ILifetimeScope scope,
			RouteListJournalFilterViewModel routeListJournalFilterViewModel= null);
	}
}

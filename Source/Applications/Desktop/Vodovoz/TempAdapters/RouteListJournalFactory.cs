using Autofac;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.TempAdapters
{
	public class RouteListJournalFactory : IRouteListJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateRouteListJournalAutocompleteSelectorFactory(ILifetimeScope scope,
			RouteListJournalFilterViewModel routeListJournalFilterViewModel = null)
		{
			return new EntityAutocompleteSelectorFactory<RouteListJournalViewModel>(typeof(RouteList), () =>
			{
				var routeListJournalViewModel = scope.Resolve<RouteListJournalViewModel>(
					new TypedParameter(typeof(RouteListJournalFilterViewModel), routeListJournalFilterViewModel));
				return routeListJournalViewModel;
			});
		}
	}
}

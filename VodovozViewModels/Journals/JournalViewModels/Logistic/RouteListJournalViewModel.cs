using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
    public class RouteListJournalViewModel : FilterableSingleEntityJournalViewModelBase<RouteList, RouteListViewModel, RouteListJournalNode, RouteListJournalFilterViewModel>
    {
        public RouteListJournalViewModel(RouteListJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
            : base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Журнал Маршрутных листов";
        }

        protected override Func<IUnitOfWork, IQueryOver<RouteList>> ItemsSourceQueryFunction => (uow) =>
        {
            RouteListJournalNode routeListJournalNodeAlias = null;
            RouteList routeListAlias = null;

            var query = uow.Session.QueryOver<RouteList>(() => routeListAlias);

            query.Where(GetSearchCriterion(
                () => routeListAlias.Id,
                () => routeListAlias.Title
            ));

            var result = query.SelectList(list => list
                .Select(u => u.Id).WithAlias(() => routeListJournalNodeAlias.Id)
                //.Select(u => u.).WithAlias(() => routeListJournalNodeAlias.)
                //.Select(u => u.Login).WithAlias(() => routeListJournalNodeAlias.Login)
                .Select(u => u.Title).WithAlias(() => routeListJournalNodeAlias.Title))
                .TransformUsing(Transformers.AliasToBean<RouteListJournalNode>());

            return result;
        };

        protected override Func<RouteListViewModel> CreateDialogFunction => () => new RouteListViewModel(EntityUoWBuilder.ForCreate(),
               UnitOfWorkFactory,
               commonServices);

        protected override Func<RouteListJournalNode, RouteListViewModel> OpenDialogFunction => (node) => new RouteListViewModel(
               EntityUoWBuilder.ForOpen(node.Id),
               UnitOfWorkFactory,
               commonServices);
    }
}

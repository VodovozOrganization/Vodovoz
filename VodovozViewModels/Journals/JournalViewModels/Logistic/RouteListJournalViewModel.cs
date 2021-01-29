using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
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
            DeliveryShift shiftAlias = null;
            Car carAlias = null;
            Employee driverAlias = null;
            Subdivision subdivisionAlias = null;

            var query = uow.Session.QueryOver<RouteList>(() => routeListAlias);

            query.Where(GetSearchCriterion(
                () => routeListAlias.Id,
                () => routeListAlias.Driver,
                () => routeListAlias.Car.Model,
                () => routeListAlias.Car.RegistrationNumber
            ));

            query.Left.JoinAlias(o => o.Driver, () => driverAlias);

            var result = query
                .Left.JoinAlias(o => o.Shift, () => shiftAlias)
                .Left.JoinAlias(o => o.Car, () => carAlias)
                .Left.JoinAlias(o => o.ClosingSubdivision, () => subdivisionAlias)
                .SelectList(list => list
                   .SelectGroup(() => routeListAlias.Id).WithAlias(() => routeListJournalNodeAlias.Id)
                   .Select(() => routeListAlias.Date).WithAlias(() => routeListJournalNodeAlias.Date)
                   .Select(() => routeListAlias.Status).WithAlias(() => routeListJournalNodeAlias.StatusEnum)
                   .Select(() => shiftAlias.Name).WithAlias(() => routeListJournalNodeAlias.ShiftName)
                   .Select(() => carAlias.Model).WithAlias(() => routeListJournalNodeAlias.CarModel)
                   .Select(() => carAlias.RegistrationNumber).WithAlias(() => routeListJournalNodeAlias.CarNumber)
                   .Select(() => driverAlias.LastName).WithAlias(() => routeListJournalNodeAlias.DriverSurname)
                   .Select(() => driverAlias.Name).WithAlias(() => routeListJournalNodeAlias.DriverName)
                   .Select(() => driverAlias.Patronymic).WithAlias(() => routeListJournalNodeAlias.DriverPatronymic)
                   .Select(() => routeListAlias.LogisticiansComment).WithAlias(() => routeListJournalNodeAlias.LogisticiansComment)
                   .Select(() => routeListAlias.ClosingComment).WithAlias(() => routeListJournalNodeAlias.ClosinComments)
                   .Select(() => subdivisionAlias.Name).WithAlias(() => routeListJournalNodeAlias.ClosingSubdivision)
                   .Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => routeListJournalNodeAlias.NotFullyLoaded)
                   .Select(() => carAlias.TypeOfUse).WithAlias(() => routeListJournalNodeAlias.CarTypeOfUse)
                ).OrderBy(rl => rl.Date).Desc
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

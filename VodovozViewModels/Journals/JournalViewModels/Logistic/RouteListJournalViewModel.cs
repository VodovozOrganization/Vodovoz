using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;
using static Vodovoz.ViewModels.Journals.FilterViewModels.Logistic.RouteListJournalFilterViewModel;

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
            GeographicGroup geographicalGroupAlias = null;

            var query = uow.Session.QueryOver<RouteList>(() => routeListAlias);

            var filterViewModel = Filter as RouteListJournalFilterViewModel;

            if (filterViewModel.SelectedStatuses != null)
            {
                query.WhereRestrictionOn(o => o.Status).IsIn(filterViewModel.SelectedStatuses);
            }

            if (filterViewModel.DeliveryShift != null)
            {
                query.Where(o => o.Shift == filterViewModel.DeliveryShift);
            }

            if (filterViewModel.StartDate != null)
            {
                query.Where(o => o.Date >= filterViewModel.StartDate);
            }

            if (filterViewModel.EndDate != null)
            {
                query.Where(o => o.Date <= filterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
            }

            if (filterViewModel.GeographicGroup != null)
            {
                query.Left.JoinAlias(o => o.GeographicGroups, () => geographicalGroupAlias)
                     .Where(() => geographicalGroupAlias.Id == filterViewModel.GeographicGroup.Id);
            }

            #region RouteListAddressTypeFilter

            if (filterViewModel.WithDeliveryAddresses && filterViewModel.WithChainStoreAddresses && !filterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.VisitingMaster);
            }
            else if (filterViewModel.WithDeliveryAddresses && !filterViewModel.WithChainStoreAddresses && filterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.IsChainStoreDriver);
            }
            else if (filterViewModel.WithDeliveryAddresses && !filterViewModel.WithChainStoreAddresses && !filterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.VisitingMaster);
                query.Where(() => !driverAlias.IsChainStoreDriver);
            }
            else if (!filterViewModel.WithDeliveryAddresses && filterViewModel.WithChainStoreAddresses && filterViewModel.WithServiceAddresses)
            {
                query.Where(Restrictions.Or(
                    Restrictions.Where(() => driverAlias.VisitingMaster),
                    Restrictions.Where(() => driverAlias.IsChainStoreDriver)
                ));
            }
            else if (!filterViewModel.WithDeliveryAddresses && filterViewModel.WithChainStoreAddresses && !filterViewModel.WithServiceAddresses)
            {
                query.Where(() => driverAlias.IsChainStoreDriver);
            }
            else if (!filterViewModel.WithDeliveryAddresses && !filterViewModel.WithChainStoreAddresses && filterViewModel.WithServiceAddresses)
            {
                query.Where(() => driverAlias.VisitingMaster);
            }
            else if (!filterViewModel.WithDeliveryAddresses && !filterViewModel.WithChainStoreAddresses && !filterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.IsChainStoreDriver && !driverAlias.VisitingMaster);
            }

            #endregion

            switch (filterViewModel.TransportType)
            {
                case RLFilterTransport.Mercenaries:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar && !carAlias.IsRaskat); break;
                case RLFilterTransport.Raskat:
                    query.Where(() => carAlias.IsRaskat); break;
                case RLFilterTransport.Largus:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyLargus); break;
                case RLFilterTransport.GAZelle:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyGAZelle); break;
                case RLFilterTransport.Waggon:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyTruck); break;
                case RLFilterTransport.Others:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar); break;
                default: break;
            }

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

using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.JournalViewModels
{
    public class RouteListWorkingJournalViewModel : FilterableSingleEntityJournalViewModelBase<RouteList, TdiTabBase, RouteListJournalNode, RouteListJournalFilterViewModel>
    {
        private readonly IRouteListRepository routeListRepository;
        private readonly FuelRepository fuelRepository;
        private readonly CallTaskRepository callTaskRepository;
        private readonly BaseParametersProvider baseParametersProvider;
        private readonly SubdivisionRepository subdivisionRepository;

        public RouteListWorkingJournalViewModel(
            RouteListJournalFilterViewModel filterViewModel,
            IUnitOfWorkFactory unitOfWorkFactory,
            ICommonServices commonServices,
            IRouteListRepository routeListRepository,
            FuelRepository fuelRepository,
            CallTaskRepository callTaskRepository,
            BaseParametersProvider baseParametersProvider,
            SubdivisionRepository subdivisionRepository) :
            base(filterViewModel, unitOfWorkFactory, commonServices)
        {
            TabName = "Работа кассы с МЛ";
            this.routeListRepository = routeListRepository;
            this.fuelRepository = fuelRepository;
            this.callTaskRepository = callTaskRepository;
            this.baseParametersProvider = baseParametersProvider;
            this.subdivisionRepository = subdivisionRepository;
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

            var query = uow.Session.QueryOver(() => routeListAlias)
                .Left.JoinAlias(o => o.Shift, () => shiftAlias)
                .Left.JoinAlias(o => o.Car, () => carAlias)
                .Left.JoinAlias(o => o.ClosingSubdivision, () => subdivisionAlias)
                .Left.JoinAlias(o => o.Driver, () => driverAlias);

            if (FilterViewModel.SelectedStatuses != null)
            {
                query.WhereRestrictionOn(o => o.Status).IsIn(FilterViewModel.SelectedStatuses);
            }

            if (FilterViewModel.DeliveryShift != null)
            {
                query.Where(o => o.Shift == FilterViewModel.DeliveryShift);
            }

            if (FilterViewModel.StartDate != null)
            {
                query.Where(o => o.Date >= FilterViewModel.StartDate);
            }

            if (FilterViewModel.EndDate != null)
            {
                query.Where(o => o.Date <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
            }

            if (FilterViewModel.GeographicGroup != null)
            {
                query.Left.JoinAlias(o => o.GeographicGroups, () => geographicalGroupAlias)
                     .Where(() => geographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
            }

            #region RouteListAddressTypeFilter

            if (FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.VisitingMaster);
            }
            else if (FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.IsChainStoreDriver);
            }
            else if (FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => !driverAlias.VisitingMaster);
                query.Where(() => !driverAlias.IsChainStoreDriver);
            }
            else if (!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
            {
                query.Where(Restrictions.Or(
                    Restrictions.Where(() => driverAlias.VisitingMaster),
                    Restrictions.Where(() => driverAlias.IsChainStoreDriver)
                ));
            }
            else if (!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => driverAlias.IsChainStoreDriver);
            }
            else if (!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => driverAlias.VisitingMaster);
            }
            else if (!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
            {
                query.Where(() => routeListAlias.Id == null);
            }

            #endregion

            switch (FilterViewModel.TransportType)
            {
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.Mercenaries:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar && !carAlias.IsRaskat); break;
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.Raskat:
                    query.Where(() => carAlias.IsRaskat); break;
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.Largus:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyLargus); break;
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.GAZelle:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyGAZelle); break;
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.Waggon:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyTruck); break;
                case (RouteListJournalFilterViewModel.RLFilterTransport?)RLFilterTransport.Others:
                    query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar); break;
                default: break;
            }

            query.Where(GetSearchCriterion(
                () => routeListAlias.Id,
                () => routeListAlias.Driver,
                () => routeListAlias.Car.Model,
                () => routeListAlias.Car.RegistrationNumber
            ));

            var result = query                
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

        protected override Func<TdiTabBase> CreateDialogFunction => () => throw new NotSupportedException();

        #region restrictions

        private List<RouteListStatus> closingDlgStatuses = new List<RouteListStatus> {
            RouteListStatus.Delivered,
            RouteListStatus.OnClosing,
            RouteListStatus.MileageCheck,
        };

        private List<RouteListStatus> createCarLoadDocument = new List<RouteListStatus> {
            RouteListStatus.InLoading
        };

        private List<RouteListStatus> createCarUnloadDocument = new List<RouteListStatus> {
            RouteListStatus.Delivered,
            RouteListStatus.OnClosing,
            RouteListStatus.MileageCheck,
        };

        private List<RouteListStatus> fuelIssuingStatuses = new List<RouteListStatus> {
            RouteListStatus.New,
            RouteListStatus.Confirmed,
            RouteListStatus.InLoading,
            RouteListStatus.EnRoute,
            RouteListStatus.Delivered,
            RouteListStatus.OnClosing,
            RouteListStatus.MileageCheck
        };

        private List<RouteListStatus> canReturnToOnClosing = new List<RouteListStatus>
        {
            RouteListStatus.Closed
        };

        #endregion

        protected override Func<RouteListJournalNode, TdiTabBase> OpenDialogFunction => (node) =>
        {
            switch (node.StatusEnum)
            {
                case RouteListStatus.New:
                    return new RouteListCreateDlg(node.Id);
                case RouteListStatus.InLoading:
                    if (routeListRepository.IsTerminalRequired(UoW, node.Id))
                    {
                        return new CarLoadDocumentDlg(node.Id, null);
                    }
                    else
                    {
                        return new RouteListCreateDlg(node.Id);
                    }
                case RouteListStatus.EnRoute:
                    return new RouteListKeepingDlg(node.Id);
                case RouteListStatus.Delivered:
                case RouteListStatus.OnClosing:
                case RouteListStatus.MileageCheck:
                case RouteListStatus.Closed:
                    return new RouteListClosingDlg(node.Id);
                default:
                    throw new InvalidOperationException("Неизвестный статус МЛ");
            }
        };

        protected override void CreatePopupActions()
        {
            base.CreatePopupActions();

            var callTaskWorker = new CallTaskWorker(
                    CallTaskSingletonFactory.GetInstance(),
                    callTaskRepository,
                    OrderSingletonRepository.GetInstance(),
                    EmployeeSingletonRepository.GetInstance(),
                    baseParametersProvider,
                    commonServices.UserService,
                    SingletonErrorReporter.Instance);

            PopupActionsList.Add(new JournalAction(
                "Закрытие МЛ",
                (selectedItems) => selectedItems.Any(x => closingDlgStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) => selectedItems.Any(x => closingDlgStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) =>
                {
                    var selectedNode = selectedItems.FirstOrDefault() as RouteListJournalNode;
                    if (selectedNode != null && closingDlgStatuses.Contains(selectedNode.StatusEnum))
                    {
                        TabParent.OpenTab(
                            DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
                            () => new RouteListClosingDlg(selectedNode.Id)
                        );
                    }
                }
            ));

            PopupActionsList.Add(new JournalAction(
                "Создание талона погрузки",
                (selectedItems) => selectedItems.Any(x => createCarLoadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) => selectedItems.Any(x => createCarLoadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) =>
                {
                    var selectedNode = selectedItems.FirstOrDefault() as RouteListJournalNode;
                    if (selectedNode != null)
                    {
                        var dlg = new CarLoadDocumentDlg(selectedNode.Id, null);
                        TabParent.AddTab(dlg, this);
                    }
                }
            ));

            PopupActionsList.Add(new JournalAction(
                "Создание талона разгрузки",
                (selectedItems) => selectedItems.Any(x => createCarUnloadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) => selectedItems.Any(x => createCarUnloadDocument.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) =>
                {
                    var selectedNode = selectedItems.FirstOrDefault() as RouteListJournalNode;
                    if (selectedNode != null)
                    {
                        var dlg = new CarUnloadDocumentDlg(selectedNode.Id, null);
                        TabParent.AddTab(dlg, this);
                    }
                }
            ));

            PopupActionsList.Add(new JournalAction(
                "Выдать топливо",
                (selectedItems) => selectedItems.Any(x => fuelIssuingStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) => selectedItems.Any(x => fuelIssuingStatuses.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) =>
                {
                    var selectedNode = selectedItems.FirstOrDefault() as RouteListJournalNode;
                    if (selectedNode != null)
                    {
                        var RouteList = UoW.GetById<RouteList>(selectedNode.Id);
                        TabParent.OpenTab(
                            DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
                            () => new FuelDocumentViewModel(
                                RouteList,
                                commonServices,
                                subdivisionRepository,
                                EmployeeSingletonRepository.GetInstance(),
                                fuelRepository,
                                NavigationManagerProvider.NavigationManager
                            )
                        );
                    }
                }
            ));

            PopupActionsList.Add(new JournalAction(
                "Вернуть в статус Сдается",
                (selectedItems) => selectedItems.Any(x => canReturnToOnClosing.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) => selectedItems.Any(x => canReturnToOnClosing.Contains((x as RouteListJournalNode).StatusEnum)),
                (selectedItems) =>
                {
                    var selectedNode = selectedItems.FirstOrDefault() as RouteListJournalNode;
                    bool isSlaveTabActive = false;
                    if (selectedNode != null)
                    {
                        using (var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
                        {
                            var routeList = uowLocal.Session.QueryOver<RouteList>()
                                .Where(x => x.Id == selectedNode.Id)
                                .List().FirstOrDefault();

                            if (canReturnToOnClosing.Contains(routeList.Status))
                            {
                                if (TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
                                {
                                    MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
                                    isSlaveTabActive = true;
                                    return;
                                }
                                routeList.ChangeStatusAndCreateTask(RouteListStatus.OnClosing, callTaskWorker);
                                uowLocal.Save(routeList);
                                if (isSlaveTabActive)
                                    return;
                            }
                            uowLocal.Commit();
                        }
                        Refresh();
                    }
                }
            ));
        }


        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
        }
    }
}

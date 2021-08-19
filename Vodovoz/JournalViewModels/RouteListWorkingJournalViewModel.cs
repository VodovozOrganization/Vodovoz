using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Infrastructure;
using Vodovoz.Parameters;
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

            UseSlider = false;

            NotifyConfiguration.Enable();
            NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(OnRouteListChanged);

            InitPopupActions();
        }

        private void OnRouteListChanged(EntityChangeEvent[] changeEvents)
        {
            Refresh();
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

            if(FilterViewModel.ShowDriversWithTerminal)
            {
	            DriverAttachedTerminalDocumentBase baseAlias = null;
	            DriverAttachedTerminalGiveoutDocument giveoutAlias = null;
	            var baseQuery = QueryOver.Of(() => baseAlias)
		            .Where(doc => doc.Driver.Id == routeListAlias.Driver.Id)
		            .And(doc => doc.CreationDate.Date <= routeListAlias.Date)
		            .Select(doc => doc.Id).OrderBy(doc => doc.CreationDate).Desc.Take(1);
	            var giveoutQuery = QueryOver.Of(() => giveoutAlias).WithSubquery.WhereProperty(giveout => giveout.Id).Eq(baseQuery)
		            .Select(doc => doc.Driver.Id);
	            query.WithSubquery.WhereProperty(rl => rl.Driver.Id).In(giveoutQuery);
            }

            switch (FilterViewModel.TransportType)
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

            var driverProjection = Projections.SqlFunction(
                new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT_WS(' ', ?1, ?2, ?3)"),
                NHibernateUtil.String,
                Projections.Property(() => driverAlias.LastName),
                Projections.Property(() => driverAlias.Name),
                Projections.Property(() => driverAlias.Patronymic)
            );

            query.Where(GetSearchCriterion(
                () => routeListAlias.Id,
                () => driverAlias.Name,
                () => driverAlias.LastName,
                () => driverAlias.Patronymic,
                () => driverProjection,
                () => carAlias.Model,
                () => carAlias.RegistrationNumber
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
                case RouteListStatus.Confirmed:
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

        protected void InitPopupActions()
        {
            var callTaskWorker = new CallTaskWorker(
                    CallTaskSingletonFactory.GetInstance(),
                    callTaskRepository,
                    new OrderRepository(),
                    new EmployeeRepository(),
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
                        TabParent.OpenTab(() => new CarLoadDocumentDlg(selectedNode.Id, null));
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
                        TabParent.OpenTab(() => new CarUnloadDocumentDlg(selectedNode.Id, null));
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
                                new EmployeeRepository(),
                                fuelRepository,
                                NavigationManagerProvider.NavigationManager,
                                new TrackRepository(),
                                new CategoryRepository(new ParametersProvider())
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
                    }
                }
            ));
        }


        protected override void CreateNodeActions()
        {
            NodeActionsList.Clear();
            CreateDefaultEditAction();
        }
    }
}

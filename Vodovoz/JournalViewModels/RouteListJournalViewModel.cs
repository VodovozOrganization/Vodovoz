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
using System.Text;
using Dialogs.Logistic;
using QS.Deletion;
using QS.Dialog;
using QS.Project.Domain;
using QS.Project.Services;
using Vodovoz.Additions.Store;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Permissions;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class RouteListJournalViewModel : FilterableSingleEntityJournalViewModelBase<RouteList, TdiTabBase, RouteListJournalNode,
		RouteListJournalFilterViewModel>
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICategoryRepository _categoryRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository;
		private readonly IDeliveryShiftRepository _deliveryShiftRepository;
		private readonly IRouteListParametersProvider _routeListParametersProvider;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly ICarJournalFactory _carJournalFactory;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IOrderSelectorFactory _orderSelectorFactory;
		private readonly ICounterpartyJournalFactory _counterpartyJournalFactory;
		private readonly IDeliveryPointJournalFactory _deliveryPointJournalFactory;
		private readonly ISubdivisionJournalFactory _subdivisionJournalFactory;
		private readonly IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;

		private bool? _userHasOnlyAccessToWarehouseAndComplaints;
		private bool? _canCreateSelfDriverTerminalTransferDocument;
		private IList<Warehouse> _warehousesAvailableForUser;

		public RouteListJournalViewModel(
			RouteListJournalFilterViewModel filterViewModel,
			IRouteListRepository routeListRepository,
			IFuelRepository fuelRepository,
			ISubdivisionRepository subdivisionRepository,
			ICategoryRepository categoryRepository,
			ITrackRepository trackRepository,
			IUndeliveredOrdersRepository undeliveredOrdersRepository,
			IDeliveryShiftRepository deliveryShiftRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListParametersProvider routeListParametersProvider,
			ICallTaskWorker callTaskWorker,
			IWarehouseRepository warehouseRepository,
			ICarJournalFactory carJournalFactory,
			IEmployeeJournalFactory employeeJournalFactory,
			IEmployeeRepository employeeRepository,
			IGtkTabsOpener gtkTabsOpener,
			IOrderSelectorFactory orderSelectorFactory,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryPointJournalFactory deliveryPointJournalFactory,
			ISubdivisionJournalFactory subdivisionJournalFactory,
			IUndeliveredOrdersJournalOpener undeliveredOrdersJournalOpener,
			ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_undeliveredOrdersRepository = undeliveredOrdersRepository ?? throw new ArgumentNullException(nameof(undeliveredOrdersRepository));
			_deliveryShiftRepository = deliveryShiftRepository ?? throw new ArgumentNullException(nameof(deliveryShiftRepository));
			_routeListParametersProvider = routeListParametersProvider ?? throw new ArgumentNullException(nameof(routeListParametersProvider));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_carJournalFactory = carJournalFactory ?? throw new ArgumentNullException(nameof(carJournalFactory));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_orderSelectorFactory = orderSelectorFactory ?? throw new ArgumentNullException(nameof(orderSelectorFactory));
			_counterpartyJournalFactory = counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory));
			_deliveryPointJournalFactory = deliveryPointJournalFactory ?? throw new ArgumentNullException(nameof(deliveryPointJournalFactory));
			_subdivisionJournalFactory = subdivisionJournalFactory ?? throw new ArgumentNullException(nameof(subdivisionJournalFactory));
			_undeliveredOrdersJournalOpener = undeliveredOrdersJournalOpener ?? throw new ArgumentNullException(nameof(undeliveredOrdersJournalOpener));

			TabName = "Журнал МЛ";

			NotifyConfiguration.Enable();
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<RouteList>(changeEvents => Refresh());

			InitPopupActions();
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

			if(FilterViewModel.SelectedStatuses != null)
			{
				query.WhereRestrictionOn(o => o.Status).IsIn(FilterViewModel.SelectedStatuses);
			}

			if(FilterViewModel.DeliveryShift != null)
			{
				query.Where(o => o.Shift == FilterViewModel.DeliveryShift);
			}

			if(FilterViewModel.StartDate != null)
			{
				query.Where(o => o.Date >= FilterViewModel.StartDate);
			}

			if(FilterViewModel.EndDate != null)
			{
				query.Where(o => o.Date <= FilterViewModel.EndDate.Value.AddDays(1).AddTicks(-1));
			}

			if(FilterViewModel.GeographicGroup != null)
			{
				query.Left.JoinAlias(o => o.GeographicGroups, () => geographicalGroupAlias)
					.Where(() => geographicalGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
			}

			#region RouteListAddressTypeFilter

			if(FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses && !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.VisitingMaster);
			}
			else if(FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses &&
			        FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses &&
			        !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => !driverAlias.VisitingMaster);
				query.Where(() => !driverAlias.IsChainStoreDriver);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses &&
			        FilterViewModel.WithServiceAddresses)
			{
				query.Where(Restrictions.Or(
					Restrictions.Where(() => driverAlias.VisitingMaster),
					Restrictions.Where(() => driverAlias.IsChainStoreDriver)
				));
			}
			else if(!FilterViewModel.WithDeliveryAddresses && FilterViewModel.WithChainStoreAddresses &&
			        !FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => driverAlias.IsChainStoreDriver);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses &&
			        FilterViewModel.WithServiceAddresses)
			{
				query.Where(() => driverAlias.VisitingMaster);
			}
			else if(!FilterViewModel.WithDeliveryAddresses && !FilterViewModel.WithChainStoreAddresses &&
			        !FilterViewModel.WithServiceAddresses)
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

			switch(FilterViewModel.TransportType)
			{
				case RLFilterTransport.Mercenaries:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar && !carAlias.IsRaskat);
					break;
				case RLFilterTransport.Raskat:
					query.Where(() => carAlias.IsRaskat);
					break;
				case RLFilterTransport.Largus:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyLargus);
					break;
				case RLFilterTransport.GAZelle:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyGAZelle);
					break;
				case RLFilterTransport.Waggon:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.CompanyTruck);
					break;
				case RLFilterTransport.Others:
					query.Where(() => carAlias.TypeOfUse == CarTypeOfUse.DriverCar);
					break;
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
					.Select(() => driverAlias.Comment).WithAlias(() => routeListJournalNodeAlias.DriverComment)
					.Select(() => routeListAlias.LogisticiansComment).WithAlias(() => routeListJournalNodeAlias.LogisticiansComment)
					.Select(() => routeListAlias.ClosingComment).WithAlias(() => routeListJournalNodeAlias.ClosinComments)
					.Select(() => subdivisionAlias.Name).WithAlias(() => routeListJournalNodeAlias.ClosingSubdivision)
					.Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => routeListJournalNodeAlias.NotFullyLoaded)
					.Select(() => carAlias.TypeOfUse).WithAlias(() => routeListJournalNodeAlias.CarTypeOfUse)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListJournalNode>());

			return result;
		};

		protected override Func<TdiTabBase> CreateDialogFunction => () => new RouteListCreateDlg();

		protected override Func<RouteListJournalNode, TdiTabBase> OpenDialogFunction => node => new RouteListCreateDlg(node.Id);

		public IList<Warehouse> GetWarehousesAvailableForUser => _warehousesAvailableForUser
			?? (_warehousesAvailableForUser = StoreDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissions.WarehouseView));

		#region PopupActions

		protected void InitPopupActions()
		{
			if(UserHasOnlyAccessToWarehouseAndComplaints)
			{
				return;
			}

			PopupActionsList.Add(CreateOpenTrackAction());
			PopupActionsList.Add(CreateOpenCreateDialogAction());
			PopupActionsList.Add(CreateOpenRouteListControlDlg());
			PopupActionsList.Add(CreateSendRouteListToLoadingAction());
			PopupActionsList.Add(CreateOpenKeepingDialogAction());
			PopupActionsList.Add(CreateReturnToEnRouteAction());
			PopupActionsList.Add(CreateOpenClosingDialogAction());
			PopupActionsList.Add(CreateOpenAnalysisDialogAction());
			PopupActionsList.Add(CreateOpenMileageCheckDialogAction());
			PopupActionsList.Add(CreateDeleteRouteListAction());
			PopupActionsList.Add(CreateGiveFuelAction());
			PopupActionsList.Add(CreateTransferTerminalAction());
		}

		private IJournalAction CreateTransferTerminalAction()
		{
			return new JournalAction(
				"Перенести терминал на вторую ходку",
				selectedItems => CanCreateSelfDriverTerminalTransferDocument,
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						var routeList = UoW.GetById<RouteList>(selectedNode.Id);
						routeList?.CreateSelfDriverTerminalTransferDocument();
					}
				}
			);
		}

		private IJournalAction CreateGiveFuelAction()
		{
			return new JournalAction(
				"Выдать топливо",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _fuelIssuingStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode == null)
					{
						return;
					}

					var routeList = UoW.GetById<RouteList>(selectedNode.Id);
					TabParent.OpenTab(
						DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
						() => new FuelDocumentViewModel(
							routeList,
							commonServices,
							_subdivisionRepository,
							_employeeRepository,
							_fuelRepository,
							NavigationManagerProvider.NavigationManager,
							_trackRepository,
							_categoryRepository,
							_employeeJournalFactory,
							_carJournalFactory
						)
					);
				}
			);
		}

		private IJournalAction CreateDeleteRouteListAction()
		{
			return new JournalAction(
				"Удалить МЛ",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& CanDeleteRouteList(node),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode == null)
					{
						return;
					}

					var routeList = UoW.Session.QueryOver<RouteList>()
						.Where(x => x.Id == selectedNode.Id)
						.SingleOrDefault<RouteList>();

					var orders = new List<Order>();

					foreach(var address in routeList.Addresses)
					{
						UoW.Session.Refresh(address.Order);
						if(address.Order.OrderStatus == OrderStatus.OnLoading
						   || address.Order.OrderStatus == OrderStatus.InTravelList)
						{
							orders.Add(address.Order);
						}
					}

					if(!DeleteHelper.DeleteEntity.Invoke(typeof(RouteList), selectedNode.Id))
					{
						return;
					}

					foreach(var order in orders)
					{
						order.ChangeStatusAndCreateTasks(OrderStatus.Accepted, _callTaskWorker);
						UoW.Save(order);
					}

					UoW.Commit();
					Refresh();
				}
			);
		}

		private IJournalAction CreateOpenMileageCheckDialogAction()
		{
			return new JournalAction(
				"Открыть диалог проверки километража",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _mileageCheckDlgStatuses.Contains(node.StatusEnum)
					&& node.UsesCompanyCar
					&& node.CarTypeOfUse != CarTypeOfUse.CompanyTruck,
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null && _mileageCheckDlgStatuses.Contains(selectedNode.StatusEnum) &&
					   selectedNode.CarTypeOfUse != CarTypeOfUse.CompanyTruck)
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListMileageCheckDlg(selectedNode.Id)
						);
					}
				}
			);
		}

		private IJournalAction CreateOpenAnalysisDialogAction()
		{
			return new JournalAction(
				"Открыть диалог разбора",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _analysisViewModelStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null && _analysisViewModelStatuses.Contains(selectedNode.StatusEnum))
					{
						TabParent.AddTab(
							new RouteListAnalysisViewModel(
								EntityUoWBuilder.ForOpen(selectedNode.Id),
								UnitOfWorkFactory,
								commonServices,
								_orderSelectorFactory,
								_employeeJournalFactory,
								_counterpartyJournalFactory,
								_deliveryPointJournalFactory,
								_subdivisionJournalFactory,
								_gtkTabsOpener,
								_undeliveredOrdersJournalOpener,
								_deliveryShiftRepository,
								_undeliveredOrdersRepository
							),
							this
						);
					}
				}
			);
		}

		private IJournalAction CreateOpenClosingDialogAction()
		{
			return new JournalAction(
				"Открыть диалог закрытия",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _closingDlgStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null && _closingDlgStatuses.Contains(selectedNode.StatusEnum))
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListClosingDlg(selectedNode.Id)
						);
					}
				}
			);
		}

		private IJournalAction CreateReturnToEnRouteAction()
		{
			return new JournalAction(
				"Вернуть в путь",
				selectedItems => selectedItems.Any(x => _canReturnToEnRoute.Contains(((RouteListJournalNode)x).StatusEnum)),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<RouteListJournalNode>();
					var routeListIds = selectedNodes.Select(x => x.Id).ToArray();
					bool isSlaveTabActive = false;

					using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var routeLists = uowLocal.Session.QueryOver<RouteList>()
							.Where(x => x.Id.IsIn(routeListIds))
							.List();

						foreach(var routeList in routeLists.Where(arg => arg.Status == RouteListStatus.Delivered))
						{
							if(TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
							{
								commonServices.InteractiveService.ShowMessage(
									ImportanceLevel.Info, "Требуется закрыть подчиненную вкладку");
								isSlaveTabActive = true;
								continue;
							}
							routeList.ChangeStatusAndCreateTask(RouteListStatus.EnRoute, _callTaskWorker);
							uowLocal.Save(routeList);
						}

						if(isSlaveTabActive)
						{
							return;
						}

						uowLocal.Commit();
					}
				}
			);
		}

		private IJournalAction CreateOpenKeepingDialogAction()
		{
			return new JournalAction(
				"Открыть диалог ведения",
				selectedItems => selectedItems.Any(x => _keepingDlgStatuses.Contains(((RouteListJournalNode)x).StatusEnum)),
				selectedItems => true,
				(selectedItems) =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null && _keepingDlgStatuses.Contains(selectedNode.StatusEnum))
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListKeepingDlg(selectedNode.Id)
						);
					}
				}
			);
		}

		private IJournalAction CreateSendRouteListToLoadingAction()
		{
			return new JournalAction(
				"Отправить МЛ на погрузку",
				selectedItems => selectedItems.Any(x => ((RouteListJournalNode)x).StatusEnum == RouteListStatus.Confirmed),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNodes = selectedItems.Cast<RouteListJournalNode>().ToList();
					if(selectedNodes.Any())
					{
						SendRouteListsInLoading(selectedNodes);
					}
				}
			);
		}

		private IJournalAction CreateOpenRouteListControlDlg()
		{
			return new JournalAction(
				"Отгрузка со склада",
				selectedItems => selectedItems.Any(x => _controlDlgStatuses.Contains(((RouteListJournalNode)x).StatusEnum)),
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null && _controlDlgStatuses.Contains(selectedNode.StatusEnum))
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListControlDlg(selectedNode.Id)
						);
					}
				}
			);
		}

		private IJournalAction CreateOpenCreateDialogAction()
		{
			return new JournalAction(
				"Открыть диалог создания",
				selectedItems => true,
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null)
					{
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<RouteList>(selectedNode.Id),
							() => new RouteListCreateDlg(selectedNode.Id)
						);
					}
				}
			);
		}

		private IJournalAction CreateOpenTrackAction()
		{
			return new JournalAction(
				"Открыть трек",
				selectedItems => true,
				selectedItems => true,
				selectedItems =>
				{
					var selectedNode = selectedItems.Cast<RouteListJournalNode>().FirstOrDefault();
					if(selectedNode != null)
					{
						var track = new TrackOnMapWnd(selectedNode.Id);
						track.Show();
					}
				}
			);
		}

		private bool CanCreateSelfDriverTerminalTransferDocument => _canCreateSelfDriverTerminalTransferDocument
			?? (_canCreateSelfDriverTerminalTransferDocument = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(SelfDriverTerminalTransferDocument)).CanCreate).Value;

		private bool UserHasOnlyAccessToWarehouseAndComplaints => _userHasOnlyAccessToWarehouseAndComplaints
			?? (_userHasOnlyAccessToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission("user_have_access_only_to_warehouse_and_complaints")
				&& !commonServices.UserService.GetCurrentUser(UoW).IsAdmin).Value;

		private bool CanDeleteRouteList(RouteListJournalNode selectedNode)
		{
			if(selectedNode.StatusEnum != RouteListStatus.New)
			{
				return false;
			}
			return !_routeListRepository.RouteListContainsGivedFuelLiters(UoW, selectedNode.Id);
		}

		private void SendRouteListsInLoading(IList<RouteListJournalNode> selectedNodes)
		{
			var routeListIds = selectedNodes.Select(x => x.Id).ToArray();

			using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var routeLists = uowLocal.Session.QueryOver<RouteList>()
					.Where(x => x.Id.IsIn(routeListIds))
					.List();

				bool needShowMessage = false;
				var messageStockList = new List<LackStockNode>();

				foreach(var routeList in routeLists)
				{
					int warehouseId = 0;
					if(routeList.ClosingSubdivision.Id == _routeListParametersProvider.CashSubdivisionSofiiskayaId)
					{
						warehouseId = _routeListParametersProvider.WarehouseSofiiskayaId;
					}
					if(routeList.ClosingSubdivision.Id == _routeListParametersProvider.CashSubdivisionParnasId)
					{
						warehouseId = _routeListParametersProvider.WarehouseParnasId;
					}

					if(warehouseId > 0)
					{
						var onlineOrders = routeList.Addresses
							.SelectMany(adressItem => adressItem.Order.OrderItems)
							.Where(orderItem => orderItem.Nomenclature.OnlineStore != null)
							.ToList();

						var warehouseStocks = _warehouseRepository
							.GetWarehouseNomenclatureStock(UoW, warehouseId, onlineOrders.Select(o => o.Nomenclature.Id).Distinct())
							.ToList();

						var lackWarehouseStocks = onlineOrders
							.Join(warehouseStocks,
								o => o.Nomenclature.Id,
								w => w.NomenclatureId,
								(o, w) => new LackStockNode
								{
									OrderId = o.Order.Id,
									NomenclatureName = o.Nomenclature.Name,
									Count = o.Count,
									Stock = w.Stock,
									Measure = o.Nomenclature.Unit.Name
								})
							.Where(w => w.Stock < w.Count);

						messageStockList.AddRange(lackWarehouseStocks);

						var notExistInWarehouseNomenclatures = onlineOrders
							.Where(o => warehouseStocks.All(w => w.NomenclatureId != o.Nomenclature.Id))
							.Select(o => new LackStockNode
							{
								OrderId = o.Order.Id,
								NomenclatureName = o.Nomenclature.Name,
								Count = o.Count,
								Measure = o.Nomenclature.Unit.Name
							});

						messageStockList.AddRange(notExistInWarehouseNomenclatures);
					}
				}

				var stockMessage = new StringBuilder();

				if(messageStockList.Count > 0)
				{
					needShowMessage = true;
					stockMessage.Append("В наличии нет следующих товаров:");

					messageStockList.ForEach(messageItem =>
					{
						stockMessage.Append(Environment.NewLine);
						stockMessage.Append(
							$"Заказ {messageItem.OrderId}: {messageItem.NomenclatureName} - {messageItem.Count} {messageItem.Measure}");
					});

					stockMessage.Append($"{Environment.NewLine}Всё равно отправить МЛ на погрузку?");
				}

				if(needShowMessage && !commonServices.InteractiveService.Question(stockMessage.ToString()))
				{
					return;
				}

				bool isSlaveTabActive = false;
				foreach(var routeList in routeLists.Where(arg => arg.Status == RouteListStatus.Confirmed))
				{
					if(TabParent.FindTab(DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
					{
						MessageDialogHelper.RunInfoDialog("Требуется закрыть подчиненную вкладку");
						isSlaveTabActive = true;
						continue;
					}

					foreach(var address in routeList.Addresses)
					{
						if(address.Order.OrderStatus < OrderStatus.OnLoading)
						{
							address.Order.ChangeStatusAndCreateTasks(OrderStatus.OnLoading, _callTaskWorker);
						}
					}

					routeList.ChangeStatusAndCreateTask(RouteListStatus.InLoading, _callTaskWorker);
					uowLocal.Save(routeList);
				}

				if(isSlaveTabActive)
				{
					return;
				}

				uowLocal.Commit();
			}
		}

		#endregion

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateEditAction();
		}

		private void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				selected =>
				{
					var selectedNodes = selected.OfType<RouteListJournalNode>().ToList();
					if(selectedNodes.Count != 1)
					{
						return false;
					}
					RouteListJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanRead;
				},
				selected => true,
				selected =>
				{
					var selectedNodes = selected.OfType<RouteListJournalNode>().ToList();
					if(selectedNodes.Count != 1)
					{
						return;
					}
					var selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.First(x => x.IsIdentified(selectedNode));

					TabParent.OpenTab(() => foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode), this);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		#region Statuses

		private static readonly RouteListStatus[] _keepingDlgStatuses =
		{
			RouteListStatus.EnRoute,
		};

		private static readonly RouteListStatus[] _canReturnToEnRoute =
		{
			RouteListStatus.Delivered
		};

		private static readonly RouteListStatus[] _controlDlgStatuses =
		{
			RouteListStatus.InLoading
		};

		private static readonly RouteListStatus[] _closingDlgStatuses =
		{
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private static readonly RouteListStatus[] _analysisViewModelStatuses =
		{
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private static readonly RouteListStatus[] _mileageCheckDlgStatuses =
		{
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck,
			RouteListStatus.Closed
		};

		private static readonly RouteListStatus[] _fuelIssuingStatuses =
		{
			RouteListStatus.New,
			RouteListStatus.Confirmed,
			RouteListStatus.InLoading,
			RouteListStatus.EnRoute,
			RouteListStatus.Delivered,
			RouteListStatus.OnClosing,
			RouteListStatus.MileageCheck
		};

		#endregion

		private class LackStockNode
		{
			public int OrderId;
			public string NomenclatureName;
			public decimal Count;
			public decimal Stock;
			public string Measure;
		}
	}
}

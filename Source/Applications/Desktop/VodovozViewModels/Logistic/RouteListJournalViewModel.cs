using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Deletion;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Report;
using QS.Services;
using QS.Tdi;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.DriverTerminal;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Profitability;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Models;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Store;
using Vodovoz.ViewModels.FuelDocuments;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Print.Store;
using Order = Vodovoz.Domain.Orders.Order;
using IWarehousePermissionService = Vodovoz.Infrastructure.Services.IWarehousePermissionService;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListJournalViewModel : FilterableSingleEntityJournalViewModelBase
		<RouteList, ITdiTab, RouteListJournalNode, RouteListJournalFilterViewModel>
	{
		private readonly IRouteListService _routeListService;
		private readonly IEventsQrPlacer _eventsQrPlacer;
		private readonly ICustomPrintRdlDocumentsPrinter _carLoadDocumentsPrinter;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly IRouteListRepository _routeListRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IWarehouseRepository _warehouseRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IStockRepository _stockRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListDailyNumberProvider _routeListDailyNumberProvider;
		private readonly IUserSettingsService _userSettings;
		private readonly IStoreDocumentHelper _storeDocumentHelper;
		private readonly decimal _routeListProfitabilityIndicator;
		private readonly IWarehousePermissionValidator _warehousePermissionValidator;
		private readonly Employee _currentEmployee;
		private bool? _userHasOnlyAccessToWarehouseAndComplaints;
		private bool? _canCreateSelfDriverTerminalTransferDocument;
		private bool _canReturnFromMileageCheckToOnClosing = false;

		public RouteListJournalViewModel(
			RouteListJournalFilterViewModel filterViewModel,
			IRouteListRepository routeListRepository,
			ISubdivisionRepository subdivisionRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigationManager,
			ICallTaskWorker callTaskWorker,
			IWarehouseRepository warehouseRepository,
			IEmployeeRepository employeeRepository,
			IGtkTabsOpener gtkTabsOpener,
			IStockRepository stockRepository,
			INomenclatureSettings nomenclatureSettings,
			ICommonServices commonServices,
			IRouteListProfitabilitySettings routeListProfitabilitySettings,
			IWarehousePermissionService warehousePermissionService,
			IRouteListDailyNumberProvider routeListDailyNumberProvider,
			IUserSettingsService userSettings,
			IStoreDocumentHelper storeDocumentHelper,
			IRouteListService routeListService,
			IEventsQrPlacer eventsQrPlacer,
			ICustomPrintRdlDocumentsPrinter carLoadDocumentsPrinter,
			IReportInfoFactory reportInfoFactory,
			Action<RouteListJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_warehouseRepository = warehouseRepository ?? throw new ArgumentNullException(nameof(warehouseRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_stockRepository = stockRepository ?? throw new ArgumentNullException(nameof(stockRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_routeListProfitabilityIndicator = FilterViewModel.RouteListProfitabilityIndicator =
				(routeListProfitabilitySettings ?? throw new ArgumentNullException(nameof(routeListProfitabilitySettings)))
				.GetRouteListProfitabilityIndicatorInPercents;
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_eventsQrPlacer = eventsQrPlacer ?? throw new ArgumentNullException(nameof(eventsQrPlacer));
			_carLoadDocumentsPrinter = carLoadDocumentsPrinter ?? throw new ArgumentNullException(nameof(carLoadDocumentsPrinter));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			_routeListDailyNumberProvider = routeListDailyNumberProvider ?? throw new ArgumentNullException(nameof(routeListDailyNumberProvider));
			_userSettings = userSettings;
			_storeDocumentHelper = storeDocumentHelper;
			_currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			_warehousePermissionValidator =
				(warehousePermissionService ?? throw new ArgumentNullException(nameof(warehousePermissionService))).GetValidator();
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_canReturnFromMileageCheckToOnClosing = commonServices.CurrentPermissionService.ValidatePresetPermission(CashPermissions.PresetPermissionsRoles.Cashier);

			TabName = "Журнал МЛ";

			UpdateOnChanges(typeof(RouteList), typeof(RouteListProfitability), typeof(RouteListDebt));
			InitPopupActions();

			if(filterConfig != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		protected override Func<IUnitOfWork, IQueryOver<RouteList>> ItemsSourceQueryFunction => (uow) =>
		{
			RouteListJournalNode routeListJournalNodeAlias = null;
			RouteList routeListAlias = null;
			RouteList routeList2Alias = null;
			DeliveryShift shiftAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			CarModel carModelAlias = null;
			Employee driverAlias = null;
			Subdivision subdivisionAlias = null;
			GeoGroup geoGroupAlias = null;
			GeoGroupVersion geoGroupVersionAlias = null;
			RouteListProfitability routeListProfitabilityAlias = null;
			RouteListDebt routeListDebtAlias = null;
			RouteListItem routeListAddressAlias = null;

			var query = uow.Session.QueryOver(() => routeListAlias)
				.Left.JoinAlias(rl => rl.Shift, () => shiftAlias)
				.Left.JoinAlias(rl => rl.Car, () => carAlias)
				.Left.JoinAlias(rl => rl.GeographicGroups, () => geoGroupAlias)
				.Left.JoinAlias(rl => rl.Driver, () => driverAlias)
				.Left.JoinAlias(rl => rl.RouteListProfitability, () => routeListProfitabilityAlias)
				.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= routeListAlias.Date
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date));

			if(FilterViewModel.SelectedStatuses != null)
			{
				query.WhereRestrictionOn(o => o.Status).IsIn(FilterViewModel.SelectedStatuses);
			}

			if(FilterViewModel.DeliveryShift != null)
			{
				query.Where(o => o.Shift == FilterViewModel.DeliveryShift);
			}

			var startDate = FilterViewModel.StartDate;
			if(startDate != null)
			{
				query.Where(o => o.Date >= startDate);
			}

			var endDate = FilterViewModel.EndDate;
			if(endDate != null)
			{
				query.Where(o => o.Date <= endDate.Value.LatestDayTime());
			}

			if(FilterViewModel.GeographicGroup != null)
			{
				query.Where(() => geoGroupAlias.Id == FilterViewModel.GeographicGroup.Id);
			}

			#region RouteListAddressTypeFilter

			{
				var delivery = FilterViewModel.WithDeliveryAddresses;
				var chainStore = FilterViewModel.WithChainStoreAddresses;
				var service = FilterViewModel.WithServiceAddresses;

				if(delivery && chainStore && !service)
				{
					query.Where(() => !driverAlias.VisitingMaster);
				}
				else if(delivery && !chainStore && service)
				{
					query.Where(() => !driverAlias.IsChainStoreDriver);
				}
				else if(delivery && !chainStore && !service)
				{
					query.Where(() => !driverAlias.VisitingMaster);
					query.Where(() => !driverAlias.IsChainStoreDriver);
				}
				else if(!delivery && chainStore && service)
				{
					query.Where(Restrictions.Or(
						Restrictions.Where(() => driverAlias.VisitingMaster),
						Restrictions.Where(() => driverAlias.IsChainStoreDriver)
					));
				}
				else if(!delivery && chainStore && !service)
				{
					query.Where(() => driverAlias.IsChainStoreDriver);
				}
				else if(!delivery && !chainStore && service)
				{
					query.Where(() => driverAlias.VisitingMaster);
				}
				else if(!delivery && !chainStore && !service)
				{
					query.Where(() => routeListAlias.Id == null);
				}
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

			if(FilterViewModel.RestrictedCarOwnTypes != null)
			{
				query.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsIn(FilterViewModel.RestrictedCarOwnTypes.ToArray());
			}

			if(FilterViewModel.RestrictedCarTypesOfUse != null)
			{
				query.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(FilterViewModel.RestrictedCarTypesOfUse.ToArray());
			}

			if(FilterViewModel.ExcludeIds != null &&  FilterViewModel.ExcludeIds.Any())
			{
				query.Where(() => !routeListAlias.Id.IsIn(FilterViewModel.ExcludeIds));
			}

			var driverProjection = CustomProjections.Concat_WS(
				" ",
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic)
			);

			query.Where(GetSearchCriterion(
				() => routeListAlias.Id,
				() => driverProjection,
				() => carModelAlias.Name,
				() => carAlias.RegistrationNumber
			));

			var firstRouteListGeoGroup = QueryOver.Of(() => routeList2Alias)
				.JoinAlias(() => routeList2Alias.GeographicGroups, () => geoGroupAlias)
				.Where(() => routeList2Alias.Id == routeListAlias.Id)
				.Select(Projections.Property(() => geoGroupAlias.Id))
				.Take(1);

			var closingSubdivision = QueryOver.Of(() => subdivisionAlias)
				.JoinEntityAlias(() => geoGroupVersionAlias, () => geoGroupVersionAlias.CashSubdivision.Id == subdivisionAlias.Id)
				.Where(Restrictions.EqProperty(
					Projections.Property(() => geoGroupVersionAlias.GeoGroup.Id),
					Projections.SubQuery(firstRouteListGeoGroup)))
				.And(() => geoGroupVersionAlias.ActivationDate <= routeListAlias.Date)
				.And(() => geoGroupVersionAlias.ClosingDate == null || geoGroupVersionAlias.ClosingDate >= routeListAlias.Date)
				.Select(s => s.Name);

			var routeListDebtSubquery = QueryOver.Of(() => routeListDebtAlias)
				.Where(() => routeListAlias.Id == routeListDebtAlias.RouteList.Id)
				.Select(r => r.Debt)
				.Take(1);
			
			var anyAddressSubquery = QueryOver.Of(() => routeListAddressAlias)
				.Where(() => routeListAddressAlias.RouteList.Id == routeListAlias.Id)
				.Select(a => a.Id)
				.Take(1);
			
			var hasAddressesProjection = Projections.Conditional(
				Restrictions.IsNull(Projections.SubQuery(anyAddressSubquery)), 
				Projections.Constant(false),
				Projections.Constant(true));

			var result = query
				.SelectList(list => list
					.SelectGroup(() => routeListAlias.Id).WithAlias(() => routeListJournalNodeAlias.Id)
					.Select(() => routeListAlias.Date).WithAlias(() => routeListJournalNodeAlias.Date)
					.Select(() => routeListAlias.Status).WithAlias(() => routeListJournalNodeAlias.StatusEnum)
					.Select(() => shiftAlias.Name).WithAlias(() => routeListJournalNodeAlias.ShiftName)
					.Select(() => carModelAlias.Name).WithAlias(() => routeListJournalNodeAlias.CarModelName)
					.Select(() => carAlias.RegistrationNumber).WithAlias(() => routeListJournalNodeAlias.CarNumber)
					.Select(() => driverAlias.LastName).WithAlias(() => routeListJournalNodeAlias.DriverSurname)
					.Select(() => driverAlias.Name).WithAlias(() => routeListJournalNodeAlias.DriverName)
					.Select(() => driverAlias.Patronymic).WithAlias(() => routeListJournalNodeAlias.DriverPatronymic)
					.Select(() => driverAlias.Comment).WithAlias(() => routeListJournalNodeAlias.DriverComment)
					.SelectSubQuery(routeListDebtSubquery).WithAlias(() => routeListJournalNodeAlias.RouteListDebt)
					.Select(() => routeListAlias.LogisticiansComment).WithAlias(() => routeListJournalNodeAlias.LogisticiansComment)
					.Select(() => routeListAlias.ClosingComment).WithAlias(() => routeListJournalNodeAlias.ClosinComments)
					.SelectSubQuery(closingSubdivision).WithAlias(() => routeListJournalNodeAlias.ClosingSubdivision)
					.Select(() => routeListAlias.NotFullyLoaded).WithAlias(() => routeListJournalNodeAlias.NotFullyLoaded)
					.Select(() => carModelAlias.CarTypeOfUse).WithAlias(() => routeListJournalNodeAlias.CarTypeOfUse)
					.Select(() => carVersionAlias.CarOwnType).WithAlias(() => routeListJournalNodeAlias.CarOwnType)
					.Select(() => routeListProfitabilityAlias.GrossMarginPercents)
						.WithAlias(() => routeListJournalNodeAlias.GrossMarginPercents)
					.Select(Projections.Constant(_routeListProfitabilityIndicator))
						.WithAlias(() => routeListJournalNodeAlias.RouteListProfitabilityIndicator)
					.Select(hasAddressesProjection).WithAlias(() => routeListJournalNodeAlias.HasAddresses)
				).OrderBy(rl => rl.Date).Desc
				.TransformUsing(Transformers.AliasToBean<RouteListJournalNode>());

			return result;
		};

		protected override Func<ITdiTab> CreateDialogFunction => () => NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel;

		protected override Func<RouteListJournalNode, ITdiTab> OpenDialogFunction => node => NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(node.Id)).ViewModel;

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
			PopupActionsList.Add(CreateSendRouteListToLoadingAndPrintAction());
			PopupActionsList.Add(CreateSendRouteListToLoadingAction());
			PopupActionsList.Add(CreateOpenKeepingDialogAction());
			PopupActionsList.Add(CreateReturnToEnRouteAction());
			PopupActionsList.Add(CreateReturnToClosingAction());
			PopupActionsList.Add(CreateOpenClosingDialogAction());
			PopupActionsList.Add(CreateOpenAnalysisDialogAction());
			PopupActionsList.Add(CreateOpenMileageCheckDialogAction());
			PopupActionsList.Add(CreateDeleteRouteListAction());
			PopupActionsList.Add(CreateGiveFuelAction());
			PopupActionsList.Add(CreateTransferTerminalAction());
		}

		private IJournalAction CreateOpenTrackAction()
		{
			return new JournalAction(
				"Открыть трек",
				selectedItems => true,
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						_gtkTabsOpener.ShowTrackWindow(selectedNode.Id);
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
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);
		}

		private IJournalAction CreateOpenRouteListControlDlg()
		{
			return new JournalAction(
				"Отгрузка со склада",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _controlDlgStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						_gtkTabsOpener.OpenRouteListControlDlg(TabParent, selectedNode.Id);
					}
				}
			);
		}

		private IJournalAction CreateSendRouteListToLoadingAndPrintAction()
		{
			var cashSubdivisionIds = _subdivisionRepository.GetCashSubdivisions(UoW).Select(x => x.Id);
			var cashWarehouseIds = UoW.Session.QueryOver<Warehouse>()
				.WhereRestrictionOn(x => x.OwningSubdivisionId).IsInG(cashSubdivisionIds)
				.Select(x => x.Id)
				.List<int>();

			var defaultWarehouse = _userSettings.Settings.DefaultWarehouse;

			if(defaultWarehouse != null
			   && !cashWarehouseIds.Contains(defaultWarehouse.Id)
			   && _warehousePermissionValidator.Validate(WarehousePermissionsType.CarLoadEdit, defaultWarehouse, _currentEmployee))
			{
				return new JournalAction(
					$"Отправить МЛ на погрузку со скалада\n'{defaultWarehouse.Name}' и распечатать",
					selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
						&& node.StatusEnum == RouteListStatus.Confirmed,
					selectedItems => true,
					selectedItems =>
					{
						if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
						{
							SendToLoadingAndPrint(selectedNode, defaultWarehouse);
						}
					}
				);
			}

			var warehousesAvailableForUser =
				_storeDocumentHelper.GetRestrictedWarehousesList(UoW, WarehousePermissionsType.CarLoadEdit)
					.Where(x => !cashWarehouseIds.Contains(x.Id))
					.ToList();

			if(!warehousesAvailableForUser.Any())
			{
				return new JournalAction(
					"Отправить МЛ на погрузку и распечатать",
					selectedItems => false,
					selectedItems => true,
					selectedItems => { }
				);
			}

			var journalAction = new JournalAction(
				"Отправить МЛ на погрузку и распечатать",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& node.StatusEnum == RouteListStatus.Confirmed,
				selectedItems => true,
				selectedItems => { }
			);
			foreach(var warehouse in warehousesAvailableForUser)
			{
				journalAction.ChildActionsList.Add(new JournalAction(
					warehouse.Name,
					selectedItems => true,
					selectedItems => true,
					selectedItems =>
					{
						if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
						{
							SendToLoadingAndPrint(selectedNode, warehouse);
						}
					}
				));
			}
			return journalAction;
		}

		private IJournalAction CreateSendRouteListToLoadingAction()
		{
			return new JournalAction(
				"Отправить МЛ на погрузку",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& node.StatusEnum == RouteListStatus.Confirmed,
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

		private IJournalAction CreateOpenKeepingDialogAction()
		{
			return new JournalAction(
				"Открыть диалог ведения",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _keepingDlgStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						NavigationManager.OpenViewModel<RouteListKeepingViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);
		}

		private IJournalAction CreateReturnToEnRouteAction()
		{
			return new JournalAction(
				"Вернуть в путь",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _canReturnToEnRoute.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					var routeListIds = selectedItems
						.Cast<RouteListJournalNode>()
						.Where(x => x.StatusEnum == RouteListStatus.Delivered)
						.Select(x => x.Id)
						.ToArray();

					bool isSlaveTabActive = false;

					using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
					{
						foreach(var routeListId in routeListIds)
						{
							if(TabParent.FindTab(_gtkTabsOpener.GenerateDialogHashName<RouteList>(routeListId)) != null)
							{
								commonServices.InteractiveService.ShowMessage(
									ImportanceLevel.Info, "Требуется закрыть подчиненную вкладку");
								isSlaveTabActive = true;
								continue;
							}

							_routeListService.SendEnRoute(uowLocal, routeListId, _callTaskWorker);
						}

						Refresh();

						if(isSlaveTabActive)
						{
							return;
						}

						uowLocal.Commit();
					}
				}
			);
		}

		private IJournalAction CreateReturnToClosingAction()
		{
			return new JournalAction(
				"Вернуть в сдается",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _canReturnToOnClosing.Contains(node.StatusEnum)
					&& _canReturnFromMileageCheckToOnClosing,
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _canReturnToOnClosing.Contains(node.StatusEnum)
					&& _canReturnFromMileageCheckToOnClosing,
				selectedItems =>
				{
					var routeListIds = selectedItems.Cast<RouteListJournalNode>().Select(x => x.Id).ToArray();
					bool isSlaveTabActive = false;

					using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot())
					{
						var routeLists = uowLocal.Session.QueryOver<RouteList>()
							.Where(x => x.Id.IsIn(routeListIds))
							.List();

						foreach(var routeList in routeLists.Where(arg => arg.Status == RouteListStatus.MileageCheck))
						{
							if(TabParent.FindTab(_gtkTabsOpener.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
							{
								commonServices.InteractiveService.ShowMessage(
									ImportanceLevel.Info, "Требуется закрыть подчиненную вкладку");
								isSlaveTabActive = true;
								continue;
							}
							_routeListService.ChangeStatusAndCreateTask(uowLocal, routeList, RouteListStatus.OnClosing, _callTaskWorker);
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

		private IJournalAction CreateOpenClosingDialogAction()
		{
			return new JournalAction(
				"Открыть диалог закрытия",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _closingDlgStatuses.Contains(node.StatusEnum),
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						_gtkTabsOpener.OpenRouteListClosingDlg(TabParent, selectedNode.Id);
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
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						NavigationManager.OpenViewModel<RouteListAnalysisViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(selectedNode.Id));
					}
				}
			);
		}

		private IJournalAction CreateOpenMileageCheckDialogAction()
		{
			return new JournalAction(
				"Открыть диалог проверки километража",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _mileageCheckDlgStatuses.Contains(node.StatusEnum)
					&& node.CarOwnType == CarOwnType.Company
					&& node.CarTypeOfUse != CarTypeOfUse.Truck,
				selectedItems => true,
				selectedItems =>
				{
					if(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode)
					{
						NavigationManager.OpenViewModel<RouteListMileageCheckViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForOpen(selectedNode.Id), OpenPageOptions.AsSlave);
					}
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
					if(!(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode))
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

		private IJournalAction CreateGiveFuelAction()
		{
			return new JournalAction(
				"Выдать топливо",
				selectedItems => selectedItems.FirstOrDefault() is RouteListJournalNode node
					&& _fuelIssuingStatuses.Contains(node.StatusEnum)
					&& node.HasAddresses
					&& node.Date >= DateTime.Today,
				selectedItems => true,
				selectedItems =>
				{
					if(!(selectedItems.FirstOrDefault() is RouteListJournalNode selectedNode))
					{
						return;
					}

					var page = NavigationManager.OpenViewModel<FuelDocumentViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());

					page.ViewModel.SetRouteListById(selectedNode.Id);
				}
			);
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

		private bool CanCreateSelfDriverTerminalTransferDocument => _canCreateSelfDriverTerminalTransferDocument
			?? (_canCreateSelfDriverTerminalTransferDocument = commonServices.CurrentPermissionService
				.ValidateEntityPermission(typeof(SelfDriverTerminalTransferDocument)).CanCreate).Value;

		private bool UserHasOnlyAccessToWarehouseAndComplaints => _userHasOnlyAccessToWarehouseAndComplaints
			?? (_userHasOnlyAccessToWarehouseAndComplaints =
				commonServices.CurrentPermissionService.ValidatePresetPermission(UserPermissions.UserHaveAccessOnlyToWarehouseAndComplaints)
				&& !commonServices.UserService.GetCurrentUser().IsAdmin).Value;

		private void SendToLoadingAndPrint(RouteListJournalNode selectedNode, Warehouse warehouse)
		{
			using(var localUow = UnitOfWorkFactory.CreateWithoutRoot())
			{
				var routeList = localUow.GetById<RouteList>(selectedNode.Id);

				if(!routeList.IsDriversDebtInPermittedRangeVerification())
				{
					return;
				}

				_routeListService.ChangeStatusAndCreateTask(localUow, routeList, RouteListStatus.InLoading, _callTaskWorker);

				var carLoadDocument = new CarLoadDocument();
				FillCarLoadDocument(carLoadDocument, localUow, routeList.Id, warehouse.Id);

				var routeListFullyShipped = _routeListService.TrySendEnRoute(localUow, routeList, _callTaskWorker, out var notLoadedGoods, carLoadDocument);
				
				localUow.Save(routeList);

				_routeListDailyNumberProvider.GetOrCreateDailyNumber(routeList.Id, routeList.Date);

				//Не погружен остался только терминал
				var routeListShippedWithoutTerminal = notLoadedGoods.Count == 1
					&& notLoadedGoods.All(x => x.NomenclatureId == _nomenclatureSettings.NomenclatureIdForTerminal);

				var valid = commonServices.ValidationService.Validate(carLoadDocument, showValidationResults: false);

				if((routeListFullyShipped || routeListShippedWithoutTerminal) && valid)
				{
					carLoadDocument.ClearItemsFromZero();
					carLoadDocument.UpdateOperations(localUow, _nomenclatureSettings.NomenclatureIdForTerminal);

					if(!carLoadDocument.Items.Any())
					{
						localUow.Commit();
						return;
					}

					localUow.Save(carLoadDocument);
					localUow.Commit();

					if(routeListShippedWithoutTerminal)
					{
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
							"Водителю необходимо получить терминал на кассе");
					}

					PrintCarLoadDocuments(carLoadDocument);
				}
				else
				{
					if(!routeListShippedWithoutTerminal && notLoadedGoods.Any())
					{						
						var archivedNomenclatures = _routeListRepository.GetRouteListNomenclatures(localUow, routeList.Id, true);

						if(archivedNomenclatures.Any())
						{
							commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
								$"Не удалось автоматически отгрузить Маршрутный лист, т.к. присутствуют архивнвые номенклатуры: " +
								$"{string.Join(", ", archivedNomenclatures.Select(n => $"№{n.Id} {n.Name}"))}.");

							return;
						}
					}

					localUow.Commit();

					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Не удалось автоматически отгрузить Маршрутный лист");

					_gtkTabsOpener.OpenCarLoadDocumentDlg(TabParent, FillCarLoadDocument, routeList.Id, warehouse.Id);
				}
			}
		}

		private void PrintCarLoadDocuments(CarLoadDocument carLoadDocument)
		{
			var waterCarLoadDocument = WaterCarLoadDocumentRdl.Create(_userSettings.Settings, carLoadDocument, CarLoadDocumentPlaceEventsQr, _reportInfoFactory);
			var controlCarLoadDocument = ControlCarLoadDocumentRdl.Create(_userSettings.Settings, carLoadDocument, _reportInfoFactory);
			var equipmentCarLoadDocument = EquipmentCarLoadDocumentRdl.Create(_userSettings.Settings, carLoadDocument, _reportInfoFactory);

			_carLoadDocumentsPrinter.Print(waterCarLoadDocument);
			_carLoadDocumentsPrinter.Print(controlCarLoadDocument);
			_carLoadDocumentsPrinter.Print(equipmentCarLoadDocument);
		}

		private string CarLoadDocumentPlaceEventsQr(int documentId, string reportSource)
		{
			return _eventsQrPlacer.AddQrEventForWaterCarLoadDocument(
					UoW, documentId, reportSource);
		}

		private void FillCarLoadDocument(CarLoadDocument document, IUnitOfWork uow, int routeListId, int warehouseId)
		{
			document.RouteList = uow.GetById<RouteList>(routeListId);
			document.AuthorId = _currentEmployee?.Id;
			document.LastEditorId = _currentEmployee?.Id;
			document.LastEditedTime = DateTime.Now;
			document.Warehouse = uow.GetById<Warehouse>(warehouseId);

			document.FillFromRouteList(uow, _routeListRepository, _subdivisionRepository, true);
			document.UpdateAlreadyLoaded(uow, _routeListRepository);
			document.UpdateStockAmount(uow, _stockRepository);
			document.UpdateAmounts();
		}

		private bool CanDeleteRouteList(RouteListJournalNode selectedNode)
		{
			if(selectedNode.StatusEnum != RouteListStatus.New)
			{
				return false;
			}
			return !_routeListRepository.RouteListContainsGivenFuelLiters(UoW, selectedNode.Id);
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
					if(!routeList.IsDriversDebtInPermittedRangeVerification())
					{
						return;
					}

					int warehouseId = 0;

					var geoGroup = routeList.GeographicGroups.FirstOrDefault();
					var geoGroupVersion = geoGroup.GetVersionOrNull(routeList.Date);
					if(geoGroupVersion != null)
					{
						warehouseId = geoGroupVersion.Warehouse.Id;
					}

					if(warehouseId > 0)
					{
						var onlineOrders = routeList.Addresses
							.SelectMany(adressItem => adressItem.Order.OrderItems)
							.Where(orderItem => orderItem.Nomenclature.OnlineStore != null)
							.ToList();

						var warehouseStocks = _warehouseRepository
							.GetWarehouseNomenclatureStock(
								UoW,
								OperationType.WarehouseBulkGoodsAccountingOperation,
								warehouseId,
								onlineOrders.Select(o => o.Nomenclature.Id).Distinct())
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
					if(TabParent.FindTab(_gtkTabsOpener.GenerateDialogHashName<RouteList>(routeList.Id)) != null)
					{
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Требуется закрыть подчиненную вкладку");
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

					_routeListService.ChangeStatusAndCreateTask(uowLocal, routeList, RouteListStatus.InLoading, _callTaskWorker);
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
			CreateDefaultSelectAction();
			CreateAddActions();
			CreateEditAction();
		}

		protected void CreateAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var entityConfig = EntityConfigs.First().Value;
			var addAction = new JournalAction("Добавить",
				(selected) => entityConfig.PermissionResult.CanCreate,
				(selected) => entityConfig.PermissionResult.CanCreate,
				(selected) => {
					var page = NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate());

					page.ViewModel.EntitySaved += Tab_EntitySaved;
				},
				"Insert"
				);
			NodeActionsList.Add(addAction);
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

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
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

		private static readonly RouteListStatus[] _canReturnToOnClosing =
		{
			RouteListStatus.MileageCheck
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

﻿using Autofac;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Additions.Logistic;
using Vodovoz.Application.Logistics.RouteOptimization;
using Vodovoz.Controllers;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.TempAdapters;
using static Vodovoz.EntityRepositories.Orders.OrderRepository;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListsOnDayViewModel : TabViewModelBase
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IAtWorkRepository _atWorkRepository;
		private readonly ILogger<RouteListsOnDayViewModel> _logger;
		private readonly IGtkTabsOpener _gtkTabsOpener;
		private readonly IUserRepository _userRepository;
		private readonly DeliveryDaySchedule _defaultDeliveryDaySchedule;
		private readonly int _closingDocumentDeliveryScheduleId;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;
		private readonly IRouteListProfitabilityController _routeListProfitabilityController;

		private bool _excludeTrucks;

		public IUnitOfWork UoW;

		public RouteListsOnDayViewModel(
			ILogger<RouteListsOnDayViewModel> logger,
			ICommonServices commonServices,
			ILifetimeScope lifetimeScope,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			IGtkTabsOpener gtkTabsOpener,
			IRouteListRepository routeListRepository,
			ISubdivisionRepository subdivisionRepository,
			IOrderRepository orderRepository,
			IAtWorkRepository atWorkRepository,
			ICarRepository carRepository,
			INavigationManager navigationManager,
			IUserRepository userRepository,
			IDefaultDeliveryDayScheduleSettings defaultDeliveryDayScheduleSettings,
			IEmployeeJournalFactory employeeJournalFactory,
			IGeographicGroupRepository geographicGroupRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			ICarModelJournalFactory carModelJournalFactory,
			IRouteOptimizer routeOptimizer,
			IRouteListProfitabilityController routeListProfitabilityController)
			: base(commonServices?.InteractiveService, navigationManager)
		{
			if(defaultDeliveryDayScheduleSettings == null)
			{
				throw new ArgumentNullException(nameof(defaultDeliveryDayScheduleSettings));
			}
			if(geographicGroupRepository == null)
			{
				throw new ArgumentNullException(nameof(geographicGroupRepository));
			}
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			CarRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			ScheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			CarModelJournalFactory = carModelJournalFactory;
			Optimizer = routeOptimizer ?? throw new ArgumentNullException(nameof(routeOptimizer));
			_routeListProfitabilityController = routeListProfitabilityController ?? throw new ArgumentNullException(nameof(routeListProfitabilityController));
			_gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			_atWorkRepository = atWorkRepository ?? throw new ArgumentNullException(nameof(atWorkRepository));
			OrderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));

			_closingDocumentDeliveryScheduleId = deliveryScheduleParametersProvider?.ClosingDocumentDeliveryScheduleId ??
												throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider));

			CanСreateRoutelistInPastPeriod = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Logistic.RouteList.CanCreateRouteListInPastPeriod);

			CreateUoW();

			Employee currentEmployee = VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);

			if(currentEmployee == null)
			{
				ShowWarningMessage("Ваш пользователь не привязан к сотруднику, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}

			if(currentEmployee.Subdivision == null)
			{
				ShowWarningMessage("У сотрудника не указано подразделение, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}

			ObservableSubdivisions = new GenericObservableList<Subdivision>(subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new[] { typeof(Income) }).ToList());

			if(!ObservableSubdivisions.Any())
			{
				ShowErrorMessage("Не правильно сконфигурированы подразделения кассы, невозможно будет указать подразделение в которое будут сдаваться маршрутные листы");
				FailInitialize = true;
				return;
			}

			GeographicGroupsExceptEast =
				geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);
			var geographicGroups = geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);
			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(geographicGroups.Select(x => new GeographicGroupNode(x)).ToList());

			GeoGroup employeeGeographicGroup = currentEmployee.Subdivision.GetGeographicGroup();

			if(employeeGeographicGroup != null)
			{
				var foundGeoGroup = GeographicGroupNodes.FirstOrDefault(x => x.GeographicGroup.Id == employeeGeographicGroup.Id);

				if(foundGeoGroup != null)
				{
					foundGeoGroup.Selected = true;
				}
			}

			_defaultDeliveryDaySchedule =
				UoW.GetById<DeliveryDaySchedule>(defaultDeliveryDayScheduleSettings.GetDefaultDeliveryDayScheduleId());
			//Необходимо сразу проинициализировать, т.к вызывается Session.Clear() в методе InitializeData()
			NHibernateUtil.Initialize(_defaultDeliveryDaySchedule.Shifts);

			var deliveryShifts = UoW.GetAll<DeliveryShift>();
			DeliveryShiftNodes = deliveryShifts.Select(ds => new DeliveryShiftNode(ds)).ToList();

			CreateCommands();
			LoadAddressesTypesDefaults();
		}

		private void AddAddressTypeFilter(IQueryOver<Order, Order> query)
		{
			foreach(var node in OrderAddressTypes)
			{
				if(node.Selected)
				{
					continue;
				}

				if(node.IsFastDelivery)
				{
					query.Where(x => !x.IsFastDelivery);
				}
				else if(node.OrderAddressType == OrderAddressType.Delivery)
				{
					var isFastDeliveryChecked = OrderAddressTypes.SingleOrDefault(x => x.IsFastDelivery && x.Selected) != null;
					if(isFastDeliveryChecked)
					{
						query.Where(x => x.IsFastDelivery);
					}
					else
					{
						query.Where(x => x.OrderAddressType != node.OrderAddressType);
					}
				}
				else
				{
					query.Where(x => x.OrderAddressType != node.OrderAddressType);
				}
			}
		}

		public bool ExcludeTrucks
		{
			get => _excludeTrucks;
			set => SetField(ref _excludeTrucks, value);
		}
		public ICommonServices CommonServices { get; }
		public ILifetimeScope LifetimeScope { get; }
		public ICarRepository CarRepository { get; }
		public IList<GeoGroup> GeographicGroupsExceptEast { get; }
		public IScheduleRestrictionRepository ScheduleRestrictionRepository { get; }
		public ICarModelJournalFactory CarModelJournalFactory { get; }
		public IOrderRepository OrderRepository { get; }

		private void CreateCommands()
		{
			SaveCommand = CreateSaveCommand();
			RemoveRLItemCommand = CreateRemoveRLItemCommand();
			OpenOrderOrRouteListCommand = CreateOpenOrderOrRouteListCommand();
			AddDriverCommand = CreateAddDriverCommand();
			RemoveDriverCommand = CreateRemoveDriverCommand();
			AddForwarderCommand = CreateAddForwarderCommand();
			RemoveForwarderCommand = CreateRemoveForwarderCommand();
			RebuilOneRouteCommand = CreateRebuilOneRouteCommand();
			ShowWarningsCommand = CreateShowWarningsCommand();
		}

		public event EventHandler AutoroutingResultsSaved;

		#region SaveCommand

		public DelegateCommand SaveCommand { get; private set; }

		private DelegateCommand CreateSaveCommand()
		{
			return new DelegateCommand(
				() =>
				{
					if(SaveAutoroutingResults())
					{
						IsAutoroutingModeActive = false;
					}
				},
				() => IsAutoroutingModeActive
			);
		}

		#endregion SaveCommand

		#region RemoveRLItemCommand

		public DelegateCommand<RouteListItem> RemoveRLItemCommand { get; private set; }

		private DelegateCommand<RouteListItem> CreateRemoveRLItemCommand()
		{
			return new DelegateCommand<RouteListItem>(
				i =>
				{
					var route = i.RouteList;
					route.RemoveAddress(i);

					if(!CheckRouteListWasChanged(route))
					{
						return;
					}

					if(IsAutoroutingModeActive)
					{
						UoW.Save(route);
					}
					else
					{
						SaveRouteList(route);
					}

					route.RecalculatePlanTime(DistanceCalculator);
					route.RecalculatePlanedDistance(DistanceCalculator);

					UoW.Session.Flush();

					_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, route);
				},
				i => i != null
			);
		}

		#endregion RemoveRLItemCommand

		#region OpenOrderOrRouteListCommand

		public DelegateCommand<object> OpenOrderOrRouteListCommand { get; private set; }

		private DelegateCommand<object> CreateOpenOrderOrRouteListCommand()
		{
			return new DelegateCommand<object>(
				obj =>
				{
					//Открываем заказ
					if(obj is RouteListItem rli)
					{
						_gtkTabsOpener.OpenOrderDlg(this, rli.Order.Id);
					}

					//Открываем МЛ
					if(obj is RouteList rl)
					{
						if(HasChanges)
						{
							if(AskQuestion("Сохранить маршрутный лист перед открытием?"))
							{
								UoW.Save(rl);
								SaveRouteList(rl);
							}
							else
							{
								return;
							}
						}
						NavigationManager.OpenViewModel<RouteListCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForOpen(rl.Id));
					}
				},
				i => true
			);
		}

		#endregion OpenOrderOrRouteListCommand

		#region AddDriverCommand

		public DelegateCommand AddDriverCommand { get; private set; }

		private DelegateCommand CreateAddDriverCommand()
		{
			return new DelegateCommand(
				() =>
				{
					var drvJournalViewModel = _employeeJournalFactory.CreateWorkingDriverEmployeeJournal();
					drvJournalViewModel.SelectionMode = JournalSelectionMode.Multiple;
					drvJournalViewModel.TabName = "Водители";

					drvJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var selectedNodes = e.SelectedNodes;
						var onlyNew = selectedNodes.Where(x => ObservableDriversOnDay.All(y => y.Employee.Id != x.Id)).ToList();
						var allCars = CarRepository.GetCarsByDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());

						foreach(var n in selectedNodes)
						{
							var drv = UoW.GetById<Employee>(n.Id);

							if(ObservableDriversOnDay.Any(x => x.Employee.Id == n.Id))
							{
								_logger.LogWarning("Водитель {DriverShortName} уже добавлен. Пропускаем...", drv.ShortName);
								continue;
							}

							var daySchedule = GetDriverWorkDaySchedule(drv);

							var driver = new AtWorkDriver(
									drv,
									DateForRouting,
									allCars.FirstOrDefault(x => x.Driver.Id == n.Id),
									daySchedule
								);

							if(driver.Employee.DefaultForwarder != null)
							{
								var forwarder = observableForwardersOnDay.FirstOrDefault(x => x.Employee.Id == driver.Employee.DefaultForwarder.Id);

								if(forwarder == null)
								{
									forwarder = new AtWorkForwarder(driver.Employee.DefaultForwarder, DateForRouting);
									driver.WithForwarder = forwarder;
									ObservableForwardersOnDay.Add(forwarder);
								}
							}

							ObservableDriversOnDay.Add(driver);
						}
					};

					TabParent.AddSlaveTab(this, drvJournalViewModel);
				},
				() => true
			);
		}

		#endregion AddDriverCommand

		#region RemoveDriverCommand

		public DelegateCommand<AtWorkDriver[]> RemoveDriverCommand { get; private set; }

		private DelegateCommand<AtWorkDriver[]> CreateRemoveDriverCommand()
		{
			return new DelegateCommand<AtWorkDriver[]>(
				driversToDel =>
				{
					if(driversToDel == null)
					{
						driversToDel = SelectedDrivers;
					}

					foreach(var driver in driversToDel)
					{
						if(driver.Id > 0)
						{
							UoW.Delete(driver);
						}

						ObservableDriversOnDay.Remove(driver);
					}
				},
				driversToDel =>
				{
					if(driversToDel == null)
					{
						driversToDel = SelectedDrivers;
					}

					return driversToDel != null && driversToDel.Any();
				}
			);
		}

		#endregion RemoveDriverCommand

		#region AddForwarderCommand

		public DelegateCommand AddForwarderCommand { get; private set; }

		private DelegateCommand CreateAddForwarderCommand()
		{
			return new DelegateCommand(
				() =>
				{
					var fwdJournalViewModel = _employeeJournalFactory.CreateWorkingForwarderEmployeeJournal();
					fwdJournalViewModel.SelectionMode = JournalSelectionMode.Multiple;

					fwdJournalViewModel.OnEntitySelectedResult += (sender, e) =>
					{
						var selectedNodes = e.SelectedNodes;
						foreach(var n in selectedNodes)
						{
							var fwd = UoW.GetById<Employee>(n.Id);

							if(ObservableForwardersOnDay.Any(x => x.Employee.Id == n.Id))
							{
								_logger.LogWarning("Экспедитор {ForwarderShortName} пропущен так как уже присутствует в списке.", fwd.ShortName);
								continue;
							}

							ObservableForwardersOnDay.Add(new AtWorkForwarder(fwd, DateForRouting));
						}
					};

					TabParent.AddSlaveTab(this, fwdJournalViewModel);
				},
				() => true
			);
		}

		#endregion AddForwarderCommand

		#region RemoveForwarderCommand

		public DelegateCommand<AtWorkForwarder[]> RemoveForwarderCommand { get; private set; }

		private DelegateCommand<AtWorkForwarder[]> CreateRemoveForwarderCommand()
		{
			return new DelegateCommand<AtWorkForwarder[]>(
				forwardersToDel =>
				{
					foreach(var forwarder in forwardersToDel)
					{
						if(forwarder.Id > 0)
						{
							UoW.Delete(forwarder);
						}

						ObservableForwardersOnDay.Remove(forwarder);
					}
				},
				forwardersToDel => forwardersToDel != null && forwardersToDel.Any()
			);
		}

		#endregion RemoveForwarderCommand

		#region RebuilOneRouteCommand

		public DelegateCommand<object> RebuilOneRouteCommand { get; private set; }

		private DelegateCommand<object> CreateRebuilOneRouteCommand()
		{
			 return new DelegateCommand<object>(
				obj =>
				{
					RouteList route = obj is RouteListItem routeListItem ? routeListItem.RouteList : obj as RouteList;

					var newRoute = Optimizer.RebuidOneRoute(route);
					if(newRoute != null)
					{
						newRoute.UpdateAddressOrderInRealRoute(route);
						route.RecalculatePlanedDistance(DistanceCalculator);
					}
					else
					{
						ShowErrorMessage("Решение не найдено.");
					}
				},
				obj => obj != null
			);
		}

		#endregion RebuilOneRouteCommand

		#region ShowWarningsCommand

		public DelegateCommand ShowWarningsCommand { get; private set; }

		private DelegateCommand CreateShowWarningsCommand()
		{
			return new DelegateCommand(
				() => ShowWarningMessage(string.Join("\n", Optimizer.WarningMessages.Select(x => "⚠ " + x))),
				() => true
			);
		}

		#endregion ShowWarningsCommand

		public override string TabName
		{
			get => $"Формирование МЛ на {DateForRouting:d}";
			set => throw new InvalidOperationException("Установка протеворечит логике работы.");
		}

		#region Свойства

		public IList<RouteList> RoutesOnDay { get; set; }

		public IList<UndeliveryOrderNode> UndeliveredOrdersOnDay { get; set; }

		public IList<OrderOnDayNode> OrdersOnDay { get; set; }

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }

		public RouteGeometryCalculator DistanceCalculator { get; } = new RouteGeometryCalculator();

		private Employee driverFromRouteList;
		public virtual Employee DriverFromRouteList
		{
			get => driverFromRouteList;
			set => SetField(ref driverFromRouteList, value);
		}

		private AtWorkDriver[] selectedDrivers;
		[PropertyChangedAlso(nameof(AreDriversSelected))]
		public virtual AtWorkDriver[] SelectedDrivers
		{
			get => selectedDrivers;
			set => SetField(ref selectedDrivers, value);
		}

		private AtWorkForwarder selectedForwarder;
		[PropertyChangedAlso(nameof(IsForwarderSelected))]
		public virtual AtWorkForwarder SelectedForwarder
		{
			get => selectedForwarder;
			set => SetField(ref selectedForwarder, value);
		}

		private DateTime dateForRouting = DateTime.Today;
		public DateTime DateForRouting
		{
			get => dateForRouting;
			set
			{
				if(SetField(ref dateForRouting, value))
				{
					OnTabNameChanged();
				}
			}
		}

		public bool AreDriversSelected => SelectedDrivers != null && SelectedDrivers.Any();

		public bool IsForwarderSelected => SelectedForwarder != null;

		public bool HasChanges => !HasNoChanges;

		private bool hasNoChanges = true;
		public bool HasNoChanges
		{
			get => hasNoChanges;
			set => SetField(ref hasNoChanges, value);
		}

		private bool autoroutingMode;
		public virtual bool IsAutoroutingModeActive
		{
			get => autoroutingMode;
			set => SetField(ref autoroutingMode, value);
		}

		private IList<AtWorkForwarder> forwardersOnDay = new List<AtWorkForwarder>();
		public virtual IList<AtWorkForwarder> ForwardersOnDay
		{
			get => forwardersOnDay;
			set => SetField(ref forwardersOnDay, value);
		}

		private GenericObservableList<AtWorkForwarder> observableForwardersOnDay;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkForwarder> ObservableForwardersOnDay
		{
			get
			{
				if(observableForwardersOnDay == null)
				{
					observableForwardersOnDay = new GenericObservableList<AtWorkForwarder>(ForwardersOnDay);
				}

				return observableForwardersOnDay;
			}
		}

		private IList<AtWorkDriver> driversOnDay = new List<AtWorkDriver>();
		public virtual IList<AtWorkDriver> DriversOnDay
		{
			get => driversOnDay;
			set => SetField(ref driversOnDay, value);
		}

		private GenericObservableList<AtWorkDriver> observableDriversOnDay;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkDriver> ObservableDriversOnDay
		{
			get
			{
				if(observableDriversOnDay == null)
				{
					observableDriversOnDay = new GenericObservableList<AtWorkDriver>(DriversOnDay);
				}

				return observableDriversOnDay;
			}
		}

		private IRouteOptimizer optimizer;
		public virtual IRouteOptimizer Optimizer
		{
			get => optimizer;
			set => SetField(ref optimizer, value);
		}

		private IList<District> logisticanDistricts = new List<District>();
		public virtual IList<District> LogisticanDistricts
		{
			get => logisticanDistricts;
			set => SetField(ref logisticanDistricts, value);
		}

		private GenericObservableList<District> observableLogisticanDistricts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<District> ObservableLogisticanDistricts
		{
			get
			{
				if(observableLogisticanDistricts == null)
				{
					observableLogisticanDistricts = new GenericObservableList<District>(LogisticanDistricts);
				}

				return observableLogisticanDistricts;
			}
		}

		private bool showCompleted;
		public virtual bool ShowCompleted
		{
			get => showCompleted;
			set => SetField(ref showCompleted, value);
		}

		private bool showOnlyDriverOrders;
		public virtual bool ShowOnlyDriverOrders
		{
			get => showOnlyDriverOrders;
			set => SetField(ref showOnlyDriverOrders, value);
		}

		private int minBottles19L;
		public virtual int MinBottles19L
		{
			get => minBottles19L;
			set => SetField(ref minBottles19L, value);
		}

		private string canTake;
		public virtual string CanTake
		{
			get => canTake;
			set => SetField(ref canTake, value);
		}

		private TimeSpan deliveryFromTime = TimeSpan.Parse("00:00:00");
		public virtual TimeSpan DeliveryFromTime
		{
			get => deliveryFromTime;
			set => SetField(ref deliveryFromTime, value);
		}

		private TimeSpan deliveryToTime = TimeSpan.Parse("23:59:59");
		public virtual TimeSpan DeliveryToTime
		{
			get => deliveryToTime;
			set => SetField(ref deliveryToTime, value);
		}

		private TimeSpan driverStartTime = TimeSpan.Parse("00:00:00");
		public virtual TimeSpan DriverStartTime
		{
			get => driverStartTime;
			set => SetField(ref driverStartTime, value);
		}

		private TimeSpan driverEndTime = TimeSpan.Parse("23:59:59");
		public virtual TimeSpan DriverEndTime
		{
			get => driverEndTime;
			set => SetField(ref driverEndTime, value);
		}

		private DeliveryScheduleFilterType deliveryScheduleType = DeliveryScheduleFilterType.DeliveryStart;
		public virtual DeliveryScheduleFilterType DeliveryScheduleType
		{
			get => deliveryScheduleType;
			set => SetField(ref deliveryScheduleType, value);
		}

		public virtual GenericObservableList<Subdivision> ObservableSubdivisions { get; set; }

		private IList<DeliverySummary> deliverySummary = new List<DeliverySummary>();
		public virtual IList<DeliverySummary> DeliverySummary
		{
			get => deliverySummary;
			set => SetField(ref deliverySummary, value);
		}

		private GenericObservableList<DeliverySummary> observableDeliverySummary;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliverySummary> ObservableDeliverySummary
		{
			get
			{
				if(observableDeliverySummary == null)
				{
					observableDeliverySummary = new GenericObservableList<DeliverySummary>(DeliverySummary);
				}

				return observableDeliverySummary;
			}
		}

		#endregion

		public IEnumerable<OrderAddressTypeNode> OrderAddressTypes { get; } = new[] {
			new OrderAddressTypeNode(isFastDelivery:true),
			new OrderAddressTypeNode(OrderAddressType.Delivery),
			new OrderAddressTypeNode(OrderAddressType.Service),
			new OrderAddressTypeNode(OrderAddressType.ChainStore),
			new OrderAddressTypeNode(OrderAddressType.StorageLogistics)
		};

		public IList<DeliveryShiftNode> DeliveryShiftNodes { get; set; }

		private void LoadAddressesTypesDefaults()
		{
			var currentUserSettings = _userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId);

			foreach(var addressTypeNode in OrderAddressTypes)
			{
				switch(addressTypeNode.OrderAddressType)
				{
					case OrderAddressType.Delivery:
						addressTypeNode.Selected = currentUserSettings.LogisticDeliveryOrders;
						break;
					case OrderAddressType.Service:
						addressTypeNode.Selected = currentUserSettings.LogisticServiceOrders;
						break;
					case OrderAddressType.ChainStore:
						addressTypeNode.Selected = currentUserSettings.LogisticChainStoreOrders;
						break;
				}
			}
		}

		public void DisposeUoW() => UoW.Dispose();

		public void CreateUoW() => UoW = UnitOfWorkFactory.CreateWithoutRoot();

		public string GenerateToolTip(RouteList routeList)
		{
			var firstDP = routeList.Addresses.FirstOrDefault()?.Order.DeliveryPoint;
			var geoGroup = routeList.GeographicGroups.FirstOrDefault();
			var geoGroupVersion = geoGroup.GetVersionOrNull(routeList.Date);

			return $"Первый адрес: {routeList.FirstAddressTime:t}\n" +
				$"Путь со склада: {(firstDP != null && geoGroupVersion != null ? DistanceCalculator.DistanceFromBaseMeter(geoGroupVersion, firstDP) * 0.001 : 0):N1} км." +
				$" ({(firstDP != null && geoGroupVersion != null ? DistanceCalculator.TimeFromBase(geoGroupVersion, firstDP) / 60 : 0)} мин.)\n" +
				$"Выезд со склада: {routeList.OnLoadTimeEnd:t}\nПогрузка на складе: {routeList.TimeOnLoadMinuts} минут";
		}

		public string GetRowTitle(object row)
		{
			if(row is RouteList rl)
			{
				return $"МЛ №{rl.Id} - {rl.Driver.ShortName}({rl.Car.RegistrationNumber})";
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.DeliveryPoint.ShortAddress;
			}

			return null;
		}

		public string GetRowTime(object row)
		{
			if(row is RouteList rl)
			{
				return FormatOccupancy(rl.Addresses.Count, rl.Driver.MinRouteAddresses, rl.Driver.MaxRouteAddresses);
			}

			return (row as RouteListItem)?.Order.DeliverySchedule.Name;
		}

		public string GetRowOnloadTime(object row)
		{
			if(row is RouteList rl && rl.OnLoadTimeStart.HasValue)
			{
				if(rl.OnloadTimeFixed)
				{
					return $"<span foreground=\"{GdkColors.Turquoise.ToHtmlColor()}\">{rl.OnLoadTimeStart.Value:hh\\:mm}</span>";
				}

				return rl.OnLoadTimeStart.Value.ToString("hh\\:mm");
			}

			return null;
		}

		public string GetRowPlanTime(object row)
		{
			if(row is RouteList rl)
			{
				return string.Format("{0:hh\\:mm}-{1:hh\\:mm}",
									 rl.Addresses.FirstOrDefault()?.PlanTimeStart,
									 rl.Addresses.LastOrDefault()?.PlanTimeStart);
			}

			if(row is RouteListItem rli)
			{
				string color;

				if(rli.PlanTimeStart == null || rli.PlanTimeEnd == null)
				{
					color = GdkColors.InsensitiveText.ToHtmlColor();
				}
				else if(rli.PlanTimeEnd.Value + TimeSpan.FromSeconds(rli.TimeOnPoint) > rli.Order.DeliverySchedule.To)
				{
					color = GdkColors.DangerText.ToHtmlColor();
				}
				else if(rli.PlanTimeStart.Value < rli.Order.DeliverySchedule.From)
				{
					color = GdkColors.InfoText.ToHtmlColor();
				}
				else if(rli.PlanTimeEnd.Value == rli.PlanTimeStart.Value)
				{
					color = GdkColors.DarkRed.ToHtmlColor();
				}
				else if(rli.PlanTimeEnd.Value - rli.PlanTimeStart.Value <= new TimeSpan(0, 30, 0))
				{
					color = GdkColors.Orange.ToHtmlColor();
				}
				else
				{
					color = GdkColors.DarkGreen.ToHtmlColor();
				}

				return $"<span foreground=\"{color}\">{rli.PlanTimeStart:hh\\:mm}-{rli.PlanTimeEnd:hh\\:mm}</span> ({rli.TimeOnPoint / 60} мин.)";
			}

			return null;
		}

		public string GetRowBottles(object row)
		{
			if(row is RouteList rl)
			{
				var bottles = rl.Addresses.Sum(x => x.Order.Total19LBottlesToDeliver);
				return FormatOccupancy(bottles, rl.Car.MinBottles, rl.Car.MaxBottles);
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.Total19LBottlesToDeliver.ToString();
			}

			return null;
		}

		public string GetRowBottlesSix(object row)
		{
			if(row is RouteList rl)
			{
				return rl.Addresses.Sum(x => x.Order.Total6LBottlesToDeliver).ToString();
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.Total6LBottlesToDeliver.ToString();
			}

			return null;
		}

		public string GetRowBottlesSmall(object row)
		{
			if(row is RouteList rl)
			{
				return rl.Addresses.Sum(x => x.Order.Total600mlBottlesToDeliver).ToString();
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.Total600mlBottlesToDeliver.ToString();
			}

			return null;
		}

		public string GetRowWeight(object row)
		{
			if(row is RouteList rl)
			{
				var weight = rl.Addresses.Sum(x => x.Order.TotalWeight);
				return FormatOccupancy(weight, null, rl.Car.CarModel.MaxWeight);
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.TotalWeight.ToString();
			}

			return null;
		}

		public string GetRowDeliveryShift(object row)
		{
			if(row is RouteList rl)
			{
				return rl.Shift?.Name;
			}

			return null;
		}

		public string GetRowVolume(object row)
		{
			if(row is RouteList rl)
			{
				var volume = rl.Addresses.Sum(x => x.Order.TotalVolume);
				return FormatOccupancy(volume, null, rl.Car.CarModel.MaxVolume);
			}

			if(row is RouteListItem rli)
			{
				return rli.Order.TotalVolume.ToString();
			}

			return null;
		}

		public string GetRowDistance(object row)
		{
			if(row is RouteList rl)
			{
				var proposed = Optimizer.ProposedRoutes.FirstOrDefault(x => x.RealRoute == rl);

				if(rl.PlanedDistance == null)
				{
					return string.Empty;
				}

				if(proposed == null)
				{
					return $"{rl.PlanedDistance:N1}км";
				}
				else
				{
					return $"{rl.PlanedDistance:N1}км ({(double)proposed.RouteCost / 1000:N})";
				}
			}

			if(row is RouteListItem rli)
			{
				if(rli.IndexInRoute == 0)
				{
					var geoGroup = rli.RouteList.GeographicGroups.FirstOrDefault();
					var geoGroupVersion = geoGroup.GetVersionOrNull(rli.RouteList.Date);

					if(geoGroupVersion == null)
					{
						return null;
					}

					return $"{(double)DistanceCalculator.DistanceFromBaseMeter(geoGroupVersion, rli.Order.DeliveryPoint) / 1000:N1}км";
				}

				return $"{(double)DistanceCalculator.DistanceMeter(rli.RouteList.Addresses[rli.IndexInRoute - 1].Order.DeliveryPoint, rli.Order.DeliveryPoint) / 1000:N1}км";
			}

			return null;
		}

		public decimal? GetGrossMarginPercentage(object row)
		{
			if(row is RouteList rl)
			{
				return rl?.RouteListProfitability?.GrossMarginPercents;
			}

			return null;
		}

		public decimal? GetGrossMarginMoney(object row)
		{
			if(row is RouteList rl)
			{
				return rl?.RouteListProfitability?.GrossMarginSum;
			}

			return null;
		}

		public string GetRowEquipmentFromClient(object row)
		{
			if(row is RouteListItem rli)
			{
				return rli.Order.FromClientText;
			}

			return null;
		}

		public string GetRowEquipmentToClient(object row)
		{
			string nomenclatureName = null;

			if(row is RouteListItem rli)
			{
				foreach(var orderItem in rli.Order.OrderItems)
				{
					if(orderItem.Nomenclature.Category == NomenclatureCategory.equipment || orderItem.Nomenclature.Category == NomenclatureCategory.additional)
					{
						nomenclatureName += " " + orderItem.Nomenclature.Name;
					}
				}

				return rli.Order.EquipmentsToClient + nomenclatureName;
			}

			return null;
		}

		private string FormatOccupancy(int val, int? min, int? max)
		{
			string color = GdkColors.SuccessText.ToHtmlColor();

			if(val > max)
			{
				color = GdkColors.DangerText.ToHtmlColor();
			}

			if(val < min)
			{
				color = GdkColors.InfoText.ToHtmlColor();
			}

			if(min.HasValue && max.HasValue)
			{
				return $"<span foreground=\"{color}\">{val}</span>({min}-{max})";
			}

			if(max.HasValue)
			{
				return $"<span foreground=\"{color}\">{val}</span>({max})";
			}

			return $"<span foreground=\"{color}\">{val}</span>(min {min})";
		}

		private string FormatOccupancy(decimal val, decimal? min, decimal? max)
		{
			string color = GdkColors.SuccessText.ToHtmlColor();

			if(val > max)
			{
				color = GdkColors.DangerText.ToHtmlColor();
			}

			if(val < min)
			{
				color = GdkColors.InfoText.ToHtmlColor();
			}

			if(min.HasValue && max.HasValue)
			{
				return $"<span foreground=\"{color}\">{val}</span>({min}-{max})";
			}

			if(max.HasValue)
			{
				return $"<span foreground=\"{color}\">{val}</span>({max})";
			}

			return $"<span foreground=\"{color}\">{val}</span>(min {min})";
		}

		private PointMarkerType[] pointMarkers = {
			PointMarkerType.blue,
			PointMarkerType.green,
			PointMarkerType.orange,
			PointMarkerType.purple,
			PointMarkerType.red,
			PointMarkerType.gray,
			PointMarkerType.color2,
			PointMarkerType.color3,
			PointMarkerType.color4,
			PointMarkerType.color5,
			PointMarkerType.color6,
			PointMarkerType.color7,
			PointMarkerType.color8,
			PointMarkerType.color9,
			PointMarkerType.color10,
			PointMarkerType.color11,
			PointMarkerType.color12,
			PointMarkerType.color13,
			PointMarkerType.color14,
			PointMarkerType.color15,
			PointMarkerType.color16,
			PointMarkerType.color17,
			PointMarkerType.color18,
			PointMarkerType.color20,
			PointMarkerType.color21,
			PointMarkerType.color22,
			PointMarkerType.color23,
			PointMarkerType.color24,
		};

		public PointMarkerType GetAddressMarker(int routeNum)
		{
			var markerNum = routeNum % pointMarkers.Length;
			return pointMarkers[markerNum];
		}

		public int GetMarkerIndex(object row, int maxLen)
		{
			int index = 0;

			if(row is RouteList rl)
			{
				index = RoutesOnDay.IndexOf(rl);
			}

			if(row is RouteListItem rli)
			{
				index = RoutesOnDay.IndexOf(rli.RouteList);
			}

			if(index < 0 || index >= maxLen)
			{
				index = 0;
			}

			return index;
		}

		public PointMarkerShape GetMarkerShape(object row)
		{
			var shape = PointMarkerShape.circle;

			if(row is RouteList rl)
			{
				shape = GetMarkerShapeFromBottleQuantity(rl.TotalFullBottlesToClient);
			}

			if(row is RouteListItem rli)
			{
				shape = GetMarkerShapeFromBottleQuantity(rli.GetFullBottlesToDeliverCount());
			}

			return shape;
		}

		public PointMarkerShape GetMarkerShapeFromBottleQuantity(int bottlesCount, bool overdueOrder = false)
		{
			if(overdueOrder)
			{
				if(bottlesCount < 6)
				{
					return PointMarkerShape.overduetriangle;
				}

				if(bottlesCount < 10)
				{
					return PointMarkerShape.overduecircle;
				}

				if(bottlesCount < 20)
				{
					return PointMarkerShape.overduesquare;
				}

				if(bottlesCount < 40)
				{
					return PointMarkerShape.overduecross;
				}

				return PointMarkerShape.overduestar;
			}

			if(bottlesCount < 6)
			{
				return PointMarkerShape.triangle;
			}

			if(bottlesCount < 10)
			{
				return PointMarkerShape.circle;
			}

			if(bottlesCount < 20)
			{
				return PointMarkerShape.square;
			}

			if(bottlesCount < 40)
			{
				return PointMarkerShape.cross;
			}

			return PointMarkerShape.star;
		}

		public string GetOrdersInfo()
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			(int OrderId, decimal Count) resultAlias = default;
			
			_logger.LogInformation("Начали расчет параметров");

			var totalOrders =
				OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true, excludeTrucks: ExcludeTrucks)
					.GetExecutableQueryOver(UoW.Session)
					.Where(o => !o.IsContractCloser)
					.And(o => o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)
					.And(o => o.OrderAddressType != OrderAddressType.Service)
					.Select(Projections.CountDistinct<Order>(x => x.Id))
					.SingleOrDefault<int>();

			var totalBottles = 
				OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true, excludeTrucks: ExcludeTrucks)
					.GetExecutableQueryOver(UoW.Session)
					.JoinAlias(o => o.OrderItems, () => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
					.And(o => !o.IsContractCloser)
					.And(o => o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)
					.And(o => o.OrderAddressType != OrderAddressType.Service)
					.SelectList(list => list
						.SelectGroup(o => o.Id).WithAlias(() => resultAlias.OrderId)
						.Select(Projections.Sum(() => orderItemAlias.Count)).WithAlias(() => resultAlias.Count))
					.TransformUsing(Transformers.AliasToBean<(int OrderId, decimal Count)>())
					.List<(int OrderId, decimal Count)>()
					.Sum(x => x.Count);

			var total6LBottles =
				OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true, excludeTrucks: ExcludeTrucks)
					.GetExecutableQueryOver(UoW.Session)
					.JoinAlias(o => o.OrderItems, () => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol6L)
					.And(o => !o.IsContractCloser)
					.And(o => o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)
					.And(o => o.OrderAddressType != OrderAddressType.Service)
					.SelectList(list => list
						.SelectGroup(o => o.Id).WithAlias(() => resultAlias.OrderId)
						.Select(Projections.Sum(() => orderItemAlias.Count)).WithAlias(() => resultAlias.Count))
					.TransformUsing(Transformers.AliasToBean<(int OrderId, decimal Count)>())
					.List<(int OrderId, decimal Count)>()
					.Sum(x => x.Count);

			var total600mlBottles =
				OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true, excludeTrucks: ExcludeTrucks)
					.GetExecutableQueryOver(UoW.Session)
					.JoinAlias(o => o.OrderItems, () => orderItemAlias)
					.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
					.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol600ml)
					.And(o => !o.IsContractCloser)
					.And(o => o.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)
					.And(o => o.OrderAddressType != OrderAddressType.Service)
					.SelectList(list => list
						.SelectGroup(o => o.Id).WithAlias(() => resultAlias.OrderId)
						.Select(Projections.Sum(() => orderItemAlias.Count)).WithAlias(() => resultAlias.Count))
					.TransformUsing(Transformers.AliasToBean<(int OrderId, decimal Count)>())
					.List<(int OrderId, decimal Count)>()
					.Sum(x => x.Count);
			
			_logger.LogInformation("Закончили расчет параметров");

			var text = new List<string> {
				NumberToTextRus.FormatCase(totalOrders, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов."),
				$"19л - {totalBottles:N0}",
				$"6л - {total6LBottles:N0}",
				$"0,6л - {total600mlBottles:N0}"
			};

			return string.Join("\n", text);
		}

		public void GetWorkDriversInfo()
		{
			int totalBottles = 0;
			int totalAddresses = 0;

			var drivers = new EmployeeRepository().GetWorkingDriversAtDay(UoW, DateForRouting);

			var cars = CarRepository.GetCarsByDrivers(UoW, drivers.Select(x => x.Id).ToArray());


			if(drivers.Count > 0)
			{
				foreach(var driver in drivers)
				{
					var car = cars.SingleOrDefault(x => x.Driver.Id == driver.Id);

					if(car != null)
					{
						totalBottles += car.MaxBottles;
					}

					totalAddresses += driver.MaxRouteAddresses;
				}
			}

			var text = new List<string> { "Можем вывезти:", $"Бутылей - {totalBottles}", $"Адресов - {totalAddresses}" };

			CanTake = string.Join("\n", text);
		}

		public bool CheckAlreadyAddedAddress(OrderOnDayNode order)
		{
			var routeList = _routeListRepository.GetActualRouteListByOrder(UoW, order.OrderId);

			if(routeList != null)
			{
				ShowWarningMessage($"Адрес ({order.DeliveryPointCompiledAddress}) уже был кем-то добавлен в МЛ ({routeList.Id}). Обновите данные.");
			}

			return routeList == null;
		}

		public bool CheckRouteListWasChanged(RouteList routeList)
		{
			if(!_routeListRepository.RouteListWasChanged(routeList))
			{
				return true;
			}

			ShowWarningMessage($"МЛ ({routeList.Id}) уже был кем-то изменен. Обновите данные.");

			return false;
		}

		public void RecalculateOnLoadTime()
		{
			//FIXME Проверять что все МЛ присутствуют
			RouteList.RecalculateOnLoadTime(RoutesOnDay, DistanceCalculator);
		}

		public bool AddOrdersToRouteList(IList<OrderOnDayNode> selectedOrderNodes, RouteList routeList)
		{
			if(!routeList.IsDriversDebtInPermittedRangeVerification())
			{
				return false;
			}

			bool recalculateLoading = false;

			if(IsAutoroutingModeActive)
			{
				foreach(var order in selectedOrderNodes)
				{
					if(order.OrderStatus == OrderStatus.InTravelList)
					{
						var alreadyIn = RoutesOnDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.OrderId));

						if(alreadyIn == null)
						{
							throw new InvalidProgramException(string.Format("Маршрутный лист, в котором добавлен заказ {0} не найден.", order.OrderId));
						}

						if(alreadyIn.Id == routeList.Id) // Уже в нужном маршрутном листе.
						{
							continue;
						}

						var toRemoveAddress = alreadyIn.Addresses.First(x => x.Order.Id == order.OrderId);

						if(toRemoveAddress.IndexInRoute == 0)
						{
							recalculateLoading = true;
						}

						alreadyIn.RemoveAddress(toRemoveAddress);
						UoW.Save(alreadyIn);
					}

					var item = routeList.AddAddressFromOrder(order.OrderId);

					if(item.IndexInRoute == 0)
					{
						recalculateLoading = true;
					}
				}
				routeList.RecalculatePlanTime(DistanceCalculator);
				routeList.RecalculatePlanedDistance(DistanceCalculator);

				UoW.Save(routeList);
			}
			else
			{
				foreach(var order in selectedOrderNodes)
				{
					if(!CheckAlreadyAddedAddress(order))
					{
						return false;
					}

					var item = routeList.AddAddressFromOrder(order.OrderId);

					if(item.IndexInRoute == 0)
					{
						recalculateLoading = true;
					}
				}

				if(!CheckRouteListWasChanged(routeList))
				{
					return false;
				}

				routeList.RecalculatePlanTime(DistanceCalculator);
				routeList.RecalculatePlanedDistance(DistanceCalculator);

				SaveRouteList(routeList);
			}

			_logger.LogInformation("В МЛ №{RouteListId} добавлено {SelectedOrdersCount} адресов.", routeList.Id, selectedOrderNodes.Count);

			if(recalculateLoading)
			{
				RecalculateOnLoadTime();
			}

			bool overweight = routeList.HasOverweight();
			bool volExcess = routeList.HasVolumeExecess();
			bool reverseVolExcess = routeList.HasReverseVolumeExcess();

			if(overweight || volExcess || reverseVolExcess)
			{
				var warningMsg = new StringBuilder($"Автомобиль '{routeList.Car.Title}' в МЛ №{routeList.Id}:");

				if(overweight)
				{
					warningMsg.Append($"\n\t- перегружен на {routeList.Overweight()} кг");
				}

				if(volExcess)
				{
					warningMsg.Append($"\n\t- объём груза превышен на {routeList.VolumeExecess()} м<sup>3</sup>");
				}

				if(reverseVolExcess)
				{
					warningMsg.Append($"\n\t- объём возвращаемого груза превышен на {routeList.ReverseVolumeExecess()} м<sup>3</sup>");
				}

				ShowWarningMessage(warningMsg.ToString());
			}
			return true;
		}

		public bool SaveAutoroutingResults()
		{
			//Перестраиваем все маршруты
			RebuildAllRoutes();
			UoW.Commit();
			HasNoChanges = true;
			AutoroutingResultsSaved?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public void SaveRouteList(RouteList routeList, Action<string> actionUpdateInfo = null)
		{
			RebuildAllRoutes(actionUpdateInfo);

			UoW.Session.Flush();
			_routeListProfitabilityController.ReCalculateRouteListProfitability(UoW, routeList);

			UoW.Save(routeList);
			UoW.Commit();
			HasNoChanges = true;
		}

		public void RebuildAllRoutes(Action<string> actionUpdateInfo = null)
		{
			int ix = 0;
			var warnings = new List<string>();
			Optimizer.StatisticsTxtAction = null;

			foreach(var route in RoutesOnDay)
			{
				ix++;
				actionUpdateInfo?.Invoke($"Строим {ix} из {RoutesOnDay.Count}");

				var newRoute = Optimizer.RebuidOneRoute(route);

				if(newRoute != null)
				{
					newRoute.UpdateAddressOrderInRealRoute(route);
					route.RecalculatePlanedDistance(DistanceCalculator);
					var noPlan = route.Addresses.Count(x => !x.PlanTimeStart.HasValue);

					if(noPlan > 0)
					{
						warnings.Add($"Для маршрута №{route.Id} - {route.Driver?.ShortName}({route.Car?.RegistrationNumber}) незапланировано {noPlan} адресов.");
					}
				}
				else
				{
					warnings.Add($"Маршрут {route.Id} не был перестроен.");
				}
			}

			if(warnings.Any())
			{
				ShowWarningMessage(string.Join("\n", warnings));
			}
		}

		public void InitializeData()
		{
			UoW.Dispose();
			CreateUoW();

			if(OrdersOnDay == null)
			{
				OrdersOnDay = new List<OrderOnDayNode>();
			}
			else
			{
				OrdersOnDay.Clear();
			}

			var selectedDeliveryShiftIds = DeliveryShiftNodes
				.Where(x => x.Selected)
				.Select(x => x.DeliveryShift.Id)
				.ToArray();

			var selectedGeographicGroupIds = GeographicGroupNodes
				.Where(x => x.Selected)
				.Select(x => x.GeographicGroup.Id)
				.ToArray();

			var orderOnDayFilter = new OrderOnDayFilters
			{
				DateForRouting = DateForRouting,
				GeographicGroupIds = selectedGeographicGroupIds,
				DeliveryScheduleType = DeliveryScheduleType,
				DeliveryFromTime = DeliveryFromTime,
				DeliveryToTime = DeliveryToTime,
				ShowCompleted = ShowCompleted,
				MinBottles19L = MinBottles19L,
				FastDeliveryEnabled = OrderAddressTypes.Any(x => x.IsFastDelivery && x.Selected),
				OrderAddressTypes = OrderAddressTypes
					.Where(x => !x.IsFastDelivery && x.Selected)
					.Select(x => x.OrderAddressType),
				ClosingDocumentDeliveryScheduleId = _closingDocumentDeliveryScheduleId
			};

			OrdersOnDay = OrderRepository.GetOrdersOnDay(UoW, orderOnDayFilter);

			if(OrderAddressTypes.Any(x => x.Selected))
			{
				UndeliveredOrder undeliveredOrderAlias = null;
				Order orderAlias = null;
				Order orderAlias2 = null;

				UndeliveryOrderNode resultAlias = null;
				UndeliveredOrdersOnDay = QueryOver.Of<GuiltyInUndelivery>()
					.Left.JoinAlias(x => x.UndeliveredOrder, () => undeliveredOrderAlias)
					.Left.JoinAlias(() => undeliveredOrderAlias.OldOrder, () => orderAlias)
					.Left.JoinAlias(() => undeliveredOrderAlias.NewOrder, () => orderAlias2)
					.Where(() => orderAlias2.DeliveryDate == DateForRouting.Date && !orderAlias2.SelfDelivery)
					.Where(() => orderAlias2.DeliverySchedule != null)
					.Where(() => orderAlias2.DeliveryPoint != null)
					.And(() => orderAlias2.DeliverySchedule.Id != _closingDocumentDeliveryScheduleId)
					.GetExecutableQueryOver(UoW.Session)
					.SelectList(list => list
						.Select(x => x.GuiltySide).WithAlias(() => resultAlias.GuiltySide)
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OldOrderId)
						.Select(() => orderAlias2.Id).WithAlias(() => resultAlias.NewOrderId)
						.Select(() => orderAlias2.DeliveryPoint).WithAlias(() => resultAlias.DeliveryPoint)
						.Select(() => orderAlias2.BottlesReturn).WithAlias(() => resultAlias.Bottles))
					.TransformUsing(Transformers.AliasToBean<UndeliveryOrderNode>()).List<UndeliveryOrderNode>();
			}

			_logger.LogInformation("Загружаем МЛ на {DateForRouting:d}...", DateForRouting);

			RoutesOnDay = _routeListRepository.GetRoutesAtDay(UoW, DateForRouting, ShowCompleted, selectedGeographicGroupIds, selectedDeliveryShiftIds);

			GetWorkDriversInfo();
			CalculateOnDeliverySum();
			RoutesOnDay.ToList().ForEach(rl => rl.UoW = UoW);

			//Нужно для того чтобы диалог не падал при загрузке если присутствую поломаные МЛ.
			RoutesOnDay.ToList().ForEach(rl => rl.CheckAddressOrder());

			_logger.LogInformation("Загружаем водителей на {DateForRouting:d}...", DateForRouting);
			ObservableDriversOnDay.Clear();
			_atWorkRepository.GetDriversAtDay(UoW, DateForRouting, driverStatuses: new[] { AtWorkDriver.DriverStatus.IsWorking }).ToList().ForEach(x => ObservableDriversOnDay.Add(x));
			_logger.LogInformation("Загружаем экспедиторов на {DateForRouting:d}...", DateForRouting);
			ObservableForwardersOnDay.Clear();
			_atWorkRepository.GetForwardersAtDay(UoW, DateForRouting).ToList().ForEach(x => ObservableForwardersOnDay.Add(x));
		}

		public string GetOrdersInfo(int addressesWithoutCoordinats, int addressesWithoutRoutes, int totalBottlesCountAtDay, int bottlesWithoutRL)
		{
			var text = new List<string> {
				NumberToTextRus.FormatCase(OrdersOnDay.Count, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов.")
			};

			if(addressesWithoutCoordinats > 0)
			{
				text.Add($"Из них {addressesWithoutCoordinats} без координат.");
			}

			if(addressesWithoutRoutes > 0)
			{
				text.Add($"Из них {addressesWithoutRoutes} без маршрутных листов.");
			}

			if(totalBottlesCountAtDay > 0)
			{
				text.Add(NumberToTextRus.FormatCase(totalBottlesCountAtDay, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));
			}

			if(bottlesWithoutRL > 0)
			{
				text.Add(NumberToTextRus.FormatCase(bottlesWithoutRL, "Осталась {0} бутыль", "Осталось {0} бутыли", "Осталось {0} бутылей"));
			}

			text.Add(NumberToTextRus.FormatCase(RoutesOnDay.Count, "Всего {0} маршрутный лист.", "Всего {0} маршрутных листа.", "Всего {0} маршрутных листов."));

			return string.Join("\n", text);
		}

		public bool CreateRoutesAutomatically(Action<string> statisticsUpdateAction)
		{
			if(DriversOnDay.Any(d => d.Car != null && d.GeographicGroup == null))
			{
				ShowWarningMessage("Не всем автомобилям назначена \"База\" для погрузки-разгрузки. Пожалуйста укажите.");
				return false;
			}

			var orderIds = OrdersOnDay.Select(o => o.OrderId).Distinct().ToList();

			Optimizer.UoW = UoW;
			Optimizer.Routes = RoutesOnDay;
			Optimizer.Orders = UoW.GetAll<Order>().Where(o => orderIds.Contains(o.Id)).ToList();
			Optimizer.Drivers = DriversOnDay;
			Optimizer.Forwarders = ForwardersOnDay;
			Optimizer.StatisticsTxtAction = statisticsUpdateAction;
			Optimizer.CreateRoutes(DateForRouting, DriverStartTime, DriverEndTime, message => CommonServices.InteractiveService.Question(message));

			if(optimizer.ProposedRoutes.Any())
			{
				//Удаляем корректно адреса из уже имеющихся МЛ. Чтобы они встали в правильный статус.
				foreach(var route in RoutesOnDay.Where(x => x.Id > 0))
				{
					foreach(var odrer in route.Addresses.ToList())
					{
						route.RemoveAddress(odrer);
					}
				}

				foreach(var propose in optimizer.ProposedRoutes)
				{
					var rl = propose.Trip.OldRoute ?? new RouteList();

					rl.UoW = UoW;
					rl.Car = propose.Trip.Car;
					rl.Driver = propose.Trip.Driver;
					rl.Shift = propose.Trip.Shift;
					rl.Date = DateForRouting;
					rl.Logistician = VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);

					if(propose.Trip.OldRoute == null)
					{
						rl.GeographicGroups.Clear();
						rl.GeographicGroups.Add(propose.Trip.GeographicGroup);
					}

					foreach(var order in propose.Orders)
					{
						var address = rl.AddAddressFromOrder(order.Order);
						address.PlanTimeStart = order.ProposedTimeStart;
						address.PlanTimeEnd = order.ProposedTimeEnd;
					}

					if(propose.Trip.OldRoute == null) // Это новый маршрут и его нужно добавить.
					{
						RoutesOnDay.Add(rl);
					}

					propose.RealRoute = rl;
					UoW.Save(rl);
				}
			}

			RecalculateOnLoadTime();

			return true;
		}

		public void SelectCarForDriver(AtWorkDriver driver, Car car)
		{
			var driverNames = string.Join("\", \"", DriversOnDay.Where(x => x.Car != null && x.Car.Id == car.Id).Select(x => x.Employee.ShortName));

			if(string.IsNullOrEmpty(driverNames) || AskQuestion(
				$"Автомобиль \"{car.RegistrationNumber}\" уже назначен \"{driverNames}\". Переназначить его водителю \"{driver.Employee.ShortName}\"?"))
			{
				DriversOnDay
					.Where(x => x.Car != null && x.Car.Id == car.Id)
					.ToList()
					.ForEach(x =>
					{
						x.Car = null; x.GeographicGroup = null;
					});

				driver.Car = car;
			}
		}

		private DeliveryDaySchedule GetDriverWorkDaySchedule(Employee driver)
		{
			var driverWorkSchedule = driver
				.ObservableDriverWorkScheduleSets.SingleOrDefault(x => x.IsActive)
				?.ObservableDriverWorkSchedules.SingleOrDefault(x => (int)x.WeekDay == (int)DateForRouting.DayOfWeek);

			return driverWorkSchedule == null
				? _defaultDeliveryDaySchedule
				: driverWorkSchedule.DaySchedule;
		}

		private void CalculateOnDeliverySum()
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;
			OrdersCountNode ordersCountNode = null;
			DeliverySummaryNode resultAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			GeoGroup geographicGroupAlias = null;
			Counterparty counterpartyAlias = null;
			Order orderBaseAlias = null;

			ObservableDeliverySummary.Clear();

			var baseQuery = OrderRepository
				.GetOrdersForRLEditingQuery(DateForRouting, true, orderBaseAlias)
				.GetExecutableQueryOver(UoW.Session)
				.Where(o => !o.IsContractCloser)
				.And(o => o.OrderAddressType != OrderAddressType.Service);

			if(OrderAddressTypes.Any(x => x.Selected))
			{
				AddAddressTypeFilter(baseQuery);

				var selectedGeographicGroup = GeographicGroupNodes
					.Where(x => x.Selected)
					.Select(x => x.GeographicGroup);

				if(selectedGeographicGroup.Any())
				{
					baseQuery.Left.JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
						.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
						.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
						.Where(Restrictions.In(Projections.Property(() => geographicGroupAlias.Id),
							selectedGeographicGroup.Select(x => x.Id).ToArray()));
				}

				var selectedDeliveryShifts = DeliveryShiftNodes.Where(x => x.Selected).Select(x => x.DeliveryShift).ToArray();

				if(selectedDeliveryShifts.Any())
				{
					RouteList routeListAlias = null;
					RouteListItem routeListItemAlias = null;

					baseQuery
						.JoinEntityAlias(() => routeListItemAlias, () => routeListItemAlias.Order.Id == orderBaseAlias.Id)
						.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
						.WhereRestrictionOn(() => routeListAlias.Shift).IsIn(selectedDeliveryShifts);
				}
			}

			var ordersCount = baseQuery.Clone()
				.SelectList(list => list
					.Select(o => o.OrderStatus).WithAlias(() => ordersCountNode.OrderStatus)
					.Select(o => o.Id).WithAlias(() => ordersCountNode.Id)
				).TransformUsing(Transformers.AliasToBean<OrdersCountNode>()).List<OrdersCountNode>().GroupBy(o => o.OrderStatus);

			var deliverySummaryNodes = baseQuery.Clone()
				.Inner.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Inner.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water &&
							 (nomenclatureAlias.TareVolume == TareVolume.Vol19L))
				.SelectList(list => list
					.Select(o => o.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
					.Select(() => orderItemAlias.Count).WithAlias(() => resultAlias.Bottles)
					.Select(() => nomenclatureAlias.TareVolume).WithAlias(() => resultAlias.TareVolume)
				).TransformUsing(Transformers.AliasToBean<DeliverySummaryNode>()).List<DeliverySummaryNode>();

			var totalCancellations = new DeliverySummary { Name = "Итого отмены" };
			var totalNotRL = new DeliverySummary { Name = "Итого не в МЛ" };
			var totalInRL = new DeliverySummary { Name = "Итого в МЛ" };
			var totalOnTheWay = new DeliverySummary { Name = "Итого в пути" };
			var totalCompleted = new DeliverySummary { Name = "Итого выполнено" };

			foreach(var orderGroup in deliverySummaryNodes.GroupBy(o => o.OrderStatus))
			{
				var addressCount = ordersCount.Where(x => x.Key == orderGroup.Key).Sum(x => x.Select(y => y).Count());
				var deliverySum = new DeliverySummary(orderGroup.Key.GetEnumTitle(), addressCount, orderGroup.Select(x => x).ToList());

				ObservableDeliverySummary.Add(deliverySum);

				switch(orderGroup.Key)
				{
					case OrderStatus.DeliveryCanceled:
					case OrderStatus.NotDelivered:
						totalCancellations.Bottles += deliverySum.Bottles;
						totalCancellations.AddressCount += deliverySum.AddressCount;
						break;
					case OrderStatus.Accepted:
						totalNotRL.Bottles += deliverySum.Bottles;
						totalNotRL.AddressCount += deliverySum.AddressCount;
						break;
					case OrderStatus.InTravelList:
					case OrderStatus.OnLoading:
						totalInRL.Bottles += deliverySum.Bottles;
						totalInRL.AddressCount += deliverySum.AddressCount;
						break;
					case OrderStatus.OnTheWay:
						totalOnTheWay.Bottles += deliverySum.Bottles;
						totalOnTheWay.AddressCount += deliverySum.AddressCount;
						break;
					case OrderStatus.UnloadingOnStock:
					case OrderStatus.Shipped:
					case OrderStatus.Closed:
						totalCompleted.Bottles += deliverySum.Bottles;
						totalCompleted.AddressCount += deliverySum.AddressCount;
						break;
				}
			}

			var totalNoAway = new DeliverySummary
			{
				Name = "Итого не уехало",
				Bottles = totalNotRL.Bottles + totalInRL.Bottles,
				AddressCount = totalNotRL.AddressCount + totalInRL.AddressCount
			};

			var totalLeft = new DeliverySummary
			{
				Name = "Итого уехало",
				Bottles = totalCompleted.Bottles + totalOnTheWay.Bottles,
				AddressCount = totalCompleted.AddressCount + totalOnTheWay.AddressCount
			};

			var totalForDay = new DeliverySummary
			{
				Name = "Итого за день",
				Bottles = totalNoAway.Bottles + totalLeft.Bottles + totalCancellations.Bottles,
				AddressCount = totalNoAway.AddressCount + totalLeft.AddressCount + totalCancellations.AddressCount
			};

			ObservableDeliverySummary.Add(totalCancellations);
			ObservableDeliverySummary.Add(totalNotRL);
			ObservableDeliverySummary.Add(totalInRL);
			ObservableDeliverySummary.Add(totalNoAway);
			ObservableDeliverySummary.Add(totalOnTheWay);
			ObservableDeliverySummary.Add(totalCompleted);
			ObservableDeliverySummary.Add(totalLeft);
			ObservableDeliverySummary.Add(totalForDay);
		}

		public bool CanСreateRoutelistInPastPeriod { get; }

		public IRouteListProfitabilityController RouteListProfitabilityController => _routeListProfitabilityController;

		public static GuiltyTypes[] GuiltyTypesForMarkUndeliveries => new[] 
		{
			GuiltyTypes.Driver,
			GuiltyTypes.Department,
			GuiltyTypes.ForceMajor,
			GuiltyTypes.DirectorLO
		};

		public override void Dispose()
		{
			RoutesOnDay = null;
			UndeliveredOrdersOnDay = null;
			OrdersOnDay = null;
			ForwardersOnDay = null;
			DriversOnDay = null;
			LogisticanDistricts = null;
			DeliverySummary = null;

			UoW?.Dispose();

			base.Dispose();
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Utils;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Utilities;
using QS.ViewModels;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.TempAdapters;
using Vodovoz.Tools.Logistic;
using Order = Vodovoz.Domain.Orders.Order;
using QS.Navigation;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.ViewModels.Dialogs.Logistic;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz.ViewModels.Logistic
{
	public class RouteListsOnDayViewModel : TabViewModelBase
	{
		private readonly IRouteListRepository routeListRepository;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly IAtWorkRepository atWorkRepository;
		private readonly IGtkTabsOpener gtkTabsOpener;
		private readonly IUserRepository userRepository;
		private readonly DeliveryDaySchedule defaultDeliveryDaySchedule;
		private readonly int closingDocumentDeliveryScheduleId;
		private readonly IEmployeeJournalFactory _employeeJournalFactory;

		public IUnitOfWork UoW;

		public RouteListsOnDayViewModel(
			ICommonServices commonServices,
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
			ICarModelJournalFactory carModelJournalFactory) : base(commonServices?.InteractiveService, navigationManager)
		{
			if(defaultDeliveryDayScheduleSettings == null)
			{
				throw new ArgumentNullException(nameof(defaultDeliveryDayScheduleSettings));
			}
			if(geographicGroupRepository == null)
			{
				throw new ArgumentNullException(nameof(geographicGroupRepository));
			}

			CommonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			CarRepository = carRepository ?? throw new ArgumentNullException(nameof(carRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_employeeJournalFactory = employeeJournalFactory ?? throw new ArgumentNullException(nameof(employeeJournalFactory));
			ScheduleRestrictionRepository =
				scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			CarModelJournalFactory = carModelJournalFactory;
			this.gtkTabsOpener = gtkTabsOpener ?? throw new ArgumentNullException(nameof(gtkTabsOpener));
			this.atWorkRepository = atWorkRepository ?? throw new ArgumentNullException(nameof(atWorkRepository));
			this.OrderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			
			closingDocumentDeliveryScheduleId = deliveryScheduleParametersProvider?.ClosingDocumentDeliveryScheduleId ??
			                                    throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider));

			CanСreateRoutelistInPastPeriod = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_routelist_in_past_period");

			CreateUoW();

			Employee currentEmployee = VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);
			if(currentEmployee == null) {
				ShowWarningMessage("Ваш пользователь не привязан к сотруднику, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}

			if(currentEmployee.Subdivision == null) {
				ShowWarningMessage("У сотрудника не указано подразделение, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}

			ObservableSubdivisions = new GenericObservableList<Subdivision>(subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new[] { typeof(Income) }).ToList());
			if(!ObservableSubdivisions.Any()) {
				ShowErrorMessage("Не правильно сконфигурированы подразделения кассы, невозможно будет указать подразделение в которое будут сдаваться маршрутные листы");
				FailInitialize = true;
				return;
			}

			GeographicGroupsExceptEast =
				geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);
			var geographicGroups = geographicGroupRepository.GeographicGroupsWithCoordinates(UoW, isActiveOnly: true);
			GeographicGroupNodes = new GenericObservableList<GeographicGroupNode>(geographicGroups.Select(x => new GeographicGroupNode(x)).ToList());
			GeoGroup employeeGeographicGroup = currentEmployee.Subdivision.GetGeographicGroup();
			if(employeeGeographicGroup != null) {
				var foundGeoGroup = GeographicGroupNodes.FirstOrDefault(x => x.GeographicGroup.Id == employeeGeographicGroup.Id);
				if(foundGeoGroup != null)
					foundGeoGroup.Selected = true;
			}
			Optimizer = new RouteOptimizer(commonServices.InteractiveService, new GeographicGroupRepository());

			defaultDeliveryDaySchedule =
				UoW.GetById<DeliveryDaySchedule>(defaultDeliveryDayScheduleSettings.GetDefaultDeliveryDayScheduleId());
			//Необходимо сразу проинициализировать, т.к вызывается Session.Clear() в методе InitializeData()
			NHibernateUtil.Initialize(defaultDeliveryDaySchedule.Shifts);

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

		public ICommonServices CommonServices { get; }
		public ICarRepository CarRepository { get; }
		public IList<GeoGroup> GeographicGroupsExceptEast { get; }
		public IScheduleRestrictionRepository ScheduleRestrictionRepository { get; }
		public ICarModelJournalFactory CarModelJournalFactory { get; }
		public IOrderRepository OrderRepository { get; }

		void CreateCommands()
		{
			CreateSaveCommand();
			CreateRemoveRLItemCommand();
			CreateOpenOrderOrRouteListCommand();
			CreateAddDriverCommand();
			CreateRemoveDriverCommand();
			CreateAddForwarderCommand();
			CreateRemoveForwarderCommand();
			CreateRebuilOneRouteCommand();
			CreateShowWarningsCommand();
		}

		public event EventHandler AutoroutingResultsSaved;

		#region SaveCommand

		public DelegateCommand SaveCommand { get; private set; }
		void CreateSaveCommand()
		{
			SaveCommand = new DelegateCommand(
				() => {
					if(SaveAutoroutingResults())
						IsAutoroutingModeActive = false;
				},
				() => IsAutoroutingModeActive
			);
		}

		#endregion SaveCommand

		#region RemoveRLItemCommand

		public DelegateCommand<RouteListItem> RemoveRLItemCommand { get; private set; }
		void CreateRemoveRLItemCommand()
		{
			RemoveRLItemCommand = new DelegateCommand<RouteListItem>(
				i => {
					var route = i.RouteList;
					route.RemoveAddress(i);
					if(!CheckRouteListWasChanged(route))
						return;
					if(IsAutoroutingModeActive) {
						UoW.Save(route);
					} else
						SaveRouteList(route);
					route.RecalculatePlanTime(DistanceCalculator);
					route.RecalculatePlanedDistance(DistanceCalculator);
				},
				i => i != null
			);
		}

		#endregion RemoveRLItemCommand

		#region OpenOrderOrRouteListCommand

		public DelegateCommand<object> OpenOrderOrRouteListCommand { get; private set; }
		void CreateOpenOrderOrRouteListCommand()
		{
			OpenOrderOrRouteListCommand = new DelegateCommand<object>(
				obj => {
					//Открываем заказ
					if(obj is RouteListItem rli)
						gtkTabsOpener.OpenOrderDlg(this, rli.Order.Id);

					//Открываем МЛ
					if(obj is RouteList rl) {
						if(HasChanges) {
							if(AskQuestion("Сохранить маршрутный лист перед открытием?")) {
								UoW.Save(rl);
								SaveRouteList(rl);
							} else
								return;
						}
						gtkTabsOpener.OpenRouteListCreateDlg(this, rl.Id);
					}
				},
				i => true
			);
		}

		#endregion OpenOrderOrRouteListCommand

		#region AddDriverCommand

		public DelegateCommand AddDriverCommand { get; private set; }
		void CreateAddDriverCommand()
		{
			AddDriverCommand = new DelegateCommand(
				() =>
				{
					var drvJournalViewModel = _employeeJournalFactory.CreateWorkingDriverEmployeeJournal();
					drvJournalViewModel.SelectionMode = JournalSelectionMode.Multiple;
					drvJournalViewModel.TabName = "Водители";
					
					drvJournalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNodes = e.SelectedNodes;
						var onlyNew = selectedNodes.Where(x => ObservableDriversOnDay.All(y => y.Employee.Id != x.Id)).ToList();
						var allCars = CarRepository.GetCarsByDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());

						foreach(var n in selectedNodes) {
							var drv = UoW.GetById<Employee>(n.Id);

							if(ObservableDriversOnDay.Any(x => x.Employee.Id == n.Id)) {
								logger.Warn($"Водитель {drv.ShortName} уже добавлен. Пропускаем...");
								continue;
							}

							var daySchedule = GetDriverWorkDaySchedule(drv);

							var driver = new AtWorkDriver(
									drv,
									DateForRouting,
									allCars.FirstOrDefault(x => x.Driver.Id == n.Id),
									daySchedule
								);

							if(driver.Employee.DefaultForwarder != null) {
								var forwarder = observableForwardersOnDay.FirstOrDefault(x => x.Employee.Id == driver.Employee.DefaultForwarder.Id);

								if(forwarder == null) {
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
		void CreateRemoveDriverCommand()
		{
			RemoveDriverCommand = new DelegateCommand<AtWorkDriver[]>(
				driversToDel => {
					if(driversToDel == null)
						driversToDel = SelectedDrivers;
					foreach(var driver in driversToDel) {
						if(driver.Id > 0)
							UoW.Delete(driver);
						ObservableDriversOnDay.Remove(driver);
					}
				},
				driversToDel => {
					if(driversToDel == null)
						driversToDel = SelectedDrivers;
					return driversToDel != null && driversToDel.Any();
				}
			);
		}

		#endregion RemoveDriverCommand

		#region AddForwarderCommand

		public DelegateCommand AddForwarderCommand { get; private set; }
		void CreateAddForwarderCommand()
		{
			AddForwarderCommand = new DelegateCommand(
				() =>
				{
					var fwdJournalViewModel = _employeeJournalFactory.CreateWorkingForwarderEmployeeJournal();
					fwdJournalViewModel.SelectionMode = JournalSelectionMode.Multiple;
					
					fwdJournalViewModel.OnEntitySelectedResult += (sender, e) => {
						var selectedNodes = e.SelectedNodes;
						foreach(var n in selectedNodes) {
							var fwd = UoW.GetById<Employee>(n.Id);
							if(ObservableForwardersOnDay.Any(x => x.Employee.Id == n.Id)) {
								logger.Warn($"Экспедитор {fwd.ShortName} пропущен так как уже присутствует в списке.");
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
		void CreateRemoveForwarderCommand()
		{
			RemoveForwarderCommand = new DelegateCommand<AtWorkForwarder[]>(
				forwardersToDel => {
					foreach(var forwarder in forwardersToDel) {
						if(forwarder.Id > 0)
							UoW.Delete(forwarder);
						ObservableForwardersOnDay.Remove(forwarder);
					}
				},
				forwardersToDel => forwardersToDel != null && forwardersToDel.Any()
			);
		}

		#endregion RemoveForwarderCommand

		#region RebuilOneRouteCommand

		public DelegateCommand<object> RebuilOneRouteCommand { get; private set; }
		void CreateRebuilOneRouteCommand()
		{
			RebuilOneRouteCommand = new DelegateCommand<object>(
				obj => {
					RouteList route = obj is RouteListItem routeListItem ? routeListItem.RouteList : obj as RouteList;

					var newRoute = Optimizer.RebuidOneRoute(route);
					if(newRoute != null) {
						newRoute.UpdateAddressOrderInRealRoute(route);
						route.RecalculatePlanedDistance(DistanceCalculator);
					} else
						ShowErrorMessage("Решение не найдено.");
				},
				obj => obj != null
			);
		}

		#endregion RebuilOneRouteCommand

		#region ShowWarningsCommand

		public DelegateCommand ShowWarningsCommand { get; private set; }
		void CreateShowWarningsCommand()
		{
			ShowWarningsCommand = new DelegateCommand(
				() => ShowWarningMessage(string.Join("\n", Optimizer.WarningMessages.Select(x => "⚠ " + x))),
				() => true
			);
		}

		#endregion ShowWarningsCommand

		public override string TabName {
			get => string.Format("Формирование МЛ на {0:d}", DateForRouting);
			set => throw new InvalidOperationException("Установка протеворечит логике работы.");
		}

		#region Поля
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#endregion

		#region Свойства

		public IList<RouteList> RoutesOnDay { get; set; }

		public IList<UndeliveryOrderNode> UndeliveredOrdersOnDay { get; set; }

		public IList<Order> OrdersOnDay { get; set; }

		public GenericObservableList<GeographicGroupNode> GeographicGroupNodes { get; private set; }

		public RouteGeometryCalculator DistanceCalculator { get; } = new RouteGeometryCalculator();

		Employee driverFromRouteList;
		public virtual Employee DriverFromRouteList {
			get => driverFromRouteList;
			set => SetField(ref driverFromRouteList, value);
		}

		AtWorkDriver[] selectedDrivers;
		[PropertyChangedAlso(nameof(AreDriversSelected))]
		public virtual AtWorkDriver[] SelectedDrivers {
			get => selectedDrivers;
			set => SetField(ref selectedDrivers, value);
		}

		AtWorkForwarder selectedForwarder;
		[PropertyChangedAlso(nameof(IsForwarderSelected))]
		public virtual AtWorkForwarder SelectedForwarder {
			get => selectedForwarder;
			set => SetField(ref selectedForwarder, value);
		}

		DateTime dateForRouting = DateTime.Today;
		public DateTime DateForRouting {
			get => dateForRouting;
			set {
				if(SetField(ref dateForRouting, value))
					OnTabNameChanged();
			}
		}

		public bool AreDriversSelected => SelectedDrivers != null && SelectedDrivers.Any();

		public bool IsForwarderSelected => SelectedForwarder != null;

		public bool HasChanges => !HasNoChanges;

		bool hasNoChanges = true;
		public bool HasNoChanges {
			get => hasNoChanges;
			set => SetField(ref hasNoChanges, value);
		}

		bool autoroutingMode;
		public virtual bool IsAutoroutingModeActive {
			get => autoroutingMode;
			set => SetField(ref autoroutingMode, value);
		}

		IList<AtWorkForwarder> forwardersOnDay = new List<AtWorkForwarder>();
		public virtual IList<AtWorkForwarder> ForwardersOnDay {
			get => forwardersOnDay;
			set => SetField(ref forwardersOnDay, value);
		}

		GenericObservableList<AtWorkForwarder> observableForwardersOnDay;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkForwarder> ObservableForwardersOnDay {
			get {
				if(observableForwardersOnDay == null)
					observableForwardersOnDay = new GenericObservableList<AtWorkForwarder>(ForwardersOnDay);
				return observableForwardersOnDay;
			}
		}

		IList<AtWorkDriver> driversOnDay = new List<AtWorkDriver>();
		public virtual IList<AtWorkDriver> DriversOnDay {
			get => driversOnDay;
			set => SetField(ref driversOnDay, value);
		}

		GenericObservableList<AtWorkDriver> observableDriversOnDay;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkDriver> ObservableDriversOnDay {
			get {
				if(observableDriversOnDay == null)
					observableDriversOnDay = new GenericObservableList<AtWorkDriver>(DriversOnDay);
				return observableDriversOnDay;
			}
		}

		RouteOptimizer optimizer;
		public virtual RouteOptimizer Optimizer {
			get => optimizer;
			set => SetField(ref optimizer, value);
		}

		IList<District> logisticanDistricts = new List<District>();
		public virtual IList<District> LogisticanDistricts {
			get => logisticanDistricts;
			set => SetField(ref logisticanDistricts, value);
		}

		GenericObservableList<District> observableLogisticanDistricts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<District> ObservableLogisticanDistricts {
			get {
				if(observableLogisticanDistricts == null)
					observableLogisticanDistricts = new GenericObservableList<District>(LogisticanDistricts);
				return observableLogisticanDistricts;
			}
		}

		bool showCompleted;
		public virtual bool ShowCompleted {
			get => showCompleted;
			set => SetField(ref showCompleted, value);
		}

		bool showOnlyDriverOrders;
		public virtual bool ShowOnlyDriverOrders {
			get => showOnlyDriverOrders;
			set => SetField(ref showOnlyDriverOrders, value);
		}

		int minBottles19L;
		public virtual int MinBottles19L {
			get => minBottles19L;
			set => SetField(ref minBottles19L, value);
		}

		string canTake;
		public virtual string CanTake {
			get => canTake;
			set => SetField(ref canTake, value);
		}

		TimeSpan deliveryFromTime = TimeSpan.Parse("00:00:00");
		public virtual TimeSpan DeliveryFromTime {
			get => deliveryFromTime;
			set => SetField(ref deliveryFromTime, value);
		}

		TimeSpan deliveryToTime = TimeSpan.Parse("23:59:59");
		public virtual TimeSpan DeliveryToTime {
			get => deliveryToTime;
			set => SetField(ref deliveryToTime, value);
		}

		TimeSpan driverStartTime = TimeSpan.Parse("00:00:00");
		public virtual TimeSpan DriverStartTime {
			get => driverStartTime;
			set => SetField(ref driverStartTime, value);
		}

		TimeSpan driverEndTime = TimeSpan.Parse("23:59:59");
		public virtual TimeSpan DriverEndTime {
			get => driverEndTime;
			set => SetField(ref driverEndTime, value);
		}

		DeliveryScheduleFilterType deliveryScheduleType = DeliveryScheduleFilterType.DeliveryStart;
		public virtual DeliveryScheduleFilterType DeliveryScheduleType {
			get => deliveryScheduleType;
			set => SetField(ref deliveryScheduleType, value);
		}

		public virtual GenericObservableList<Subdivision> ObservableSubdivisions { get; set; }

		private IList<DeliverySummary> deliverySummary = new List<DeliverySummary>();
		public virtual IList<DeliverySummary> DeliverySummary {
			get => deliverySummary;
			set => SetField(ref deliverySummary, value);
		}
		
		private GenericObservableList<DeliverySummary> observableDeliverySummary;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliverySummary> ObservableDeliverySummary {
			get {
				if(observableDeliverySummary == null)
					observableDeliverySummary = new GenericObservableList<DeliverySummary>(DeliverySummary);
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
			var currentUserSettings = userRepository.GetUserSettings(UoW, CommonServices.UserService.CurrentUserId);
			foreach(var addressTypeNode in OrderAddressTypes) {
				switch(addressTypeNode.OrderAddressType) {
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

			return string.Format(
				"Первый адрес: {0:t}\nПуть со склада: {1:N1} км. ({2} мин.)\nВыезд со склада: {3:t}\nПогрузка на складе: {4} минут",
				routeList.FirstAddressTime,
				firstDP != null && geoGroupVersion  != null ? DistanceCalculator.DistanceFromBaseMeter(geoGroupVersion, firstDP) * 0.001 : 0,
				firstDP != null && geoGroupVersion != null ? DistanceCalculator.TimeFromBase(geoGroupVersion, firstDP) / 60 : 0,
				routeList.OnLoadTimeEnd,
				routeList.TimeOnLoadMinuts
			);
		}

		public string GetRowTitle(object row)
		{
			if(row is RouteList rl) {
				return string.Format("МЛ №{0} - {1}({2})",
					rl.Id,
					rl.Driver.ShortName,
					rl.Car.RegistrationNumber
				);
			}
			if(row is RouteListItem rli)
				return rli.Order.DeliveryPoint.ShortAddress;
			return null;
		}

		public string GetRowTime(object row)
		{
			if(row is RouteList rl)
				return FormatOccupancy(rl.Addresses.Count, rl.Driver.MinRouteAddresses, rl.Driver.MaxRouteAddresses);
			return (row as RouteListItem)?.Order.DeliverySchedule.Name;
		}

		public string GetRowOnloadTime(object row)
		{
			if(row is RouteList rl && rl.OnLoadTimeStart.HasValue) {
				if(rl.OnloadTimeFixed)
					return string.Format("<span foreground=\"Turquoise\">{0:hh\\:mm}</span>", rl.OnLoadTimeStart.Value);
				return rl.OnLoadTimeStart.Value.ToString("hh\\:mm");
			}
			return null;
		}

		public string GetRowPlanTime(object row)
		{
			if(row is RouteList rl)
				return string.Format("{0:hh\\:mm}-{1:hh\\:mm}",
									 rl.Addresses.FirstOrDefault()?.PlanTimeStart,
									 rl.Addresses.LastOrDefault()?.PlanTimeStart);

			if(row is RouteListItem rli) {
				string color;
				if(rli.PlanTimeStart == null || rli.PlanTimeEnd == null)
					color = "grey";
				else if(rli.PlanTimeEnd.Value + TimeSpan.FromSeconds(rli.TimeOnPoint) > rli.Order.DeliverySchedule.To)
					color = "red";
				else if(rli.PlanTimeStart.Value < rli.Order.DeliverySchedule.From)
					color = "blue";
				else if(rli.PlanTimeEnd.Value == rli.PlanTimeStart.Value)
					color = "dark red";
				else if(rli.PlanTimeEnd.Value - rli.PlanTimeStart.Value <= new TimeSpan(0, 30, 0))
					color = "orange";
				else
					color = "dark green";

				return string.Format("<span foreground=\"{2}\">{0:hh\\:mm}-{1:hh\\:mm}</span> ({3} мин.)",
									 rli.PlanTimeStart, rli.PlanTimeEnd, color, rli.TimeOnPoint / 60);
			}

			return null;
		}

		public string GetRowBottles(object row)
		{
			if(row is RouteList rl) {
				var bottles = rl.Addresses.Sum(x => x.Order.Total19LBottlesToDeliver);
				return FormatOccupancy(bottles, rl.Car.MinBottles, rl.Car.MaxBottles);
			}

			if(row is RouteListItem rli)
				return rli.Order.Total19LBottlesToDeliver.ToString();
			return null;
		}

		public string GetRowBottlesSix(object row)
		{
			if(row is RouteList rl)
				return rl.Addresses.Sum(x => x.Order.Total6LBottlesToDeliver).ToString();

			if(row is RouteListItem rli)
				return rli.Order.Total6LBottlesToDeliver.ToString();
			return null;
		}

		public string GetRowBottlesSmall(object row)
		{
			if(row is RouteList rl)
				return rl.Addresses.Sum(x => x.Order.Total600mlBottlesToDeliver).ToString();

			if(row is RouteListItem rli)
				return rli.Order.Total600mlBottlesToDeliver.ToString();
			return null;
		}

		public string GetRowWeight(object row)
		{
			if(row is RouteList rl) {
				var weight = rl.Addresses.Sum(x => x.Order.TotalWeight);
				return FormatOccupancy(weight, null, rl.Car.CarModel.MaxWeight);
			}

			if(row is RouteListItem rli)
				return rli.Order.TotalWeight.ToString();
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
			if(row is RouteList rl) {
				var volume = rl.Addresses.Sum(x => x.Order.TotalVolume);
				return FormatOccupancy(volume, null, rl.Car.CarModel.MaxVolume);
			}

			if(row is RouteListItem rli)
				return rli.Order.TotalVolume.ToString();
			return null;
		}

		public string GetRowDistance(object row)
		{
			if(row is RouteList rl) {
				var proposed = Optimizer.ProposedRoutes.FirstOrDefault(x => x.RealRoute == rl);
				if(rl.PlanedDistance == null)
					return string.Empty;
				if(proposed == null)
					return string.Format("{0:N1}км", rl.PlanedDistance);
				else
					return string.Format("{0:N1}км ({1:N})",
										 rl.PlanedDistance,
										 (double)proposed.RouteCost / 1000);
			}

			if(row is RouteListItem rli) {
				if(rli.IndexInRoute == 0) {
					var geoGroup = rli.RouteList.GeographicGroups.FirstOrDefault();
					var geoGroupVersion = geoGroup.GetVersionOrNull(rli.RouteList.Date);
					if(geoGroupVersion == null) 
					{
						return null;
					}
					return string.Format("{0:N1}км", (double)DistanceCalculator.DistanceFromBaseMeter(geoGroupVersion, rli.Order.DeliveryPoint) / 1000);
				}

				return string.Format("{0:N1}км", (double)DistanceCalculator.DistanceMeter(rli.RouteList.Addresses[rli.IndexInRoute - 1].Order.DeliveryPoint, rli.Order.DeliveryPoint) / 1000);
			}
			return null;
		}

		public string GetRowEquipmentFromClient(object row)
		{
			if(row is RouteListItem rli) {
				return rli.Order.FromClientText;
			}
			return null;
		}

		public string GetRowEquipmentToClient(object row)
		{
			string nomenclatureName = null;
			if(row is RouteListItem rli) {
				foreach(var orderItem in rli.Order.OrderItems) {
					if(orderItem.Nomenclature.Category == NomenclatureCategory.equipment || orderItem.Nomenclature.Category == NomenclatureCategory.additional)
						nomenclatureName += " " + orderItem.Nomenclature.Name;
				}
				return rli.Order.EquipmentsToClient + nomenclatureName;
			}
			return null;
		}

		string FormatOccupancy(int val, int? min, int? max)
		{
			string color = "green";
			if(val > max)
				color = "red";
			if(val < min)
				color = "blue";

			if(min.HasValue && max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2}-{3})", color, val, min, max);
			if(max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2})", color, val, max);
			return string.Format("<span foreground=\"{0}\">{1}</span>(min {2})", color, val, min);
		}

		string FormatOccupancy(decimal val, decimal? min, decimal? max)
		{
			string color = "green";
			if(val > max)
				color = "red";
			if(val < min)
				color = "blue";

			if(min.HasValue && max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2}-{3})", color, val, min, max);
			if(max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2})", color, val, max);
			return string.Format("<span foreground=\"{0}\">{1}</span>(min {2})", color, val, min);
		}

		PointMarkerType[] pointMarkers = {
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
				index = RoutesOnDay.IndexOf(rl);
			if(row is RouteListItem rli)
				index = RoutesOnDay.IndexOf(rli.RouteList);
			if(index < 0 || index >= maxLen)
				index = 0;
			return index;
		}

		public PointMarkerShape GetMarkerShape(object row)
		{
			PointMarkerShape shape = PointMarkerShape.circle;
			if(row is RouteList rl)
				shape = GetMarkerShapeFromBottleQuantity(rl.TotalFullBottlesToClient);
			if(row is RouteListItem rli)
				shape = GetMarkerShapeFromBottleQuantity(rli.GetFullBottlesToDeliverCount());
			return shape;
		}

		public PointMarkerShape GetMarkerShapeFromBottleQuantity(int bottlesCount, bool overdueOrder = false)
		{
			if(overdueOrder)
			{
				if(bottlesCount < 6)
					return PointMarkerShape.overduetriangle;
				if(bottlesCount < 10)
					return PointMarkerShape.overduecircle;
				if(bottlesCount < 20)
					return PointMarkerShape.overduesquare;
				if(bottlesCount < 40)
					return PointMarkerShape.overduecross;
				return PointMarkerShape.overduestar;
			}
			if(bottlesCount < 6)
				return PointMarkerShape.triangle;
			if(bottlesCount < 10)
				return PointMarkerShape.circle;
			if(bottlesCount < 20)
				return PointMarkerShape.square;
			if(bottlesCount < 40)
				return PointMarkerShape.cross;
			return PointMarkerShape.star;
		}

		public string GetOrdersInfo()
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			int totalOrders = OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true)
											 .GetExecutableQueryOver(UoW.Session)
											 .Select(Projections.Count<Order>(x => x.Id))
											 .Where(o => !o.IsContractCloser)
											 .And(o => o.OrderAddressType != OrderAddressType.Service)
											 .SingleOrDefault<int>();

			decimal totalBottles = OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true)
											  .GetExecutableQueryOver(UoW.Session)
											  .JoinAlias(o => o.OrderItems, () => orderItemAlias)
											  .JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
											  .Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
											  .Select(Projections.Sum(() => orderItemAlias.Count))
											  .Where(o => !o.IsContractCloser)
											  .And(o => o.OrderAddressType != OrderAddressType.Service)
											  .SingleOrDefault<decimal>();
												
			decimal total6LBottles = OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true)
											  .GetExecutableQueryOver(UoW.Session)
											  .JoinAlias(o => o.OrderItems, () => orderItemAlias)
											  .JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
											  .Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol6L)
											  .Select(Projections.Sum(() => orderItemAlias.Count))
											  .Where(o => !o.IsContractCloser)
											  .And(o => o.OrderAddressType != OrderAddressType.Service)
											  .SingleOrDefault<decimal>();

			decimal total600mlBottles = OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true)
											  .GetExecutableQueryOver(UoW.Session)
											  .JoinAlias(o => o.OrderItems, () => orderItemAlias)
											  .JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
											  .Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol600ml)
											  .Select(Projections.Sum(() => orderItemAlias.Count))
											  .Where(o => !o.IsContractCloser)
											  .And(o => o.OrderAddressType != OrderAddressType.Service)
											  .SingleOrDefault<decimal>();

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

			if(drivers.Count > 0) {
				foreach(var driver in drivers) {
					var car = CarRepository.GetCarByDriver(UoW, driver);

					if(car != null)
						totalBottles += car.MaxBottles;

					totalAddresses += driver.MaxRouteAddresses;
				}
			}

			var text = new List<string> { "Можем вывезти:", $"Бутылей - {totalBottles}", $"Адресов - {totalAddresses}" };

			CanTake = string.Join("\n", text);
		}

		public bool CheckAlreadyAddedAddress(Order order)
		{
			var routeList = routeListRepository.GetActualRouteListByOrder(UoW, order);
			if(routeList != null)
				ShowWarningMessage($"Адрес ({order.DeliveryPoint.CompiledAddress}) уже был кем-то добавлен в МЛ ({routeList.Id}). Обновите данные.");
			return routeList == null;
		}

		public bool CheckRouteListWasChanged(RouteList routeList)
		{
			if(!routeListRepository.RouteListWasChanged(routeList))
				return true;
			ShowWarningMessage($"МЛ ({routeList.Id}) уже был кем-то изменен. Обновите данные.");
			return false;
		}

		public void RecalculateOnLoadTime()
		{
			//FIXME Проверять что все МЛ присутствуют
			RouteList.RecalculateOnLoadTime(RoutesOnDay, DistanceCalculator);
		}

		public bool AddOrdersToRouteList(IList<Order> selectedOrders, RouteList routeList)
		{
			bool recalculateLoading = false;

			if(IsAutoroutingModeActive) {
				foreach(var order in selectedOrders) {
					if(order.OrderStatus == OrderStatus.InTravelList) {
						var alreadyIn = RoutesOnDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));
						if(alreadyIn == null)
							throw new InvalidProgramException(string.Format("Маршрутный лист, в котором добавлен заказ {0} не найден.", order.Id));
						if(alreadyIn.Id == routeList.Id) // Уже в нужном маршрутном листе.
							continue;
						var toRemoveAddress = alreadyIn.Addresses.First(x => x.Order.Id == order.Id);
						if(toRemoveAddress.IndexInRoute == 0)
							recalculateLoading = true;
						alreadyIn.RemoveAddress(toRemoveAddress);
						UoW.Save(alreadyIn);
					}
					var item = routeList.AddAddressFromOrder(order);
					if(item.IndexInRoute == 0)
						recalculateLoading = true;
				}
				routeList.RecalculatePlanTime(DistanceCalculator);
				routeList.RecalculatePlanedDistance(DistanceCalculator);
				UoW.Save(routeList);
			} else {
				foreach(var order in selectedOrders) {
					if(!CheckAlreadyAddedAddress(order))
						return false;

					var item = routeList.AddAddressFromOrder(order);
					if(item.IndexInRoute == 0)
						recalculateLoading = true;
				}
				if(!CheckRouteListWasChanged(routeList))
					return false;

				routeList.RecalculatePlanTime(DistanceCalculator);
				routeList.RecalculatePlanedDistance(DistanceCalculator);
				SaveRouteList(routeList);
			}
			logger.Info("В МЛ №{0} добавлено {1} адресов.", routeList.Id, selectedOrders.Count);
			if(recalculateLoading)
				RecalculateOnLoadTime();

			bool overweight = routeList.HasOverweight();
			bool volExcess = routeList.HasVolumeExecess();

			if(overweight || volExcess) {
				StringBuilder warningMsg = new StringBuilder(string.Format("Автомобиль '{0}' в МЛ №{1}:", routeList.Car.Title, routeList.Id));
				if(overweight)
					warningMsg.Append(string.Format("\n\t- перегружен на {0} кг", routeList.Overweight()));
				if(volExcess)
					warningMsg.Append(string.Format("\n\t- объём груза превышен на {0} м<sup>3</sup>", routeList.VolumeExecess()));
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
			UoW.Save(routeList);
			UoW.Commit();
			HasNoChanges = true;
		}

		public void RebuildAllRoutes(Action<string> actionUpdateInfo = null)
		{
			int ix = 0;
			List<string> warnings = new List<string>();
			Optimizer.StatisticsTxtAction = null;

			foreach(var route in RoutesOnDay) {
				ix++;
				actionUpdateInfo?.Invoke($"Строим {ix} из {RoutesOnDay.Count}");

				var newRoute = Optimizer.RebuidOneRoute(route);
				if(newRoute != null) {
					newRoute.UpdateAddressOrderInRealRoute(route);
					route.RecalculatePlanedDistance(DistanceCalculator);
					var noPlan = route.Addresses.Count(x => !x.PlanTimeStart.HasValue);
					if(noPlan > 0)
						warnings.Add($"Для маршрута №{route.Id} - {route.Driver?.ShortName}({route.Car?.RegistrationNumber}) незапланировано {noPlan} адресов.");
				} else {
					warnings.Add($"Маршрут {route.Id} не был перестроен.");
				}
			}
			if(warnings.Any())
				ShowWarningMessage(string.Join("\n", warnings));
		}

		public void InitializeData()
		{
			UoW.Dispose();
			CreateUoW();

			if(OrdersOnDay == null)
			{
				OrdersOnDay = new List<Order>();
			}
			else
			{
				OrdersOnDay.Clear();
			}

			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			GeoGroup geographicGroupAlias = null;
			Counterparty counterpartyAlias = null;
			Order orderBaseAlias = null;

			var selectedGeographicGroup = GeographicGroupNodes.Where(x => x.Selected).Select(x => x.GeographicGroup);

			if(OrderAddressTypes.Any(x => x.Selected))
			{
				var query = QueryOver.Of(() => orderBaseAlias)
					.Where(order => order.DeliveryDate == DateForRouting.Date && !order.SelfDelivery)
					.Where(o => o.DeliverySchedule != null)
					.Where(x => x.DeliveryPoint != null)
					.And(x => x.DeliverySchedule.Id != closingDocumentDeliveryScheduleId);

				if(!ShowCompleted)
					query.Where(order => order.OrderStatus == OrderStatus.Accepted || order.OrderStatus == OrderStatus.InTravelList);
				else
					query.Where(order =>
						order.OrderStatus != OrderStatus.Canceled && order.OrderStatus != OrderStatus.NewOrder &&
						order.OrderStatus != OrderStatus.WaitForPayment);

				var baseOrderQuery = query.GetExecutableQueryOver(UoW.Session);

				AddAddressTypeFilter(baseOrderQuery);

				if(selectedGeographicGroup.Any())
				{
					baseOrderQuery.Left.JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
						.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
						.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
						.Where(Restrictions.In(Projections.Property(() => geographicGroupAlias.Id),
							selectedGeographicGroup.Select(x => x.Id).ToArray()));
				}

				var ordersQuery = baseOrderQuery.Fetch(SelectMode.Fetch, x => x.DeliveryPoint).Future()
					.Where(x => x.IsContractCloser == false)
					.Where(x => !OrderRepository.IsOrderCloseWithoutDelivery(UoW, x));

				baseOrderQuery.Fetch(SelectMode.Fetch, x => x.OrderItems).Future();

				switch(DeliveryScheduleType)
				{
					case DeliveryScheduleFilterType.DeliveryStart:
						OrdersOnDay = ordersQuery.Where(x => x.DeliveryPoint.CoordinatesExist)
								.Where(x => x.DeliverySchedule.From >= DeliveryFromTime)
								.Where(x => x.DeliverySchedule.From <= DeliveryToTime)
								.Where(o => o.Total19LBottlesToDeliver >= MinBottles19L)
								.Distinct().ToList()
							;
						break;
					case DeliveryScheduleFilterType.DeliveryEnd:
						OrdersOnDay = ordersQuery.Where(x => x.DeliveryPoint.CoordinatesExist)
								.Where(x => x.DeliverySchedule.To >= DeliveryFromTime)
								.Where(x => x.DeliverySchedule.To <= DeliveryToTime)
								.Where(o => o.Total19LBottlesToDeliver >= MinBottles19L)
								.Distinct().ToList()
							;
						break;
					case DeliveryScheduleFilterType.DeliveryStartAndEnd:
						OrdersOnDay = ordersQuery.Where(x => x.DeliveryPoint.CoordinatesExist)
								.Where(x => x.DeliverySchedule.To >= DeliveryFromTime)
								.Where(x => x.DeliverySchedule.To <= DeliveryToTime)
								.Where(x => x.DeliverySchedule.From >= DeliveryFromTime)
								.Where(x => x.DeliverySchedule.From <= DeliveryToTime)
								.Where(o => o.Total19LBottlesToDeliver >= MinBottles19L)
								.Distinct().ToList()
							;
						break;
					case DeliveryScheduleFilterType.OrderCreateDate:
						OrdersOnDay = ordersQuery.Where(x => x.DeliveryPoint.CoordinatesExist)
								.Where(x => x.CreateDate.Value.TimeOfDay >= DeliveryFromTime)
								.Where(x => x.CreateDate.Value.TimeOfDay <= DeliveryToTime)
								.Where(o => o.Total19LBottlesToDeliver >= MinBottles19L)
								.Distinct().ToList()
							;
						break;
				}


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
					.And(() => orderAlias2.DeliverySchedule.Id != closingDocumentDeliveryScheduleId)
					.GetExecutableQueryOver(UoW.Session)
					.SelectList(list => list
						.Select(x=>x.GuiltySide).WithAlias(() => resultAlias.GuiltySide)
						.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OldOrderId)
						.Select(() => orderAlias2.Id).WithAlias(() => resultAlias.NewOrderId)
						.Select(() => orderAlias2.DeliveryPoint).WithAlias(() => resultAlias.DeliveryPoint)
						.Select(() => orderAlias2.BottlesReturn).WithAlias(() => resultAlias.Bottles))
					.TransformUsing(Transformers.AliasToBean<UndeliveryOrderNode>()).List<UndeliveryOrderNode>();
			}

			logger.Info("Загружаем МЛ на {0:d}...", DateForRouting);

			var routesQuery1 = routeListRepository.GetRoutesAtDay(DateForRouting)
				.GetExecutableQueryOver(UoW.Session);
			if(!ShowCompleted)
				routesQuery1.Where(x => x.Status == RouteListStatus.New);
			GeoGroup routeGeographicGroupAlias = null;
			if(selectedGeographicGroup.Any())
			{
				routesQuery1
					.Left.JoinAlias(x => x.GeographicGroups, () => routeGeographicGroupAlias)
					.Where(Restrictions.In(Projections.Property(() => routeGeographicGroupAlias.Id),
						selectedGeographicGroup.Select(x => x.Id).ToArray()));
			}

			var selectedDeliveryShifts = DeliveryShiftNodes.Where(x => x.Selected).Select(x => x.DeliveryShift).ToArray();

			if(selectedDeliveryShifts.Any())
			{
				routesQuery1.WhereRestrictionOn(rl => rl.Shift).IsIn(selectedDeliveryShifts);
			}

			var routesQuery = routesQuery1
				.Fetch(SelectMode.Undefined, x => x.Addresses)
				.Future();

			GetWorkDriversInfo();
			CalculateOnDeliverySum();
			RoutesOnDay = routesQuery.ToList();
			RoutesOnDay.ToList().ForEach(rl => rl.UoW = UoW);
			//Нужно для того чтобы диалог не падал при загрузке если присутствую поломаные МЛ.
			RoutesOnDay.ToList().ForEach(rl => rl.CheckAddressOrder());

			logger.Info("Загружаем водителей на {0:d}...", DateForRouting);
			ObservableDriversOnDay.Clear();
			atWorkRepository.GetDriversAtDay(UoW, DateForRouting, driverStatuses: new [] { AtWorkDriver.DriverStatus.IsWorking }).ToList().ForEach(x => ObservableDriversOnDay.Add(x));
			logger.Info("Загружаем экспедиторов на {0:d}...", DateForRouting);
			ObservableForwardersOnDay.Clear();
			atWorkRepository.GetForwardersAtDay(UoW, DateForRouting).ToList().ForEach(x => ObservableForwardersOnDay.Add(x));
		}

		public string GetOrdersInfo(int addressesWithoutCoordinats, int addressesWithoutRoutes, int totalBottlesCountAtDay, int bottlesWithoutRL)
		{
			var text = new List<string> {
				NumberToTextRus.FormatCase(OrdersOnDay.Count, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов.")
			};
			if(addressesWithoutCoordinats > 0)
				text.Add(string.Format("Из них {0} без координат.", addressesWithoutCoordinats));
			if(addressesWithoutRoutes > 0)
				text.Add(string.Format("Из них {0} без маршрутных листов.", addressesWithoutRoutes));
			if(totalBottlesCountAtDay > 0)
				text.Add(NumberToTextRus.FormatCase(totalBottlesCountAtDay, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));
			if(bottlesWithoutRL > 0)
				text.Add(NumberToTextRus.FormatCase(bottlesWithoutRL, "Осталась {0} бутыль", "Осталось {0} бутыли", "Осталось {0} бутылей"));

			text.Add(NumberToTextRus.FormatCase(RoutesOnDay.Count, "Всего {0} маршрутный лист.", "Всего {0} маршрутных листа.", "Всего {0} маршрутных листов."));

			return string.Join("\n", text);
		}

		public bool CreateRoutesAutomatically(Action<string> statisticsUpdateAction)
		{
			if(DriversOnDay.Any(d => d.Car != null && d.GeographicGroup == null)) {
				ShowWarningMessage("Не всем автомобилям назначена \"База\" для погрузки-разгрузки. Пожалуйста укажите.");
				return false;
			}

			Optimizer.UoW = UoW;
			Optimizer.Routes = RoutesOnDay;
			Optimizer.Orders = OrdersOnDay;
			Optimizer.Drivers = DriversOnDay;
			Optimizer.Forwarders = ForwardersOnDay;
			Optimizer.StatisticsTxtAction = statisticsUpdateAction;
			Optimizer.CreateRoutes(DateForRouting, DriverStartTime, DriverEndTime);

			if(optimizer.ProposedRoutes.Any()) {
				//Удаляем корректно адреса из уже имеющихся МЛ. Чтобы они встали в правильный статус.
				foreach(var route in RoutesOnDay.Where(x => x.Id > 0)) {
					foreach(var odrer in route.Addresses.ToList()) {
						route.RemoveAddress(odrer);
					}
				}

				foreach(var propose in optimizer.ProposedRoutes) {
					var rl = propose.Trip.OldRoute ?? new RouteList();
					rl.UoW = UoW;
					rl.Car = propose.Trip.Car;
					rl.Driver = propose.Trip.Driver;
					rl.Shift = propose.Trip.Shift;
					rl.Date = DateForRouting;
					rl.Logistician = VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);

					if(propose.Trip.OldRoute == null) {
						rl.GeographicGroups.Clear();
						rl.GeographicGroups.Add(propose.Trip.GeographicGroup);
					}

					foreach(var order in propose.Orders) {
						var address = rl.AddAddressFromOrder(order.Order);
						address.PlanTimeStart = order.ProposedTimeStart;
						address.PlanTimeEnd = order.ProposedTimeEnd;
					}
					if(propose.Trip.OldRoute == null) // Это новый маршрут и его нужно добавить.
						RoutesOnDay.Add(rl);
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
			if(
				string.IsNullOrEmpty(driverNames) || AskQuestion(
					string.Format(
						"Автомобиль \"{0}\" уже назначен \"{1}\". Переназначить его водителю \"{2}\"?",
						car.RegistrationNumber,
						driverNames,
						driver.Employee.ShortName
					)
				)
			)
			{
				DriversOnDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x =>
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
				? defaultDeliveryDaySchedule
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

			var baseQuery = OrderRepository.GetOrdersForRLEditingQuery(DateForRouting, true, orderBaseAlias)
				.GetExecutableQueryOver(UoW.Session)
				.Where(o => !o.IsContractCloser)
				.And(o => o.OrderAddressType != OrderAddressType.Service);
			if(OrderAddressTypes.Any(x => x.Selected))
			{
				AddAddressTypeFilter(baseQuery);

				var selectedGeographicGroup = GeographicGroupNodes.Where(x => x.Selected).Select(x => x.GeographicGroup);
				
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
					.Select(o=>o.OrderStatus).WithAlias(() => ordersCountNode.OrderStatus)
					.Select(o=>o.Id).WithAlias(() => ordersCountNode.Id)
				).TransformUsing(Transformers.AliasToBean<OrdersCountNode>()).List<OrdersCountNode>().GroupBy(o=>o.OrderStatus);
			
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

			var totalCancellations = new DeliverySummary {Name = "Итого отмены"};
			var totalNotRL = new DeliverySummary {Name = "Итого не в МЛ"};
			var totalInRL = new DeliverySummary {Name = "Итого в МЛ"};
			var totalOnTheWay = new DeliverySummary {Name = "Итого в пути"};
			var totalCompleted = new DeliverySummary {Name = "Итого выполнено"};
			
			foreach (var orderGroup in deliverySummaryNodes.GroupBy(o=>o.OrderStatus))
			{
				var addressCount = ordersCount.Where(x => x.Key == orderGroup.Key).Sum(x => x.Select(y => y).Count());
				var deliverySum = new DeliverySummary(orderGroup.Key.GetEnumTitle(), addressCount, orderGroup.Select(x => x).ToList());
				ObservableDeliverySummary.Add(deliverySum);
				switch (orderGroup.Key)
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
				Name = "Итого не уехало", Bottles = totalNotRL.Bottles + totalInRL.Bottles,
				AddressCount = totalNotRL.AddressCount + totalInRL.AddressCount
			};
			var totalLeft = new DeliverySummary
			{
				Name = "Итого уехало", Bottles = totalCompleted.Bottles + totalOnTheWay.Bottles,
				AddressCount = totalCompleted.AddressCount + totalOnTheWay.AddressCount
			};
			var totalForDay = new DeliverySummary
			{
				Name = "Итого за день", Bottles = totalNoAway.Bottles + totalLeft.Bottles + totalCancellations.Bottles,
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
	}
}

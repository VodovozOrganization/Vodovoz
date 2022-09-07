using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gamma.Widgets;
using Gdk;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using Gtk;
using NHibernate;
using NHibernate.Criterion;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Utilities;
using QSOrmProject;
using QSWidgetLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Tools.Logistic;
using Vodovoz.ViewModels.Logistic;
using Order = Vodovoz.Domain.Orders.Order;
using QS.Project.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Gamma.Binding.Core.LevelTreeConfig;

namespace Vodovoz
{
	[Obsolete ("Этот класс подлежит удалению, используйте RouteListOnDay") ]
	public partial class RoutesAtDayDlg : SingleUowDialogBase
	{
		#region Поля
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();		
		private readonly ISubdivisionRepository _subdivisionRepository = new SubdivisionRepository(new ParametersProvider());
		private readonly ICarRepository _carRepository = new CarRepository();
		private readonly IGeographicGroupRepository _geographicGroupRepository = new GeographicGroupRepository();
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository = new ScheduleRestrictionRepository();
		private readonly IRouteListRepository _routeListRepository =
			new RouteListRepository(new StockRepository(), new BaseParametersProvider(new ParametersProvider()));
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		readonly GMapOverlay districtsOverlay = new GMapOverlay("districts");
		readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		readonly GMapOverlay selectionOverlay = new GMapOverlay("selection");
		readonly GMapOverlay routeOverlay = new GMapOverlay("route");
		GMapPolygon brokenSelection;
		List<GMapMarker> selectedMarkers = new List<GMapMarker>();
		IList<Order> ordersAtDay;
		IList<RouteList> routesAtDay;
		IList<AtWorkDriver> driversAtDay;
		IList<AtWorkForwarder> forwardersAtDay;
		IList<District> logisticanDistricts;
		RouteOptimizer optimizer = new RouteOptimizer(ServicesConfig.InteractiveService);
		RouteGeometryCalculator distanceCalculator = new RouteGeometryCalculator();

		GenericObservableList<AtWorkDriver> observableDriversAtDay;
		GenericObservableList<AtWorkForwarder> observableForwardersAtDay;

		private readonly EmployeeFilterViewModel _forwardersFilter;
		private readonly EmployeeFilterViewModel _driversFilter;
		private readonly IEmployeeJournalFactory _employeeJournalFactory = new EmployeeJournalFactory();

		int addressesWithoutCoordinats, addressesWithoutRoutes, totalBottlesCountAtDay, bottlesWithoutRL;
		Pixbuf[] pixbufMarkers;
		#endregion

		#region Свойства
		private bool hasNoChanges;

		public bool HasNoChanges {
			get => hasNoChanges;
			private set {
				hasNoChanges = value;

				ydateForRoutes.Sensitive = checkShowCompleted.Sensitive = ytimeToDeliveryFrom.Sensitive = ytimeToDeliveryTo.Sensitive = ySpnMin19Btls.Sensitive
					= hasNoChanges;
			}
		}

		DateTime CurDate => ydateForRoutes.Date;

		IList<AtWorkForwarder> ForwardersAtDay {
			set {
				forwardersAtDay = value;
				observableForwardersAtDay = new GenericObservableList<AtWorkForwarder>(forwardersAtDay);
				ytreeviewOnDayForwarders.SetItemsSource(observableForwardersAtDay);
			}
			get => forwardersAtDay;
		}

		private IList<AtWorkDriver> DriversAtDay {
			set {
				driversAtDay = value;
				observableDriversAtDay = new GenericObservableList<AtWorkDriver>(driversAtDay);
				ytreeviewOnDayDrivers.SetItemsSource(observableDriversAtDay);
			}
			get => driversAtDay;
		}

		#endregion

		public override string TabName {
			get => string.Format("Формирование МЛ на {0:d}", ydateForRoutes.Date);
			protected set => throw new InvalidOperationException("Установка протеворечит логике работы.");
		}

		GenericObservableList<GeographicGroupNode> geographicGroupNodes;

		public RoutesAtDayDlg()
		{
			this.Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			Employee currentEmployee = VodovozGtkServicesConfig.EmployeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId);
			if(currentEmployee == null) {
				MessageDialogHelper.RunWarningDialog("Ваш пользователь не привязан к сотруднику, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}

			if(currentEmployee.Subdivision == null) {
				MessageDialogHelper.RunWarningDialog("У сотрудника не указано подразделение, продолжение работы невозможно");
				FailInitialize = true;
				return;
			}
			GeographicGroup employeeGeographicGroup = currentEmployee.Subdivision.GetGeographicGroup();

			ytreeviewGeographicGroup.ColumnsConfig = FluentColumnsConfig<GeographicGroupNode>.Create()
				.AddColumn("Выбрать").AddToggleRenderer(x => x.Selected).Editing()
				.AddColumn("Район города").AddTextRenderer(x => x.GeographicGroup.Name)
				.Finish();
			geographicGroupNodes = new GenericObservableList<GeographicGroupNode>(UoW.GetAll<GeographicGroup>().ToList().Select(x => new GeographicGroupNode(x)).ToList());

			if(employeeGeographicGroup != null) {
				var foundGeoGroup = geographicGroupNodes.FirstOrDefault(x => x.GeographicGroup.Id == employeeGeographicGroup.Id);
				if(foundGeoGroup != null) {
					foundGeoGroup.Selected = true;
				}
			}

			ytreeviewGeographicGroup.ItemsDataSource = geographicGroupNodes;

			if(progressOrders.Adjustment == null)
				progressOrders.Adjustment = new Adjustment(0, 0, 0, 1, 1, 0);

			//Configure map
			districtsOverlay.IsVisibile = false;
			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(districtsOverlay);
			gmapWidget.Overlays.Add(routeOverlay);
			gmapWidget.Overlays.Add(addressesOverlay);
			gmapWidget.Overlays.Add(selectionOverlay);
			gmapWidget.DisableAltForSelection = true;
			gmapWidget.OnSelectionChange += GmapWidget_OnSelectionChange;
			gmapWidget.ButtonPressEvent += GmapWidget_ButtonPressEvent;
			gmapWidget.ButtonReleaseEvent += GmapWidget_ButtonReleaseEvent;
			gmapWidget.MotionNotifyEvent += GmapWidget_MotionNotifyEvent;

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";

			LoadDistrictsGeometry();

			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<object>.Create()
															.AddColumn("Маркер")
																.AddPixbufRenderer(x => GetRowMarker(x))
															.AddColumn("МЛ/Адрес")
																.AddTextRenderer(x => GetRowTitle(x))
															.AddColumn("Адр./Время")
																.AddTextRenderer(x => GetRowTime(x), useMarkup: true)
															.AddColumn("План")
																.AddTextRenderer(x => GetRowPlanTime(x), useMarkup: true)
															.AddColumn("Бутылей")
																.AddTextRenderer(x => GetRowBottles(x), useMarkup: true)
															.AddColumn("Бут. 6л")
																.AddTextRenderer(x => GetRowBottlesSix(x))
															.AddColumn("Бут. менее 6л")
																.AddTextRenderer(x => GetRowBottlesSmall(x))
															.AddColumn("Вес")
																.AddTextRenderer(x => GetRowWeight(x), useMarkup: true)
															.AddColumn("Погрузка")
																.Tag(RouteColumnTag.OnloadTime)
																.AddTextRenderer(x => GetRowOnloadTime(x), useMarkup: true)
																	.AddSetter((c, n) => c.Editable = n is RouteList)
																	.EditedEvent(OnLoadTimeEdited)
															.AddColumn("Километраж")
																.AddTextRenderer(x => GetRowDistance(x))
															.AddColumn("К клиенту")
																.AddTextRenderer(x => GetRowEquipmentToClient(x))
															.AddColumn("От клиента")
																.AddTextRenderer(x => GetRowEquipmentFromClient(x))
															.Finish();

			ytreeRoutes.HasTooltip = true;
			ytreeRoutes.QueryTooltip += YtreeRoutes_QueryTooltip;
			ytreeRoutes.Selection.Changed += YtreeRoutes_Selection_Changed;

			var colorWhite = new Color(0xff, 0xff, 0xff);
			var colorLightRed = new Color(0xff, 0x66, 0x66);
			ytreeviewOnDayDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>.Create()
																		  .AddColumn("Водитель")
																		 	.AddTextRenderer(x => x.Employee.ShortName)
																		  .AddColumn("Автомобиль")
																			.AddPixbufRenderer(x => x.Car != null && x.Car.GetActiveCarVersionOnDate(x.Date).IsCompanyCar ? vodovozCarIcon : null)
																			.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
																		  .AddColumn("База")
																			.AddComboRenderer(x => x.GeographicGroup)
																			.SetDisplayFunc(x => x.Name)
																			.FillItems(_geographicGroupRepository.GeographicGroupsWithCoordinates(UoW))
																			.AddSetter(
																				(c, n) => {
																					c.Editable = n.Car != null;
																					c.BackgroundGdk = n.GeographicGroup == null && n.Car != null
																						? colorLightRed
																						: colorWhite;
																				}
																			)
																		  .AddColumn("")
																		  .Finish();
			ytreeviewOnDayDrivers.Selection.Mode = SelectionMode.Multiple;

			ytreeviewOnDayDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;

			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>.Create()
																				.AddColumn("Экспедитор")
																					 .AddTextRenderer(x => x.Employee.ShortName)
																				.Finish();
			ytreeviewOnDayForwarders.Selection.Mode = SelectionMode.Multiple;

			ytreeviewOnDayForwarders.Selection.Changed += YtreeviewForwarders_Selection_Changed;

			ytimeToDeliveryFrom.Time = TimeSpan.Parse("00:00:00");
			ytimeToDeliveryTo.Time = TimeSpan.Parse("23:59:00");
			ydateForRoutes.Date = DateTime.Today;

			yspinMaxTime.Binding.AddBinding(optimizer, e => e.MaxTimeSeconds, w => w.ValueAsInt);

			var subdivisions = _subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income) });
			if(!subdivisions.Any()) {
				MessageDialogHelper.RunErrorDialog("Не правильно сконфигурированы подразделения кассы, невозможно будет указать подразделение в которое будут сдаваться маршрутные листы");
				FailInitialize = true;
				return;
			}
			yspeccomboboxCashSubdivision.ShowSpecialStateNot = true;
			yspeccomboboxCashSubdivision.ItemsList = subdivisions;
			yspeccomboboxCashSubdivision.SelectedItem = SpecialComboState.Not;

			_forwardersFilter = new EmployeeFilterViewModel();
			_forwardersFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.forwarder,
				x => x.CanChangeStatus = true,
				x => x.Status = EmployeeStatus.IsWorking);

			_driversFilter = new EmployeeFilterViewModel();
			_driversFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.driver,
				x => x.CanChangeStatus = true,
				x => x.Status = EmployeeStatus.IsWorking);
		}

		private Subdivision ClosingSubdivision => yspeccomboboxCashSubdivision.SelectedItem as Subdivision;

		void GmapWidget_ButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
		{
			if(dragSelectionPointId != -1) {
				gmapWidget.DisableAltForSelection = true;
				OnPoligonSelectionUpdated();
				dragSelectionPointId = -1;
			}
		}

		void GmapWidget_MotionNotifyEvent(object o, MotionNotifyEventArgs args)
		{
			if(dragSelectionPointId > -1) {
				brokenSelection.Points[dragSelectionPointId] = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
				gmapWidget.Refresh();
			}
		}

		void YtreeRoutes_QueryTooltip(object o, QueryTooltipArgs args)
		{
			ytreeRoutes.ConvertWidgetToBinWindowCoords(args.X, args.Y, out int binX, out int binY);

			if(ytreeRoutes.GetPathAtPos(binX, binY, out TreePath path, out TreeViewColumn col) && ytreeRoutes.Model.GetIter(out TreeIter iter, path)) {
				var loadtimeCol = ytreeRoutes.ColumnsConfig.GetColumnsByTag(RouteColumnTag.OnloadTime).Where(x => x == col).ToArray();
				if(loadtimeCol.Any() && ytreeRoutes.YTreeModel.NodeFromIter(iter) is RouteList node) {
					var firstDP = node.Addresses.FirstOrDefault()?.Order.DeliveryPoint;
					args.RetVal = true;
					args.Tooltip.Text = string.Format(
											"Первый адрес: {0:t}\nПуть со склада: {1:N1} км. ({2} мин.)\nВыезд со склада: {3:t}\nПогрузка на складе: {4} минут",
											node.FirstAddressTime,
											firstDP != null ? distanceCalculator.DistanceFromBaseMeter(node.GeographicGroups.FirstOrDefault(), firstDP) * 0.001 : 0,
											firstDP != null ? distanceCalculator.TimeFromBase(node.GeographicGroups.FirstOrDefault(), firstDP) / 60 : 0,
											node.OnLoadTimeEnd,
											node.TimeOnLoadMinuts
										);
				}
			}
		}

		bool poligonSelection;
		int dragSelectionPointId = -1;

		void GmapWidget_ButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1) {
				bool markerIsSelect = false;
				//if(args.Event.State.HasFlag(ModifierType.Mod1Mask)) {
				if(args.Event.State.HasFlag(ModifierType.LockMask)) {
					foreach(var marker in addressesOverlay.Markers) {
						if(marker.IsMouseOver) {
							var markerUnderMouse = selectedMarkers.FirstOrDefault(m => ((Order)m.Tag).Id == ((Order)marker.Tag).Id);
							if(markerUnderMouse == null) {
								selectedMarkers.Add(marker);
								logger.Debug("Маркер с заказом №{0} добавлен в список выделенных", ((Order)marker.Tag).Id);
							} else {
								selectedMarkers.Remove(markerUnderMouse);
								logger.Debug("Маркер с заказом №{0} исключен из списка выделенных", ((Order)marker.Tag).Id);
							}
							markerIsSelect = true;
						}
					}
					//Требуется просмотреть код, для возможного улучшения
					UpdateSelectedInfo(selectedMarkers);
					UpdateAddressesOnMap();
					return;
				}
				if(!markerIsSelect) {
					selectedMarkers.Clear();
					logger.Debug("Список выделенных маркеров очищен");
				}
				UpdateAddressesOnMap();

				if(poligonSelection) {
					GRect rect = new GRect((long)args.Event.X - 5, (long)args.Event.Y - 5, 10, 10);
					rect.OffsetNegative(gmapWidget.RenderOffset);

					dragSelectionPointId = brokenSelection.LocalPoints.FindIndex(rect.Contains);
					if(dragSelectionPointId != -1) {
						gmapWidget.DisableAltForSelection = false;
						return;
					}
				}

				if(args.Event.State.HasFlag(ModifierType.ControlMask)) {
					if(!poligonSelection) {
						poligonSelection = true;
						logger.Debug("Старт выделения через полигон.");
						var startPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						brokenSelection = new GMapPolygon(new List<PointLatLng> { startPoint }, "Выделение");
						gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
						selectionOverlay.Polygons.Add(brokenSelection);
					} else {
						logger.Debug("Продолжили.");
						var newPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						brokenSelection.Points.Add(newPoint);
						gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
					}
					OnPoligonSelectionUpdated();
				} else {
					logger.Debug("Закончили.");
					poligonSelection = false;
					UpdateSelectedInfo(new List<GMapMarker>());
					selectionOverlay.Clear();
				}
			}

			if(args.Event.Button == 3 && addressesOverlay.Markers.FirstOrDefault(m => m.IsMouseOver)?.Tag is Order order) {
				Menu popupMenu = new Menu();
				var item = new MenuItem(string.Format("Открыть {0}", order));
				item.Activated += (sender, e) => {
					var dlg = new OrderDlg(order) {
						HasChanges = false
					};
					dlg.SetDlgToReadOnly();
					OpenSlaveTab(dlg);
				};
				popupMenu.Add(item);
				popupMenu.ShowAll();
				popupMenu.Popup();
			}
		}

		void OnPoligonSelectionUpdated()
		{
			var selected = addressesOverlay.Markers.Where(m => brokenSelection.IsInside(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		void YtreeRoutes_Selection_Changed(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			buttonRemoveAddress.Sensitive = row is RouteListItem && !checkShowCompleted.Active;
			buttonOpen.Sensitive = buttonRebuildRoute.Sensitive = (row is RouteListItem) || (row is RouteList);

			//Рисуем выделенный маршрут
			routeOverlay.Clear();
			if(row != null) {
				if(!(row is RouteList rl))
					rl = (row as RouteListItem).RouteList;

				MapDrawingHelper.DrawRoute(routeOverlay, rl, distanceCalculator);

				//Если выбран адрес, центруем на него карту.
				if(row is RouteListItem rli) {
					gmapWidget.Position = rli.Order.DeliveryPoint.GmapPoint;
				}
			}
			logger.Info("Ok");
		}

		void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveDriver.Sensitive = buttonDriverSelectAuto.Sensitive = ytreeviewOnDayDrivers.Selection.CountSelectedRows() > 0;
		}

		void YtreeviewForwarders_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveForwarder.Sensitive = ytreeviewOnDayForwarders.Selection.CountSelectedRows() > 0;
		}

		void GmapWidget_OnSelectionChange(RectLatLng Selection, bool ZoomToFit)
		{
			if(poligonSelection)
				return;
			var selected = addressesOverlay.Markers.Where(m => Selection.Contains(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		void UpdateSelectedInfo(List<GMapMarker> selected)
		{
			var orders = selected.Select(x => x.Tag).OfType<Order>();
			if(!orders.Any()) {
				labelSelected.Markup = "Адресов\nне выбрано";
				menuAddToRL.Sensitive = false;
				return;
			}
			var selectedBottle = orders.Sum(o => o.Total19LBottlesToDeliver);
			var selectedKilos = orders.Sum(o => o.TotalWeight);
			var selectedCbm = orders.Sum(o => o.TotalVolume);
			labelSelected.Markup = string.Format(
				"{0} адр.; {1} бут.;\n{2} кг; {3} м<sup>3</sup>",
				orders.Count(),
				selectedBottle,
				selectedKilos,
				selectedCbm
			);
			menuAddToRL.Sensitive = selected.Any() && routesAtDay.Any() && !checkShowCompleted.Active;
		}

		Pixbuf vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		string GetRowTitle(object row)
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

		string GetRowTime(object row)
		{
			if(row is RouteList rl)
				return FormatOccupancy(rl.Addresses.Count, rl.Driver.MinRouteAddresses, rl.Driver.MaxRouteAddresses);
			return (row as RouteListItem)?.Order.DeliverySchedule.Name;
		}

		string GetRowOnloadTime(object row)
		{
			if(row is RouteList rl && rl.OnLoadTimeStart.HasValue) {
				if(rl.OnloadTimeFixed)
					return string.Format("<span foreground=\"Turquoise\">{0:hh\\:mm}</span>", rl.OnLoadTimeStart.Value);
				else
					return rl.OnLoadTimeStart.Value.ToString("hh\\:mm");
			}
			return null;
		}

		string GetRowPlanTime(object row)
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

		string GetRowBottles(object row)
		{
			if(row is RouteList rl) {
				var bottles = rl.Addresses.Sum(x => x.Order.Total19LBottlesToDeliver);
				return FormatOccupancy(bottles, rl.Car.MinBottles, rl.Car.MaxBottles);
			}

			if(row is RouteListItem rli)
				return rli.Order.Total19LBottlesToDeliver.ToString();
			return null;
		}

		string GetRowBottlesSix(object row)
		{
			if(row is RouteList rl)
				return rl.Addresses.Sum(x => x.Order.Total6LBottlesToDeliver).ToString();

			if(row is RouteListItem rli)
				return rli.Order.Total6LBottlesToDeliver.ToString();
			return null;
		}

		string GetRowBottlesSmall(object row)
		{
			if(row is RouteList rl)
				return rl.Addresses.Sum(x => x.Order.Total600mlBottlesToDeliver).ToString();

			if(row is RouteListItem rli)
				return rli.Order.Total600mlBottlesToDeliver.ToString();
			return null;
		}

		string GetRowWeight(object row)
		{
			if(row is RouteList rl) {
				var weight = rl.Addresses.Sum(x => x.Order.TotalWeight);
				return FormatOccupancy(weight, null, rl.Car.CarModel.MaxWeight);
			}

			if(row is RouteListItem rli)
				return rli.Order.TotalWeight.ToString();
			return null;
		}

		string GetRowDistance(object row)
		{
			if(row is RouteList rl) {
				var proposed = optimizer.ProposedRoutes.FirstOrDefault(x => x.RealRoute == rl);
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
				if(rli.IndexInRoute == 0)
					return string.Format("{0:N1}км", (double)distanceCalculator.DistanceFromBaseMeter(rli.RouteList.GeographicGroups.FirstOrDefault(), rli.Order.DeliveryPoint) / 1000);

				return string.Format("{0:N1}км", (double)distanceCalculator.DistanceMeter(rli.RouteList.Addresses[rli.IndexInRoute - 1].Order.DeliveryPoint, rli.Order.DeliveryPoint) / 1000);
			}
			return null;
		}

		string GetRowEquipmentFromClient(object row)
		{
			if(row is RouteListItem rli) {
				return rli.Order.FromClientText;
			}
			return null;
		}

		string GetRowEquipmentToClient(object row)
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
			string color;
			if(val > max)
				color = "red";
			else if(val < min)
				color = "blue";
			else
				color = "green";

			if(min.HasValue && max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2}-{3})", color, val, min, max);
			else if(max.HasValue)
				return string.Format("<span foreground=\"{0}\">{1}</span>({2})", color, val, max);
			else
				return string.Format("<span foreground=\"{0}\">{1}</span>(min {2})", color, val, min);
		}

		Pixbuf GetRowMarker(object row)
		{
			PointMarkerShape shape = PointMarkerShape.circle;
			int index = 0;
			if(row is RouteList rl) {
				index = routesAtDay.IndexOf(rl);
				shape = GetMarkerShape(rl.TotalFullBottlesToClient);
			}
			if(row is RouteListItem rli) {
				index = routesAtDay.IndexOf(rli.RouteList);
				shape = GetMarkerShape(rli.GetFullBottlesToDeliverCount());
			}

			if(index < 0 || index >= pixbufMarkers.Length)
				index = 0;

			return PointMarker.GetIconPixbuf(GetAddressMarker(index).ToString(), shape);
		}

		private void FillFullOrdersInfo(IOrderRepository orderRepository)
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			int totalOrders = orderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, true)
				.GetExecutableQueryOver(UoW.Session)
				.Select(Projections.Count<Order>(x => x.Id)).SingleOrDefault<int>();

			int totalBottles = orderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, true)
				.GetExecutableQueryOver(UoW.Session)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(Projections.Sum(() => orderItemAlias.Count)).SingleOrDefault<int>();

			var text = new List<string> {
				NumberToTextRus.FormatCase(totalOrders, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов."),
				NumberToTextRus.FormatCase(totalBottles, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей")
			};

			ytextFullOrdersInfo.Buffer.Text = string.Join("\n", text);
		}

		void FillDialogAtDay()
		{
			logger.Info("Загружаем заказы на {0:d}...", ydateForRoutes.Date);
			UoW.Session.Clear();

			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			GeographicGroup geographicGroupAlias = null;

			var baseOrderQuery = _orderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(UoW.Session);
			var selectedGeographicGroup = geographicGroupNodes.Where(x => x.Selected).Select(x => x.GeographicGroup);
			if(selectedGeographicGroup.Any()) {
				baseOrderQuery
				.Left.JoinAlias(x => x.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => districtAlias.GeographicGroup, () => geographicGroupAlias)
				.Where(Restrictions.In(Projections.Property(() => geographicGroupAlias.Id), selectedGeographicGroup.Select(x => x.Id).ToArray()));
			}

			var ordersQuery = baseOrderQuery.Fetch(SelectMode.Fetch, x => x.DeliveryPoint)
				.Future();

			_orderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(UoW.Session)
				.Fetch(SelectMode.Fetch, x => x.OrderItems)
				.Future();
			var withoutTime = ordersQuery.Where(x => x.DeliverySchedule == null).ToList();
			var withoutLocation = ordersQuery.Where(x => x.DeliveryPoint == null || !x.DeliveryPoint.CoordinatesExist).ToList();
			if(withoutTime.Any() || withoutLocation.Any())
				MessageDialogHelper.RunWarningDialog("Не все заказы были загружены!" +
													(withoutTime.Any() ? ("\n* У заказов отсутсвует время доставки: " + string.Join(", ", withoutTime.Select(x => x.Id.ToString()))) : "") +
													(withoutLocation.Any() ? ("\n* У заказов отсутствуют координаты: " + string.Join(", ", withoutLocation.Select(x => x.Id.ToString()))) : "")
												   );

			int.TryParse(ySpnMin19Btls.Text, out int minBtls);
			ordersAtDay = ordersQuery.Where(x => x.DeliverySchedule != null)
									 .Where(x => x.DeliverySchedule.From >= ytimeToDeliveryFrom.Time)
									 .Where(x => x.DeliverySchedule.From <= ytimeToDeliveryTo.Time)
									 .Where(x => x.DeliveryPoint != null)
									 .Where(o => o.Total19LBottlesToDeliver >= minBtls)
									 .Where(x => !x.IsContractCloser)
									 .Where(x => !_orderRepository.IsOrderCloseWithoutDelivery(UoW, x))
									 .ToList()
									 ;

			var outLogisticAreas = ordersAtDay
				.Where(
					x => !logisticanDistricts.Any(
						a => x.DeliveryPoint.NetTopologyPoint != null && a.DistrictBorder.Contains(x.DeliveryPoint.NetTopologyPoint)
					)
				)
				.ToList();
			if(outLogisticAreas.Any())
				MessageDialogHelper.RunWarningDialog("Обратите внимание, координаты точек доставки для следущих заказов не попадают ни в один логистический район: "
													+ string.Join(", ", outLogisticAreas.Select(x => x.Id.ToString())));

			logger.Info("Загружаем МЛ на {0:d}...", ydateForRoutes.Date);

			var routesQuery1 = _routeListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(UoW.Session);
			if(!checkShowCompleted.Active)
				routesQuery1.Where(x => x.Status == RouteListStatus.New);
			GeographicGroup routeGeographicGroupAlias = null;
			if(selectedGeographicGroup.Any()) {
				routesQuery1
				.Left.JoinAlias(x => x.GeographicGroups, () => routeGeographicGroupAlias)
				.Where(Restrictions.In(Projections.Property(() => routeGeographicGroupAlias.Id), selectedGeographicGroup.Select(x => x.Id).ToArray()));
			}
			var routesQuery = routesQuery1
				.Fetch(SelectMode.Undefined, x => x.Addresses)
				.Future();

			routesAtDay = routesQuery.ToList();
			routesAtDay.ToList().ForEach(rl => rl.UoW = UoW);
			//Нужно для того чтобы диалог не падал при загрузке если присутствую поломаные МЛ.
			routesAtDay.ToList().ForEach(rl => rl.CheckAddressOrder());

			UpdateRoutesPixBuf();
			UpdateRoutesButton();

			logger.Info("Загружаем водителей на {0:d}...", ydateForRoutes.Date);
			DriversAtDay = new AtWorkRepository().GetDriversAtDay(UoW, ydateForRoutes.Date, AtWorkDriver.DriverStatus.IsWorking);

			logger.Info("Загружаем экспедиторов на {0:d}...", ydateForRoutes.Date);
			ForwardersAtDay = new AtWorkRepository().GetForwardersAtDay(UoW, ydateForRoutes.Date);

			UpdateAddressesOnMap();

			var levels = LevelConfigFactory.FirstLevel<RouteList, RouteListItem>(x => x.Addresses).LastLevel(c => c.RouteList).EndConfig();
			ytreeRoutes.YTreeModel = new LevelTreeModel<RouteList>(routesAtDay, levels);
		}

		void UpdateAddressesOnMap()
		{
			logger.Info("Обновляем адреса на карте...");
			addressesWithoutCoordinats = 0;
			addressesWithoutRoutes = 0;
			totalBottlesCountAtDay = 0;
			bottlesWithoutRL = 0;
			addressesOverlay.Clear();
			//добавляем маркеры складов
			foreach(var b in _geographicGroupRepository.GeographicGroupsWithCoordinates(UoW)) {
				var addressMarker = new PointMarker(
					new PointLatLng(
						(double)b.BaseLatitude,
						(double)b.BaseLongitude
					),
					PointMarkerType.vodonos,
					PointMarkerShape.custom
				) {
					Tag = b
				};
				addressesOverlay.Markers.Add(addressMarker);
			}

			//добавляем маркеры адресов заказов
			foreach(var order in ordersAtDay.Select(x => x).Where(x => x.OrderAddressType != OrderAddressType.Service)) {
				totalBottlesCountAtDay += order.Total19LBottlesToDeliver;
				var route = routesAtDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));

				if(route == null) {
					addressesWithoutRoutes++;
					bottlesWithoutRL += order.Total19LBottlesToDeliver;
				}

				if(order.DeliveryPoint.Latitude.HasValue && order.DeliveryPoint.Longitude.HasValue) {
					PointMarkerShape shape = GetMarkerShape(order.Total19LBottlesToDeliver);

					PointMarkerType type = PointMarkerType.black;
					if(route == null) {
						if((order.DeliverySchedule.To - order.DeliverySchedule.From).TotalHours <= 1)
							type = PointMarkerType.black_and_red;
						else {
							double from = order.DeliverySchedule.From.TotalMinutes;
							double to = order.DeliverySchedule.To.TotalMinutes;
							if(from >= 1080 && to <= 1439)//>= 18:00, <= 23:59
								type = PointMarkerType.grey_stripes;
							else if(from >= 0) {
								if(to <= 720)//<= 12:00
									type = PointMarkerType.red_stripes;
								else if(to <= 900)//<=15:00
									type = PointMarkerType.yellow_stripes;
								else if(to <= 1080)//<= 18:00
									type = PointMarkerType.green_stripes;
							}
						}
					} else
						type = GetAddressMarker(routesAtDay.IndexOf(route));

					if(selectedMarkers.FirstOrDefault(m => ((Order)m.Tag).Id == order.Id) != null)
						type = PointMarkerType.white;

					var addressMarker = new PointMarker(new PointLatLng((double)order.DeliveryPoint.Latitude, (double)order.DeliveryPoint.Longitude), type, shape) {
						Tag = order
					};

					string ttText = order.DeliveryPoint.ShortAddress;
					if(order.Total19LBottlesToDeliver > 0)
						ttText += string.Format("\nБутылей 19л: {0}", order.Total19LBottlesToDeliver);
					if(order.Total6LBottlesToDeliver > 0)
						ttText += string.Format("\nБутылей 6л: {0}", order.Total6LBottlesToDeliver);
					if(order.Total600mlBottlesToDeliver > 0)
						ttText += string.Format("\nБутылей 0,6л: {0}", order.Total600mlBottlesToDeliver);

					ttText += string.Format("\nВремя доставки: {0}\nРайон: {1}",
						order.DeliverySchedule?.Name ?? "Не назначено",
						logisticanDistricts?.FirstOrDefault(x => x.DistrictBorder.Contains(order.DeliveryPoint.NetTopologyPoint))?.DistrictName);

					addressMarker.ToolTipText = ttText;

					var identicalPoint = addressesOverlay.Markers.Count(g => g.Position.Lat == (double)order.DeliveryPoint.Latitude && g.Position.Lng == (double)order.DeliveryPoint.Longitude);
					var pointShift = 5;
					if(identicalPoint >= 1) {
						addressMarker.Offset = new System.Drawing.Point(identicalPoint * pointShift, identicalPoint * pointShift);
					}

					if(route != null)
						addressMarker.ToolTipText += string.Format(" Везёт: {0}", route.Driver.ShortName);

					addressesOverlay.Markers.Add(addressMarker);

				} else
					addressesWithoutCoordinats++;
			}

			UpdateOrdersInfo();
			logger.Info("Ок.");
		}

		private void Refresh()
		{
			FillDialogAtDay();
			FillFullOrdersInfo(_orderRepository);
			OnTabNameChanged();
		}

		protected void OnYdateForRoutesDateChanged(object sender, EventArgs e)
		{
			Refresh();
		}

		protected void OnYenumcomboMapTypeChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		void UpdateOrdersInfo()
		{
			var text = new List<string> {
				NumberToTextRus.FormatCase(ordersAtDay.Count, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов.")
			};
			if(addressesWithoutCoordinats > 0)
				text.Add(string.Format("Из них {0} без координат.", addressesWithoutCoordinats));
			if(addressesWithoutRoutes > 0)
				text.Add(string.Format("Из них {0} без маршрутных листов.", addressesWithoutRoutes));
			if(totalBottlesCountAtDay > 0)
				text.Add(NumberToTextRus.FormatCase(totalBottlesCountAtDay, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));
			if(bottlesWithoutRL > 0)
				text.Add(NumberToTextRus.FormatCase(bottlesWithoutRL, "Осталась {0} бутыль", "Осталось {0} бутыли", "Осталось {0} бутылей"));

			text.Add(NumberToTextRus.FormatCase(routesAtDay.Count, "Всего {0} маршрутный лист.", "Всего {0} маршрутных листа.", "Всего {0} маршрутных листов."));

			textOrdersInfo.Buffer.Text = string.Join("\n", text);

			if(progressOrders.Adjustment != null) {
				progressOrders.Adjustment.Upper = ordersAtDay.Count;
				progressOrders.Adjustment.Value = ordersAtDay.Count - addressesWithoutRoutes;
			}
			if(!ordersAtDay.Any())
				progressOrders.Text = string.Empty;
			else if(addressesWithoutRoutes == 0)
				progressOrders.Text = "Готово.";
			else
				progressOrders.Text = NumberToTextRus.FormatCase(addressesWithoutRoutes, "Остался {0} заказ", "Осталось {0} заказа", "Осталось {0} заказов");
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

		PointMarkerType GetAddressMarker(int routeNum)
		{
			var markerNum = routeNum % pointMarkers.Length;
			return pointMarkers[markerNum];
		}

		PointMarkerShape GetMarkerShape(int bottlesCount)
		{
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

		void UpdateRoutesPixBuf()
		{
			if(pixbufMarkers != null && routesAtDay.Count == pixbufMarkers.Length)
				return;
			pixbufMarkers = new Pixbuf[routesAtDay.Count];
			for(int i = 0; i < routesAtDay.Count; i++) {
				PointMarkerShape shape = GetMarkerShape(routesAtDay[i].TotalFullBottlesToClient);
				pixbufMarkers[i] = PointMarker.GetIconPixbuf(GetAddressMarker(i).ToString(), shape);
			}
		}

		void RoutesWasUpdated()
		{
			HasNoChanges = false;
			ytreeRoutes.YTreeModel.EmitModelChanged();
		}

		void UpdateRoutesButton()
		{
			var menu = new Menu();
			foreach(var route in routesAtDay) {
				var carrierInfo = string.Format("№{0} - {1}", route.Id, route.Driver.ShortName);
				if(route.GeographicGroups.Any())
					carrierInfo = string.Concat(carrierInfo, " (", route.GeographicGroups.FirstOrDefault().Name, ')');
				carrierInfo = string.Concat(
					carrierInfo,
					string.Format("; {0} кг; {1} куб.м.", route.Car?.CarModel?.MaxWeight, route.Car?.CarModel?.MaxVolume)
				);
				var item = new MenuItemId<RouteList>(carrierInfo) {
					ID = route
				};
				item.Activated += AddToRLItem_Activated;
				menu.Append(item);
			}
			menu.ShowAll();
			menuAddToRL.Menu = menu;
		}

		void AddToRLItem_Activated(object sender, EventArgs e)
		{
			if(yspeccomboboxCashSubdivision.IsSelectedNot || ClosingSubdivision == null) {
				MessageDialogHelper.RunWarningDialog("Необходимо выбрать кассу в которую должны будут сдаваться МЛ");
				return;
			}

			bool recalculateLoading = false;
			var selectedOrders = GetSelectedOrders();

			var route = ((MenuItemId<RouteList>)sender).ID;

			foreach(var order in selectedOrders) {
				if(!CheckAlreadyAddedAddress(order)) {
					return;
				}
				var item = route.AddAddressFromOrder(order);
				if(item.IndexInRoute == 0)
					recalculateLoading = true;
			}
			if(!CheckRouteListWasChanged(route)) {
				return;
			}
			try {
				route.RecalculatePlanTime(distanceCalculator);
				route.RecalculatePlanedDistance(distanceCalculator);
				SaveRouteList(route);
			} catch(Exception ex) {
				MessageDialogHelper.RunErrorDialog("Возникла ошибка при добавлении адресов, возможно из-за одновременного добавления одного адреса несколькими пользователями.\n" +
					"Данные для формирования будут автоматически обновлены для продолжения работы.\n" +
					"Повторите попытку добавления адресов.\n" +
					$"Текст ошибки: {ex.Message}", "Ошибка при добавлении адресов");
				Refresh();
				return;
			}

			logger.Info("В МЛ №{0} добавлено {1} адресов.", route.Id, selectedOrders.Count);
			if(recalculateLoading)
				RecalculateOnLoadTime();
			UpdateAddressesOnMap();
			RoutesWasUpdated();

			StringBuilder warningMsg = new StringBuilder(string.Format("Автомобиль '{0}' в МЛ №{1}:", route.Car.Title, route.Id));
			if(route.HasOverweight())
				warningMsg.Append(string.Format("\n\t- перегружен на {0} кг", route.Overweight()));
			if(route.HasVolumeExecess())
				warningMsg.Append(string.Format("\n\t- объём груза превышен на {0} м<sup>3</sup>", route.VolumeExecess()));

			if(route.HasOverweight() || route.HasVolumeExecess())
				MessageDialogHelper.RunWarningDialog(warningMsg.ToString());
		}

		void RecalculateOnLoadTime()
		{
			//FIXME Проверять что все МЛ присутствуют
			RouteList.RecalculateOnLoadTime(routesAtDay, distanceCalculator);
		}

		private IList<Order> GetSelectedOrders()
		{
			List<Order> orders = new List<Order>();
			//Добавление заказов из кликов по маркеру
			orders.AddRange(selectedMarkers.Select(m => m.Tag).OfType<Order>().ToList());
			//Добавление заказов из квадратного выделения
			orders.AddRange(addressesOverlay.Markers
				.Where(m => gmapWidget.SelectedArea.Contains(m.Position))
				.Select(x => x.Tag).OfType<Order>().ToList());
			//Добавление закзаво через непрямоугольную область
			GMapOverlay overlay = gmapWidget.Overlays.FirstOrDefault(o => o.Id.Contains(selectionOverlay.Id));
			GMapPolygon polygons = overlay?.Polygons.FirstOrDefault(p => p.Name.ToLower().Contains("выделение"));
			if(polygons != null) {
				var temp = addressesOverlay.Markers
					.Where(m => polygons.IsInside(m.Position))
					.Select(x => x.Tag).OfType<Order>().ToList();
				orders.AddRange(temp);
			}

			return orders;
		}

		protected void OnButtonRemoveAddressClicked(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject<RouteListItem>();
			var route = row.RouteList;
			route.RemoveAddress(row);
			if(!CheckRouteListWasChanged(route)) {
				return;
			}
			SaveRouteList(route);
			route.RecalculatePlanTime(distanceCalculator);
			route.RecalculatePlanedDistance(distanceCalculator);
			RoutesWasUpdated();
		}

		protected void OnCheckShowCompletedToggled(object sender, EventArgs e)
		{
			FillDialogAtDay();
		}

		#region TDIDialog

		public override bool Save()
		{
			return false;
		}

		private void SaveRouteList(RouteList routeList)
		{
			RebuildAllRoutes();
			UoW.Save(routeList);
			UoW.Commit();
			HasNoChanges = true;
		}

		public override bool HasChanges {
			get {
				return !HasNoChanges;
			}
		}

		private bool CheckAlreadyAddedAddress(Order order)
		{
			var routeList = _routeListRepository.GetActualRouteListByOrder(UoW, order);
			
			if(routeList == null)
			{
				return true;
			}
			
			MessageDialogHelper.RunWarningDialog($"Адрес ({order.DeliveryPoint.CompiledAddress}) уже был кем-то добавлен в МЛ ({routeList.Id}). Обновите данные.");
			return false;
		}

		private bool CheckRouteListWasChanged(RouteList routeList)
		{
			if(_routeListRepository.RouteListWasChanged(routeList))
			{
				MessageDialogHelper.RunWarningDialog($"МЛ ({routeList.Id}) уже был кем-то изменен. Обновите данные.");
				return false;
			}
			return true;
		}

		void RebuildAllRoutes()
		{
			int ix = 0;
			List<string> warnings = new List<string>();
			optimizer.StatisticsTxtAction = null;

			foreach(var route in routesAtDay) {
				ix++;
				textOrdersInfo.Buffer.Text = $"Строим {ix} из {routesAtDay.Count}";

				var newRoute = optimizer.RebuidOneRoute(route);
				if(newRoute != null) {
					newRoute.UpdateAddressOrderInRealRoute(route);
					route.RecalculatePlanedDistance(distanceCalculator);
					var noPlan = route.Addresses.Count(x => !x.PlanTimeStart.HasValue);
					if(noPlan > 0)
						warnings.Add($"Для маршрута №{route.Id} - {route.Driver?.ShortName}({route.Car?.RegistrationNumber}) незапланировано {noPlan} адресов.");
				} else {
					warnings.Add($"Маршрут {route.Id} не был перестроен.");
				}
			}
			if(warnings.Any())
				MessageDialogHelper.RunWarningDialog(string.Join("\n", warnings));
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			//Открываем заказ
			if(row is RouteListItem) {
				Order order = (row as RouteListItem).Order;
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Order>(order.Id),
					() => new OrderDlg(order)
				);
			}
			//Открываем МЛ
			if(row is RouteList) {
				RouteList routeList = row as RouteList;
				if(!HasNoChanges) {
					if(MessageDialogHelper.RunQuestionDialog("Сохранить маршрутный лист перед открытием?"))
						if(!Save()) {
							return;
						} else
							return;
				}
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id),
					() => new RouteListCreateDlg(routeList)
				);
			}
		}

		#endregion

		protected void OnButtonMapHelpClicked(object sender, EventArgs e)
		{
			new RouresAtDayInfoWnd().Show();
		}

		protected void FillItems()
		{
			if(CurDate != default(DateTime))
				FillDialogAtDay();
		}

		private void LoadDistrictsGeometry()
		{
			logger.Info("Загружаем районы...");
			districtsOverlay.Clear();
			logisticanDistricts = _scheduleRestrictionRepository.GetDistrictsWithBorder(UoW);
			foreach(var district in logisticanDistricts) {
				var poligon = new GMapPolygon(
					district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(),
					district.DistrictName
				);
				districtsOverlay.Polygons.Add(poligon);
			}
			logger.Info("Ок.");
		}

		protected void OnCheckShowDistrictsToggled(object sender, EventArgs e)
		{
			districtsOverlay.IsVisibile = checkShowDistricts.Active;
		}

		protected void OnButtonAddDriverClicked(object sender, EventArgs e)
		{
			var driversJournal = _employeeJournalFactory.CreateEmployeesJournal(_driversFilter);
			driversJournal.SelectionMode = JournalSelectionMode.Multiple;
			driversJournal.OnEntitySelectedResult += OnDriversSelected;
			TabParent.AddSlaveTab(this, driversJournal);
		}

		private void OnDriversSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var loaded = UoW.GetById<Employee>(e.SelectedNodes.Select(x => x.Id));
			logger.Info("Получаем авто для водителей...");
			var onlyNew = loaded.Where(x => driversAtDay.All(y => y.Employee.Id != x.Id)).ToList();
			var allCars = _carRepository.GetCarsByDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());

			foreach(var driver in loaded)
			{
				var driverToAdd = new AtWorkDriver(driver, CurDate, allCars.FirstOrDefault(x => x.Driver.Id == driver.Id));
				driversAtDay.Add(driverToAdd);
			}
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
			logger.Info("Ок");
		}

		protected void OnButtonAddForwarderClicked(object sender, EventArgs e)
		{
			var forwardersJournal = _employeeJournalFactory.CreateEmployeesJournal(_forwardersFilter);
			forwardersJournal.SelectionMode = JournalSelectionMode.Multiple;
			forwardersJournal.OnEntitySelectedResult += OnForwardersSelected;
			TabParent.AddSlaveTab(this, forwardersJournal);
		}

		private void OnForwardersSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var loaded = UoW.GetById<Employee>(e.SelectedNodes.Select(x => x.Id));
			foreach(var forwarder in loaded)
			{
				if(forwardersAtDay.Any(x => x.Employee.Id == forwarder.Id))
				{
					logger.Warn($"Экспедитор {forwarder.ShortName} пропущен так как уже присутствует в списке.");
					continue;
				}
				forwardersAtDay.Add(new AtWorkForwarder(forwarder, CurDate));
			}
			ForwardersAtDay = forwardersAtDay.OrderBy(x => x.Employee.ShortName).ToList();
		}

		protected void OnButtonRemoveDriverClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewOnDayDrivers.GetSelectedObjects<AtWorkDriver>();
			foreach(var driver in toDel) {
				if(driver.Id > 0)
					UoW.Delete(driver);
				observableDriversAtDay.Remove(driver);
			}
		}

		protected void OnButtonRemoveForwarderClicked(object sender, EventArgs e)
		{
			var toDel = ytreeviewOnDayForwarders.GetSelectedObjects<AtWorkForwarder>();
			foreach(var forwarder in toDel) {
				if(forwarder.Id > 0)
					UoW.Delete(forwarder);
				observableForwardersAtDay.Remove(forwarder);
			}
		}

		bool creatingInProgress;

		protected void OnButtonAutoCreateClicked(object sender, EventArgs e)
		{
			var logistican = _employeeRepository.GetEmployeeForCurrentUser(UoW);
			if(logistican == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				return;
			}

			if(DriversAtDay.Any(d => d.Car != null && d.GeographicGroup == null)) {
				MessageDialogHelper.RunWarningDialog("Не всем автомобилям назначена \"База\" для погрузки-разгрузки. Пожалуйста укажите.");
				return;
			}

			UpdateWarningButton();

			if(creatingInProgress) {
				buttonAutoCreate.Label = "Создать маршруты";
				return;
			}
			creatingInProgress = true;
			buttonAutoCreate.Label = "Остановить";

			optimizer.UoW = UoW;
			optimizer.Routes = routesAtDay;
			optimizer.Orders = ordersAtDay;
			optimizer.Drivers = driversAtDay;
			optimizer.Forwarders = forwardersAtDay;
			optimizer.StatisticsTxtAction = txt => textOrdersInfo.Buffer.Text = txt;
			//optimizer.CreateRoutes();

			if(optimizer.ProposedRoutes.Any()) {
				//Удаляем корректно адреса из уже имеющихся МЛ. Чтобы они встали в правильный статус.
				foreach(var route in routesAtDay.Where(x => x.Id > 0)) {
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
					rl.Date = CurDate;
					rl.Logistician = logistican;

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
						routesAtDay.Add(rl);
					propose.RealRoute = rl;
				}
			}
			UpdateRoutesPixBuf();
			UpdateRoutesButton();

			RecalculateOnLoadTime();
			UpdateAddressesOnMap();
			RoutesWasUpdated();
			UpdateWarningButton();
			creatingInProgress = false;
			buttonAutoCreate.Label = "Создать маршруты";
		}

		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			var driver = ytreeviewOnDayDrivers.GetSelectedObjects<AtWorkDriver>().FirstOrDefault();
			
			if(driver == null)
			{
				MessageDialogHelper.RunWarningDialog("Не выбран водитель!");
				return;
			}
			
			var filter = new CarJournalFilterViewModel(new CarModelJournalFactory());
			filter.SetAndRefilterAtOnce(
				x => x.Archive = false,
				x => x.RestrictedCarOwnTypes = new List<CarOwnType> { CarOwnType.Company }
			);
			var journal = new CarJournalViewModel(filter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices);
			journal.SelectionMode = JournalSelectionMode.Single;
			journal.OnEntitySelectedResult += (o, args) =>
			{
				var car = UoW.GetById<Car>(args.SelectedNodes.First().Id);
				SelectCarForDriver(driver, car);
			};
			TabParent.AddSlaveTab(this, journal);
		}

		private void SelectCarForDriver(AtWorkDriver driver, Car car)
		{
			var driverNames = string.Join("\", \"", driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).Select(x => x.Employee.ShortName));
			if(
				string.IsNullOrEmpty(driverNames) || MessageDialogHelper.RunQuestionDialog(
					string.Format(
						"Автомобиль \"{0}\" уже назначен \"{1}\". Переназначить его водителю \"{2}\"?",
						car.RegistrationNumber,
						driverNames,
						driver.Employee.ShortName
					)
				)
			)
			{
				driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x =>
				{
					x.Car = null; x.GeographicGroup = null;
				});
				driver.Car = car;
			}
		}

		void OnLoadTimeEdited(object o, EditedArgs args)
		{
			var routeList = (RouteList)ytreeRoutes.YTreeModel.NodeAtPath(new TreePath(args.Path));
			bool NeedRecalculate = false;

			if(string.IsNullOrWhiteSpace(args.NewText)) {
				NeedRecalculate = routeList.OnloadTimeFixed;
				routeList.OnloadTimeFixed = false;
			} else if(TimeSpan.TryParse(args.NewText, out TimeSpan fixedTime)) {
				if(fixedTime != routeList.OnLoadTimeStart)
					NeedRecalculate = true;
				routeList.OnloadTimeFixed = true;
				routeList.OnLoadTimeStart = fixedTime;
				routeList.OnLoadTimeEnd = fixedTime.Add(TimeSpan.FromMinutes(routeList.TimeOnLoadMinuts));
			}

			if(NeedRecalculate)
				RecalculateOnLoadTime();
		}

		protected void OnButtonRebuildRouteClicked(object sender, EventArgs e)
		{
			var selected = ytreeRoutes.GetSelectedObject();
			RouteList route = selected is RouteListItem ? ((RouteListItem)selected).RouteList : selected as RouteList;

			optimizer.StatisticsTxtAction = txt => textOrdersInfo.Buffer.Text = txt;
			var newRoute = optimizer.RebuidOneRoute(route);
			if(newRoute != null) {
				newRoute.UpdateAddressOrderInRealRoute(route);
				route.RecalculatePlanedDistance(distanceCalculator);
			} else
				MessageDialogHelper.RunErrorDialog("Решение не найдено.");
		}

		protected void OnButtonWarningsClicked(object sender, EventArgs e)
		{
			MessageDialogHelper.RunWarningDialog(
				string.Join("\n", optimizer.WarningMessages.Select(x => "⚠ " + x))
			);
		}

		private void UpdateWarningButton()
		{
			buttonWarnings.Visible = optimizer.WarningMessages.Any();
			buttonWarnings.Label = optimizer.WarningMessages.Count.ToString();
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			FillItems();
		}

		protected void OnFilterWidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)) {
					FillItems();
				}
			}
		}

		protected void OnLabel2WidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.ButtonPress && (args.Event as EventButton).Button == 1) {
				var user = _employeeRepository.GetEmployeeForCurrentUser(UoW);
				if(user.User.Id != 94)
					return;

				TabParent.AddSlaveTab(this, new OptimizingParametersDlg());
			}
		}

		protected void OnYtreeviewOnDayDriversRowActivated(object o, RowActivatedArgs args)
		{
			OnButtonDriverSelectAutoClicked(o, args);
		}

		public override void Destroy()
		{
			distanceCalculator?.Dispose();
			base.Destroy();
		}

		protected void OnBtnRefreshClicked(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}

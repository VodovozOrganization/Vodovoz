using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using Gtk;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Utilities;
using QSOrmProject;
using QSWidgetLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Logistic.RouteOptimization;
using Vodovoz.Dialogs.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository;
using Vodovoz.Repository.Logistics;
using Vodovoz.Tools.Logistic;

namespace Vodovoz
{
	public partial class RoutesAtDayDlg : SingleUowDialogBase
	{
		#region Поля
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly GMapOverlay districtsOverlay = new GMapOverlay("districts");
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		private readonly GMapOverlay selectionOverlay = new GMapOverlay("selection");
		private readonly GMapOverlay routeOverlay = new GMapOverlay("route");
		private GMapPolygon brokenSelection;
		private List<GMapMarker> selectedMarkers = new List<GMapMarker>();
		IList<Domain.Orders.Order> ordersAtDay;
		IList<RouteList> routesAtDay;
		IList<AtWorkDriver> driversAtDay;
		IList<AtWorkForwarder> forwardersAtDay;
		IList<LogisticsArea> logisticanDistricts;
		RouteOptimizer optimizer = new RouteOptimizer();
		RouteGeometryCalculator distanceCalculator = new RouteGeometryCalculator(DistanceProvider.Osrm);

		GenericObservableList<AtWorkDriver> observableDriversAtDay;
		GenericObservableList<AtWorkForwarder> observableForwardersAtDay;

		int addressesWithoutCoordinats, addressesWithoutRoutes, totalBottlesCountAtDay, bottlesWithoutRL;
		Pixbuf[] pixbufMarkers;
		#endregion

		#region Свойства
		private bool hasNoChanges;

		public bool HasNoChanges {
			get => hasNoChanges; 
			private set {
				hasNoChanges = value;

				ydateForRoutes.Sensitive = checkShowCompleted.Sensitive = ytimeToDelivery.Sensitive
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
			get {
				return String.Format("Формирование МЛ на {0:d}", ydateForRoutes.Date);
			}
			protected set {
				throw new InvalidOperationException("Установка протеворечит логике работы.");
			}
		}

		public enum RouteColumnTag
		{
			OnloadTime
		}

		public RoutesAtDayDlg()
		{
			this.Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();

			if(progressOrders.Adjustment == null)
				progressOrders.Adjustment = new Gtk.Adjustment(0, 0, 0, 1, 1, 0);

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

			LoadDistrictsGeometry();


			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<object>.Create()
					.AddColumn("Маркер").AddPixbufRenderer(x => GetRowMarker(x))
					.AddColumn("МЛ/Адрес").AddTextRenderer(x => GetRowTitle(x))
					.AddColumn("Адр./Время").AddTextRenderer(x => GetRowTime(x), useMarkup: true)
					.AddColumn("План").AddTextRenderer(x => GetRowPlanTime(x), useMarkup: true)
					.AddColumn("Бутылей").AddTextRenderer(x => GetRowBottles(x), useMarkup: true)
					.AddColumn("Бут. 6л").AddTextRenderer(x => GetRowBottlesSix(x))
					.AddColumn("Бут. менее 6л").AddTextRenderer(x => GetRowBottlesSmall(x))	
					.AddColumn("Вес").AddTextRenderer(x => GetRowWeight(x), useMarkup: true)
					.AddColumn("Погрузка").Tag(RouteColumnTag.OnloadTime).AddTextRenderer(x => GetRowOnloadTime(x), useMarkup: true).AddSetter((c, n) => c.Editable = n is RouteList).EditedEvent(OnLoadTimeEdited)
					.AddColumn("Километраж").AddTextRenderer(x => GetRowDistance(x))
					.AddColumn("К клиенту").AddTextRenderer(x => GetRowEquipmentToClien(x))
					.AddColumn("От клиента").AddTextRenderer(x => GetRowEquipmentFromClien(x))
					.Finish();

			ytreeRoutes.HasTooltip = true;
			ytreeRoutes.QueryTooltip += YtreeRoutes_QueryTooltip;
			ytreeRoutes.Selection.Changed += YtreeRoutes_Selection_Changed;

			ytreeviewOnDayDrivers.ColumnsConfig = FluentColumnsConfig<AtWorkDriver>.Create()
				.AddColumn("Водитель").AddTextRenderer(x => x.Employee.ShortName)
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x => x.Car != null && x.Car.IsCompanyHavings ? vodovozCarIcon : null)
					.AddTextRenderer(x => x.Car != null ? x.Car.RegistrationNumber : "нет")
				.AddColumn("")
				.Finish();
			ytreeviewOnDayDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewOnDayDrivers.Selection.Changed += YtreeviewDrivers_Selection_Changed;

			ytreeviewOnDayForwarders.ColumnsConfig = FluentColumnsConfig<AtWorkForwarder>.Create()
				.AddColumn("Экспедитор").AddTextRenderer(x => x.Employee.ShortName)
				.Finish();
			ytreeviewOnDayForwarders.Selection.Mode = Gtk.SelectionMode.Multiple;

			ytreeviewOnDayForwarders.Selection.Changed += ytreeviewForwarders_Selection_Changed;

			ytimeToDelivery.Time = TimeSpan.Parse("23:59:00");
			ydateForRoutes.Date = DateTime.Today;

			yspinMaxTime.Binding.AddBinding(optimizer, e => e.MaxTimeSeconds, w => w.ValueAsInt);
			btnRefresh.Clicked += OnButtonCancelChangesClicked;

			OrmMain.GetObjectDescription<RouteList>().ObjectUpdatedGeneric += RouteListExternalUpdated;
		}

		void GmapWidget_ButtonReleaseEvent(object o, Gtk.ButtonReleaseEventArgs args)
		{
			if(dragSelectionPointId != -1) {
				gmapWidget.DisableAltForSelection = true;
				OnPoligonSelectionUpdated();
				dragSelectionPointId = -1;
			}
		}

		void GmapWidget_MotionNotifyEvent(object o, Gtk.MotionNotifyEventArgs args)
		{
			if(dragSelectionPointId > -1) {
				brokenSelection.Points[dragSelectionPointId] = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
				gmapWidget.Refresh();
			}
		}

		void YtreeRoutes_QueryTooltip(object o, Gtk.QueryTooltipArgs args)
		{
			int binX;
			int binY;
			ytreeRoutes.ConvertWidgetToBinWindowCoords(args.X, args.Y, out binX, out binY);
			TreePath path;
			TreeViewColumn col;
			TreeIter iter;

			if(ytreeRoutes.GetPathAtPos(binX, binY, out path, out col) && ytreeRoutes.Model.GetIter(out iter, path)) {
				var loadtimeCol = ytreeRoutes.ColumnsConfig.GetColumnsByTag(RouteColumnTag.OnloadTime).Where(x => x == col).ToArray();
				if(loadtimeCol.Length > 0) {
					var node = ytreeRoutes.YTreeModel.NodeFromIter(iter) as RouteList;
					if(node == null)
						return;
					var firstDP = node.Addresses.FirstOrDefault()?.Order.DeliveryPoint;
					args.RetVal = true;
					args.Tooltip.Text = String.Format("Первый адрес: {0:t}\n" +
										 "Путь со склада: {1:N1} км. ({2} мин.)\n" +
										 "Выезд со склада: {3:t}\n" +
										 "Погрузка на складе: {4} минут",
													  node.FirstAddressTime,
													  firstDP != null ? distanceCalculator.DistanceFromBaseMeter(firstDP) * 0.001 : 0,
													  firstDP != null ? distanceCalculator.TimeFromBase(firstDP) / 60 : 0,
													  node.OnLoadTimeEnd,
													  node.TimeOnLoadMinuts
										);
				}

			}
		}

		bool poligonSelection;
		int dragSelectionPointId = -1;

		void GmapWidget_ButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
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

		}

		void OnPoligonSelectionUpdated()
		{
			var selected = addressesOverlay.Markers.Where(m => brokenSelection.IsInside(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		void RouteListExternalUpdated(object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<RouteList> e)
		{
			List<RouteList> routeLists = e.UpdatedSubjects
											.Where(rl => rl.Date.Date == ydateForRoutes.Date.Date)
											.ToList<RouteList>();

			bool foundRL = routeLists != null && routeLists.Any();

			if(foundRL) {
				bool answer = !HasNoChanges && MessageDialogHelper.RunQuestionDialog(
						"Сохраненный маршрут открыт на вкладке маршруты за день." +
						"При продолжении работы в этой вкладке, внесенные внешние изменения могут быть потеряны. " +
						"При отмене данные в этом диалоге будут перезаписаны." +
						"\nПродолжить работу в этой вкладке?");
				if(!answer)
					FillDialogAtDay();
			}
		}

		void YtreeRoutes_Selection_Changed(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			buttonRemoveAddress.Sensitive = row is RouteListItem && !checkShowCompleted.Active;
			buttonOpen.Sensitive = buttonRebuildRoute.Sensitive = (row is RouteListItem) || (row is RouteList);

			//Рисуем выделенный маршрут
			routeOverlay.Clear();
			if(row != null) {
				RouteList rl = row as RouteList;
				if(rl == null)
					rl = (row as RouteListItem).RouteList;

				MapDrawingHelper.DrawRoute(routeOverlay, rl, distanceCalculator);

				//Если выбран адрес, центруем на него карту.
				var rli = row as RouteListItem;
				if(rli != null) {
					gmapWidget.Position = rli.Order.DeliveryPoint.GmapPoint;
				}
			}
			logger.Info("Ok");
		}

		void YtreeviewDrivers_Selection_Changed(object sender, EventArgs e)
		{
			buttonRemoveDriver.Sensitive = buttonDriverSelectAuto.Sensitive = ytreeviewOnDayDrivers.Selection.CountSelectedRows() > 0;
		}

		void ytreeviewForwarders_Selection_Changed(object sender, EventArgs e)
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
			var selectedBottle = selected.Select(x => x.Tag).Cast<Order>().Sum(o => o.TotalDeliveredBottles);
			labelSelected.LabelProp = String.Format("Выбрано адресов: {0}\nБутылей: {1}", selected.Count, selectedBottle);
			menuAddToRL.Sensitive = selected.Any() && routesAtDay.Any() && !checkShowCompleted.Active;
		}

		Pixbuf vodovozCarIcon = Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		string GetRowTitle(object row)
		{
			if(row is RouteList) {
				var rl = (RouteList)row;
				return String.Format("МЛ №{0} - {1}({2})",
					rl.Id,
					rl.Driver.ShortName,
					rl.Car.RegistrationNumber
				);
			}
			if(row is RouteListItem) {
				var rli = (RouteListItem)row;
				return rli.Order.DeliveryPoint.ShortAddress;
			}
			return null;
		}

		string GetRowTime(object row)
		{
			var rl = row as RouteList;
			if(rl != null)
				return FormatOccupancy(rl.Addresses.Count, rl.Car.MinRouteAddresses, rl.Car.MaxRouteAddresses);
			return (row as RouteListItem)?.Order.DeliverySchedule.Name;
		}

		string GetRowOnloadTime(object row)
		{
			var rl = row as RouteList;
			if(rl != null && rl.OnLoadTimeStart.HasValue) {
				if(rl.OnloadTimeFixed)
					return String.Format("<span foreground=\"Turquoise\">{0:hh\\:mm}</span>", rl.OnLoadTimeStart.Value);
				else
					return rl.OnLoadTimeStart.Value.ToString("hh\\:mm");
			}
			return null;
		}

		string GetRowPlanTime(object row)
		{
			var rl = row as RouteList;
			if(rl != null)
				return String.Format("{0:hh\\:mm}-{1:hh\\:mm}",
									 rl.Addresses.FirstOrDefault()?.PlanTimeStart,
									 rl.Addresses.LastOrDefault()?.PlanTimeStart);

			var rli = row as RouteListItem;
			if(rli != null) {
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

				return String.Format("<span foreground=\"{2}\">{0:hh\\:mm}-{1:hh\\:mm}</span> ({3} мин.)",
									 rli.PlanTimeStart, rli.PlanTimeEnd, color, rli.TimeOnPoint / 60);
			}

			return null;
		}

		string GetRowBottles(object row)
		{
			var rl = row as RouteList;
			if(rl != null) {
				var bottles = rl.Addresses.Sum(x => x.Order.TotalDeliveredBottles);
				return FormatOccupancy(bottles, rl.Car.MinBottles, rl.Car.MaxBottles);
			}

			var rli = row as RouteListItem;
			if(rli != null)
				return rli.Order.TotalDeliveredBottles.ToString();
			return null;
		}

		string GetRowBottlesSix(object row)
		{
			var rl = row as RouteList;
			if(rl != null)
				return rl.Addresses.Sum(x => x.Order.TotalDeliveredBottlesSix).ToString();

			var rli = row as RouteListItem;
			if(rli != null)
				return rli.Order.TotalDeliveredBottlesSix.ToString();
			return null;
		}

		string GetRowBottlesSmall(object row)
		{
			var rl = row as RouteList;
			if(rl != null)
				return rl.Addresses.Sum(x => x.Order.TotalDeliveredBottlesSmall).ToString();

			var rli = row as RouteListItem;
			if(rli != null)
				return rli.Order.TotalDeliveredBottlesSmall.ToString();
			return null;
		}

		string GetRowWeight(object row)
		{
			var rl = row as RouteList;
			if(rl != null) {
				var weight = rl.Addresses.Sum(x => x.Order.TotalWeight);
				return FormatOccupancy(weight, null, rl.Car.MaxWeight);
			}

			var rli = row as RouteListItem;
			if(rli != null)
				return rli.Order.TotalWeight.ToString();
			return null;
		}

		string GetRowDistance(object row)
		{
			var rl = row as RouteList;
			if(rl != null) {
				var proposed = optimizer.ProposedRoutes.FirstOrDefault(x => x.RealRoute == rl);
				if(rl.PlanedDistance == null)
					return String.Empty;
				if(proposed == null)
					return String.Format("{0:N1}км", rl.PlanedDistance);
				else
					return String.Format("{0:N1}км ({1:N})",
										 rl.PlanedDistance,
										 (double)proposed.RouteCost / 1000);
			}

			var rli = row as RouteListItem;
			if(rli != null) {
				if(rli.IndexInRoute == 0)
					return String.Format("{0:N1}км", (double)distanceCalculator.DistanceFromBaseMeter(rli.Order.DeliveryPoint) / 1000);

				return String.Format("{0:N1}км", (double)distanceCalculator.DistanceMeter(rli.RouteList.Addresses[rli.IndexInRoute - 1].Order.DeliveryPoint, rli.Order.DeliveryPoint) / 1000);
			}
			return null;
		}

		string GetRowEquipmentFromClien(object row)
		{
			var rli = row as RouteListItem;
			if(rli != null) {
				return rli.Order.FromClientText;
			}
			return null;
		}

		string GetRowEquipmentToClien(object row)
		{
			string nomenclatureName = null;
			var rli = row as RouteListItem;
			if(rli != null) {
				foreach(var orderItem in rli.Order.OrderItems) {
					if(orderItem.Nomenclature.Category == NomenclatureCategory.equipment || orderItem.Nomenclature.Category == NomenclatureCategory.additional) {
						nomenclatureName += " " + orderItem.Nomenclature.Name;
					}
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
				return String.Format("<span foreground=\"{0}\">{1}</span>({2}-{3})", color, val, min, max);
			else if(max.HasValue)
				return String.Format("<span foreground=\"{0}\">{1}</span>({2})", color, val, max);
			else
				return String.Format("<span foreground=\"{0}\">{1}</span>(min {2})", color, val, min);
		}

		Pixbuf GetRowMarker(object row)
		{
			var rl = row as RouteList;
			if(rl == null) {
				var rli = row as RouteListItem;
				if(rli != null)
					rl = rli.RouteList;
			}
			if(rl != null) {
				var routeIndex = routesAtDay.IndexOf(rl);
				if(routeIndex >= 0 && routeIndex < pixbufMarkers.Length)
					return pixbufMarkers[routeIndex];
			}

			return null;
		}

		private void FillFullOrdersInfo()
		{
			OrderItem orderItemAlias = null;
			Nomenclature nomenclatureAlias = null;

			int totalOrders = Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, true)
				.GetExecutableQueryOver(UoW.Session)
				.Select(NHibernate.Criterion.Projections.Count<Order>(x => x.Id)).SingleOrDefault<int>();

			int totalBottles = Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, true)
				.GetExecutableQueryOver(UoW.Session)
				.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.JoinAlias(() => orderItemAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Category == NomenclatureCategory.water && nomenclatureAlias.TareVolume == TareVolume.Vol19L)
				.Select(NHibernate.Criterion.Projections.Sum(() => orderItemAlias.Count)).SingleOrDefault<int>();

			var text = new List<string>();
			text.Add(NumberToTextRus.FormatCase(totalOrders, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов."));
			text.Add(NumberToTextRus.FormatCase(totalBottles, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));

			ytextFullOrdersInfo.Buffer.Text = String.Join("\n", text);
		}

		void FillDialogAtDay()
		{
			logger.Info("Загружаем заказы на {0:d}...", ydateForRoutes.Date);
			MainClass.progressBarWin.ProgressStart(5);
			UoW.Session.Clear();

			var ordersQuery = Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(UoW.Session)
				.Fetch(x => x.DeliveryPoint).Eager
				.Future();

			Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(UoW.Session)
				.Fetch(x => x.OrderItems).Eager
				.Future();

			var withoutTime = ordersQuery.Where(x => x.DeliverySchedule == null).ToList();
			var withoutLocation = ordersQuery.Where(x => x.DeliveryPoint == null || !x.DeliveryPoint.CoordinatesExist).ToList();
			if(withoutTime.Any() || withoutLocation.Any())
				MessageDialogHelper.RunWarningDialog("Не все заказы были загружены!" +
				                                    (withoutTime.Any() ? ("\n* У заказов отсутсвует время доставки: " + String.Join(", ", withoutTime.Select(x => x.Id.ToString()))) : "") +
				                                    (withoutLocation.Any() ? ("\n* У заказов отсутствуют координаты: " + String.Join(", ", withoutLocation.Select(x => x.Id.ToString()))) : "")
												   );

			ordersAtDay = ordersQuery.Where(x => x.DeliverySchedule != null)
									 .Where(x => x.DeliverySchedule.To <= ytimeToDelivery.Time)
									 .Where(x => x.DeliveryPoint != null)
									 .ToList();

			var outLogisticAreas = ordersAtDay
				.Where(
					x => !logisticanDistricts.Any(
						a => x.DeliveryPoint.NetTopologyPoint != null && a.Geometry.Contains(x.DeliveryPoint.NetTopologyPoint)
					)
				)
				.ToList();
			if(outLogisticAreas.Any())
				MessageDialogHelper.RunWarningDialog("Обратите внимание, координаты точек доставки для следущих заказов не попадают ни в один логистический район: "
													+ String.Join(", ", outLogisticAreas.Select(x => x.Id.ToString())));

			logger.Info("Загружаем МЛ на {0:d}...", ydateForRoutes.Date);
			MainClass.progressBarWin.ProgressAdd();

			var routesQuery1 = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(UoW.Session);
			if(!checkShowCompleted.Active)
				routesQuery1.Where(x => x.Status == RouteListStatus.New);
			var routesQuery = routesQuery1
				.Fetch(x => x.Addresses).Default
				.Future();

			var routesQuery2 = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(UoW.Session);
			if(!checkShowCompleted.Active)
				routesQuery2.Where(x => x.Status == RouteListStatus.New);
			routesQuery2
				.Where(x => x.Status == RouteListStatus.New)
				.Fetch(x => x.Driver).Eager
				.Fetch(x => x.Car).Eager
				.Future();

			routesAtDay = routesQuery.ToList();
			routesAtDay.ToList().ForEach(rl => rl.UoW = UoW);
			//Нужно для того чтобы диалог не падал при загрузке если присутствую поломаные МЛ.
			routesAtDay.ToList().ForEach(rl => rl.CheckAddressOrder());


			UpdateRoutesPixBuf();
			UpdateRoutesButton();

			MainClass.progressBarWin.ProgressAdd();
			logger.Info("Загружаем водителей на {0:d}...", ydateForRoutes.Date);
			DriversAtDay = Repository.Logistics.AtWorkRepository.GetDriversAtDay(UoW, ydateForRoutes.Date);

			MainClass.progressBarWin.ProgressAdd();
			logger.Info("Загружаем экспедиторов на {0:d}...", ydateForRoutes.Date);
			ForwardersAtDay = Repository.Logistics.AtWorkRepository.GetForwardersAtDay(UoW, ydateForRoutes.Date);

			MainClass.progressBarWin.ProgressAdd();
			UpdateAddressesOnMap();

			MainClass.progressBarWin.ProgressAdd();
			var levels = LevelConfigFactory.FirstLevel<RouteList, RouteListItem>(x => x.Addresses).LastLevel(c => c.RouteList).EndConfig();
			ytreeRoutes.YTreeModel = new LevelTreeModel<RouteList>(routesAtDay, levels);

			MainClass.progressBarWin.ProgressClose();
		}

		void UpdateAddressesOnMap()
		{
			logger.Info("Обновляем адреса на карте...");
			addressesWithoutCoordinats = 0;
			addressesWithoutRoutes = 0;
			totalBottlesCountAtDay = 0;
			bottlesWithoutRL = 0;
			addressesOverlay.Clear();
			foreach(var order in ordersAtDay.Select(x => x).Where(x => x.IsService == false)) {
				totalBottlesCountAtDay += order.TotalDeliveredBottles;
				var route = routesAtDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));

				if(route == null) {
					addressesWithoutRoutes++;
					bottlesWithoutRL += order.TotalDeliveredBottles;
				}

				if(order.DeliveryPoint.Latitude.HasValue && order.DeliveryPoint.Longitude.HasValue) {
					PointMarkerShape shape = GetMarkerShape(order.TotalDeliveredBottles);

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

					var addressMarker = new PointMarker(new PointLatLng((double)order.DeliveryPoint.Latitude, (double)order.DeliveryPoint.Longitude), type, shape);
					addressMarker.Tag = order;

					string ttText = order.DeliveryPoint.ShortAddress;
					if(order.TotalDeliveredBottles > 0)
						ttText += String.Format("\nБутылей 19л: {0}", order.TotalDeliveredBottles);
					if(order.TotalDeliveredBottlesSix > 0)
						ttText += String.Format("\nБутылей 6л: {0}", order.TotalDeliveredBottlesSix);
					if(order.TotalDeliveredBottlesSmall > 0)
						ttText += String.Format("\nБутылей 0,6л: {0}", order.TotalDeliveredBottlesSmall);

					ttText += String.Format("\nВремя доставки: {0}\nРайон: {1}",
						order.DeliverySchedule?.Name ?? "Не назначено",
						logisticanDistricts?.FirstOrDefault(x => x.Geometry.Contains(order.DeliveryPoint.NetTopologyPoint))?.Name);

					addressMarker.ToolTipText = ttText;

					var identicalPoint = addressesOverlay.Markers.Count(g => g.Position.Lat == (double)order.DeliveryPoint.Latitude && g.Position.Lng == (double)order.DeliveryPoint.Longitude);
					var pointShift = 5;
					if(identicalPoint >= 1) {
						addressMarker.Offset = new System.Drawing.Point(identicalPoint * pointShift, identicalPoint * pointShift);
					}

					if(route != null)
						addressMarker.ToolTipText += String.Format(" Везёт: {0}", route.Driver.ShortName);

					addressesOverlay.Markers.Add(addressMarker);

				} else
					addressesWithoutCoordinats++;
			}
			UpdateOrdersInfo();
			logger.Info("Ок.");
		}

		protected void OnYdateForRoutesDateChanged(object sender, EventArgs e)
		{
			FillDialogAtDay();
			FillFullOrdersInfo();
			OnTabNameChanged();
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
				text.Add(String.Format("Из них {0} без координат.", addressesWithoutCoordinats));
			if(addressesWithoutRoutes > 0)
				text.Add(String.Format("Из них {0} без маршрутных листов.", addressesWithoutRoutes));
			if(totalBottlesCountAtDay > 0)
				text.Add(NumberToTextRus.FormatCase(totalBottlesCountAtDay, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));
			if(bottlesWithoutRL > 0)
				text.Add(NumberToTextRus.FormatCase(bottlesWithoutRL, "Осталась {0} бутыль", "Осталось {0} бутыли", "Осталось {0} бутылей"));

			text.Add(NumberToTextRus.FormatCase(routesAtDay.Count, "Всего {0} маршрутный лист.", "Всего {0} маршрутных листа.", "Всего {0} маршрутных листов."));

			textOrdersInfo.Buffer.Text = String.Join("\n", text);

			if(progressOrders.Adjustment != null) {
				progressOrders.Adjustment.Upper = ordersAtDay.Count;
				progressOrders.Adjustment.Value = ordersAtDay.Count - addressesWithoutRoutes;
			}
			if(ordersAtDay.Count == 0)
				progressOrders.Text = String.Empty;
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

		PointMarkerShape GetMarkerShape(int bottlesCount){
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
			var menu = new Gtk.Menu();
			foreach(var route in routesAtDay) {
				var name = String.Format("МЛ №{0} - {1}", route.Id, route.Driver.ShortName);
				var item = new MenuItemId<RouteList>(name);
				item.ID = route;
				item.Activated += AddToRLItem_Activated;
				menu.Append(item);
			}
			menu.ShowAll();
			menuAddToRL.Menu = menu;
		}

		void AddToRLItem_Activated(object sender, EventArgs e)
		{
			bool recalculateLoading = false;
			var selectedOrders = GetSelectedOrders();

			var route = ((MenuItemId<RouteList>)sender).ID;

			foreach(var order in selectedOrders) {
				if(order.OrderStatus == OrderStatus.InTravelList) {
					var alreadyIn = routesAtDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));
					if(alreadyIn == null)
						throw new InvalidProgramException(String.Format("Маршрутный лист, в котором добавлен заказ {0} не найден.", order.Id));
					if(alreadyIn.Id == route.Id) // Уже в нужном маршрутном листе.
						continue;
					var toRemoveAddress = alreadyIn.Addresses.First(x => x.Order.Id == order.Id);
					if(toRemoveAddress.IndexInRoute == 0)
						recalculateLoading = true;
					alreadyIn.RemoveAddress(toRemoveAddress);
					UoW.Save(alreadyIn);
				}
				var item = route.AddAddressFromOrder(order);
				if(item.IndexInRoute == 0)
					recalculateLoading = true;
			}
			route.RecalculatePlanTime(distanceCalculator);
			route.RecalculatePlanedDistance(distanceCalculator);
			UoW.Save(route);
			logger.Info("В МЛ №{0} добавлено {1} адресов.", route.Id, selectedOrders.Count);
			if(recalculateLoading)
				RecalculateOnLoadTime();
			UpdateAddressesOnMap();
			RoutesWasUpdated();

			if(route.HasOverweight()){
				MessageDialogHelper.RunWarningDialog(
					String.Format("Автомобиль '{0}' в МЛ №{1} перегружен на {2} кг.", route.Car.Title, route.Id, route.Overweight())
				);
			}
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
			orders.AddRange(selectedMarkers.Select(m => m.Tag).Cast<Order>().ToList());
			//Добавление заказов из квадратного выделения
			orders.AddRange(addressesOverlay.Markers
				.Where(m => gmapWidget.SelectedArea.Contains(m.Position))
				.Select(x => x.Tag).Cast<Order>().ToList());
			//Добавление закзаво через непрямоугольную область
			GMapOverlay overlay = gmapWidget.Overlays.FirstOrDefault(o => o.Id.Contains(selectionOverlay.Id));
			GMapPolygon polygons = overlay?.Polygons.FirstOrDefault(p => p.Name.ToLower().Contains("выделение"));
			if(polygons != null) {
				var temp = addressesOverlay.Markers
					.Where(m => polygons.IsInside(m.Position))
					.Select(x => x.Tag).Cast<Order>().ToList();
				orders.AddRange(temp);
			}

			return orders;
		}

		protected void OnButtonSaveChangesClicked(object sender, EventArgs e)
		{
			Save();
		}

		protected void OnButtonCancelChangesClicked(object sender, EventArgs e)
		{
			UoW.Session.Clear();
			HasNoChanges = true;
			FillDialogAtDay();
		}

		protected void OnButtonRemoveAddressClicked(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject<RouteListItem>();
			var route = row.RouteList;
			route.RemoveAddress(row);
			UoW.Save(route);
			UpdateAddressesOnMap();
			route.RecalculatePlanTime(distanceCalculator);
			route.RecalculatePlanedDistance(distanceCalculator);
			RoutesWasUpdated();
		}

		protected void OnCheckShowCompletedToggled(object sender, EventArgs e)
		{
			FillDialogAtDay();
			buttonSaveChanges.Sensitive = !checkShowCompleted.Active;
		}

		#region TDIDialog

		public override bool Save()
		{
			//Перестраиваем все маршруты
			RebuildAllRoutes();
			routesAtDay.ToList().ForEach(x => UoW.Save(x));
			//DriversAtDay.ToList().ForEach(x => uow.Save(x));
			//ForwardersAtDay.ToList().ForEach(x => uow.Save(x));
			UoW.Commit();
			HasNoChanges = true;
			FillDialogAtDay();
			return true;
		}

		public override bool HasChanges {
			get {
				return !HasNoChanges;
			}
		}

		void RebuildAllRoutes()
		{
			int ix = 0;
			List<string> warnings = new List<string>();
			optimizer.DebugBuffer = null;

			foreach(var route in routesAtDay) {
				ix++;
				textOrdersInfo.Buffer.Text = $"Строим {ix} из {routesAtDay.Count}";

				var newRoute = optimizer.RebuidOneRoute(route);
				if(newRoute != null) {
					newRoute.UpdateAddressOrderInRealRoute(route);
					route.RecalculatePlanedDistance(distanceCalculator);
					var noPlan = route.Addresses.Count(x => !x.PlanTimeStart.HasValue);
					if(noPlan > 0)
						warnings.Add($"Для маршрута {route.Id} незапланировано {noPlan} адресов.");
				} else {
					warnings.Add($"Маршрут {route.Id} не был перестроен.");
				}
			}
			if(warnings.Any())
				MessageDialogHelper.RunWarningDialog(String.Join("\n", warnings));
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			//Открываем заказ
			if(row is RouteListItem) {
				Domain.Orders.Order order = (row as RouteListItem).Order;
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<Domain.Orders.Order>(order.Id),
					() => new OrderDlg(order)
				);
			}
			//Открываем МЛ
			if(row is RouteList) {
				RouteList routeList = row as RouteList;
				if(!HasNoChanges) {
					if(MessageDialogHelper.RunQuestionDialog("Сохранить маршрутный лист перед открытием?"))
						Save();
					else
						return;
				}
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<RouteList>(routeList.Id),
					() => new RouteListCreateDlg(routeList)
				);
			}
		}
		protected override void OnDestroyed()
		{
			logger.Debug("RoutesAtDayDlg Destroyed() called.");
			//Отписываемся от событий.
			OrmMain.GetObjectDescription<RouteList>().ObjectUpdatedGeneric -= RouteListExternalUpdated;
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
			logisticanDistricts = LogisticAreaRepository.AreaWithGeometry(UoW);
			foreach(var district in logisticanDistricts) {
				var poligon = new GMapPolygon(
					district.Geometry.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList()
					, district.Name);
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
			var SelectDrivers = new OrmReference(
				UoW,
				Repository.EmployeeRepository.ActiveDriversOrderedQuery()
			);
			SelectDrivers.Mode = OrmReferenceMode.MultiSelect;
			SelectDrivers.ObjectSelected += SelectDrivers_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectDrivers);
		}

		void SelectDrivers_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var addDrivers = e.GetEntities<Employee>().ToList();
			logger.Info("Получаем авто для водителей...");
			MainClass.progressBarWin.ProgressStart(2);
			var onlyNew = addDrivers.Where(x => driversAtDay.All(y => y.Employee.Id != x.Id)).ToList();
			var allCars = CarRepository.GetCarsbyDrivers(UoW, onlyNew.Select(x => x.Id).ToArray());
			MainClass.progressBarWin.ProgressAdd();

			foreach(var driver in addDrivers) {
				driversAtDay.Add(new AtWorkDriver(driver, CurDate,
												  allCars.FirstOrDefault(x => x.Driver.Id == driver.Id)
												 ));
			}
			MainClass.progressBarWin.ProgressAdd();
			DriversAtDay = driversAtDay.OrderBy(x => x.Employee.ShortName).ToList();
			logger.Info("Ок");
			MainClass.progressBarWin.ProgressClose();
		}

		protected void OnButtonAddForwarderClicked(object sender, EventArgs e)
		{
			var SelectForwarder = new OrmReference(
				UoW,
				Repository.EmployeeRepository.ActiveForwarderOrderedQuery()
			);
			SelectForwarder.Mode = OrmReferenceMode.MultiSelect;
			SelectForwarder.ObjectSelected += SelectForwarder_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectForwarder);
		}

		void SelectForwarder_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var addForwarder = e.GetEntities<Employee>();
			foreach(var forwarder in addForwarder) {
				if(forwardersAtDay.Any(x => x.Employee.Id == forwarder.Id)) {
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
			var logistican = Repository.EmployeeRepository.GetEmployeeForCurrentUser(UoW);
			if(logistican == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать маршрутные листы, так как некого указывать в качестве логиста.");
				return;
			}

			UpdateWarningButton();

			if(creatingInProgress) {
				buttonAutoCreate.Label = "Создать маршруты";
				optimizer.Cancel = true;
				return;
			}
			creatingInProgress = true;
			buttonAutoCreate.Label = "Остановить";

			optimizer.UoW = UoW;
			optimizer.Routes = routesAtDay;
			optimizer.Orders = ordersAtDay;
			optimizer.Drivers = driversAtDay;
			optimizer.Forwarders = forwardersAtDay;
			optimizer.OrdersProgress = progressOrders;
			optimizer.DebugBuffer = textOrdersInfo.Buffer;
			optimizer.CreateRoutes();

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
					rl.Logistican = logistican;

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
			MainClass.progressBarWin.ProgressClose();
			creatingInProgress = false;
			optimizer.Cancel = false;
			buttonAutoCreate.Label = "Создать маршруты";
		}

		protected void OnButtonDriverSelectAutoClicked(object sender, EventArgs e)
		{
			var SelectDriverCar = new OrmReference(
				UoW,
				Repository.Logistics.CarRepository.ActiveCompanyCarsQuery()
			);
			var driver = ytreeviewOnDayDrivers.GetSelectedObjects<AtWorkDriver>().First();
			SelectDriverCar.Tag = driver;
			SelectDriverCar.Mode = OrmReferenceMode.Select;
			SelectDriverCar.ObjectSelected += SelectDriverCar_ObjectSelected;
			TabParent.AddSlaveTab(this, SelectDriverCar);
		}

		void SelectDriverCar_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var driver = e.Tag as AtWorkDriver;
			var car = e.Subject as Car;
			driversAtDay.Where(x => x.Car != null && x.Car.Id == car.Id).ToList().ForEach(x => x.Car = null);
			driver.Car = car;
		}

		void OnLoadTimeEdited(object o, Gtk.EditedArgs args)
		{
			var routeList = (RouteList)ytreeRoutes.YTreeModel.NodeAtPath(new Gtk.TreePath(args.Path));
			bool NeedRecalculate = false;
			TimeSpan fixedTime;

			if(String.IsNullOrWhiteSpace(args.NewText)) {
				NeedRecalculate = routeList.OnloadTimeFixed;
				routeList.OnloadTimeFixed = false;
			} else if(TimeSpan.TryParse(args.NewText, out fixedTime)) {
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

			optimizer.DebugBuffer = textOrdersInfo.Buffer;
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
				String.Join("\n", optimizer.WarningMessages.Select(x => "⚠ " + x))
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

		protected void OnYtimeToDeliveryWidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.KeyPress) {
				EventKey eventKey = args.Args.OfType<EventKey>().FirstOrDefault();
				if(eventKey != null && (eventKey.Key == Gdk.Key.Return || eventKey.Key == Gdk.Key.KP_Enter)){
					FillItems();
				}
			}
		}

		protected void OnLabel2WidgetEvent(object o, WidgetEventArgs args)
		{
			if(args.Event.Type == EventType.ButtonPress && (args.Event as EventButton).Button == 1) {
				var user = EmployeeRepository.GetEmployeeForCurrentUser(UoW);
				if(user.User.Id != 94)
					return;

				TabParent.AddSlaveTab(this, new OptimizingParametersDlg());
			}
		}
	}
}

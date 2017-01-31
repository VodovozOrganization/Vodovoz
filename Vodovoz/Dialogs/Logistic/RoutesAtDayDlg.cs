using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSWidgetLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;

namespace Vodovoz
{
	public partial class RoutesAtDayDlg : TdiTabBase, ITdiDialog
	{
		#region Поля
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		private readonly GMapOverlay selectionOverlay = new GMapOverlay("selection");
		private GMapPolygon brokenSelection;
		private List<GMapMarker> selectedMarkers = new List<GMapMarker>();
		IList<Order> ordersAtDay;
		IList<RouteList> routesAtDay;
		int addressesWithoutCoordinats, addressesWithoutRoutes, totalBottlesCountAtDay, bottlesWithoutRL;
		Pixbuf[] pixbufMarkers;
		#endregion

		#region Свойства
		private bool hasNoChanges;

		public bool HasNoChanges  {
			get {return hasNoChanges; }

			private set {
				hasNoChanges = value;

				ydateForRoutes.Sensitive = checkShowCompleted.Sensitive
					= hasNoChanges;
			}
		}

		#endregion

		public override string TabName
		{
			get
			{
				return String.Format("Маршруты на {0:d}", ydateForRoutes.Date);
			}
			protected set
			{
				throw new InvalidOperationException("Установка протеворечит логике работы.");
			}
		}

		public RoutesAtDayDlg()
		{
			this.Build();

			if (progressOrders.Adjustment == null)
				progressOrders.Adjustment = new Gtk.Adjustment(0, 0, 0, 1, 1, 0);

			//Configure map
			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(addressesOverlay);
			gmapWidget.Overlays.Add(selectionOverlay);
			gmapWidget.DisableAltForSelection = true;
			gmapWidget.OnSelectionChange += GmapWidget_OnSelectionChange;
			gmapWidget.ButtonPressEvent += GmapWidget_ButtonPressEvent;
			gmapWidget.ButtonReleaseEvent += GmapWidget_ButtonReleaseEvent;
			gmapWidget.MotionNotifyEvent += GmapWidget_MotionNotifyEvent;

			yenumcomboMapType.ItemsEnum = typeof(MapProviders);

			ytreeRoutes.ColumnsConfig = FluentColumnsConfig <object>.Create()
				.AddColumn("МЛ/Адрес").AddTextRenderer(x => GetRowTitle(x))
				.AddColumn("Время").AddTextRenderer(x => GetRowTime(x))
				.AddColumn("Бутылей").AddTextRenderer(x => GetRowBottles(x))
				.AddColumn("Маркер").AddPixbufRenderer(x => GetRowMarker(x))
				.Finish();

			ytreeRoutes.Selection.Changed += YtreeRoutes_Selection_Changed;

			ydateForRoutes.Date = DateTime.Today;

			OrmMain.GetObjectDescription<RouteList>().ObjectUpdatedGeneric += RouteListExternalUpdated;
		}

		void GmapWidget_ButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			if (dragSelectionPointId != -1)
			{
				gmapWidget.DisableAltForSelection = true;
				OnPoligonSelectionUpdated();
				dragSelectionPointId = -1;
			}
		}

		void GmapWidget_MotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			if(dragSelectionPointId > -1)
			{
				brokenSelection.Points[dragSelectionPointId] = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
				gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
				gmapWidget.Refresh();
			}
		}

		bool poligonSelection;
		int dragSelectionPointId = -1;

		void GmapWidget_ButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if(args.Event.Button == 1)
			{
				bool markerIsSelect = false;
				if (args.Event.State.HasFlag(ModifierType.Mod1Mask))
				{
					foreach (var marker in addressesOverlay.Markers)
					{
						if (marker.IsMouseOver) {
							var markerUnderMouse = selectedMarkers.FirstOrDefault(m => ((Order)m.Tag).Id == ((Order)marker.Tag).Id);
							if( markerUnderMouse == null) {
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
				if (!markerIsSelect) {
					selectedMarkers.Clear();
					logger.Debug("Список выделенных маркеров очищен");
				}
				UpdateAddressesOnMap();

				if(poligonSelection)
				{
					GRect rect = new GRect((long)args.Event.X - 5, (long)args.Event.Y - 5, 10, 10);
					rect.OffsetNegative(gmapWidget.RenderOffset);

					dragSelectionPointId = brokenSelection.LocalPoints.FindIndex(rect.Contains);
					if(dragSelectionPointId != -1)
					{
						gmapWidget.DisableAltForSelection = false;
						return;
					}
				}

				if(args.Event.State.HasFlag(ModifierType.ControlMask))
				{
					if(!poligonSelection)
					{
						poligonSelection = true;
						logger.Debug("Старт выделения через полигон.");
						var startPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						brokenSelection = new GMapPolygon(new List<PointLatLng>{startPoint}, "Выделение" );
						gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
						selectionOverlay.Polygons.Add(brokenSelection);
					}
					else
					{
						logger.Debug("Продолжили.");
						var newPoint = gmapWidget.FromLocalToLatLng((int)args.Event.X, (int)args.Event.Y);
						brokenSelection.Points.Add(newPoint);
						gmapWidget.UpdatePolygonLocalPosition(brokenSelection);
					}
					OnPoligonSelectionUpdated();
				}
				else
				{
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

		void RouteListExternalUpdated (object sender, QSOrmProject.UpdateNotification.OrmObjectUpdatedGenericEventArgs<RouteList> e)
		{
			List<RouteList> routeLists = e.UpdatedSubjects
											.Where(rl => rl.Date.Date == ydateForRoutes.Date.Date)
											.ToList<RouteList>();
			
			bool foundRL = routeLists?.Count > 0;

			if (foundRL)
			{
				bool answer;
				if (HasNoChanges)
					answer = false;
				else
					answer = MessageDialogWorks.RunQuestionDialog(
						"Сохраненный маршрут открыт на вкладке маршруты за день." +
						"При продолжении работы в этой вкладке, внесенные внешние изменения могут быть потеряны. " +
						"При отмене данные в этом диалоге будут перезаписаны." +
						"\nПродолжить работу в этой вкладке?");
				if (!answer)
					FillDialogAtDay();
			}
		}

		void YtreeRoutes_Selection_Changed (object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			buttonRemoveAddress.Sensitive = row is RouteListItem && !checkShowCompleted.Active;
			buttonOpen.Sensitive = (row is RouteListItem) || (row is RouteList);
		}

		void GmapWidget_OnSelectionChange (RectLatLng Selection, bool ZoomToFit)
		{
			if (poligonSelection)
				return;
			var selected = addressesOverlay.Markers.Where(m => Selection.Contains(m.Position)).ToList();
			UpdateSelectedInfo(selected);
		}

		void UpdateSelectedInfo(List<GMapMarker> selected)
		{
			var selectedBottle = selected.Select(x => x.Tag).Cast<Order>().Sum(o => o.TotalDeliveredBottles);
			labelSelected.LabelProp = String.Format("Выбрано адресов: {0}\nБутылей: {1}", selected.Count, selectedBottle);
			menuAddToRL.Sensitive = selected.Count > 0 && routesAtDay.Count > 0 && !checkShowCompleted.Active;
		}

		string GetRowTitle(object row)
		{
			if(row is RouteList)
			{
				var rl = (RouteList)row;
				return String.Format("Маршрутный лист №{0} - {1}({2})",
					rl.Id,
					rl.Driver.ShortName,
					rl.Car.RegistrationNumber
				);
			}
			if(row is RouteListItem)
			{
				var rli = (RouteListItem)row;
				return rli.Order.DeliveryPoint.ShortAddress;
			}
			return null;
		}

		string GetRowTime(object row)
		{
			var rl = row as RouteList;
			if (rl != null)
				return rl.Addresses.Count.ToString();
			return (row as RouteListItem)?.Order.DeliverySchedule.Name;
		}

		string GetRowBottles(object row)
		{
			var rl = row as RouteList;
			if (rl != null)
				return rl.Addresses.Sum(x => x.Order.TotalDeliveredBottles).ToString();
			
			var rli = row as RouteListItem;
			if(rli != null)
				return rli.Order.TotalDeliveredBottles.ToString();
			return null;
		}

		Pixbuf GetRowMarker(object row)
		{
			var rl = row as RouteList;
			if (rl == null)
			{
				var rli = row as RouteListItem;
				if (rli != null)
					rl = rli.RouteList;
			}
			if (rl != null)
				return pixbufMarkers[routesAtDay.IndexOf(rl)];
			else
				return null;
		}

		void FillDialogAtDay()
		{
			logger.Info("Загружаем заказы на {0:d}...", ydateForRoutes.Date);
			uow.Session.Clear();

			var ordersQuery = Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(uow.Session)
				.Fetch(x => x.DeliveryPoint).Eager
				.Future();

			Repository.OrderRepository.GetOrdersForRLEditingQuery(ydateForRoutes.Date, checkShowCompleted.Active)
				.GetExecutableQueryOver(uow.Session)
				.Fetch(x => x.OrderItems).Eager
				.Future();

			ordersAtDay = ordersQuery.ToList();

			var routesQuery1 = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(uow.Session);
			if (!checkShowCompleted.Active)
				routesQuery1.Where(x => x.Status == RouteListStatus.New);
			var routesQuery = routesQuery1
				.Fetch(x => x.Addresses).Default
				.Future();

			var routesQuery2 = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydateForRoutes.Date)
				.GetExecutableQueryOver(uow.Session);
			if (!checkShowCompleted.Active)
				routesQuery2.Where(x => x.Status == RouteListStatus.New);
			routesQuery2
				.Where(x => x.Status == RouteListStatus.New)
				.Fetch(x => x.Driver).Eager
				.Fetch(x => x.Car).Eager
				.Future();

			routesAtDay = routesQuery.ToList();
			routesAtDay.ToList().ForEach(rl => rl.UoW = uow);

			UpdateRoutesPixBuf();
			UpdateRoutesButton();

			var levels = LevelConfigFactory.FirstLevel<RouteList, RouteListItem>(x => x.Addresses).LastLevel(c => c.RouteList).EndConfig();
			ytreeRoutes.YTreeModel = new LevelTreeModel<RouteList>(routesAtDay, levels);

			UpdateAddressesOnMap();
		}

		void UpdateAddressesOnMap()
		{
			logger.Info("Обновляем адреса на карте...");
			addressesWithoutCoordinats = 0;
			addressesWithoutRoutes = 0;
			totalBottlesCountAtDay = 0;
			bottlesWithoutRL = 0;
			addressesOverlay.Clear();

			foreach(var order in ordersAtDay)
			{
				totalBottlesCountAtDay += order.TotalDeliveredBottles;
				var route = routesAtDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));

				if (route == null) {
					addressesWithoutRoutes++;
					bottlesWithoutRL += order.TotalDeliveredBottles;
				}

				if (order.DeliveryPoint.Latitude.HasValue && order.DeliveryPoint.Longitude.HasValue)
				{
					PointMarkerType type;
					if (route == null)
					{
						if ((order.DeliverySchedule.To - order.DeliverySchedule.From).TotalHours <= 1)
							type = PointMarkerType.black_and_red;
						else if (order.DeliverySchedule.From.Hours >= 17)
							type = PointMarkerType.blue_stripes;
						else
							type = PointMarkerType.black;
					}
					else
						type = GetAddressMarker(routesAtDay.IndexOf(route));
					
					if (selectedMarkers.FirstOrDefault(m => ((Order)m.Tag).Id  == order.Id) != null)
						type = PointMarkerType.white;
					
					var addressMarker = new PointMarker(new PointLatLng((double)order.DeliveryPoint.Latitude, (double)order.DeliveryPoint.Longitude),	type);
					addressMarker.Tag = order;
					addressMarker.ToolTipText = String.Format("{0}\nБутылей: {1}, Время доставки: {2}",
						order.DeliveryPoint.ShortAddress,
						order.TotalDeliveredBottles,
						order.DeliverySchedule?.Name ?? "Не назначено"
					);
					if (route != null)
						addressMarker.ToolTipText += String.Format(" Везёт: {0}", route.Driver.ShortName);
					addressesOverlay.Markers.Add(addressMarker);
				}
				else
					addressesWithoutCoordinats++;
			}
			UpdateOrdersInfo();
			logger.Info("Ок.");
		}

		protected void OnYdateForRoutesDateChanged(object sender, EventArgs e)
		{
			FillDialogAtDay();
			OnTabNameChanged();
		}

		protected void OnYenumcomboMapTypeChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		void UpdateOrdersInfo()
		{
			var text = new List<string>();
			text.Add(RusNumber.FormatCase(ordersAtDay.Count, "На день {0} заказ.", "На день {0} заказа.", "На день {0} заказов."));
			if (addressesWithoutCoordinats > 0)
				text.Add(String.Format("Из них {0} без координат.", addressesWithoutCoordinats));
			if (addressesWithoutRoutes > 0)
				text.Add(String.Format("Из них {0} без маршрутных листов.", addressesWithoutRoutes));
			if (totalBottlesCountAtDay > 0)
				text.Add(RusNumber.FormatCase(totalBottlesCountAtDay, "Всего {0} бутыль", "Всего {0} бутыли", "Всего {0} бутылей"));
			if (bottlesWithoutRL > 0)
				text.Add(RusNumber.FormatCase(bottlesWithoutRL, "Осталась {0} бутыль", "Осталось {0} бутыли", "Осталось {0} бутылей"));
			
			text.Add(RusNumber.FormatCase(routesAtDay.Count, "Всего {0} маршрутный лист.", "Всего {0} маршрутных листа.", "Всего {0} маршрутных листов.") );

			textOrdersInfo.Buffer.Text = String.Join("\n", text);

			if (progressOrders.Adjustment != null)
			{
				progressOrders.Adjustment.Upper = ordersAtDay.Count;
				progressOrders.Adjustment.Value = ordersAtDay.Count - addressesWithoutRoutes;
			}
			if (ordersAtDay.Count == 0)
				progressOrders.Text = String.Empty;
			else if (addressesWithoutRoutes == 0)
				progressOrders.Text = "Готово.";
			else
				progressOrders.Text = RusNumber.FormatCase(addressesWithoutRoutes, "Остался {0} заказ", "Осталось {0} заказа", "Осталось {0} заказов");
		}

		private PointMarkerType[] pointMarkers = new []{
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

		void UpdateRoutesPixBuf()
		{
			if (pixbufMarkers != null && routesAtDay.Count == pixbufMarkers.Length)
				return;
			pixbufMarkers = new Pixbuf[routesAtDay.Count];
			for(int i = 0; i < routesAtDay.Count; i++)
			{
				pixbufMarkers[i] = PointMarker.GetIconPixbuf(GetAddressMarker(i).ToString());
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
			foreach(var route in routesAtDay)
			{
				var name = String.Format("МЛ №{0} - {1}", route.Id, route.Driver.ShortName);
				var item = new MenuItemId<RouteList>(name);
				item.ID = route;
				item.Activated += AddToRLItem_Activated;
				menu.Append(item);
			}
			menu.ShowAll();
			menuAddToRL.Menu = menu;
		}

		void AddToRLItem_Activated (object sender, EventArgs e)
		{
			var selectedOrders = GetSelectedOrders();

			var route = ((MenuItemId<RouteList>)sender).ID;

			foreach(var order in selectedOrders)
			{
				if(order.OrderStatus == OrderStatus.InTravelList)
				{
					var alreadyIn = routesAtDay.FirstOrDefault(rl => rl.Addresses.Any(a => a.Order.Id == order.Id));
					if (alreadyIn == null)
						throw new InvalidProgramException(String.Format("Маршрутный лист, в котором добавлен заказ {0} не найден.", order.Id));
					if (alreadyIn.Id == route.Id) // Уже в нужном маршрутном листе.
						continue;
						
					alreadyIn.RemoveAddress(alreadyIn.Addresses.First(x => x.Order.Id == order.Id));
					uow.Save(alreadyIn);
				}
				route.AddAddressFromOrder(order);
			}
			route.ReorderAddressesByTime();
			uow.Save(route);
			logger.Info("В МЛ №{0} добавлено {1} адресов.", route.Id, selectedOrders.Count);
			UpdateAddressesOnMap();
			RoutesWasUpdated();
		}

		private IList<Order> GetSelectedOrders(){
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
			uow.Session.Clear();
			HasNoChanges = true;
			FillDialogAtDay();
		}

		protected void OnButtonRemoveAddressClicked(object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject<RouteListItem>();
			var route = row.RouteList;
			route.RemoveAddress(row);
			uow.Save(route);
			UpdateAddressesOnMap();
			RoutesWasUpdated();
		}

		protected void OnCheckShowCompletedToggled(object sender, EventArgs e)
		{
			FillDialogAtDay();
			buttonSaveChanges.Sensitive = !checkShowCompleted.Active;
		}

		#region TDIDialog

		public event EventHandler<EntitySavedEventArgs> EntitySaved;

		public bool Save()
		{
			uow.Commit();
			HasNoChanges = true;
			FillDialogAtDay();
			return true;
		}

		public void SaveAndClose()
		{
			throw new NotImplementedException();
		}

		public bool HasChanges
		{
			get
			{
				return uow.HasChanges;
			}
		}


		protected void OnButtonOpenClicked (object sender, EventArgs e)
		{
			var row = ytreeRoutes.GetSelectedObject();
			//Открываем заказ
			if (row is RouteListItem)
			{
				Order order = (row as RouteListItem).Order;
				TabParent.OpenTab(
					OrmMain.GenerateDialogHashName<Order>(order.Id),
					() => new OrderDlg (order)
				);
			}
			//Открываем МЛ
			if (row is RouteList)
			{
				RouteList routeList = row as RouteList;
				if (!HasNoChanges)
				{
					if (MessageDialogWorks.RunQuestionDialog("Сохранить маршрутный лист перед открытием?"))
						Save();
					else
						return;
				}
				TabParent.OpenTab(
					OrmMain.GenerateDialogHashName<RouteList>(routeList.Id),
					() => new RouteListCreateDlg (routeList)
				);
			}
		}
		protected override void OnDestroyed()
		{
			logger.Debug ("RoutesAtDayDlg Destroyed() called.");
			//Отписываемся от событий.
			OrmMain.GetObjectDescription<RouteList>().ObjectUpdatedGeneric -= RouteListExternalUpdated;
		}

		#endregion

		protected void OnButtonMapHelpClicked(object sender, EventArgs e)
		{
			MessageDialogWorks.RunInfoDialog("Черные маркеры — не добавленные в МЛ адреса.\n" +
				"Полосатые маркеры — адреса с временем доставки после 18:00.\n" +
				"Маркеры с красным контуром — график доставки продолжительностью менее часа.\n\n" +
				"Перетаскивание карты, правой кнопкой мыши.\n" +
				"Обычное(прямоугольное) выделение адресов на карте осуществляется перемещением мыши с нажатой левой кнопкой.\n" +
				"Для выделения по одному маркеру, зажмите Alt и левой кнопкой мыши для выделения\\удаления, кликните по нему\n" +
				"Для выделения полигоном(сложной формой), зажмите CTRL и левой кнопкой установите углы очерчивающие полигон. " +
				"В процессе работы CTRL можно отпускать и зажимат заново для добавления новых углов. " +
				"Уже зафиксированные углы полигона можно перетаскивать левой кнопкой мыши.");
		}

	}
}


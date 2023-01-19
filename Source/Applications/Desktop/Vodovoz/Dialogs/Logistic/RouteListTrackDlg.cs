using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Services;
using QS.Utilities;
using QSOrmProject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Chats;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Services;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Layout = Pango.Layout;

namespace Vodovoz
{
	public partial class RouteListTrackDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IChatRepository _chatRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;

		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private Employee _currentEmployee;
		private uint timerId;
		private const uint carRefreshInterval = 10000;
		private readonly GMapOverlay carsOverlay = new GMapOverlay("cars");
		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private readonly GMapOverlay _fastDeliveryOverlay = new GMapOverlay("fast delivery");
		private readonly GMapOverlay _districtsOverlay = new GMapOverlay("districts") { IsVisibile = false};
		private Dictionary<int, CarMarker> carMarkers;
		private Dictionary<int, CarMarkerType> lastSelectedDrivers = new Dictionary<int, CarMarkerType>();
		private Gtk.Window mapWindow;
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();
		private readonly TimeSpan _fastDeliveryTime;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly double _fastDeliveryMaxDistanceKm;

		public RouteListTrackDlg(IEmployeeRepository employeeRepository, IChatRepository chatRepository, ITrackRepository trackRepository,
			IRouteListRepository routeListRepository, IScheduleRestrictionRepository scheduleRestrictionRepository,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_chatRepository = chatRepository ?? throw new ArgumentNullException(nameof(chatRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_fastDeliveryTime =
				(deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider)))
				.MaxTimeForFastDelivery;
			_fastDeliveryMaxDistanceKm = deliveryRulesParametersProvider.MaxDistanceToLatestTrackPointKm;
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			Build();
			TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new WorkingDriversVM(uow, routelisttrackfilterview1.FilterViewModel);
			yTreeViewDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;

			routelisttrackfilterview1.FilterViewModel.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(routelisttrackfilterview1.FilterViewModel.IsFastDeliveryOnly))
				{
					Application.Invoke((s, a) => UpdateCarPosition());
				}
				if(args.PropertyName == nameof(routelisttrackfilterview1.FilterViewModel.ShowFastDeliveryCircle))
				{
					_districtsOverlay.IsVisibile = routelisttrackfilterview1.FilterViewModel.ShowFastDeliveryCircle;
					Application.Invoke((s, a) => UpdateCarPosition());
				}
			};

			_currentEmployee = employeeRepository.GetEmployeeForCurrentUser(uow);
			
			if (_currentEmployee == null)
			{
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к сотруднику. Чат не будет работать.");
			}

			//Configure map
			gmapWidget.MapProvider = GMapProviders.GoogleMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			//MapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(_districtsOverlay);
			gmapWidget.Overlays.Add(carsOverlay);
			gmapWidget.Overlays.Add(tracksOverlay);
			gmapWidget.Overlays.Add(_fastDeliveryOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
			gmapWidget.OnMarkerEnter += GmapWidgetOnMarkerEnter;
			UpdateCarPosition();
			timerId = GLib.Timeout.Add(carRefreshInterval, new GLib.TimeoutHandler (UpdateCarPosition));
			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
			yenumcomboMapType.TooltipText = "Если карта отображается некорректно или не отображается вовсе - смените тип карты";
			yenumcomboMapType.SelectedItem = MapProviders.GoogleMap;
			
			LoadFastDeliveryDistrictsGeometry();
		}

		private void LoadFastDeliveryDistrictsGeometry()
		{
			_districtsOverlay.Clear();
			var districts = _scheduleRestrictionRepository.GetDistrictsWithBorderForFastDelivery(uow);
			
			foreach(var district in districts)
			{
				var polygon = new GMapPolygon(
					district.DistrictBorder.Coordinates.Select(p => new PointLatLng(p.X, p.Y)).ToList(),
					district.DistrictName)
				{
					Fill = new SolidBrush(System.Drawing.Color.Transparent)
				};

				_districtsOverlay.Polygons.Add(polygon);
			}
		}

		private void GmapWidgetOnMarkerEnter(GMapMarker item)
		{
			if(!(item.Tag is DriverRouteListAddressVMNode node) || !node.Order.IsFastDelivery)
			{
				return;
			}

			var index = item.ToolTipText.LastIndexOf("\nОсталось времени", StringComparison.CurrentCulture);
			if(index != -1)
			{
				item.ToolTipText = item.ToolTipText.Remove(index);
			}
			if(node.RouteListItem.Status != RouteListItemStatus.EnRoute)
			{
				return;
			}
			var timeDiff = node.RouteListItem.CreationDate.Add(_fastDeliveryTime) - DateTime.Now;
			var timeRemainingStr = timeDiff.Days == 0
				? timeDiff.ToString("hh':'mm':'ss")
				: $"{Math.Abs(timeDiff.Days)} {NumberToTextRus.Case(timeDiff.Days, "день", "дня", "дней")} {timeDiff:hh':'mm':'ss}";

			if(DateTime.Now > node.RouteListItem.CreationDate.Add(_fastDeliveryTime))
			{
				timeRemainingStr = $"-{timeRemainingStr}";
			}

			item.ToolTipText += $"\nОсталось времени: {timeRemainingStr}";
		}

		void GmapWidget_ExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			if (tracksDistance.Count == 0)
				return;
			var g = args.Event.Window;
			var aria = args.Event.Area;
			int layoutWidth, layoutHeight, voffset = 0;
			var gc = gmapWidget.Style.TextGC(Gtk.StateType.Normal);

			foreach(var distance in tracksDistance)
			{
				distance.PangoLayout.GetPixelSize(out layoutWidth, out layoutHeight);
				g.DrawLayout(gc, aria.Right - 6 - layoutWidth, aria.Top + 6 + voffset, distance.PangoLayout);
				voffset += 3 + layoutHeight;
			}
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewDrivers.Selection.CountSelectedRows() > 0;
			buttonOpenKeeping.Sensitive = selected;
			UpdateSelectionOfCar();
		}

		protected void OnToggleButtonHideAddressesToggled(object sender, EventArgs e)
		{
			GtkScrolledWindow1.Visible = label2.Visible = toggleButtonHideAddresses.Active;
		}

		protected void OnYTreeViewDriversRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var driverId = yTreeViewDrivers.GetSelectedId();
			yTreeAddresses.RepresentationModel = new ViewModel.DriverRouteListAddressesVM(uow, driverId);
			yTreeAddresses.RepresentationModel.UpdateNodes();
			LoadTracksForDriver(driverId);
		}

		void UpdateSelectionOfCar()
		{
			var selectedDriverIds = yTreeViewDrivers.GetSelectedIds ();

			foreach(var driverId in selectedDriverIds)
			{
				if(!lastSelectedDrivers.ContainsKey(driverId) && carMarkers != null && carMarkers.ContainsKey(driverId))
				{
					lastSelectedDrivers.Add(driverId, carMarkers[driverId].Type);
					carMarkers[driverId].Type = CarMarkerType.BlackCar;
				}
			}

			foreach(var pair in lastSelectedDrivers.ToList())
			{
				if(!selectedDriverIds.Contains(pair.Key) && carMarkers != null)
				{
					if(carMarkers.ContainsKey(pair.Key))
					{
						carMarkers[pair.Key].Type = pair.Value;
					}

					lastSelectedDrivers.Remove(pair.Key);
				}
			}
		}

		public override void Destroy()
		{
			GLib.Source.Remove(timerId);
			gmapWidget.Destroy();
			if (mapWindow != null)
				mapWindow.Destroy();
			uow?.Dispose();
			base.Destroy();
		}

		private bool UpdateCarPosition()
		{
			try {
				var routesIds = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>)
				.SelectMany(x => x.RouteListsIds.Keys).ToArray();
				var driversWithAdditionalLoading = _routeListRepository.GetDriversWithAdditionalLoading(uow, routesIds)
					.Select(x => x.Id).ToArray();
				var start = DateTime.Now;
				var lastPoints = _trackRepository.GetLastPointForRouteLists(uow, routesIds);

				var movedDrivers = lastPoints.Where(x => x.Time > DateTime.Now.AddMinutes(-20)).Select(x => x.RouteListId).ToArray();
				var ere20Minuts = _trackRepository.GetLastPointForRouteLists(uow, movedDrivers, DateTime.Now.AddMinutes(-20));
				logger.Debug("Время запроса точек: {0}", DateTime.Now - start);
				carsOverlay.Clear();
				_fastDeliveryOverlay.Clear();
				carMarkers = new Dictionary<int, CarMarker>();
				foreach(var pointsForDriver in lastPoints.GroupBy(x => x.DriverId)) {
					var lastPoint = pointsForDriver.OrderBy(x => x.Time).Last();
					var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<WorkingDriverVMNode>)
						.First(x => x.Id == lastPoint.DriverId);

					CarMarkerType iconType;
					var ere20 = ere20Minuts.Where(x => x.DriverId == pointsForDriver.Key).OrderBy(x => x.Time).LastOrDefault();
					if(lastPoint.Time < DateTime.Now.AddMinutes(-20)) {
						iconType = driverRow.IsVodovozAuto ? CarMarkerType.BlueCarVodovoz : CarMarkerType.BlueCar;
					} else if(ere20 != null) {
						var point1 = new PointLatLng(lastPoint.Latitude, lastPoint.Longitude);
						var point2 = new PointLatLng(ere20.Latitude, ere20.Longitude);
						var diff = gmapWidget.MapProvider.Projection.GetDistance(point1, point2);
						if(diff <= 0.1)
							iconType = driverRow.IsVodovozAuto ? CarMarkerType.RedCarVodovoz : CarMarkerType.RedCar;
						else
							iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;
					} else
						iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;

					if(lastSelectedDrivers.ContainsKey(lastPoint.DriverId)) {
						lastSelectedDrivers[lastPoint.DriverId] = iconType;
						iconType = driverRow.IsVodovozAuto ? CarMarkerType.BlackCarVodovoz : CarMarkerType.BlackCar;
					}

					string text = $"{driverRow.ShortName}({driverRow.CarNumber})";
					var marker = new CarMarker(new PointLatLng(lastPoint.Latitude, lastPoint.Longitude),
						iconType);
					if(lastPoint.Time < DateTime.Now.AddSeconds(-30))
						text += lastPoint.Time.Date == DateTime.Today
							? $"\nБыл виден: {lastPoint.Time:t} "
							: $"\nБыл виден: {lastPoint.Time:g} ";
					marker.ToolTipText = text;
					carsOverlay.Markers.Add(marker);

					if(routelisttrackfilterview1.FilterViewModel.ShowFastDeliveryCircle && driversWithAdditionalLoading.Contains(pointsForDriver.Key))
					{
						_fastDeliveryOverlay.Polygons.Add(CustomPolygons.CreateCirclePolygon(new PointLatLng(lastPoint.Latitude, lastPoint.Longitude), _fastDeliveryMaxDistanceKm, Color.OrangeRed));
					}

					carMarkers.Add(lastPoint.DriverId, marker);
				}
			} catch(Exception ex) {
				logger.Error("Ошибка при обновлении позиции автомобиля", ex);
				return false;
			}
			return true;

		}

		private void LoadTracksForDriver(int driverId)
		{
			tracksOverlay.Clear();
			tracksDistance.Clear();
			//Load tracks
			var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>).FirstOrDefault(x => x.Id == driverId);
				int colorIter = 0;
			foreach(var routeId in driverRow.RouteListsIds)
			{
				var pointList = _trackRepository.GetPointsForRouteList(uow, routeId.Key);
				if (pointList.Count == 0)
					continue;

				var points = pointList.Select(p => new PointLatLng(p.Latitude, p.Longitude));

				var route = new GMapRoute(points, routeId.ToString());

				route.Stroke = new System.Drawing.Pen(GetTrackColor(colorIter));
				colorIter++;
				route.Stroke.Width = 4;
				route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

				tracksDistance.Add(MakeDistanceLayout(route));
				tracksOverlay.Routes.Add(route);
			}

			//LoadAddresses
			foreach(var point in (IList<DriverRouteListAddressVMNode>)yTreeAddresses.RepresentationModel.ItemsList)
			{
				if(point.DeliveryPoint == null)
				{
					logger.Warn("Для заказа №{0}, отсутствует точка доставки. Поэтому добавление маркера пропущено.", point.Order.Id);
					continue;
				}
				if(point.DeliveryPoint.Latitude.HasValue && point.DeliveryPoint.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					switch(point.Status)
					{
						case RouteListItemStatus.Completed:
							type = GMarkerGoogleType.green_small;
							break;
						case RouteListItemStatus.EnRoute:
							if(point.Order != null && point.Order.IsFastDelivery)
							{
								type = GMarkerGoogleType.yellow_small;
							}
							else
							{
								type = GMarkerGoogleType.gray_small;
							}
							break;
						case RouteListItemStatus.Canceled:
							type = GMarkerGoogleType.purple_small;
							break;
						case RouteListItemStatus.Overdue:
							type = GMarkerGoogleType.red_small;
							break;
						default:
							type = GMarkerGoogleType.none;
							break;
					}
					var addressMarker = new GMarkerGoogle(new PointLatLng((double)point.DeliveryPoint.Latitude, (double)point.DeliveryPoint.Longitude),	type);
					addressMarker.Tag = point;
					addressMarker.ToolTipText = $"{point.DeliveryPoint.ShortAddress}\nВремя доставки: {point.Time?.Name ?? "Не назначено"}";
					tracksOverlay.Markers.Add(addressMarker);
				}
			}
			buttonCleanTrack.Sensitive = true;
		}

		private DistanceTextInfo MakeDistanceLayout(GMapRoute route)
		{
			var layout = new Pango.Layout(this.PangoContext) {Alignment = Pango.Alignment.Right};
			var colTXT = System.Drawing.ColorTranslator.ToHtml(route.Stroke.Color);
			layout.SetMarkup(String.Format("<span foreground=\"{1}\"><span font=\"Segoe UI Symbol\">⛽</span> {0:N1} км.</span>", route.Distance, colTXT));

			return new DistanceTextInfo{
				PangoLayout = layout
			};
		}

		private System.Drawing.Color[] trackColors = new System.Drawing.Color[]{
			System.Drawing.Color.Red,
			System.Drawing.Color.Green,
			System.Drawing.Color.Blue,
			System.Drawing.Color.Coral,
			System.Drawing.Color.DarkOrange,
			System.Drawing.Color.DarkRed,
			System.Drawing.Color.DeepPink,
			System.Drawing.Color.HotPink,
			System.Drawing.Color.GreenYellow,
			System.Drawing.Color.Gold,
		};

		System.Drawing.Color GetTrackColor(int iteration)
		{
			var colorNum = iteration % 10;
			return System.Drawing.Color.FromArgb(144, trackColors[colorNum]);
		}

		protected void OnButtonMapInWindowClicked(object sender, EventArgs e)
		{
			if (mapWindow == null)
			{
				toggleButtonHideAddresses.Sensitive = false;
				toggleButtonHideAddresses.Active = false;
				mapWindow = new Gtk.Window("Карта мониторинга автомобилей на маршруте");
				mapWindow.SetDefaultSize(700, 600);
				mapWindow.DeleteEvent += MapWindow_DeleteEvent;
				vboxRight.Remove(gmapWidget);
				mapWindow.Add(gmapWidget);
				mapWindow.Show();
			}
			else
			{
				toggleButtonHideAddresses.Sensitive = true;
				mapWindow.Remove(gmapWidget);
				vboxRight.PackEnd(gmapWidget, true, true, 1);
				gmapWidget.Show();
				mapWindow.Destroy();
				mapWindow = null;
			}
		}

		void MapWindow_DeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			buttonMapInWindow.Click();
			args.RetVal = false;
		}

		protected void OnYenumcomboMapTypeChangedByUser(object sender, EventArgs e)
		{
			gmapWidget.MapProvider = MapProvidersHelper.GetPovider((MapProviders)yenumcomboMapType.SelectedItem);
		}

		protected void OnButtonCleanTrackClicked(object sender, EventArgs e)
		{
			tracksOverlay.Clear();
		}

		class DistanceTextInfo{
			public Layout PangoLayout;
		}

		protected void OnButtonRefreshClicked (object sender, EventArgs e)
		{
			logger.Info ("Обновляем данные диалога...");
			yTreeViewDrivers.RepresentationModel.UpdateNodes ();
			UpdateCarPosition ();
			logger.Info ("Ок");
		}

		protected void OnButtonOpenKeepingClicked (object sender, EventArgs e)
		{
			var selectedDrivers = yTreeViewDrivers.GetSelectedObjects<WorkingDriverVMNode> ();
			foreach (var driver in selectedDrivers) {
				foreach (var routeId in driver.RouteListsIds.Select (x => x.Key)) {
					MainClass.MainWin.TdiMain.OpenTab (
						OrmMain.GenerateDialogHashName<RouteList> (routeId),
						() => new RouteListKeepingDlg (routeId)
					);
				}
			}
		}

		protected void OnButtonTrackPointsClicked(object sender, EventArgs e)
		{
			var filterViewModel = new TrackPointJournalFilterViewModel();
			TabParent.OpenTab(() => new TrackPointJournalViewModel(filterViewModel, _unitOfWorkFactory, _commonServices));
		}
	}
}


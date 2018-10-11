using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Pango;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Chats;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository;
using Vodovoz.Repository.Chats;
using Vodovoz.ServiceDialogs.Chat;
using Vodovoz.ViewModel;
using VodovozService.Chats;

namespace Vodovoz
{
	public partial class RouteListTrackDlg : TdiTabBase, IChatCallbackObserver
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private Employee currentEmployee;
		private uint timerId;
		private const uint carRefreshInterval = 10000;
		private readonly GMapOverlay carsOverlay = new GMapOverlay("cars");
		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private Dictionary<int, CarMarker> carMarkers;
		private Dictionary<int, CarMarkerType> lastSelectedDrivers = new Dictionary<int, CarMarkerType>();
		private Gtk.Window mapWindow;
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		public RouteListTrackDlg()
		{
			this.Build();
			this.TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new ViewModel.WorkingDriversVM(uow);
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
			yTreeViewDrivers.Selection.Mode = Gtk.SelectionMode.Multiple;
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			buttonChat.Sensitive = false;
			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if (currentEmployee == null)
			{
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к сотруднику. Чат не будет работать.");
			}
			else
			{
				if (!ChatCallbackObservable.IsInitiated)
					ChatCallbackObservable.CreateInstance(currentEmployee.Id);
				ChatCallbackObservable.GetInstance().AddObserver(this);
			}

			//Configure map
			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			//MapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(carsOverlay);
			gmapWidget.Overlays.Add(tracksOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
			UpdateCarPosition();
			timerId = GLib.Timeout.Add(carRefreshInterval, new GLib.TimeoutHandler (UpdateCarPosition));
			yenumcomboMapType.ItemsEnum = typeof(MapProviders);
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
			buttonChat.Sensitive = buttonSendMessage.Sensitive = selected && currentEmployee != null;
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
				if(!lastSelectedDrivers.ContainsKey(driverId) && carMarkers.ContainsKey (driverId))
				{
					lastSelectedDrivers.Add(driverId, carMarkers[driverId].Type);
					carMarkers[driverId].Type = CarMarkerType.BlackCar;
				}
			}

			foreach(var pair in lastSelectedDrivers.ToList())
			{
				if(!selectedDriverIds.Contains(pair.Key))
				{
					carMarkers[pair.Key].Type = pair.Value;
					lastSelectedDrivers.Remove (pair.Key);
				}
			}
		}

		protected void OnButtonChatClicked(object sender, EventArgs e)
		{
			var drivers = uow.GetById<Employee>(yTreeViewDrivers.GetSelectedIds());
			foreach (var driver in drivers) {

				var chat = ChatRepository.GetChatForDriver (uow, driver);
				if (chat == null) {
					var chatUoW = UnitOfWorkFactory.CreateWithNewRoot<Chat> ();
					chatUoW.Root.ChatType = ChatType.DriverAndLogists;
					chatUoW.Root.Driver = driver;
					chatUoW.Save ();
					chat = chatUoW.Root;
				}
				TabParent.OpenTab (ChatWidget.GenerateHashName (chat.Id),
					() => new ChatWidget (chat.Id)
				);
			}
		}

		public override void Destroy()
		{
			if(ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.GetInstance().RemoveObserver(this);
			GLib.Source.Remove(timerId);
			gmapWidget.Destroy();
			if (mapWindow != null)
				mapWindow.Destroy();
			base.Destroy();
		}

		private bool UpdateCarPosition()
		{
			var routesIds = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>)
				.SelectMany(x => x.RouteListsIds.Keys).ToArray();
			var start = DateTime.Now;
			var lastPoints = Repository.Logistics.TrackRepository.GetLastPointForRouteLists(uow, routesIds);

			var movedDrivers = lastPoints.Where(x => x.Time > DateTime.Now.AddMinutes(-20)).Select(x => x.RouteListId).ToArray();
			var ere20Minuts = Repository.Logistics.TrackRepository.GetLastPointForRouteLists(uow, movedDrivers, DateTime.Now.AddMinutes(-20));
			logger.Debug("Время запроса точек: {0}", DateTime.Now - start);
			carsOverlay.Clear();
			carMarkers = new Dictionary<int, CarMarker>();
			foreach(var pointsForDriver in lastPoints.GroupBy(x => x.DriverId))
			{
				var lastPoint = pointsForDriver.OrderBy(x => x.Time).Last();
				var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<WorkingDriverVMNode>)
							.First(x => x.Id == lastPoint.DriverId);

				CarMarkerType iconType;
				var ere20 = ere20Minuts.Where(x => x.DriverId == pointsForDriver.Key).OrderBy(x => x.Time).LastOrDefault();
				if (lastPoint.Time < DateTime.Now.AddMinutes(-20))
				{
					iconType = driverRow.IsVodovozAuto ? CarMarkerType.BlueCarVodovoz : CarMarkerType.BlueCar;
				}
				else if (ere20 != null)
				{
					var point1 = new PointLatLng(lastPoint.Latitude, lastPoint.Longitude);
					var point2 = new PointLatLng(ere20.Latitude, ere20.Longitude);
					var diff = gmapWidget.MapProvider.Projection.GetDistance(point1, point2);
					if (diff <= 0.1)
						iconType = driverRow.IsVodovozAuto ? CarMarkerType.RedCarVodovoz : CarMarkerType.RedCar;
					else
						iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;
				}
				else
					iconType = driverRow.IsVodovozAuto ? CarMarkerType.GreenCarVodovoz : CarMarkerType.GreenCar;

				if(lastSelectedDrivers.ContainsKey(lastPoint.DriverId))
				{
					lastSelectedDrivers[lastPoint.DriverId] = iconType;
					iconType = driverRow.IsVodovozAuto ? CarMarkerType.BlackCarVodovoz : CarMarkerType.BlackCar;
				}
				
				string text = String.Format("{0}({1})", driverRow.ShortName, driverRow.CarNumber);
				var marker = new CarMarker(new PointLatLng(lastPoint.Latitude, lastPoint.Longitude),
					iconType);
				if (lastPoint.Time < DateTime.Now.AddSeconds(-30))
					text += lastPoint.Time.Date == DateTime.Today 
						? String.Format("\nБыл виден: {0:t} ", lastPoint.Time)
						: String.Format("\nБыл виден: {0:g} ", lastPoint.Time);
				marker.ToolTipText = text;
				carsOverlay.Markers.Add(marker);
				carMarkers.Add(lastPoint.DriverId, marker);
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
				var pointList = Repository.Logistics.TrackRepository.GetPointsForRouteList(uow, routeId.Key);
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
			foreach(var point in yTreeAddresses.RepresentationModel.ItemsList as IList<DriverRouteListAddressVMNode>)
			{
				if(point.Address == null)
				{
					logger.Warn ("Для заказа №{0}, отсутствует точка доставки. Поэтому добавление маркера пропущено.", point.OrderId);
					continue;
				}
				if(point.Address.Latitude.HasValue && point.Address.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					switch(point.Status)
					{
						case RouteListItemStatus.Completed:
							type = GMarkerGoogleType.green_small;
							break;
						case RouteListItemStatus.EnRoute:
							type = GMarkerGoogleType.gray_small;
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
					var addressMarker = new GMarkerGoogle(new PointLatLng((double)point.Address.Latitude, (double)point.Address.Longitude),	type);
					addressMarker.ToolTipText = String.Format("{0}\nВремя доставки: {1}",
						point.Address.ShortAddress,
						point.Time?.Name ??"Не назначено"
					);
					tracksOverlay.Markers.Add(addressMarker);
				}
			}
			buttonCleanTrack.Sensitive = true;
		}

		private DistanceTextInfo MakeDistanceLayout(GMapRoute route)
		{
			var layout = new Pango.Layout(this.PangoContext);
			layout.Alignment = Pango.Alignment.Right;
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

		#region IChatCallbackObserver implementation

		public void HandleChatUpdate()
		{
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
		}

		public int? ChatId { get { return null; } }

		public uint? RequestedRefreshInterval { get { return null; } }

		#endregion

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

		protected void OnButtonSendMessageClicked (object sender, EventArgs e)
		{
			var selected = yTreeViewDrivers.GetSelectedObjects<WorkingDriverVMNode> ();
			var drivers = selected.Select (x => x.Id).ToArray();
			var sendDlg = new SendMessageDlg (drivers);
			
			if(sendDlg.Run () == (int)Gtk.ResponseType.Ok)
			{
				yTreeViewDrivers.RepresentationModel.UpdateNodes ();
			}
			sendDlg.Destroy ();
		}

	}
}


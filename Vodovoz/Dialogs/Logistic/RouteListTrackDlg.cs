using System;
using System.Collections.Generic;
using System.Linq;
using Chat;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Chat;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.Repository.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListTrackDlg : TdiTabBase, IChatCallbackObserver
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private Employee currentEmployee;
		private uint timerId;
		private const uint carRefreshInterval = 30000;
		private readonly GMapOverlay carsOverlay = new GMapOverlay("cars");
		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private Dictionary<int, CarMarker> carMarkers;
		private int lastSelectedDriver = -1;
		private CarMarkerType lastMarkerType;

		public RouteListTrackDlg()
		{
			this.Build();
			this.TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new ViewModel.WorkingDriversVM(uow);
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			buttonChat.Sensitive = false;
			currentEmployee = EmployeeRepository.GetEmployeeForCurrentUser(uow);
			if (currentEmployee == null)
			{
				MessageDialogWorks.RunErrorDialog("Ваш пользователь не привязан к сотруднику. Чат не будет работать.");
				buttonChat.Sensitive = false;
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
			UpdateCarPosition();
			timerId = GLib.Timeout.Add(carRefreshInterval, new GLib.TimeoutHandler (UpdateCarPosition));
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewDrivers.Selection.CountSelectedRows() > 0;
			buttonChat.Sensitive = selected && currentEmployee != null;
			UpdateSelectionOfCar(selected);
		}

		protected void OnToggleButtonHideAddressesToggled(object sender, EventArgs e)
		{
			GtkScrolledWindow1.Visible = label2.Visible = !toggleButtonHideAddresses.Active;
		}

		protected void OnYTreeViewDriversRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			var driverId = yTreeViewDrivers.GetSelectedId();
			yTreeAddresses.RepresentationModel = new ViewModel.DriverRouteListAddressesVM(uow, driverId);
			yTreeAddresses.RepresentationModel.UpdateNodes();
			LoadTracksForDriver(driverId);
		}

		void UpdateSelectionOfCar(bool selected)
		{
			if(lastSelectedDriver > 0)
			{
				carMarkers[lastSelectedDriver].Type = lastMarkerType;
				lastSelectedDriver = -1;
			}
			if(selected)
			{
				var driverId = yTreeViewDrivers.GetSelectedId();;
				if (carMarkers.ContainsKey(driverId))
				{
					lastSelectedDriver = driverId;
					lastMarkerType = carMarkers[lastSelectedDriver].Type;
					carMarkers[lastSelectedDriver].Type = CarMarkerType.BlackCar;
				}
			}
		}

		protected void OnButtonChatClicked(object sender, EventArgs e)
		{
			var driver = uow.GetById<Employee>(yTreeViewDrivers.GetSelectedId());
			var chat = ChatRepository.GetChatForDriver(uow, driver);
			if (chat == null)
			{
				var chatUoW = UnitOfWorkFactory.CreateWithNewRoot<ChatClass>();
				chatUoW.Root.ChatType = ChatType.DriverAndLogists;
				chatUoW.Root.Driver = driver;
				chatUoW.Save();
				chat = chatUoW.Root;

			}
			TabParent.OpenTab(ChatWidget.GenerateHashName(chat.Id),
				() => new ChatWidget(chat.Id)
			);
		}

		public override void Destroy()
		{
			ChatCallbackObservable.GetInstance().RemoveObserver(this);
			GLib.Source.Remove(timerId);
			gmapWidget.Destroy();
			base.Destroy();
		}

		private bool UpdateCarPosition()
		{
			var driversIds = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>)
				.Select(x => x.Id).ToArray();
			var lastPoints = Repository.Logistics.TrackRepository.GetLastPointForDrivers(uow, driversIds);
			var movedDrivers = lastPoints.Where(x => x.Time > DateTime.Now.AddMinutes(-20)).Select(x => x.DriverId).ToArray();
			var ere20Minuts = Repository.Logistics.TrackRepository.GetLastPointForDrivers(uow, movedDrivers, DateTime.Now.AddMinutes(-20));
			carsOverlay.Clear();
			carMarkers = new Dictionary<int, CarMarker>();
			foreach(var point in lastPoints)
			{
				CarMarkerType iconType;
				var ere20 = ere20Minuts.FirstOrDefault(x => x.DriverId == point.DriverId);
				if (point.Time < DateTime.Now.AddMinutes(-20))
				{
					iconType = CarMarkerType.BlueCar;
				}
				else if (ere20 != null)
				{
					var point1 = new PointLatLng(point.Latitude, point.Longitude);
					var point2 = new PointLatLng(ere20.Latitude, ere20.Longitude);
					var diff = gmapWidget.MapProvider.Projection.GetDistance(point1, point2);
					if (diff <= 0.1)
						iconType = CarMarkerType.RedCar;
					else
						iconType = CarMarkerType.GreenCar;
				}
				else
					iconType = CarMarkerType.GreenCar;

				if(lastSelectedDriver == point.DriverId)
				{
					lastMarkerType = iconType;
					iconType = CarMarkerType.BlackCar;
				}
				
				var marker = new CarMarker(new PointLatLng(point.Latitude, point.Longitude),
					iconType);
				var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>)
					.First(x => x.Id == point.DriverId);
				string text = String.Format("{0}({1})", driverRow.ShortName, driverRow.CarNumber);
				if (point.Time < DateTime.Now.AddSeconds(-30))
					text += point.Time.Date == DateTime.Today 
						? String.Format("\nБыл виден: {0:t} ", point.Time)
						: String.Format("\nБыл виден: {0:g} ", point.Time);
				marker.ToolTipText = text;
				carsOverlay.Markers.Add(marker);
				carMarkers.Add(point.DriverId, marker);
			}
			return true;
		}

		private void LoadTracksForDriver(int driverId)
		{
			tracksOverlay.Clear();
			var driverRow = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>).FirstOrDefault(x => x.Id == driverId);
				int colorIter = 0;
			foreach(var routeId in driverRow.RouteListsIds)
			{
				var pointList = Repository.Logistics.TrackRepository.GetPointsForRouteList(uow, routeId);
				if (pointList.Count == 0)
					continue;

				var points = pointList.Select(p => new PointLatLng(p.Latitude, p.Longitude));

				var route = new GMapRoute(points, routeId.ToString());

				route.Stroke = new System.Drawing.Pen(GetTrackColor(colorIter));
				colorIter++;
				route.Stroke.Width = 4;
				route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

				tracksOverlay.Routes.Add(route);
			}
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
	}
}


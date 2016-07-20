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
		private readonly GMapOverlay carsOverlay = new GMapOverlay();

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
			//gmapWidget.MinZoom = 0;
			//gmapWidget.MaxZoom = 24;
			//gmapWidget.Zoom = 9;
			gmapWidget.HeightRequest = 150;
			//MapWidget.HasFrame = true;
			gmapWidget.Overlays.Add(carsOverlay);
			UpdateCarPosition();
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewDrivers.Selection.CountSelectedRows() > 0;
			buttonChat.Sensitive = selected && currentEmployee != null;
		}

		protected void OnToggleButtonHideAddressesToggled(object sender, EventArgs e)
		{
			GtkScrolledWindow1.Visible = label2.Visible = !toggleButtonHideAddresses.Active;
		}

		protected void OnYTreeViewDriversRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			yTreeAddresses.RepresentationModel = new ViewModel.DriverRouteListAddressesVM(uow, yTreeViewDrivers.GetSelectedId());
			yTreeAddresses.RepresentationModel.UpdateNodes();
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
			gmapWidget.Destroy();
			base.Destroy();
		}

		private void UpdateCarPosition()
		{
			var driversIds = (yTreeViewDrivers.RepresentationModel.ItemsList as IList<Vodovoz.ViewModel.WorkingDriverVMNode>)
				.Select(x => x.Id).ToArray();
			var lastPoints = Repository.Logistics.TrackRepository.GetLastPointForDrivers(uow, driversIds);
			var movedDrivers = lastPoints.Where(x => x.Time > DateTime.Now.AddMinutes(-20)).Select(x => x.DriverId).ToArray();
			var ere20Minuts = Repository.Logistics.TrackRepository.GetLastPointForDrivers(uow, movedDrivers, DateTime.Now.AddMinutes(-20));
			carsOverlay.Clear();
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
					if (diff <= 0.05)
						iconType = CarMarkerType.RedCar;
					else
						iconType = CarMarkerType.GreenCar;
				}
				else
					iconType = CarMarkerType.GreenCar;
				
				var marker = new Vodovoz.Additions.Logistic.CarMarker(new PointLatLng(point.Latitude, point.Longitude),
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
			}
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


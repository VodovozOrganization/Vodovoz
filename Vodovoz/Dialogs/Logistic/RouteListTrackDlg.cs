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
			carsOverlay.Clear();
			foreach(var point in lastPoints)
			{
				var marker = new GMap.NET.GtkSharp.Markers.GMarkerGoogle(new PointLatLng(point.Latitude, point.Longitude),
					GMap.NET.GtkSharp.Markers.GMarkerGoogleType.green_dot);
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


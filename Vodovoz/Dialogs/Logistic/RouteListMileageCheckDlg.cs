using System;
using System.Collections.Generic;
using Gamma.GtkWidgets;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using GMap.NET;
using System.Linq;
using GMap.NET.GtkSharp.Markers;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListMileageCheckDlg : OrmGtkDialogBase<RouteList>
	{
		#region Поля

		private Gtk.Window mapWindow = null;
		private GMapControl gmapWidget = new GMap.NET.GtkSharp.GMapControl();
		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		private bool editing = true;

		List<RouteListKeepingItemNode> items;

		#endregion

		public RouteListMileageCheckDlg(int id, bool canEdit = true)
		{
			this.Build ();
			editing = canEdit;
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			TabName = String.Format("Контроль за километражом маршрутного листа №{0}",Entity.Id);
			ConfigureDlg ();
		}

		#region Настройка конфигураций

		public void ConfigureDlg(){
			referenceCar.Binding.AddBinding(Entity, rl => rl.Car, widget => widget.Subject).InitializeFromSource();
			referenceCar.Sensitive = editing;

			referenceDriver.ItemsQuery = Repository.EmployeeRepository.DriversQuery ();
			referenceDriver.Binding.AddBinding(Entity, rl => rl.Driver, widget => widget.Subject).InitializeFromSource();
			referenceDriver.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceDriver.Sensitive = editing;

			referenceForwarder.ItemsQuery = Repository.EmployeeRepository.ForwarderQuery ();
			referenceForwarder.Binding.AddBinding(Entity, rl => rl.Forwarder, widget => widget.Subject).InitializeFromSource();
			referenceForwarder.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceForwarder.Sensitive = editing;

			referenceLogistican.ItemsQuery = Repository.EmployeeRepository.ActiveEmployeeQuery();
			referenceLogistican.Binding.AddBinding(Entity, rl => rl.Logistican, widget => widget.Subject).InitializeFromSource();
			referenceLogistican.SetObjectDisplayFunc<Employee> (r => StringWorks.PersonNameWithInitials (r.LastName, r.Name, r.Patronymic));
			referenceLogistican.Sensitive = editing;

			speccomboShift.ItemsList = DeliveryShiftRepository.ActiveShifts(UoW);
			speccomboShift.Binding.AddBinding(Entity, rl => rl.Shift, widget => widget.SelectedItem).InitializeFromSource();
			speccomboShift.Sensitive = editing;

			yspinActualDistance.Binding.AddBinding(Entity, rl => rl.ActualDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinActualDistance.Sensitive = editing;

			datePickerDate.Binding.AddBinding(Entity, rl => rl.Date, widget => widget.Date).InitializeFromSource();
			datePickerDate.Sensitive = editing;

			yspinConfirmedDistance.Binding.AddBinding(Entity, rl => rl.ConfirmedDistance, widget => widget.ValueAsDecimal).InitializeFromSource();
			yspinConfirmedDistance.Sensitive = editing;

			ytreeviewAddresses.ColumnsConfig = ColumnsConfigFactory.Create<RouteListKeepingItemNode>()
				.AddColumn("Заказ")
				.AddTextRenderer(node => node.RouteListItem.Order.Id.ToString())					
				.AddColumn("Адрес")
				.AddTextRenderer(node => String.Format("{0} д.{1}", node.RouteListItem.Order.DeliveryPoint.Street, node.RouteListItem.Order.DeliveryPoint.Building))					
				.AddColumn("Время")
				.AddTextRenderer(node => node.RouteListItem.Order.DeliverySchedule == null ? "" : node.RouteListItem.Order.DeliverySchedule.Name)					
				.AddColumn("Статус")
				.AddEnumRenderer(node => node.Status).Editing(false)					
				.AddColumn("Последнее редактирование")
				.AddTextRenderer(node => node.LastUpdate)
				.RowCells ()
				.AddSetter<CellRenderer> ((cell, node) => cell.CellBackgroundGdk = node.RowColor)
				.Finish();

			items = new List<RouteListKeepingItemNode>();
			foreach (var item in Entity.Addresses)
				items.Add(new RouteListKeepingItemNode{RouteListItem=item});

			items.Sort((x, y) => {
				if(x.RouteListItem.StatusLastUpdate.HasValue && y.RouteListItem.StatusLastUpdate.HasValue){
					if(x.RouteListItem.StatusLastUpdate > y.RouteListItem.StatusLastUpdate) return 1;
					if(x.RouteListItem.StatusLastUpdate < y.RouteListItem.StatusLastUpdate) return -1;
				}
				return 0;
			} );

			ytreeviewAddresses.ItemsDataSource = items;
			ConfigureMap();
		}

		private void ConfigureMap()
		{
			gmapWidget.MapProvider = GMapProviders.OpenStreetMap;
			gmapWidget.Position = new PointLatLng(59.93900, 30.31646);
			gmapWidget.HeightRequest = 150;
			gmapWidget.MinZoom = 0;
			gmapWidget.MaxZoom = 24;
			gmapWidget.Zoom = 9;
			gmapWidget.MouseWheelZoomEnabled = true;
			gmapWidget.Overlays.Add(tracksOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
		}

		#endregion

		#region implemented abstract members of OrmGtkDialogBase

		public override bool Save()
		{
			UoWGeneric.Save();
			return true;
		}
		#endregion

		#region Обработка нажатий кнопок
		protected void OnButtonConfirmClicked (object sender, EventArgs e)
		{
			Entity.ConfirmedDistance = Entity.ActualDistance;
		}

		protected void OnButtonCloseRouteListClicked (object sender, EventArgs e)
		{
			var valid = new QSValidator<RouteList>(Entity, 
				             new Dictionary<object, object>
				{
					{ "NewStatus", RouteListStatus.Closed }
				});
			if (valid.RunDlgIfNotValid((Window)this.Toplevel))
				return;

			if(Entity.ConfirmedDistance != Entity.ActualDistance)
			{
				Entity.RecalculateFuelOutlay();
			}

			yspinConfirmedDistance.Sensitive = false;
			buttonConfirm.Sensitive = false;
			buttonCloseRouteList.Sensitive = false;

			Entity.ConfirmMileage();
		}

		protected void OnButtonOpenMapClicked (object sender, EventArgs e)
		{
			ClearTracks();

			if (mapWindow == null)
			{
				mapWindow = new Gtk.Window("Трек водителя");
				mapWindow.SetDefaultSize(700, 600);
				mapWindow.DeleteEvent += MapWindow_DeleteEvent;
				mapWindow.Add(gmapWidget);

				int pointsCount = LoadTracks();
				if (pointsCount <= 0)
					MessageDialogWorks.RunInfoDialog("Нет данных о треке");
				LoadAddresses();

				gmapWidget.Show();
				mapWindow.Show();
			}
			else
			{
				mapWindow.Remove(gmapWidget);
				mapWindow.Destroy();
				mapWindow = null;
			}
		}
		#endregion

		#region Методы
		private void ClearTracks()
		{
			tracksOverlay.Clear();
			tracksDistance.Clear();
		}

		private int LoadTracks()
		{
			var pointList = Repository.Logistics.TrackRepository.GetPointsForRouteList(UoW, Entity.Id);
			if (pointList.Count == 0)
				return 0;

			var points = pointList.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			var route = new GMapRoute(points, Entity.Id.ToString());

			route.Stroke = new System.Drawing.Pen(System.Drawing.Color.Red);
			route.Stroke.Width = 4;
			route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			tracksDistance.Add(MakeDistanceLayout(route));
			tracksOverlay.Routes.Add(route);

			return pointList.Count;
		}

		private void LoadAddresses()
		{
			foreach(var orderItem in Entity.Addresses)
			{
				var point = orderItem.Order.DeliveryPoint;
				if(point.Latitude.HasValue && point.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					switch(orderItem.Status)
					{
						case RouteListItemStatus.Completed:
							type = GMarkerGoogleType.green_small;
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
					var addressMarker = new GMarkerGoogle(new PointLatLng((double)point.Latitude, (double)point.Longitude),	type);
					addressMarker.ToolTipText = point.ShortAddress;
					tracksOverlay.Markers.Add(addressMarker);
				}
			}
		}

		private DistanceTextInfo MakeDistanceLayout(GMapRoute route)
		{
			var layout = new Pango.Layout(mapWindow.PangoContext);
			layout.Alignment = Pango.Alignment.Right;
			var colTXT = System.Drawing.ColorTranslator.ToHtml(route.Stroke.Color);
			layout.SetMarkup(String.Format("<span foreground=\"{1}\"><span font=\"Segoe UI Symbol\">⛽</span> {0:N1} км.</span>", route.Distance, colTXT));

			return new DistanceTextInfo{
				PangoLayout = layout
			};
		}
		#endregion

		#region Методы событий
		void GmapWidget_ExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			if (tracksDistance.Count == 0)
				return;
			var g = args.Event.Window;
			var area = args.Event.Area;
			int layoutWidth, layoutHeight, voffset = 0;
			var gc = gmapWidget.Style.TextGC(Gtk.StateType.Normal);

			foreach(var distance in tracksDistance)
			{
				distance.PangoLayout.GetPixelSize(out layoutWidth, out layoutHeight);
				g.DrawLayout(gc, area.Right - 6 - layoutWidth, area.Top + 6 + voffset, distance.PangoLayout);
				voffset += 3 + layoutHeight;
			}
		}

		void MapWindow_DeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			buttonOpenMap.Click();
			args.RetVal = false;
		}
		#endregion

		class DistanceTextInfo{
			public Pango.Layout PangoLayout;
		}
	}
}


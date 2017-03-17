using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using QSOrmProject;
using QSProjectsLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Polylines;

namespace Dialogs.Logistic
{
	public partial class TrackOnMapWnd : Gtk.Window
	{
		#region Поля

		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private readonly GMapOverlay trackToBaseOverlay = new GMapOverlay("track_to_base");
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		RouteList routeList = null;
		Track track;
		IUnitOfWork UoW;

		#endregion

		public TrackOnMapWnd(int routeListId) : base(Gtk.WindowType.Toplevel)
		{
			this.Build();

			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.routeList = UoW.Session.QueryOver<RouteList>()
				.Where(rl => rl.Id == routeListId).SingleOrDefault();
			if (routeList == null)
				return;

			track = Vodovoz.Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, routeList.Id);
			if(track == null)
			{
				buttonRecalculateToBase.Sensitive = false;
				MessageDialogWorks.RunInfoDialog($"Маршрутный лист №{routeList.Id}\nТрек не обнаружен");
			}

			ConfigureMap();
			OpenMap();
			UpdateDistanceLabel();
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
			gmapWidget.Overlays.Add(trackToBaseOverlay);
			gmapWidget.Overlays.Add(addressesOverlay);
			gmapWidget.ExposeEvent += GmapWidget_ExposeEvent;
		}

		private void OpenMap()
		{
			ClearTracks();

			this.Title = $"Трек маршрутного листа №{routeList.Id}";
			this.SetDefaultSize(700, 600);
			this.DeleteEvent += MapWindow_DeleteEvent;

			LoadTrack();
			LoadAddresses();

			gmapWidget.Show();
		}

		private void ClearTracks()
		{
			tracksOverlay.Clear();
			tracksDistance.Clear();
		}

		void MapWindow_DeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			args.RetVal = false;
		}

		private void LoadTrack()
		{
			if (track == null)
				return;

			var points = track.TrackPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			var route = new GMapRoute(points, routeList.Id.ToString());

			route.Stroke = new System.Drawing.Pen(System.Drawing.Color.Red);
			route.Stroke.Width = 4;
			route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			tracksDistance.Add(MakeDistanceLayout(route));
			tracksOverlay.Routes.Add(route);
		}

		void UpdateDistanceLabel()
		{
			if(track == null)
			{
				labelDistance.LabelProp = String.Empty;
				return;
			}

			var text = new List<string>();
			if (track.Distance.HasValue)
				text.Add(String.Format("Дистанция трека: {0:N1} км.", track.Distance));
			if(track.DistanceToBase.HasValue)
				text.Add(String.Format("Дистанция до базы: {0:N1} км.", track.DistanceToBase));
			if (track.DistanceToBase.HasValue && track.Distance.HasValue)
				text.Add(String.Format("Общее расстояние: {0:N1} км.", track.TotalDistance));

			labelDistance.LabelProp = String.Join("\n", text);
		}

		private void LoadAddresses()
		{
			Console.WriteLine("Загружаем адреса");
			addressesOverlay.Clear();

			foreach(var orderItem in routeList.Addresses)
			{
				var point = orderItem.Order.DeliveryPoint;
				if (point == null)
					continue;
				if(point.Latitude.HasValue && point.Longitude.HasValue)
				{
					GMarkerGoogleType type;
					switch(orderItem.Status)
					{
						case RouteListItemStatus.EnRoute:
							type = GMarkerGoogleType.blue_small;
							break;
						case RouteListItemStatus.Completed:
							type = GMarkerGoogleType.green_small;
							break;
						case RouteListItemStatus.Canceled:
							type = GMarkerGoogleType.purple_small;
							break;
						case RouteListItemStatus.Overdue:
							type = GMarkerGoogleType.red_small;
							break;
						case RouteListItemStatus.Transfered:
							type = GMarkerGoogleType.gray_small;
							break;
						default:
							continue;
					}
					GMapMarker addressMarker;
					if(radioNumbers.Active && type == GMarkerGoogleType.green_small)
					{
						int index = routeList.Addresses
							.Where(x => x.Status == RouteListItemStatus.Completed)
							.OrderBy(x => x.StatusLastUpdate)
							.ToList().IndexOf(orderItem);

						addressMarker = new NumericPointMarker(new PointLatLng((double)point.Latitude, (double)point.Longitude),
							NumericPointMarkerType.green_large, index + 1);
					}
					else
						addressMarker = new GMarkerGoogle(new PointLatLng((double)point.Latitude, (double)point.Longitude),	type);
					
					var text = point.ShortAddress;
					if (orderItem.StatusLastUpdate.HasValue)
						text += String.Format("\nСтатус изменялся в {0:t}", orderItem.StatusLastUpdate.Value);
					addressMarker.ToolTipText = text;
					addressesOverlay.Markers.Add(addressMarker);
				}
			}
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

		protected void OnRadioSmallClicked(object sender, EventArgs e)
		{
			if(radioSmall.Active)
				LoadAddresses();
		}

		protected void OnRadioNumbersClicked(object sender, EventArgs e)
		{
			if(radioNumbers.Active)
				LoadAddresses();
		}

		protected void OnButtonRecalculateToBaseClicked(object sender, EventArgs e)
		{
			var track = Vodovoz.Repository.Logistics.TrackRepository.GetTrackForRouteList(UoW, routeList.Id);
			var response = track.CalculateDistanceToBase();
			UoW.Save(track);
			UoW.Commit();
			UpdateDistanceLabel();

			trackToBaseOverlay.Clear();
			var decodedPoints = Polyline.DecodePolyline(response.RouteGeometry);
			var points = decodedPoints.Select(p => new PointLatLng(p.Latitude * 0.1, p.Longitude * 0.1)).ToList();

			var route = new GMapRoute(points, "RouteToBase");
			route.Stroke = new System.Drawing.Pen(System.Drawing.Color.Blue);
			route.Stroke.Width = 4;
			route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			tracksDistance.Add(MakeDistanceLayout(route));
			trackToBaseOverlay.Routes.Add(route);

			buttonRecalculateToBase.Sensitive = false;

			MessageDialogWorks.RunInfoDialog(String.Format("Расстояние от {0} до склада {1} км. Время в пути {2}.",
				response.RouteSummary.StartPoint,
				response.RouteSummary.TotalDistanceKm,
				response.RouteSummary.TotalTime
			));
		}
	}

	class DistanceTextInfo{
		public Pango.Layout PangoLayout;
	}
}


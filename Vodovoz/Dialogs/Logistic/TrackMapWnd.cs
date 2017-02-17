using System;
using System.Collections.Generic;
using System.Linq;
using QSProjectsLib;
using QSOrmProject;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.MapProviders;
using GMap.NET.GtkSharp.Markers;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public partial class TrackMapWnd : Gtk.Window
	{
		#region Поля

		private GMapControl gmapWidget = new GMap.NET.GtkSharp.GMapControl();
		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		RouteList routeList = null;
		IUnitOfWork UoW;

		#endregion

		public TrackMapWnd(int routeListId) : base("")
		{
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			this.routeList = UoW.Session.QueryOver<RouteList>()
				.Where(rl => rl.Id == routeListId).SingleOrDefault();
			if (routeList == null)
				return;
			ConfigureMap();
			OpenMap();
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

		private void OpenMap()
		{
			ClearTracks();

			this.Title = $"Трек маршрутного листа №{routeList.Id}";
			this.SetDefaultSize(700, 600);
			this.DeleteEvent += MapWindow_DeleteEvent;
			this.Add(gmapWidget);

			int pointsCount = LoadTracks();
			if (pointsCount <= 0)
				MessageDialogWorks.RunInfoDialog($"Маршрутный лист №{routeList.Id}\nТрек не обнаружен");
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

		private int LoadTracks()
		{
			var pointList = Repository.Logistics.TrackRepository.GetPointsForRouteList(UoW, routeList.Id);
			if (pointList.Count == 0)
				return 0;

			var points = pointList.Select(p => new PointLatLng(p.Latitude, p.Longitude));

			var route = new GMapRoute(points, routeList.Id.ToString());

			route.Stroke = new System.Drawing.Pen(System.Drawing.Color.Red);
			route.Stroke.Width = 4;
			route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			tracksDistance.Add(MakeDistanceLayout(route));
			tracksOverlay.Routes.Add(route);

			return pointList.Count;
		}

		private void LoadAddresses()
		{
			foreach(var orderItem in routeList.Addresses)
			{
				var point = orderItem.Order.DeliveryPoint;
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
					var addressMarker = new GMarkerGoogle(new PointLatLng((double)point.Latitude, (double)point.Longitude),	type);
					addressMarker.ToolTipText = point.ShortAddress;
					tracksOverlay.Markers.Add(addressMarker);
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
	}

	class DistanceTextInfo{
		public Pango.Layout PangoLayout;
	}
}


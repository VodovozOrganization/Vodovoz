using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.GtkSharp;
using GMap.NET.GtkSharp.Markers;
using GMap.NET.MapProviders;
using Polylines;
using QSOrmProject;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;

namespace Dialogs.Logistic
{
	public partial class TrackOnMapWnd : Gtk.Window
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger ();

		#region Поля

		private readonly GMapOverlay tracksOverlay = new GMapOverlay("tracks");
		private readonly GMapOverlay trackToBaseOverlay = new GMapOverlay("track_to_base");
		private readonly GMapOverlay trackOnGapOverlay = new GMapOverlay ("track_on_gap");
		private readonly GMapOverlay addressesOverlay = new GMapOverlay("addresses");
		private List<DistanceTextInfo> tracksDistance = new List<DistanceTextInfo>();

		RouteList routeList = null;
		Track track;
		GMapRoute trackRoute;
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
				buttonRecalculateToBase.Sensitive = buttonFindGap.Sensitive = false;
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
			gmapWidget.Overlays.Add (trackOnGapOverlay);
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

			trackRoute = new GMapRoute(points, routeList.Id.ToString());

			trackRoute.Stroke = new System.Drawing.Pen(System.Drawing.Color.Red);
			trackRoute.Stroke.Width = 4;
			trackRoute.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			tracksDistance.Add(MakeDistanceLayout(trackRoute));
			tracksOverlay.Routes.Add(trackRoute);
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
				text.Add(String.Format("Дистанция трека: {0:N1} км.{1}", track.Distance, track.DistanceEdited ? "(пересчитанная)" : ""));
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

		private DistanceTextInfo MakeDistanceLayout(params GMapRoute[] route)
		{
			var layout = new Pango.Layout(this.PangoContext);
			layout.Alignment = Pango.Alignment.Right;
			var colTXT = System.Drawing.ColorTranslator.ToHtml(route[0].Stroke.Color);
			layout.SetMarkup(String.Format("<span foreground=\"{1}\"><span font=\"Segoe UI Symbol\">⛽</span> {0:N1} км.</span>", route.Sum(x => x.Distance), colTXT));

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

		protected void OnButtonFindGapClicked (object sender, EventArgs e)
		{
			trackOnGapOverlay.Clear ();
			string message = "Найдены разрывы в треке:";
			double replacedDistance = 0;

			TrackPoint lastPoint = null;
			foreach(var point in track.TrackPoints)
			{
				if(lastPoint == null)
				{
					lastPoint = point;
					continue;
				}

				var distance = GMapProviders.EmptyProvider.Projection.GetDistance (
					new PointLatLng (lastPoint.Latitude, lastPoint.Longitude), 
					new PointLatLng (point.Latitude, point.Longitude));
				
				if(distance > 2)
				{
					logger.Info ("Найден разрыв в треке расстоянием в {0}", distance);
					message += String.Format ("\n* разрыв c {1:t} по {2:t} — {0:N1} км.",
					                          distance,
					                          lastPoint.TimeStamp,
					                          point.TimeStamp
					                         );
					replacedDistance += distance;

					var addressesByCompletion = routeList.Addresses
							.Where (x => x.Status == RouteListItemStatus.Completed)
							.OrderBy (x => x.StatusLastUpdate)
							.ToList ();

					RouteListItem addressBeforeGap = addressesByCompletion.LastOrDefault (x => x.StatusLastUpdate < lastPoint.TimeStamp.AddMinutes (2));
					RouteListItem addressAfterGap = addressesByCompletion.FirstOrDefault (x => x.StatusLastUpdate > point.TimeStamp.AddMinutes (-2));

					var beforeIndex = addressBeforeGap == null ? -1 : addressesByCompletion.IndexOf (addressBeforeGap);
					var afterIndex = addressAfterGap == null ? addressesByCompletion.Count : addressesByCompletion.IndexOf (addressAfterGap);
					var routePoints = new List<PointOnEarth>();
					routePoints.Add (new PointOnEarth (lastPoint.Latitude, lastPoint.Longitude));

					if (afterIndex - beforeIndex > 1)
					{
						var throughAddress = addressesByCompletion.GetRange (beforeIndex + 1, afterIndex - beforeIndex - 1);
						logger.Info ("В разрыве найдены выполенные адреса порядковый(е) номер(а) {0}", String.Join (", ", throughAddress.Select (x => x.IndexInRoute)));
						routePoints.AddRange (
							throughAddress.Where (x => x.Order?.DeliveryPoint?.Latitude != null && x.Order?.DeliveryPoint?.Longitude != null)
							.Select (x => new PointOnEarth (x.Order.DeliveryPoint.Latitude.Value, x.Order.DeliveryPoint.Longitude.Value)));

						message += $" c выполненными адресами({beforeIndex + 2}-{afterIndex}) п/п в МЛ " + String.Join (", ", throughAddress.Select (x => x.IndexInRoute));
					}
					routePoints.Add (new PointOnEarth (point.Latitude, point.Longitude));

					var missedTrack = SputnikMain.GetRoute (routePoints);

					var decodedPoints = Polyline.DecodePolyline (missedTrack.RouteGeometry);
					var points = decodedPoints.Select (p => new PointLatLng (p.Latitude * 0.1, p.Longitude * 0.1)).ToList ();

					var route = new GMapRoute (points, "MissedRoute");
					route.Stroke = new System.Drawing.Pen (System.Drawing.Color.DarkMagenta);
					route.Stroke.Width = 4;
					route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;

					trackOnGapOverlay.Routes.Add (route);
				}

				lastPoint = point;
			}

			if(trackOnGapOverlay.Routes.Count > 1)
			{
				tracksDistance.Add (MakeDistanceLayout (trackOnGapOverlay.Routes.ToArray()));
				var oldDistance = track.DistanceEdited ? trackRoute.Distance : track.Distance;
				var missedRouteDistance = trackOnGapOverlay.Routes.Sum (x => x.Distance);
				var newDistance = oldDistance - replacedDistance + missedRouteDistance;
				var diffDistance = newDistance - oldDistance;

				message += $"\n Старая длинна трека:{oldDistance:N1} км." +
					$"\n Новая длинна трека: {newDistance:N1} км.(+{diffDistance:N1})" +
					"\n Сохранить изменения длинны трека?";

				if(MessageDialogWorks.RunQuestionDialog(message))
				{
					track.Distance = newDistance;
					track.DistanceEdited = true;
					UoW.Save (track);
					UoW.Commit ();
					UpdateDistanceLabel ();
				}
			}
		}
	}

	class DistanceTextInfo{
		public Pango.Layout PangoLayout;
	}
}


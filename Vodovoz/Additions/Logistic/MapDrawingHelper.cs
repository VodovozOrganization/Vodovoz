using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using GMap.NET.GtkSharp;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public static class MapDrawingHelper
	{
		public static GMapRoute DrawRoute(GMapOverlay overlay, RouteList routeList, RouteGeometryCalculator geometryCalc = null)
		{
			List<PointLatLng> points;
			var baseLat = (double)routeList.GeographicGroups.FirstOrDefault().BaseLatitude.Value;
			var baseLon = (double)routeList.GeographicGroups.FirstOrDefault().BaseLongitude.Value;

			if(geometryCalc != null) {
				var address = routeList.GenerateHashPointsOfRoute();
				StartProgress(address.Length);
				points = geometryCalc.GetGeometryOfRoute(address, UpdateProgress);
				CloseProgress();
			} else {
				points = new List<PointLatLng>();
				points.Add(new PointLatLng(baseLat, baseLon));
				points.AddRange(routeList.Addresses.Select(x => x.Order.DeliveryPoint.GmapPoint));
				points.Add(new PointLatLng(baseLat, baseLon));
			}

			var route = new GMapRoute(points, routeList.Id.ToString()) {
				Stroke = new System.Drawing.Pen(System.Drawing.Color.Blue) {
					Width = 2,
					DashStyle = System.Drawing.Drawing2D.DashStyle.Solid
				}
			};

			overlay.Routes.Add(route);
			return route;
		}

		static void StartProgress(int length)
		{
			Gtk.Application.Invoke((sender, args) => MainClass.progressBarWin.ProgressStart(length));
		}

		static void UpdateProgress(uint value, uint max)
		{
			Gtk.Application.Invoke((sender, args) => MainClass.progressBarWin.ProgressUpdate(value));
		}

		static void CloseProgress()
		{
			Gtk.Application.Invoke((sender, args) => MainClass.progressBarWin.ProgressClose());
		}

		public static void DrawAddressesOfRoute(GMapOverlay overlay, RouteList routeList)
		{
			foreach(var orderItem in routeList.Addresses) {
				var point = orderItem.Order.DeliveryPoint;
				if(point == null)
					continue;
				if(point.Latitude.HasValue && point.Longitude.HasValue) {
					GMapMarker addressMarker = new NumericPointMarker(
						new PointLatLng(
							(double)point.Latitude,
							(double)point.Longitude
						),
						NumericPointMarkerType.white_large,
						orderItem.IndexInRoute + 1
					);
					var text = point.ShortAddress;
					addressMarker.ToolTipText = text;
					overlay.Markers.Add(addressMarker);
				}
			}
		}
	}
}

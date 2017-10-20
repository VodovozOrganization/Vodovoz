using System;
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
			if(geometryCalc != null) {
				var address = GenerateHashPiontsOfRoute(routeList);
				MainClass.MainWin.ProgressStart(address.Count);
				points = geometryCalc.GetGeometryOfRoute(address.ToArray(), (val, max) => MainClass.MainWin.ProgressUpdate(val));
				MainClass.MainWin.ProgressClose();
			} else {
				points = new List<PointLatLng>();
				points.Add(DistanceCalculator.BasePoint);
				points.AddRange(routeList.Addresses.Select(x => x.Order.DeliveryPoint.GmapPoint));
				points.Add(DistanceCalculator.BasePoint);
			}

			var route = new GMapRoute(points, routeList.Id.ToString());

			route.Stroke = new System.Drawing.Pen(System.Drawing.Color.Blue);
			route.Stroke.Width = 2;
			route.Stroke.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

			overlay.Routes.Add(route);
			return route;
		}

		public static List<long> GenerateHashPiontsOfRoute(RouteList rl)
		{
			var result = new List<long>();
			result.Add(RouteGeometryCalculator.BaseHash);
			result.AddRange(rl.Addresses.Where(x => x.Order.DeliveryPoint.СoordinatesExist).Select(x => CachedDistance.GetHash(x.Order.DeliveryPoint)));
			result.Add(RouteGeometryCalculator.BaseHash);
			return result;
		}

		public static void DrawAddressesOfRoute(GMapOverlay overlay, RouteList routeList)
		{
			foreach(var orderItem in routeList.Addresses) {
				var point = orderItem.Order.DeliveryPoint;
				if(point == null)
					continue;
				if(point.Latitude.HasValue && point.Longitude.HasValue) {
					GMapMarker addressMarker = new NumericPointMarker(new PointLatLng((double)point.Latitude, (double)point.Longitude),
					                                                  NumericPointMarkerType.green_large, orderItem.IndexInRoute + 1);
					var text = point.ShortAddress;
					addressMarker.ToolTipText = text;
					overlay.Markers.Add(addressMarker);
				}
			}
		}
	}
}

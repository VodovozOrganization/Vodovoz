using GMap.NET;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Additions.Logistic
{
	public static class TrackPointsExtensions
	{
		public static PointLatLng ToPointLatLng(this TrackPoint trackPoint) => new PointLatLng(trackPoint.Latitude, trackPoint.Longitude);

		public static IList<PointLatLng> ToPointLatLng(this IList<TrackPoint> trackPoints)
		{
			var result = new List<PointLatLng>();

			foreach (var trackPoint in trackPoints)
			{
				result.Add(new PointLatLng(trackPoint.Latitude, trackPoint.Longitude));
			}

			return result;
		}
	}
}

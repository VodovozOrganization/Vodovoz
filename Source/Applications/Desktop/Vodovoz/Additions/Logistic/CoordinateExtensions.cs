using GMap.NET;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Vodovoz.Additions.Logistic
{
	public static class CoordinateExtensions
	{
		public static PointLatLng ToPointLatLng(this Coordinate coordinate) => new PointLatLng(coordinate.X, coordinate.Y);

		public static List<PointLatLng> ToPointLatLng(this IEnumerable<Coordinate> coordinates)
		{
			var result = new List<PointLatLng>();

			foreach (var coordinate in coordinates)
			{
				result.Add(coordinate.ToPointLatLng());
			}

			return result;
		}
	}
}

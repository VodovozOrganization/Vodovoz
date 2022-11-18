using System;
using GMap.NET.MapProviders;
using System.Reflection;

namespace Vodovoz.Additions.Logistic
{
	public static class MapProvidersHelper
	{
		public static GMapProvider GetPovider(MapProviders poviderEnum)
		{
			var info = typeof(GMapProviders).GetField(poviderEnum.ToString(), BindingFlags.Public | BindingFlags.Static);
			return info.GetValue(null) as GMapProvider;
		}
	}

	public enum MapProviders{
		OpenStreetMap,
		OpenStreet4UMap,
		OpenCycleMap,
		OpenCycleLandscapeMap,
		OpenCycleTransportMap,
		BingMap,
		BingSatelliteMap,
		BingHybridMap,
		GoogleMap,
		GoogleSatelliteMap,
		GoogleHybridMap,
		GoogleTerrainMap,
		YandexMap,
		YandexSatelliteMap,
		YandexHybridMap,
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Polylines;
using QSOrmProject;
using QSOsm;
using QSOsm.Osrm;
using QSOsm.Spuntik;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс позволяет получать расстояния между точками от внешнего провайдера и из кеша.
	/// В отличии от класса <c>ExtDistanceCalculator</c> у него можно спрашивать сразу маршрут последовательный
	/// маршрут, от точки А к Б потом к С, потом к Д. А так же этот класс кеширует не только время и растояния,
	/// но и геометрию движения по дорожной сети.
	/// </summary>
	public class RouteGeometryCalculator : IDistanceCalculator
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		Dictionary<int, int> calculetedRoutes = new Dictionary<int, int>();

		int totalCached, addedCached, totalErrors;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();
		public DistanceProvider Provider;

		public RouteGeometryCalculator(DistanceProvider provider)
		{
			Provider = provider;
		}

		/// <summary>
		/// Метод добавляющий заначения в словари.
		/// </summary>
		private void AddNewCacheDistance(CachedDistance distance)
		{
			if(!cache.ContainsKey(distance.FromGeoHash))
				cache[distance.FromGeoHash] = new Dictionary<long, CachedDistance>();
			cache[distance.FromGeoHash][distance.ToGeoHash] = distance;
			totalCached++;
		}

		/// <summary>
		/// Почучаем расстояния в метрах между точками
		/// </summary>
		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(fromHash, toHash);
		}

		/// <summary>
		/// Всемя пути в секундах между точками
		/// </summary>
		public int TimeSec(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toHash = CachedDistance.GetHash(toDP);
			return TimeSec(fromHash, toHash);
		}

		/// <summary>
		/// Расстояние в метрах от базы до точки.
		/// </summary>
		public int DistanceFromBaseMeter(DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(CachedDistance.BaseHash, toHash);
		}

		/// <summary>
		/// Возвращаем время от базы в секундах
		/// </summary>
		public int TimeFromBase(DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			return TimeSec(CachedDistance.BaseHash, toHash);
		}

		/// <summary>
		/// Возвращаем время до базы в секундах
		/// </summary>
		public int TimeToBase(DeliveryPoint fromDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			return TimeSec(fromHash, CachedDistance.BaseHash);
		}

		/// <summary>
		/// Расстояние в метрах от точки до базы.
		/// </summary>
		public int DistanceToBaseMeter(DeliveryPoint fromDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			return DistanceMeter(fromHash, CachedDistance.BaseHash);
		}

		private int DistanceMeter(long fromHash, long toHash)
		{
			var wayHash = new WayHash(fromHash, toHash);
			var way = GetCachedGeometry(wayHash, true);
			return way?.DistanceMeters ?? GetSimpleDistance(wayHash);
		}

		private int TimeSec(long fromHash, long toHash)
		{
			var wayHash = new WayHash(fromHash, toHash);
			var way = GetCachedGeometry(wayHash, true);
			return way?.TravelTimeSec ?? GetSimpleTime(wayHash);
		}

		public List<WayHash> GenerateWaysOfRoute(long[] route)
		{
			List<WayHash> resultRoute = new List<WayHash>();
			for (int ix = 1; ix < route.Length; ix++)
			{
				resultRoute.Add(new WayHash(route[ix-1], route[ix]));
			}
			return resultRoute;
		}

		public List<WayHash> FilterForDBCheck(List<WayHash> list)
		{
			return list.Where(x => !(cache.ContainsKey(x.FromHash) && cache[x.FromHash].ContainsKey(x.ToHash))
			                  && !ErrorWays.Any(y => x.FromHash == y.FromHash && x.ToHash == y.ToHash)
			                 )
				       .ToList();
		}

		/// <summary>
		/// Получаем расстояние по всем точкам маршрута в метрах.
		/// </summary>
		/// <returns>Расстояние в метрах</returns>
		/// <param name="route">Хеши координат точек маршрута</param>
		public int GetRouteDistance(long[] route)
		{
			var routeHash = String.Concat(route).GetHashCode();
			if (calculetedRoutes.ContainsKey(routeHash))
				return calculetedRoutes[routeHash];
			var ways = GenerateWaysOfRoute(route);
			LoadDBCacheIfNeed(ways);
			var distance = ways.Sum(x =>
			                GetCachedGeometry(x, false)?.DistanceMeters ?? GetSimpleDistance(x)
			               );
			if (distance > 0)
				calculetedRoutes.Add(routeHash, distance);
			return distance;
		}

		private int GetSimpleDistance(WayHash way)
		{
			return (int)(GMap.NET.MapProviders.GMapProviders.EmptyProvider.Projection.GetDistance(
				CachedDistance.GetPointLatLng(way.FromHash),
				CachedDistance.GetPointLatLng(way.ToHash)
			) * 1000);
		}

		private int GetSimpleTime(WayHash way)
		{
			return (int)(GetSimpleDistance(way) / 13.3); //13.3 м/с среднее время получаемое по спутнику.
		}

		/// <summary>
		/// Метод загружает недостающие расстояния из базы, если это необходимо.
		/// </summary>
		public void LoadDBCacheIfNeed(List<WayHash> ways)
		{
			var prepared = FilterForDBCheck(ways);
			if (prepared.Count > 0)
			{
				IList<CachedDistance> fromDB;
				lock(UoW)
				{
					fromDB = CachedDistanceRepository.GetCache(UoW, prepared.ToArray());
				}
				foreach (var loaded in fromDB)
				{
					AddNewCacheDistance(loaded);
				}
			}

		}

		/// <param name="pulseProgress">делегат вызываемый для отображения прогресс, первый параметр текущее значение. Второй сколько всего.</param>
		public List<PointLatLng> GetGeometryOfRoute(long[] route, Action<uint, uint> pulseProgress)
		{
			//Запрашиваем кешь одним запросом для всего маршрута.
			LoadDBCacheIfNeed(GenerateWaysOfRoute(route));

			List<PointLatLng> resultRoute = new List<PointLatLng>();
			for (int ix = 1; ix < route.Length; ix++)
			{
				CachedDistance way = GetCachedGeometry(route[ix - 1], route[ix], false);

				if(way?.PolylineGeometry != null)
				{
					var decodedPoints = Polyline.DecodePolyline(way.PolylineGeometry);
					if(Provider == DistanceProvider.Sputnik)
						resultRoute.AddRange(decodedPoints.Select(p => new PointLatLng(p.Latitude * 0.1, p.Longitude * 0.1)));
					else
						resultRoute.AddRange(decodedPoints.Select(p => new PointLatLng(p.Latitude, p.Longitude)));
				}
				else
				{
					double lat, lon;
					CachedDistance.GetLatLon(route[ix-1], out lat, out lon);
					resultRoute.Add(new PointLatLng(lat, lon));
					CachedDistance.GetLatLon(route[ix], out lat, out lon);
					resultRoute.Add(new PointLatLng(lat, lon));
				}

				if (pulseProgress != null)
					pulseProgress((uint)ix, (uint)route.Length);
			}

			return resultRoute;
		}

		CachedDistance GetCachedGeometry(WayHash way, bool checkDB = true)
		{
			return GetCachedGeometry(way.FromHash, way.ToHash, checkDB);
		}

		CachedDistance GetCachedGeometry(long fromP, long toP, bool checkDB = true)
		{
			CachedDistance distance = null;
			bool needAdd = false;
			//Проверяем в локальном кеше
			if (cache.ContainsKey(fromP) && cache[fromP].ContainsKey(toP))
			{
				distance = cache[fromP][toP];
			}
			//Проверяем в базе данных если разрешено.
			if(distance == null && checkDB)
			{
				IList<CachedDistance> list;
				lock(UoW)
				{
					list = CachedDistanceRepository.GetCache(UoW, new[] { new WayHash(fromP, toP) });
				}
				distance = list.FirstOrDefault();
			}
			//Не нашли создаем новый.
			if(distance == null)
			{
				distance = new CachedDistance();
				distance.FromGeoHash = fromP;
				distance.ToGeoHash = toP;
				needAdd = true;
			}
			if(distance.PolylineGeometry == null)
			{
				if (!UpdateFromProvider(distance))
					return null;
			}

			if (distance.PolylineGeometry == null)
				return null;

			if (needAdd)
			{
				lock(UoW)
				{
					AddNewCacheDistance(distance);
					UoW.TrySave(distance);
					UoW.Commit();
				}
			}

			return distance;
		}

		bool UpdateFromProvider(CachedDistance distance)
		{
			if (ErrorWays.Any(x => x.FromHash == distance.FromGeoHash && x.ToHash == distance.ToGeoHash))
			{
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return false;
			}

			logger.Info("Запрашиваем путь {0}->{1} у сервиса {0}.", distance.FromGeoHash, distance.ToGeoHash, Provider);
			List<PointOnEarth> points = new List<PointOnEarth>();
			double latitude, longitude;
			CachedDistance.GetLatLon(distance.FromGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			CachedDistance.GetLatLon(distance.ToGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			bool ok = false;
			if(Provider == DistanceProvider.Osrm) {
				var result = OsrmMain.GetRoute(points, false, true);
				ok = result?.Code == "Ok";
				if(ok && result.Routes.Any()) {
					distance.Created = DateTime.Now;
					distance.DistanceMeters = result.Routes.First().TotalDistance;
					distance.TravelTimeSec = result.Routes.First().TotalTimeSeconds;
					distance.PolylineGeometry = result.Routes.First().RouteGeometry;
				}
			} else {
				var result = SputnikMain.GetRoute(points, false, true);
				ok = result.Status == 0;
				if(ok) {
					distance.Created = DateTime.Now;
					distance.DistanceMeters = result.RouteSummary.TotalDistance;
					distance.TravelTimeSec = result.RouteSummary.TotalTimeSeconds;
					distance.PolylineGeometry = result.RouteGeometry;
				}
			}

			if(ok)
			{
				lock(UoW) {
					AddNewCacheDistance(distance);
					UoW.TrySave(distance);
					UoW.Commit();
				}
				addedCached++;
				return true;
			}

			ErrorWays.Add(new WayHash(distance.FromGeoHash, distance.ToGeoHash));
			totalErrors++;
			return false;
		}
	}
}

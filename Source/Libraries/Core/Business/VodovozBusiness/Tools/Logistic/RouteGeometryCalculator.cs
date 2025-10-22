using GMap.NET;
using Polylines;
using QS.DomainModel.UoW;
using QS.Osrm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Settings.Common;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс позволяет получать расстояния между точками от внешнего провайдера и из кеша.
	/// В отличии от класса ExtDistanceCalculator у него
	/// можно спрашивать сразу последовательный маршрут, от точки А к Б потом к С, потом к Д.
	/// А так же этот класс кеширует не только время и растояния, но и геометрию движения по дорожной сети.
	/// </summary>
	public class RouteGeometryCalculator : IDistanceCalculator, IDisposable
	{
		private readonly ICachedDistanceRepository _cachedDistanceRepository;
		private readonly IOsrmSettings _osrmSettings;
		private readonly IOsrmClient _osrmClient;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IUnitOfWork _uow;

		Dictionary<int, int> calculatedRoutes = new Dictionary<int, int>();

		int totalCached, addedCached, totalErrors;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public RouteGeometryCalculator(
			IUnitOfWorkFactory uowFactory,
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient,
			ICachedDistanceRepository cachedDistanceRepository
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_osrmSettings = osrmSettings ?? throw new ArgumentNullException(nameof(osrmSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_cachedDistanceRepository = cachedDistanceRepository ?? throw new ArgumentNullException(nameof(cachedDistanceRepository));
			_uow = _uowFactory.CreateWithoutRoot($"Калькулятор геометрии маршрута");
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
		public int DistanceMeter(PointCoordinates fromDeliveryPoint, PointCoordinates toDeliveryPoint)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPoint);
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			return DistanceMeter(fromHash, toHash);
		}

		/// <summary>
		/// Всемя пути в секундах между точками
		/// </summary>
		public int TimeSec(PointCoordinates fromDeliveryPoint, PointCoordinates toDeliveryPoint)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPoint);
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			return TimeSec(fromHash, toHash);
		}

		/// <summary>
		/// Расстояние в метрах от базы до точки.
		/// </summary>
		public int DistanceFromBaseMeter(PointCoordinates fromBase, PointCoordinates toDeliveryPoint)
		{
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			return DistanceMeter(fromBaseHash, toHash);
		}

		/// <summary>
		/// Возвращаем время от базы в секундах
		/// </summary>
		public int TimeFromBase(PointCoordinates fromBase, PointCoordinates toDeliveryPoint)
		{
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			return TimeSec(fromBaseHash, toHash);
		}

		/// <summary>
		/// Возвращаем время до базы в секундах
		/// </summary>
		public int TimeToBase(PointCoordinates fromDeliveryPoint, PointCoordinates toBase)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPoint);
			var toBaseHash = CachedDistance.GetHash(toBase);
			return TimeSec(fromHash, toBaseHash);
		}

		/// <summary>
		/// Расстояние в метрах от точки до базы.
		/// </summary>
		public int DistanceToBaseMeter(PointCoordinates fromDeliveryPoint, PointCoordinates toBase)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPoint);
			var toBaseHash = CachedDistance.GetHash(toBase);
			return DistanceMeter(fromHash, toBaseHash);
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
			if (calculatedRoutes.ContainsKey(routeHash))
				return calculatedRoutes[routeHash];
			var ways = GenerateWaysOfRoute(route);
			LoadDBCacheIfNeed(ways);
			var distance = ways.Sum(x =>
			                GetCachedGeometry(x, false)?.DistanceMeters ?? GetSimpleDistance(x)
			               );
			if (distance > 0)
				calculatedRoutes.Add(routeHash, distance);
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
				lock(_uow)
				{
					fromDB = _cachedDistanceRepository.GetCache(_uow, prepared.ToArray());
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
				lock(_uow)
				{
					list = _cachedDistanceRepository.GetCache(_uow, new[] { new WayHash(fromP, toP) });
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
				lock(_uow)
				{
					AddNewCacheDistance(distance);
					_uow.Save(distance);
					_uow.Commit();
				}
			}

			return distance;
		}

		bool UpdateFromProvider(CachedDistance distance)
		{
			if (ErrorWays.Any(x => x.FromHash == distance.FromGeoHash && x.ToHash == distance.ToGeoHash))
			{
				//logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return false;
			}

			//logger.Info("Запрашиваем путь {0}->{1} у сервиса {0}.", distance.FromGeoHash, distance.ToGeoHash, Provider);
			List<PointOnEarth> points = new List<PointOnEarth>();
			double latitude, longitude;
			CachedDistance.GetLatLon(distance.FromGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			CachedDistance.GetLatLon(distance.ToGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			bool ok = false;
			var result = _osrmClient.GetRoute(points, false, GeometryOverview.Full, _osrmSettings.ExcludeToll);
			ok = result?.Code == "Ok";
			if(ok && result.Routes.Any()) {
				distance.DistanceMeters = result.Routes.First().TotalDistance;
				distance.TravelTimeSec = result.Routes.First().TotalTimeSeconds;
				distance.PolylineGeometry = result.Routes.First().RouteGeometry;
			}

			if(ok)
			{
				lock(_uow) {
					AddNewCacheDistance(distance);
					_uow.Save(distance);
					_uow.Commit();
				}
				addedCached++;
				return true;
			}

			ErrorWays.Add(new WayHash(distance.FromGeoHash, distance.ToGeoHash));
			totalErrors++;
			return false;
		}

		public void Dispose() => _uow?.Dispose();
	}
}

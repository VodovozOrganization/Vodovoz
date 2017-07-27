using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Polylines;
using QSOrmProject;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Tools.Logistic
{
	public class RouteGeometrySputnikCalculator : IDistanceCalculator
	{
		#region Настройки
		public static int DistanceFalsePenality = 100000;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		public static long BaseHash = CachedDistance.GetHash(Constants.BaseLatitude, Constants.BaseLongitude);

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		Gtk.TextBuffer staticBuffer;
		int startCached, totalCached, addedCached, totalPoints, totalErrors;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public RouteGeometrySputnikCalculator(WayHash[] ways)
		{
			var fromDB = Repository.Logistics.CachedDistanceRepository.GetCache(UoW, ways, true);
			startCached = fromDB.Count;
			foreach (var distance in fromDB)
			{
				AddNewCacheDistance(distance);
			}
			UpdateText();
		}

		public RouteGeometrySputnikCalculator()
		{
			
		}

		private void AddNewCacheDistance(CachedDistance distance)
		{
			if(!cache.ContainsKey(distance.FromGeoHash))
				cache[distance.FromGeoHash] = new Dictionary<long, CachedDistance>();
			cache[distance.FromGeoHash][distance.ToGeoHash] = distance;
			UoW.TrySave(distance);
			UoW.Commit();
			totalCached++;
		}

		public int DistanceMeter(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(fromHash, toHash);
		}

		public int DistanceFromBaseMeter(DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			return DistanceMeter(BaseHash, toHash);
		}

		public int DistanceToBaseMeter(DeliveryPoint fromDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			return DistanceMeter(fromHash, BaseHash);
		}

		private int DistanceMeter(long fromHash, long toHash)
		{
			if(cache.ContainsKey(fromHash) && cache[fromHash].ContainsKey(toHash))
				return cache[fromHash][toHash].DistanceMeters;

			if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
			{
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return DistanceFalsePenality;
			}

			logger.Debug("Расстояние {0}->{1} не найдено в кеше запрашиваем.", fromHash, toHash);
			List<PointOnEarth> points = new List<PointOnEarth>();
			double latitude, longitude;
			CachedDistance.GetLatLon(fromHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			CachedDistance.GetLatLon(toHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			var result = SputnikMain.GetRoute(points, false, false);
			if(result.Status == 0)
			{
				var cachedValue = new CachedDistance {
					Created = DateTime.Now,
					DistanceMeters = result.RouteSummary.TotalDistance,
					TravelTimeSec = result.RouteSummary.TotalTimeSeconds,
					FromGeoHash = fromHash,
					ToGeoHash = toHash
				};
				UoW.TrySave(cachedValue);
				UoW.Commit();
				AddNewCacheDistance(cachedValue);
				addedCached++;
				UpdateText();
				return cachedValue.DistanceMeters;
			}
			ErrorWays.Add(new WayHash(fromHash, toHash));
			totalErrors++;
			UpdateText();
			//FIXME Реализовать запрос манхентанского расстояния.
			return DistanceFalsePenality; 
		}

		void UpdateText()
		{
			if (staticBuffer == null)
				return;
			staticBuffer.Text = String.Format("Уникальных координат: {0}\nРасстояний загружено: {1}\nРасстояний в кеше: {2}\nНовых со спутника: {3}\nОшибок в запросах: {4}",
			                                  totalPoints, startCached, totalCached, addedCached, totalErrors);
			QSMain.WaitRedraw(200);
		}

		/// <param name="pulseProgress">делегат вызываемый для отображения прогресс, первый параметр текущее значение. Второй сколько всего.</param>
		public List<PointLatLng> GetGeometryOfRoute(long[] route, Action<uint, uint> pulseProgress)
		{
			List<PointLatLng> resultRoute = new List<PointLatLng>();
			for (int ix = 1; ix < route.Length; ix++)
			{
				CachedDistance way = GetCachedGeometry(route[ix-1], route[ix]);

				if(way?.PolylineGeometry != null)
				{
					var decodedPoints = Polyline.DecodePolyline(way.PolylineGeometry);
					resultRoute.AddRange(decodedPoints.Select(p => new PointLatLng(p.Latitude * 0.1, p.Longitude * 0.1)));
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

		CachedDistance GetCachedGeometry(long fromP, long toP)
		{
			CachedDistance dist;
			bool needAdd = false;
			if (cache.ContainsKey(fromP) && cache[fromP].ContainsKey(toP))
			{
				dist = cache[fromP][toP];
			}
			else
			{
				dist = new CachedDistance();
				dist.FromGeoHash = fromP;
				dist.ToGeoHash = toP;
				needAdd = true;
			}
			if(dist.PolylineGeometry == null)
			{
				if (!UpdateFromSputnik(dist))
					return null;
			}

			if (dist.PolylineGeometry == null)
				return null;

			if (needAdd)
				AddNewCacheDistance(dist);

			return dist;
		}

		bool UpdateFromSputnik(CachedDistance distance)
		{
			if (ErrorWays.Any(x => x.FromHash == distance.FromGeoHash && x.ToHash == distance.ToGeoHash))
			{
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return false;
			}

			logger.Info("Запрашиваем путь {0}->{1} у спутника.", distance.FromGeoHash, distance.ToGeoHash);
			List<PointOnEarth> points = new List<PointOnEarth>();
			double latitude, longitude;
			CachedDistance.GetLatLon(distance.FromGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			CachedDistance.GetLatLon(distance.ToGeoHash, out latitude, out longitude);
			points.Add(new PointOnEarth(latitude, longitude));
			var result = SputnikMain.GetRoute(points, false, true);
			if (result.Status == 0)
			{
				distance.Created = DateTime.Now;
				distance.DistanceMeters = result.RouteSummary.TotalDistance;
				distance.TravelTimeSec = result.RouteSummary.TotalTimeSeconds;
				distance.PolylineGeometry = result.RouteGeometry;
				AddNewCacheDistance(distance);
				addedCached++;
				UpdateText();
				return true;
			}
			ErrorWays.Add(new WayHash(distance.FromGeoHash, distance.ToGeoHash));
			totalErrors++;
			UpdateText();
			return false;
		}
	}
}

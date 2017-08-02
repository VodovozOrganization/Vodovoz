using System;
using System.Collections.Generic;
using System.Linq;
using QSOrmProject;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Tools.Logistic
{
	public class DistanceCalculatorSputnik : IDistanceCalculator
	{
		#region Настройки
		public static int DistanceFalsePenality = 100000;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		static long BaseHash = CachedDistance.GetHash(Constants.BaseLatitude, Constants.BaseLongitude);

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		Gtk.TextBuffer staticBuffer;
		public int ProposeNeedCached = 0;
		int startCached, totalCached, addedCached, totalPoints, totalErrors;
		long totalMeters, totalSec;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public DistanceCalculatorSputnik(DeliveryPoint[] points, Gtk.TextBuffer buffer)
		{
			staticBuffer = buffer;
			var hashes = points.Select(x => CachedDistance.GetHash(x))
			                   .Concat(new[] {BaseHash})
			                   .Distinct().ToArray();
			totalPoints = hashes.Length;
			var fromDB = Repository.Logistics.CachedDistanceRepository.GetCache(UoW, hashes);
			startCached = fromDB.Count;
			foreach(var distance in fromDB)
			{
				AddNewCacheDistance(distance);
			}
			UpdateText();
		}

		private void AddNewCacheDistance(CachedDistance distance)
		{
			if(!cache.ContainsKey(distance.FromGeoHash))
				cache[distance.FromGeoHash] = new Dictionary<long, CachedDistance>();
			cache[distance.FromGeoHash][distance.ToGeoHash] = distance;
			totalCached++;
			totalMeters += distance.DistanceMeters;
			totalSec += distance.TravelTimeSec;
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

		public int TimeSec(DeliveryPoint fromDP, DeliveryPoint toDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toHash = CachedDistance.GetHash(toDP);
			return TimeSec(fromHash, toHash);
		}

		public int TimeFromBaseSec(DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			return TimeSec(BaseHash, toHash);
		}

		public int TimeToBaseSec(DeliveryPoint fromDP)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			return TimeSec(fromHash, BaseHash);
		}

		private int DistanceMeter(long fromHash, long toHash)
		{
			var dist = GetCache(fromHash, toHash);
			return dist?.DistanceMeters ?? DistanceFalsePenality;
		}

		private int TimeSec(long fromHash, long toHash)
		{
			var dist = GetCache(fromHash, toHash);
			return dist?.TravelTimeSec ?? GetSimpleTime(new WayHash(fromHash, toHash));
		}

		private int GetSimpleTime(WayHash way)
		{
			return (int)(GetSimpleDistance(way) / 13.3); //13.3 м/с среднее время получаемое по спутнику.
		}

		private int GetSimpleDistance(WayHash way)
		{
			return (int)(GMap.NET.MapProviders.GMapProviders.EmptyProvider.Projection.GetDistance(
				CachedDistance.GetPointLatLng(way.FromHash),
				CachedDistance.GetPointLatLng(way.ToHash)
			) * 1000);
		}

		private CachedDistance GetCache(long fromHash, long toHash)
		{
			if(cache.ContainsKey(fromHash) && cache[fromHash].ContainsKey(toHash))
				return cache[fromHash][toHash];

			if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
			{
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return null;
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
				return cachedValue;
			}
			ErrorWays.Add(new WayHash(fromHash, toHash));
			totalErrors++;
			UpdateText();
			//FIXME Реализовать запрос манхентанского расстояния.
			return null;
		}

		void UpdateText()
		{
			staticBuffer.Text = String.Format("Уникальных координат: {0}\nРасстояний загружено: {1}\nРасстояний в кеше: {2}\nНовых со спутника: {3}\nОшибок в запросах: {4}\nСреднее скорость: {5:F2}м/с",
			                                  totalPoints, startCached, totalCached, addedCached, totalErrors, (double)totalMeters/totalSec
			                                 );
			QSMain.WaitRedraw(200);
		}
	}

	public struct WayHash
	{
		public long FromHash;
		public long ToHash;

		public WayHash(long fromHash, long toHash)
		{
			FromHash = fromHash;
			ToHash = toHash;
		}
	}

}

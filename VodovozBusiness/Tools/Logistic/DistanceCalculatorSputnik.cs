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
		int startCached, totalCached, addedCached, totalPoints, totalErrors;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public DistanceCalculatorSputnik(DeliveryPoint[] points, Gtk.TextBuffer buffer)
		{
			staticBuffer = buffer;
			var hashes = points.Select(x => CachedDistance.GetHash(x)).Distinct().ToArray();
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
			staticBuffer.Text = String.Format("Уникальных координат: {0}\nРасстояний загружено: {1}\nРасстояний в кеше: {2}\nНовых со спутника: {3}\nОшибок в запросах: {4}",
			                                  totalPoints, startCached, totalCached, addedCached, totalErrors);
			QSMain.WaitRedraw(200);
		}

		public struct WayHash{
			public long FromHash;
			public long ToHash;

			public WayHash(long fromHash, long toHash)
			{
				FromHash = fromHash;
				ToHash = toHash;
			}
		}
	}
}

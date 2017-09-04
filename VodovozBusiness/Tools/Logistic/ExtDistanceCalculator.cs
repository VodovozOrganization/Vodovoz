﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using QSOrmProject;
using QSOsm;
using QSOsm.Osrm;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Tools.Logistic
{
	public class ExtDistanceCalculator : IDistanceCalculator
	{
		#region Настройки
		public static int DistanceFalsePenality = 100000;
		public static int SaveBy = 500;
		public static int ThreadCount = 5;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		static long BaseHash = CachedDistance.GetHash(Constants.BaseLatitude, Constants.BaseLongitude);

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		Gtk.TextBuffer statisticBuffer;
		int ProposeNeedCached = 0;
		int startCached, totalCached, addedCached, totalPoints, totalErrors;
		long totalMeters, totalSec;

		//Для работы с потоками
		long[] hashes;
		Dictionary<long, int> hashPos;
		Thread[] Threads;
		List<WayHash> inWorkWays = new List<WayHash>();
		WayHash? waitDistance;
		NextPos NextTheadsPos = new NextPos();


		int unsavedItems = 0;

		#if DEBUG
		CachedDistance[,] matrix;
		public int[,] matrixcount;
		#endif
		public DistanceProvider Provider;
		public bool MultiThreadLoad = true;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		public ExtDistanceCalculator(DistanceProvider provider, DeliveryPoint[] points, Gtk.TextBuffer buffer, bool multiThreadLoad = true)
		{
			UoW.Session.SetBatchSize(SaveBy);
			Provider = provider;
			statisticBuffer = buffer;
			MultiThreadLoad = multiThreadLoad;
			hashes = points.Select(x => CachedDistance.GetHash(x))
			                   .Concat(new[] {BaseHash})
			                   .Distinct().ToArray();
			totalPoints = hashes.Length;
			ProposeNeedCached = (hashes.Length * hashes.Length) - hashes.Length;

			hashPos = hashes.Select((hash, index) => new { hash, index }).ToDictionary(x => x.hash, x => x.index);
			#if DEBUG
			matrix = new CachedDistance[hashes.Length, hashes.Length];
			matrixcount = new int[hashes.Length, hashes.Length];
			#endif
			var fromDB = Repository.Logistics.CachedDistanceRepository.GetCache(UoW, hashes);
			startCached = fromDB.Count;
			foreach(var distance in fromDB)
			{
				#if DEBUG
				matrix[hashPos[distance.FromGeoHash], hashPos[distance.ToGeoHash]] = distance;
				#endif
				AddNewCacheDistance(distance);
			}
			UpdateText();
			#if DEBUG
			StringBuilder matrixText = new StringBuilder(" ");
			for(int x = 0; x < matrix.GetLength(1); x++)
				matrixText.Append(x % 10);

			for(int y = 0; y < matrix.GetLength(0); y++)
			{
				matrixText.Append("\n" + y % 10);
				for(int x = 0; x < matrix.GetLength(1); x++)
					matrixText.Append(matrix[y, x] != null ? 1 : 0);
			}
			logger.Debug(matrixText);

			logger.Debug(String.Join(";", hashes.Select(point => CachedDistance.GetTextLonLat(point))));
			#endif

			if(MultiThreadLoad && fromDB.Count < ProposeNeedCached)
				RunPreCalculation();
		}

		private void RunPreCalculation()
		{
			Threads = new Thread[ThreadCount];
			foreach(var ix in Enumerable.Range(0, ThreadCount))
			{
				Threads[ix] = new Thread(DoBackground);
				Threads[ix].Start();
			}
		}

		private void DoBackground()
		{
			while(waitDistance != null || NextTheadsPos.FromIx < hashes.Length)
			{
				long fromHash, toHash;

				lock(NextTheadsPos) {
					if(waitDistance != null) {
						fromHash = waitDistance.Value.FromHash;
						toHash = waitDistance.Value.ToHash;
						waitDistance = null;
						logger.Debug("Обрабатываем вне очереди [{0},{1}] очередь на [{2},{3}]", hashPos[fromHash], hashPos[toHash], NextTheadsPos.FromIx, NextTheadsPos.ToIx);
					}
					else
					{
						//exist = matrix[NextTheadsPos.FromIx, NextTheadsPos.ToIx] != null;
						fromHash = hashes[NextTheadsPos.FromIx];
						toHash = hashes[NextTheadsPos.ToIx];
						if(NextTheadsPos.ToIx >= hashes.Length - 1) {
							NextTheadsPos.FromIx++;
							NextTheadsPos.ToIx = 0;
						} else
							NextTheadsPos.ToIx++;
					}
					if(inWorkWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
						continue;
					inWorkWays.Add(new WayHash(fromHash, toHash));
				}
				if(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash))
					LoadDistanceFromService(fromHash, toHash);

				lock(NextTheadsPos)
				{
					inWorkWays.RemoveAll(x => x.FromHash == fromHash && x.ToHash == toHash);
				}
			}
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
#if DEBUG
			matrixcount[hashPos[fromHash], hashPos[toHash]] += 1;
#endif
			if(cache.ContainsKey(fromHash) && cache[fromHash].ContainsKey(toHash))
				return cache[fromHash][toHash];

			if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash)) {
				logger.Warn("Повторный запрос дистанции с ошибкой расчета. Пропускаем...");
				return null;
			}
			if(MultiThreadLoad && Threads.Any(x => x != null && x.IsAlive)) {
				waitDistance = new WayHash(fromHash, toHash);
				while(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash))
				{
					UpdateText();
					//Внутри вызывается QSMain.WaitRedraw();
				}

				return cache[fromHash][toHash];
			} else
			{
				var result = LoadDistanceFromService(fromHash, toHash);
				UpdateText();
				return result;
			}
		}

		private CachedDistance LoadDistanceFromService(long fromHash, long toHash)
		{
			List<PointOnEarth> points = new List<PointOnEarth>();
			points.Add(CachedDistance.GetPointOnEarth(fromHash));
			points.Add(CachedDistance.GetPointOnEarth(toHash));
			bool ok = false;
			CachedDistance cachedValue = null;
			if(Provider == DistanceProvider.Osrm) {
				var result = OsrmMain.GetRoute(points, false, false);
				ok = result.Code == "Ok";
				if(ok && result.Routes.Any()) {
					cachedValue = new CachedDistance {
						Created = DateTime.Now,
						DistanceMeters = result.Routes.First().TotalDistance,
						TravelTimeSec = result.Routes.First().TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			} else{
				var result = SputnikMain.GetRoute(points, false, false);
				ok = result.Status == 0;
				if(ok){
					cachedValue = new CachedDistance {
						Created = DateTime.Now,
						DistanceMeters = result.RouteSummary.TotalDistance,
						TravelTimeSec = result.RouteSummary.TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			};
			if(ok)
			{
				lock(UoW) {
					UoW.TrySave(cachedValue, false);
					unsavedItems++;
					if(unsavedItems >= SaveBy)
						FlushCache();
					AddNewCacheDistance(cachedValue);
					addedCached++;
				}
				return cachedValue;
			}
			ErrorWays.Add(new WayHash(fromHash, toHash));
			totalErrors++;
			//FIXME Реализовать запрос манхентанского расстояния.
			return null;
		}

		public void FlushCache()
		{
			if(unsavedItems <= 0)
				return;
			var start = DateTime.Now;
			UoW.Commit();
			logger.Debug("Сохранили {0} расстояний в кеш за {1} сек.", unsavedItems, (DateTime.Now - start).TotalSeconds);
			unsavedItems = 0;
		}

		void UpdateText()
		{
			if(statisticBuffer == null)
				return;

			statisticBuffer.Text = String.Format("Уникальных координат: {0}\nРасстояний загружено: {1}\nРасстояний в кеше: {2}/{7}(~{6:P})\nНовых запрошено: {3}({8})\nОшибок в запросах: {4}\nСреднее скорости: {5:F2}м/с",
			                                  totalPoints, startCached, totalCached, addedCached, totalErrors, (double)totalMeters/totalSec,
			                                  (double)totalCached/ProposeNeedCached, ProposeNeedCached, unsavedItems
			                                 );
			QSMain.WaitRedraw(100);
		}

		private class NextPos
		{
			public int FromIx;
			public int ToIx;
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

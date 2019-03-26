using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using QSOsm;
using QSOsm.Osrm;
using QSOsm.Spuntik;
using QSProjectsLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс для массового расчета расстояний между точками.
	/// В конструктор класса можно передать список точек доставки, класс автоматически в фоновом
	/// режим начнет рассчитывать матрицу расстояний между каждой точкой.
	/// </summary>
	public class ExtDistanceCalculator : IDistanceCalculator
	{
		#region Настройки
		/// <summary>
		/// Это расстояние вернется если произошла ошибка получения реального расстояния.
		/// Не возвращаем 0, потому что спутник часто возвращал ошибки. И маршруты строились к этим точкам очень плохие.
		/// </summary>
		public static int DistanceFalsePenality = 100000;
		/// <summary>
		/// Сохраняем кеш в базу данных при накоплении указанного количетва подсчитанных расстояний.
		/// </summary>
		public static int SaveBy = 5000;
		/// <summary>
		/// Количество потоков получеления расстояний от внешней службы.
		/// </summary>
		public static int ThreadCount = 5;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot("Расчет расстояний");

		Gtk.TextBuffer statisticBuffer;
		int proposeNeedCached = 0;
		DateTime? startLoadTime;
		int startCached, totalCached, addedCached, totalPoints, totalErrors;
		long totalMeters, totalSec;
		public bool Canceled { get; set; }

		//Для работы с потоками
		//FIXME рекомендую переписать работу с потоками на использование специализированных коллекций(очередей). Узанал о существовании после реализации.
		// Поэтому код относящийся к потоку сильно не задокументирован. Его лучше переписать на более простой.
		long[] hashes;
		Dictionary<long, int> hashPos;
		Thread[] Threads;
		Task[] tasks;     
		List<WayHash> inWorkWays = new List<WayHash>();
		WayHash? waitDistance;
		NextPos NextThreadsPos = new NextPos();

		int unsavedItems = 0;


		ConcurrentQueue<long> cQueue;// = new ConcurrentQueue<long>();



#if DEBUG
		CachedDistance[,] matrix;
		public int[,] matrixcount;
#endif
		public DistanceProvider Provider;
		public bool MultiThreadLoad = true;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		/// <param name="provider">Используемый провайдер данных</param>
		/// <param name="points">Точки для первоначального заполенения из базы.</param>
		/// <param name="buffer">Буфер для отображения статистики</param>
		/// <param name="multiThreadLoad">Если <c>true</c> включается моногопоточная загрузка.</param>
		public ExtDistanceCalculator(DistanceProvider provider, DeliveryPoint[] points, Gtk.TextBuffer buffer, bool multiThreadLoad = true)
		{
			UoW.Session.SetBatchSize(SaveBy);
			Provider = provider;
			statisticBuffer = buffer;
			MultiThreadLoad = multiThreadLoad;
			Canceled = false;
			var basesHashes = GeographicGroupRepository.GeographicGroupsWithCoordinates(UoW).Select(CachedDistance.GetHash);
			hashes = points.Select(CachedDistance.GetHash)
						   .Concat(basesHashes)
						   .Distinct()
						   .ToArray();



			cQueue = new ConcurrentQueue<long>(hashes);




			totalPoints = hashes.Length;
			proposeNeedCached = hashes.Length * (hashes.Length - 1);

			hashPos = hashes.Select((hash, index) => new { hash, index })
							.ToDictionary(x => x.hash, x => x.index);
#if DEBUG
			matrix = new CachedDistance[hashes.Length, hashes.Length];
			matrixcount = new int[hashes.Length, hashes.Length];
#endif
			var fromDB = Repository.Logistics.CachedDistanceRepository.GetCache(UoW, hashes);
			startCached = fromDB.Count;
			foreach(var distance in fromDB) {
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

			for(int y = 0; y < matrix.GetLength(0); y++) {
				matrixText.Append("\n" + y % 10);
				for(int x = 0; x < matrix.GetLength(1); x++)
					matrixText.Append(matrix[y, x] != null ? 1 : 0);
			}
			logger.Debug(matrixText);

			logger.Debug(string.Join(";", hashes.Select(CachedDistance.GetTextLonLat)));
#endif

			if(MultiThreadLoad && fromDB.Count < proposeNeedCached)
				RunPreCalculation();
			else
				MultiThreadLoad = false;
		}

		/// <summary>
		/// Метод запускает многопоточное скачивание.
		/// </summary>
		private void RunPreCalculation()
		{
			startLoadTime = DateTime.Now;
			tasks = new Task[ThreadCount];
			foreach(var ix in Enumerable.Range(0, ThreadCount)) {
				tasks[ix] = new Task(DoBackground);
				tasks[ix].Start();
			}
			разобраться   Task.WaitAll(tasks);
		}

		/// <summary>
		/// Метод содержащий работу одного многопоточного воркера.
		/// </summary>
		private void DoBackground()
		{
			while(cQueue.TryDequeue(out long fromHash)) {
				if(waitDistance != null) {
					long fromHash2 = waitDistance.Value.FromHash;
					long toHash = waitDistance.Value.ToHash;
					waitDistance = null;
					if(!cache.ContainsKey(fromHash2) || !cache[fromHash2].ContainsKey(toHash))
						LoadDistanceFromService(waitDistance.Value.FromHash, toHash);
				}
				foreach(var toHash in hashes) {
					if(Canceled) {
						MultiThreadLoad = false;
						Thread.CurrentThread.Abort();
						break;
					}
					if(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash))
						LoadDistanceFromService(fromHash, toHash);
				}
			}

			Gtk.Application.Invoke(delegate {
				CheckAndDisableThreads();
			});
		}

		/// <summary>
		/// Метод отключающий режим многопоточного скачивания, при завершении работы всех потоков.
		/// </summary>
		private void CheckAndDisableThreads()
		{
			if(tasks.All(x => !x.IsCompleted))
				MultiThreadLoad = false;
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
			totalMeters += distance.DistanceMeters;
			totalSec += distance.TravelTimeSec;
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
		/// Расстояние в метрах от базы до точки.
		/// </summary>
		public int DistanceFromBaseMeter(GeographicGroup fromBase, DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			return DistanceMeter(fromBaseHash, toHash);
		}

		/// <summary>
		/// Расстояние в метрах от точки до базы.
		/// </summary>
		public int DistanceToBaseMeter(DeliveryPoint fromDP, GeographicGroup toBase)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toBaseHash = CachedDistance.GetHash(toBase);
			return DistanceMeter(fromHash, toBaseHash);
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
		/// Время пути в секундах от базы до точки
		/// </summary>
		public int TimeFromBaseSec(GeographicGroup fromBase, DeliveryPoint toDP)
		{
			var toHash = CachedDistance.GetHash(toDP);
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			return TimeSec(fromBaseHash, toHash);
		}

		/// <summary>
		/// Время пути в секундах от точки до базы.
		/// </summary>
		public int TimeToBaseSec(DeliveryPoint fromDP, GeographicGroup toBase)
		{
			var fromHash = CachedDistance.GetHash(fromDP);
			var toBaseHash = CachedDistance.GetHash(toBase);
			return TimeSec(fromHash, toBaseHash);
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
			//if(MultiThreadLoad && Threads.Any(x => x != null && x.IsAlive)) {
			if(MultiThreadLoad) {
				waitDistance = new WayHash(fromHash, toHash);
				while(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash)) {
					//Внутри вызывается QSMain.WaitRedraw();
					UpdateText();
					//Если по какой то причине, не получили расстояние. Не висим. Пробуем еще раз через сервис.
					if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
						return LoadDistanceFromService(fromHash, toHash);
				}

				return cache[fromHash][toHash];
			} else {
				var result = LoadDistanceFromService(fromHash, toHash);
				UpdateText();
				return result;
			}
		}

		private CachedDistance LoadDistanceFromService(long fromHash, long toHash)
		{
			CachedDistance cachedValue = null;
			if(fromHash == toHash) {
				cachedValue = new CachedDistance {
					DistanceMeters = 0,
					TravelTimeSec = 0,
					FromGeoHash = fromHash,
					ToGeoHash = toHash
				};
				AddNewCacheDistance(cachedValue);
				addedCached++;
				return cachedValue;
			}

			List<PointOnEarth> points = new List<PointOnEarth> {
				CachedDistance.GetPointOnEarth(fromHash),
				CachedDistance.GetPointOnEarth(toHash)
			};
			bool ok = false;
			if(Provider == DistanceProvider.Osrm) {
				var result = OsrmMain.GetRoute(points, false, GeometryOverview.False);
				ok = result?.Code == "Ok";
				if(ok && result.Routes.Any()) {
					cachedValue = new CachedDistance {
						DistanceMeters = result.Routes.First().TotalDistance,
						TravelTimeSec = result.Routes.First().TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			} else {
				var result = SputnikMain.GetRoute(points, false, false);
				ok = result.Status == 0;
				if(ok) {
					cachedValue = new CachedDistance {
						DistanceMeters = result.RouteSummary.TotalDistance,
						TravelTimeSec = result.RouteSummary.TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			}
			if(ok) {
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

		/// <summary>
		/// Сохраняем накопленные рассчитанные значения в базу.
		/// </summary>
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

			double remainTime = 0;
			if(startLoadTime.HasValue)
				remainTime = (DateTime.Now - startLoadTime.Value).Ticks * ((double)(proposeNeedCached - totalCached) / addedCached);
			statisticBuffer.Text = string.Format(
													"Уникальных координат: {0}\nРасстояний загружено: {1}\nРасстояний в кеше: {2}/{7}(~{6:P})\nОсталось времени: {9:hh\\:mm\\:ss}\nНовых запрошено: {3}({8})\nОшибок в запросах: {4}\nСреднее скорости: {5:F2}м/с",
													totalPoints,
													startCached,
													totalCached,
													addedCached,
													totalErrors,
													(double)totalMeters / totalSec,
													(double)totalCached / proposeNeedCached,
													proposeNeedCached,
													unsavedItems,
													TimeSpan.FromTicks((long)remainTime)
												);
			QSMain.WaitRedraw(100);
		}

		private class NextPos
		{
			public int FromIx;
			public int ToIx;
		}
	}

	/// <summary>
	/// Структура храняшая 2 хеша кординат точки отправления и точки прибытия.
	/// </summary>
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
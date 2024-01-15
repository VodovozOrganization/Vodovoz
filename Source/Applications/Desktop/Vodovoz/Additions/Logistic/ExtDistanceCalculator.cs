using QS.DomainModel.UoW;
using QS.Osrm;
using QSProjectsLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Tools.Logistic
{
	/// <summary>
	/// Класс для массового расчета расстояний между точками.
	/// В конструктор класса можно передать список точек доставки, класс автоматически в фоновом
	/// режим начнет рассчитывать матрицу расстояний между каждой точкой.
	/// </summary>
	public class ExtDistanceCalculator : IDistanceCalculator, IDisposable
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
		public static int TasksCount = 5;
		#endregion

		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private readonly ICachedDistanceRepository _cachedDistanceRepository = new CachedDistanceRepository();
		private readonly IGlobalSettings _globalSettings = new GlobalSettings(new ParametersProvider());

		IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot("Расчет расстояний");

		int proposeNeedCached = 0;
		readonly Action<string> statisticsTxtAction;

		DateTime? startLoadTime;
		int startCached, totalCached, addedCached, totalPoints, totalErrors;
		long totalMeters, totalSec;
		public bool Canceled { get; set; }

		//Для работы с потоками
		//FIXME рекомендую переписать работу с потоками на использование специализированных коллекций(очередей). Узанал о существовании после реализации.
		// Поэтому код относящийся к потоку сильно не задокументирован. Его лучше переписать на более простой.
		long[] hashes;
		Dictionary<long, int> hashPos;
		WayHash? waitDistance;

		int unsavedItems = 0;

		Task<int>[] tasks;
		ConcurrentQueue<long> cQueue;

#if DEBUG
		CachedDistance[,] matrix;
		public int[,] matrixcount;
#endif
		public bool MultiTaskLoad = true;

		private Dictionary<long, Dictionary<long, CachedDistance>> cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays = new List<WayHash>();

		/// <param name="provider">Используемый провайдер данных</param>
		/// <param name="points">Точки для первоначального заполенения из базы.</param>
		/// <param name="statisticsTxtAction">Функция для буфера для отображения статистики</param>
		/// <param name="multiThreadLoad">Если <c>true</c> включается моногопоточная загрузка.</param>
		public ExtDistanceCalculator(DeliveryPoint[] points, IEnumerable<GeoGroupVersion> geoGroupVersions, Action<string> statisticsTxtAction, bool multiThreadLoad = true)
		{
			this.statisticsTxtAction = statisticsTxtAction;
			UoW.Session.SetBatchSize(SaveBy);
			MultiTaskLoad = multiThreadLoad;
			Canceled = false;
			var basesHashes = geoGroupVersions.Select(x => CachedDistance.GetHash(x.PointCoordinates));
			hashes = points.Select(x => CachedDistance.GetHash(x.PointCoordinates))
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
			var fromDB = _cachedDistanceRepository.GetCache(UoW, hashes);
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

			if(MultiTaskLoad && fromDB.Count < proposeNeedCached)
				RunPreCalculation();
			else
				MultiTaskLoad = false;
		}

		/// <summary>
		/// Метод запускает многопоточное скачивание.
		/// </summary>
		private void RunPreCalculation()
		{
			startLoadTime = DateTime.Now;
			tasks = new Task<int>[TasksCount];
			foreach(var ix in Enumerable.Range(0, TasksCount)) {
				tasks[ix] = new Task<int>(DoBackground);
				tasks[ix].Start();
			}

			while(MultiTaskLoad) {
				Gtk.Main.Iteration();
			}
		}

		/// <summary>
		/// Метод содержащий работу одного многопоточного воркера.
		/// </summary>
		private int DoBackground()
		{
			int result = 0;
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
						MultiTaskLoad = false;
						result = -1;
						break;
					}
					if(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash))
						LoadDistanceFromService(fromHash, toHash);
					result = 1;
				}
				Gtk.Application.Invoke(delegate {
					UpdateText();
				});
			}

			Gtk.Application.Invoke(delegate {
				CheckAndDisableTasks();
			});
			return result;
		}

		/// <summary>
		/// Метод отключающий режим многопоточного скачивания, при завершении работы всех задач.
		/// </summary>
		void CheckAndDisableTasks()
		{
			//if(tasks.All(x => x.Result == 1))
			MultiTaskLoad = false;
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
		public int DistanceMeter(PointCoordinates fromDeliveryPont, PointCoordinates toDeliveryPont)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPont);
			var toHash = CachedDistance.GetHash(toDeliveryPont);
			return DistanceMeter(fromHash, toHash);
		}

		/// <summary>
		/// Расстояние в метрах от базы до точки.
		/// </summary>
		public int DistanceFromBaseMeter(PointCoordinates fromBase, PointCoordinates toDeliveryPoint)
		{
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			return DistanceMeter(fromBaseHash, toHash);
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

		/// <summary>
		/// Всемя пути в секундах между точками
		/// </summary>
		public int TimeSec(PointCoordinates fromDeliveryPont, PointCoordinates toDeliveryPoint)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPont);
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			return TimeSec(fromHash, toHash);
		}

		/// <summary>
		/// Время пути в секундах от базы до точки
		/// </summary>
		public int TimeFromBaseSec(PointCoordinates fromBase, PointCoordinates toDeliveryPoint)
		{
			var toHash = CachedDistance.GetHash(toDeliveryPoint);
			var fromBaseHash = CachedDistance.GetHash(fromBase);
			return TimeSec(fromBaseHash, toHash);
		}

		/// <summary>
		/// Время пути в секундах от точки до базы.
		/// </summary>
		public int TimeToBaseSec(PointCoordinates fromDeliveryPont, PointCoordinates toBase)
		{
			var fromHash = CachedDistance.GetHash(fromDeliveryPont);
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
				logger.Warn(string.Format("Повторный запрос дистанции с ошибкой расчета для FromHash = {0} и ToHash = {1}. Пропускаем...", fromHash, toHash));
				return null;
			}
			if(MultiTaskLoad && tasks.Any(x => x != null && !x.IsCompleted)) {
				waitDistance = new WayHash(fromHash, toHash);
				while(!cache.ContainsKey(fromHash) || !cache[fromHash].ContainsKey(toHash)) {
					//Внутри вызывается QSMain.WaitRedraw();
					UpdateText();
					//Если по какой то причине, не получили расстояние. Не висим. Пробуем еще раз через сервис.
					if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
						return LoadDistanceFromService(fromHash, toHash);
				}

				return cache[fromHash][toHash];
			}
			var result = LoadDistanceFromService(fromHash, toHash);
			UpdateText();
			return result;
		}

		private CachedDistance LoadDistanceFromService(long fromHash, long toHash)
		{
			CachedDistance cachedValue = null;
			bool ok = false;
			if(fromHash == toHash) {
				cachedValue = new CachedDistance {
					DistanceMeters = 0,
					TravelTimeSec = 0,
					FromGeoHash = fromHash,
					ToGeoHash = toHash
				};
				AddNewCacheDistance(cachedValue);
				addedCached++;
				ok = true;
			}

			if(!ok) {
				List<PointOnEarth> points = new List<PointOnEarth> {
					CachedDistance.GetPointOnEarth(fromHash),
					CachedDistance.GetPointOnEarth(toHash)
				};
				var result = OsrmClientFactory.Instance.GetRoute(points, false, GeometryOverview.False, _globalSettings.ExcludeToll);
				ok = result?.Code == "Ok";
				if(ok && result.Routes.Any()) {
					cachedValue = new CachedDistance {
						DistanceMeters = result.Routes.First().TotalDistance,
						TravelTimeSec = result.Routes.First().TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			}
			if(MultiTaskLoad && ok) {
				lock(UoW) {
					UoW.TrySave(cachedValue as CachedDistance, false);
					unsavedItems++;
					if(unsavedItems >= SaveBy)
						FlushCache();
					AddNewCacheDistance(cachedValue);
					addedCached++;
				}
				return cachedValue;
			}
			if(ok) {
				AddNewCacheDistance(cachedValue);
				addedCached++;
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
			if(statisticsTxtAction == null)
				return;

			double remainTime = 0;
			if(startLoadTime.HasValue)
				remainTime = (DateTime.Now - startLoadTime.Value).Ticks * ((double)(proposeNeedCached - totalCached) / addedCached);
			statisticsTxtAction.Invoke(
				string.Format(
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
				)
			);
			QSMain.WaitRedraw(100);
		}

		public void Dispose()
		{
			UoW.Dispose();
		}

		private class NextPos
		{
			public int FromIx;
			public int ToIx;
		}
	}
}

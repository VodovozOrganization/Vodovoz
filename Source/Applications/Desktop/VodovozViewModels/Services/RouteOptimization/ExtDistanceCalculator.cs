using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Osrm;
using QS.Project.Services;
using Vodovoz.Core.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Factories;
using Vodovoz.Settings.Common;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Services.RouteOptimization
{
	/// <summary>
	/// Класс для массового расчета расстояний между точками.
	/// В конструктор класса можно передать список точек доставки, класс автоматически в фоновом
	/// режим начнет рассчитывать матрицу расстояний между каждой точкой.
	/// </summary>
	public partial class ExtDistanceCalculator : IExtDistanceCalculator, IDisposable
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

		private readonly ILogger<Vodovoz.ViewModels.Services.RouteOptimization.ExtDistanceCalculator> _logger;
		private readonly ICachedDistanceRepository _cachedDistanceRepository;
		private readonly IOsrmSettings _osrmSettings;
		private readonly IOsrmClient _osrmClient;
		private IUnitOfWork _unitOfWork = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Расчет расстояний");
		private int _proposeNeedCached = 0;
		private readonly Action<string> _statisticsTxtAction;
		private DateTime? _startLoadTime;
		private int _startCached;
		private int _totalCached;
		private int _addedCached;
		private int _totalPoints;
		private int _totalErrors;
		private long _totalMeters;
		private long _totalSec;
		public bool Canceled { get; set; }

		//Для работы с потоками
		//FIXME рекомендую переписать работу с потоками на использование специализированных коллекций(очередей). Узанал о существовании после реализации.
		// Поэтому код относящийся к потоку сильно не задокументирован. Его лучше переписать на более простой.
		private long[] _hashes;
		private Dictionary<long, int> _hashPos;
		private WayHash? _waitDistance;
		private int _unsavedItems = 0;
		private Task<int>[] _tasks;
		private ConcurrentQueue<long> _cQueue;

#if DEBUG
		private CachedDistance[,] _matrix;
		public int[,] MatrixCount { get; private set; }
#endif
		public bool MultiTaskLoad = true;

		private Dictionary<long, Dictionary<long, CachedDistance>> _cache = new Dictionary<long, Dictionary<long, CachedDistance>>();

		public List<WayHash> ErrorWays { get; } = new List<WayHash>();

		/// <param name="provider">Используемый провайдер данных</param>
		/// <param name="points">Точки для первоначального заполенения из базы.</param>
		/// <param name="statisticsTxtAction">Функция для буфера для отображения статистики</param>
		/// <param name="multiThreadLoad">Если <c>true</c> включается моногопоточная загрузка.</param>
		public ExtDistanceCalculator(
			ILogger<Vodovoz.ViewModels.Services.RouteOptimization.ExtDistanceCalculator> logger,
			IOsrmSettings osrmSettings,
			IOsrmClient osrmClient,
			ICachedDistanceRepository cachedDistanceRepository,
			DeliveryPoint[] points,
			IEnumerable<GeoGroupVersion> geoGroupVersions,
			Action<string> statisticsTxtAction,
			bool multiThreadLoad = true)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_osrmSettings = osrmSettings ?? throw new ArgumentNullException(nameof(osrmSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_cachedDistanceRepository = cachedDistanceRepository ?? throw new ArgumentNullException(nameof(cachedDistanceRepository));
			_statisticsTxtAction = statisticsTxtAction;
			_unitOfWork.Session.SetBatchSize(SaveBy);
			MultiTaskLoad = multiThreadLoad;
			Canceled = false;
			var basesHashes = geoGroupVersions.Select(x => CachedDistance.GetHash(x.PointCoordinates));

			_hashes = points.Select(x => CachedDistance.GetHash(x.PointCoordinates))
				.Concat(basesHashes)
				.Distinct()
				.ToArray();

			_cQueue = new ConcurrentQueue<long>(_hashes);

			_totalPoints = _hashes.Length;
			_proposeNeedCached = _hashes.Length * (_hashes.Length - 1);

			_hashPos = _hashes.Select((hash, index) => new { hash, index })
							.ToDictionary(x => x.hash, x => x.index);
#if DEBUG
			_matrix = new CachedDistance[_hashes.Length, _hashes.Length];
			MatrixCount = new int[_hashes.Length, _hashes.Length];
#endif
			var fromDB = _cachedDistanceRepository.GetCache(_unitOfWork, _hashes);
			_startCached = fromDB.Count;

			foreach(var distance in fromDB)
			{
#if DEBUG
				_matrix[_hashPos[distance.FromGeoHash], _hashPos[distance.ToGeoHash]] = distance;
#endif
				AddNewCacheDistance(distance);
			}

			UpdateText();
#if DEBUG
			var matrixText = new StringBuilder(" ");

			for(int x = 0; x < _matrix.GetLength(1); x++)
			{
				matrixText.Append(x % 10);
			}

			for(int y = 0; y < _matrix.GetLength(0); y++)
			{
				matrixText.Append("\n" + y % 10);

				for(int x = 0; x < _matrix.GetLength(1); x++)
				{
					matrixText.Append(_matrix[y, x] != null ? 1 : 0);
				}
			}

			_logger.LogDebug(matrixText.ToString());

			_logger.LogDebug(string.Join(";", _hashes.Select(CachedDistance.GetTextLonLat)));
#endif

			if(MultiTaskLoad && fromDB.Count < _proposeNeedCached)
			{
				RunPreCalculation();
			}
			else
			{
				MultiTaskLoad = false;
			}
		}

		/// <summary>
		/// Метод запускает многопоточное скачивание.
		/// </summary>
		private void RunPreCalculation()
		{
			_startLoadTime = DateTime.Now;
			_tasks = new Task<int>[TasksCount];

			foreach(var ix in Enumerable.Range(0, TasksCount))
			{
				_tasks[ix] = new Task<int>(DoBackground);
				_tasks[ix].Start();
			}

			while(MultiTaskLoad)
			{
				Task.Delay(TimeSpan.FromSeconds(1));
			}
		}

		/// <summary>
		/// Метод содержащий работу одного многопоточного воркера.
		/// </summary>
		private int DoBackground()
		{
			int result = 0;
			while(_cQueue.TryDequeue(out long fromHash))
			{
				if(_waitDistance != null)
				{
					long fromHash2 = _waitDistance.Value.FromHash;
					long toHash = _waitDistance.Value.ToHash;
					_waitDistance = null;
					if(!_cache.ContainsKey(fromHash2) || !_cache[fromHash2].ContainsKey(toHash))
					{
						LoadDistanceFromService(_waitDistance.Value.FromHash, toHash);
					}
				}
				foreach(var toHash in _hashes)
				{
					if(Canceled)
					{
						MultiTaskLoad = false;
						result = -1;
						break;
					}
					if(!_cache.ContainsKey(fromHash) || !_cache[fromHash].ContainsKey(toHash))
					{
						LoadDistanceFromService(fromHash, toHash);
					}

					result = 1;
				}
				UpdateText();
			}

			CheckAndDisableTasks();
			return result;
		}

		/// <summary>
		/// Метод отключающий режим многопоточного скачивания, при завершении работы всех задач.
		/// </summary>
		private void CheckAndDisableTasks()
		{
			MultiTaskLoad = false;
		}

		/// <summary>
		/// Метод добавляющий заначения в словари.
		/// </summary>
		private void AddNewCacheDistance(CachedDistance distance)
		{
			if(!_cache.ContainsKey(distance.FromGeoHash))
			{
				_cache[distance.FromGeoHash] = new Dictionary<long, CachedDistance>();
			}

			_cache[distance.FromGeoHash][distance.ToGeoHash] = distance;
			_totalCached++;
			_totalMeters += distance.DistanceMeters;
			_totalSec += distance.TravelTimeSec;
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
			if(_cache.ContainsKey(fromHash) && _cache[fromHash].ContainsKey(toHash))
			{
				return _cache[fromHash][toHash];
			}

			if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
			{
				_logger.LogWarning("Повторный запрос дистанции с ошибкой расчета для FromHash = {FromHash} и ToHash = {ToHash}. Пропускаем...", fromHash, toHash);
				return null;
			}

			if(MultiTaskLoad && _tasks.Any(x => x != null && !x.IsCompleted))
			{
				_waitDistance = new WayHash(fromHash, toHash);

				while(!_cache.ContainsKey(fromHash) || !_cache[fromHash].ContainsKey(toHash))
				{
					//Внутри вызывается QSMain.WaitRedraw();
					UpdateText();

					//Если по какой то причине, не получили расстояние. Не висим. Пробуем еще раз через сервис.
					if(ErrorWays.Any(x => x.FromHash == fromHash && x.ToHash == toHash))
					{
						return LoadDistanceFromService(fromHash, toHash);
					}
				}

				return _cache[fromHash][toHash];
			}
			var result = LoadDistanceFromService(fromHash, toHash);
			UpdateText();

			return result;
		}

		private CachedDistance LoadDistanceFromService(long fromHash, long toHash)
		{
			CachedDistance cachedValue = null;
			bool ok = false;

			if(fromHash == toHash)
			{
				cachedValue = new CachedDistance
				{
					DistanceMeters = 0,
					TravelTimeSec = 0,
					FromGeoHash = fromHash,
					ToGeoHash = toHash
				};

				AddNewCacheDistance(cachedValue);
				_addedCached++;
				ok = true;
			}

			if(!ok)
			{
				var points = new List<PointOnEarth>
				{
					CachedDistance.GetPointOnEarth(fromHash),
					CachedDistance.GetPointOnEarth(toHash)
				};

				var result = _osrmClient.GetRoute(points, false, GeometryOverview.False, _osrmSettings.ExcludeToll);
				ok = result?.Code == "Ok";

				if(ok && result.Routes.Any())
				{
					cachedValue = new CachedDistance
					{
						DistanceMeters = result.Routes.First().TotalDistance,
						TravelTimeSec = result.Routes.First().TotalTimeSeconds,
						FromGeoHash = fromHash,
						ToGeoHash = toHash
					};
				}
			}

			if(MultiTaskLoad && ok)
			{
				lock(_unitOfWork)
				{
					_unitOfWork.Save(cachedValue, false);
					_unsavedItems++;
					if(_unsavedItems >= SaveBy)
					{
						FlushCache();
					}

					AddNewCacheDistance(cachedValue);
					_addedCached++;
				}
				return cachedValue;
			}

			if(ok)
			{
				AddNewCacheDistance(cachedValue);
				_addedCached++;
				return cachedValue;
			}

			ErrorWays.Add(new WayHash(fromHash, toHash));
			_totalErrors++;
			//FIXME Реализовать запрос манхентанского расстояния.
			return null;
		}

		/// <summary>
		/// Сохраняем накопленные рассчитанные значения в базу.
		/// </summary>
		public void FlushCache()
		{
			if(_unsavedItems <= 0)
			{
				return;
			}

			var start = DateTime.Now;
			_unitOfWork.Commit();
			_logger.LogDebug("Сохранили {UnsavedItemsCount} расстояний в кеш за {TotalSeconds} сек.", _unsavedItems, (DateTime.Now - start).TotalSeconds);
			_unsavedItems = 0;
		}

		private void UpdateText()
		{
			if(_statisticsTxtAction == null)
			{
				return;
			}

			double remainTime = 0;
			if(_startLoadTime.HasValue)
			{
				remainTime = (DateTime.Now - _startLoadTime.Value).Ticks * ((double)(_proposeNeedCached - _totalCached) / _addedCached);
			}

			_statisticsTxtAction.Invoke(
				$"Уникальных координат: {_totalPoints}\n" +
				$"Расстояний загружено: {_startCached}\n" +
				$"Расстояний в кеше: {_totalCached}/{_proposeNeedCached}(~{(double)_totalCached / _proposeNeedCached:P})\n" +
				$"Осталось времени: {TimeSpan.FromTicks((long)remainTime):hh\\:mm\\:ss}\n" +
				$"Новых запрошено: {_addedCached}({_unsavedItems})\n" +
				$"Ошибок в запросах: {_totalErrors}\n" +
				$"Среднее скорости: {(double)_totalMeters / _totalSec:F2}м/с"
			);
		}

		public void Dispose()
		{
			_unitOfWork.Dispose();
		}
	}
}

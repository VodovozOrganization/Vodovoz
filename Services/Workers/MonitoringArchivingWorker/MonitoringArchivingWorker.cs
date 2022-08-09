using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.HistoryLog.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Parameters;

namespace MonitoringArchivingWorker
{
	public class MonitoringArchivingWorker : BackgroundService
	{
		private const string _monitoringName = "мониторинга";
		private const string _trackPointsName = "точек трэка";
		private const string _distanceCacheName = "кэша расстояний";
		private const int _delayInMinutes = 20;
		private readonly ILogger<MonitoringArchivingWorker> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IArchiveDataSettings _archiveDataSettings;
		private readonly IOldHistoryChangesRepository _oldHistoryChangesRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IOldTrackPointRepository _oldTrackPointRepository;
		private readonly ICachedDistanceRepository _cachedDistanceRepository;
		private DateTime? _archiveMonitoring = null;
		private DateTime? _archiveTrackPoints = null;
		private DateTime? _deleteDistanceCache = null;
		private bool _workInProgress;

		public MonitoringArchivingWorker(
			ILogger<MonitoringArchivingWorker> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IArchiveDataSettings archiveDataSettings,
			IOldHistoryChangesRepository oldHistoryChangesRepository,
			ITrackRepository trackRepository,
			IOldTrackPointRepository oldTrackPointRepository,
			ICachedDistanceRepository cachedDistanceRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_archiveDataSettings = archiveDataSettings ?? throw new ArgumentNullException(nameof(archiveDataSettings));
			_oldHistoryChangesRepository = oldHistoryChangesRepository ?? throw new ArgumentNullException(nameof(oldHistoryChangesRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_oldTrackPointRepository = oldTrackPointRepository ?? throw new ArgumentNullException(nameof(oldTrackPointRepository));
			_cachedDistanceRepository = cachedDistanceRepository ?? throw new ArgumentNullException(nameof(cachedDistanceRepository));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				if(DateTime.Now.Hour >= 4)
				{
					if(_workInProgress)
					{
						return;
					}

					_workInProgress = true;

					try
					{
						ArchiveMonitoring();
						ArchiveTrackPoints();
						DeleteDistanceCache();
					}
					catch(Exception e)
					{
						_logger.LogError(e, $"Ошибка при выполнении процесса архивации всех сущностей {DateTime.Today:dd-MM-yyyy}");
					}
					finally
					{
						_workInProgress = false;
					}
				}

				_logger.LogInformation($"Ожидаем {_delayInMinutes}мин перед следующим запуском");
				await Task.Delay(1000 * 60 * _delayInMinutes, stoppingToken);
			}
		}

		private void ArchiveMonitoring()
		{
			if(!_archiveMonitoring.HasValue || DateTime.Today > _archiveMonitoring)
			{
				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					var lastOldChangedEntity = _oldHistoryChangesRepository.GetLastOldChangedEntity(uow);
					var lastDateToArchive = DateTime.Today.AddDays(-_archiveDataSettings.GetMonitoringPeriodAvailableInDays);
					var firstDateInWorkingBase = lastDateToArchive.AddDays(1);
					var firstChangedEntityDate = HistoryChangesRepository.GetFirstChangedEntity(uow).ChangeTime;

					if(lastOldChangedEntity != null && lastOldChangedEntity.ChangeTime < lastDateToArchive)
					{
						ArchiveData(
							uow,
							lastDateToArchive,
							lastOldChangedEntity.ChangeTime,
							ref _archiveMonitoring,
							_monitoringName,
							ArchiveMonitoringByOneDay);
					}
					else if(firstChangedEntityDate < firstDateInWorkingBase)
					{
						DeleteData(
							uow,
							firstDateInWorkingBase,
							firstChangedEntityDate,
							ref _archiveMonitoring,
							_monitoringName,
							HistoryChangesRepository.DeleteHistoryChanges);
					}
					else
					{
						_archiveMonitoring = DateTime.Today;
					}
				}
			}
		}

		private void ArchiveTrackPoints()
		{
			if(!_archiveTrackPoints.HasValue || DateTime.Today > _archiveTrackPoints)
			{
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					var maxOldTrackPointDate = _oldTrackPointRepository.GetMaxOldTrackPointDate(uow);
					var lastDateToArchive = DateTime.Today.AddYears(-_archiveDataSettings.GetDistanceCacheDataPeriodAvailable);
					var firstDateInWorkingBase = lastDateToArchive.AddDays(1);
					var firstTrackPointDate = _trackRepository.GetMinTrackPointDate(uow);

					if(maxOldTrackPointDate < lastDateToArchive)
					{
						ArchiveData(
							uow,
							lastDateToArchive,
							maxOldTrackPointDate,
							ref _archiveTrackPoints,
							_trackPointsName,
							ArchiveTrackPointsByOneDay);
					}
					else if(firstTrackPointDate < firstDateInWorkingBase)
					{
						DeleteData(
							uow,
							firstDateInWorkingBase,
							firstTrackPointDate,
							ref _archiveTrackPoints,
							_trackPointsName,
							TrackRepository.DeleteTrackPoints);
					}
					else
					{
						_archiveTrackPoints = DateTime.Today;
					}
				}
			}
		}

		private void DeleteDistanceCache()
		{
			if(!_deleteDistanceCache.HasValue || DateTime.Today > _deleteDistanceCache)
			{
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					var firstDateInWorkingBase = DateTime.Today.AddYears(-_archiveDataSettings.GetDistanceCacheDataPeriodAvailable).AddDays(1);
					var firstCachedDistance = _cachedDistanceRepository.GetFirstCacheByCreateDate(uow);

					if(!DeleteData(
							uow,
							firstDateInWorkingBase,
							firstCachedDistance.Created,
							ref _deleteDistanceCache,
							_distanceCacheName,
							CachedDistanceRepository.DeleteCachedDistance))
					{
						return;
					}
					_deleteDistanceCache = DateTime.Today;
				}
			}
		}

		private bool ArchiveData(
			IUnitOfWork uow,
			DateTime lastDateToArchive,
			DateTime lastArchiveDataDate,
			ref DateTime? actionDate,
			string dataName,
			Func<IUnitOfWork, DateTime, bool> ArchiveDataByOneDay)
		{
			var needArchiveDays = (int)(lastDateToArchive - lastArchiveDataDate.Date).TotalDays;

			for(var i = 0; i < needArchiveDays; i++)
			{
				var archivingDate = lastArchiveDataDate.Date.AddDays(i + 1);
				var formattedArchivingDate = $"{archivingDate:dd-MM-yyyy}";
				var tx = uow.Session.BeginTransaction();

				try
				{
					_logger.LogInformation($"Начинаем архивацию {dataName} за {formattedArchivingDate}");
					if(ArchiveDataByOneDay(uow, archivingDate))
					{
						tx.Commit();
						_logger.LogInformation($"Архивация {dataName} за {formattedArchivingDate} успешна");
					}
					else
					{
						tx.Commit();
						_logger.LogInformation($"Нет данных для архивации {dataName} за {formattedArchivingDate}");
					}
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при архивации {dataName} за {formattedArchivingDate}");
					if(tx.IsActive)
					{
						tx.Rollback();
					}
					tx.Dispose();
					actionDate = null;
					return false;
				}
				finally
				{
					tx.Dispose();
				}
			}
			actionDate = DateTime.Today;
			return true;
		}

		private bool ArchiveMonitoringByOneDay(IUnitOfWork uow, DateTime dateFrom)
		{
			var dateTo = dateFrom.AddDays(1).AddMilliseconds(-1);
			var dataToArchiveExists = HistoryChangesRepository.ChangedEntitiesExists(uow, dateFrom, dateTo);

			if(dataToArchiveExists)
			{
				_oldHistoryChangesRepository.ChangeSetsArchiving(uow, dateFrom, dateTo);
				_oldHistoryChangesRepository.ChangedEntitiesArchiving(uow, dateFrom, dateTo);
				_oldHistoryChangesRepository.FieldChangesArchiving(uow, dateFrom, dateTo);
				HistoryChangesRepository.DeleteHistoryChanges(uow, dateFrom, dateTo);

				return true;
			}
			return false;
		}

		private bool ArchiveTrackPointsByOneDay(IUnitOfWork uow, DateTime dateFrom)
		{
			var dateTo = dateFrom.AddDays(1).AddMilliseconds(-1);
			var dataToArchiveExists = _trackRepository.TrackPointsExists(uow, dateFrom, dateTo);

			if(dataToArchiveExists)
			{
				_oldTrackPointRepository.TrackPointsArchiving(uow, dateFrom, dateTo);
				TrackRepository.DeleteTrackPoints(uow, dateFrom, dateTo);

				return true;
			}
			return false;
		}

		private bool DeleteData(
		IUnitOfWork uow,
		DateTime firstDateInWorkingBase,
		DateTime firstDataDate,
		ref DateTime? actionDate,
		string dataName,
		Action<IUnitOfWork, DateTime, DateTime> DeleteData)
		{
			var needDeleteDays = (int)(firstDateInWorkingBase - firstDataDate.Date).TotalDays;

			for(var i = 0; i < needDeleteDays; i++)
			{
				var deletingDate = firstDataDate.Date.AddDays(i);
				var formattedDeletingDate = $"{deletingDate:dd-MM-yyyy}";
				var tx = uow.Session.BeginTransaction();
				try
				{
					_logger.LogInformation($"Начинаем удаление {dataName} за {formattedDeletingDate}");
					DeleteData(uow, deletingDate, deletingDate.AddDays(1).AddMilliseconds(-1));
					tx.Commit();
					_logger.LogInformation($"Удаление {dataName} за {formattedDeletingDate} успешно");
				}
				catch(Exception e)
				{
					_logger.LogError(e, $"Ошибка при удалении {dataName} за {formattedDeletingDate}");
					if(tx.IsActive)
					{
						tx.Rollback();
					}
					tx.Dispose();
					actionDate = null;
					return false;
				}
				finally
				{
					tx.Dispose();
				}
			}
			actionDate = DateTime.Today;
			return true;
		}
	}
}

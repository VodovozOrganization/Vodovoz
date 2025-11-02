using NLog;
using QS.DomainModel.UoW;
using QS.HistoryLog.Repositories;
using System;
using Vodovoz.EntityRepositories.HistoryChanges;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Settings.Common;

namespace Vodovoz.Tools
{
	public class DataArchiver : IDataArchiver
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		private const string _monitoringName = "мониторинга";
		private const string _trackPointsName = "точек трэка";
		private const string _distanceCacheName = "кэша расстояний";
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IArchiveDataSettings _archiveDataSettings;
		private readonly IArchivedHistoryChangesRepository _oldHistoryChangesRepository;
		private readonly ITrackRepository _trackRepository;
		private readonly IArchivedTrackPointRepository _oldTrackPointRepository;
		private readonly ICachedDistanceRepository _cachedDistanceRepository;
		private DateTime? _archiveMonitoringDate = null;
		private DateTime? _archiveTrackPointsDate = null;
		private DateTime? _deleteDistanceCacheDate = null;

		public DataArchiver(
			IUnitOfWorkFactory unitOfWorkFactory,
			IArchiveDataSettings archiveDataSettings,
			IArchivedHistoryChangesRepository oldHistoryChangesRepository,
			ITrackRepository trackRepository,
			IArchivedTrackPointRepository oldTrackPointRepository,
			ICachedDistanceRepository cachedDistanceRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_archiveDataSettings = archiveDataSettings ?? throw new ArgumentNullException(nameof(archiveDataSettings));
			_oldHistoryChangesRepository = oldHistoryChangesRepository ?? throw new ArgumentNullException(nameof(oldHistoryChangesRepository));
			_trackRepository = trackRepository ?? throw new ArgumentNullException(nameof(trackRepository));
			_oldTrackPointRepository = oldTrackPointRepository ?? throw new ArgumentNullException(nameof(oldTrackPointRepository));
			_cachedDistanceRepository = cachedDistanceRepository ?? throw new ArgumentNullException(nameof(cachedDistanceRepository));
		}

		public void ArchiveMonitoring()
		{
			if(_archiveMonitoringDate.HasValue && DateTime.Today <= _archiveMonitoringDate)
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var lastOldChangedEntity = _oldHistoryChangesRepository.GetLastOldChangedEntity(uow);
				var lastDateToArchive = DateTime.Today.AddDays(-_archiveDataSettings.GetMonitoringPeriodAvailableInDays);
				var firstDateInWorkingBase = lastDateToArchive.AddDays(1);
				var firstChangedEntityDate = HistoryChangesRepository.GetFirstChangedEntity(uow).ChangeTime;

				if(lastOldChangedEntity != null && lastOldChangedEntity.ChangeTime < lastDateToArchive)
				{
					var result = ArchiveData(
						uow,
						lastDateToArchive,
						lastOldChangedEntity.ChangeTime,
						_monitoringName,
						ArchiveMonitoringByOneDay);
					_archiveMonitoringDate = result.SuccessActionDate;
				}
				else if(firstChangedEntityDate < firstDateInWorkingBase)
				{
					var result = DeleteData(
						uow,
						firstDateInWorkingBase,
						firstChangedEntityDate,
						_monitoringName,
						HistoryChangesRepository.DeleteHistoryChanges);
					_archiveMonitoringDate = result.SuccessActionDate;
				}
				else
				{
					_archiveMonitoringDate = DateTime.Today;
				}
			}
		}

		public void ArchiveTrackPoints()
		{
			if(_archiveTrackPointsDate.HasValue && DateTime.Today <= _archiveTrackPointsDate)
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var maxOldTrackPointDate = _oldTrackPointRepository.GetMaxOldTrackPointDate(uow);
				var lastDateToArchive = DateTime.Today.AddYears(-_archiveDataSettings.GetDistanceCacheDataPeriodAvailable);
				var firstDateInWorkingBase = lastDateToArchive.AddDays(1);
				var firstTrackPointDate = _trackRepository.GetMinTrackPointDate(uow);

				if(maxOldTrackPointDate < lastDateToArchive)
				{
					var result = ArchiveData(
						uow,
						lastDateToArchive,
						maxOldTrackPointDate,
						_trackPointsName,
						ArchiveTrackPointsByOneDay);
					_archiveTrackPointsDate = result.SuccessActionDate;
				}
				else if(firstTrackPointDate < firstDateInWorkingBase)
				{
					var result = DeleteData(
						uow,
						firstDateInWorkingBase,
						firstTrackPointDate,
						_trackPointsName,
						_trackRepository.DeleteTrackPoints);
					_archiveTrackPointsDate = result.SuccessActionDate;
				}
				else
				{
					_archiveTrackPointsDate = DateTime.Today;
				}
			}
		}

		public void DeleteDistanceCache()
		{
			if(_deleteDistanceCacheDate.HasValue && DateTime.Today <= _deleteDistanceCacheDate)
			{
				return;
			}

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var firstDateInWorkingBase = DateTime.Today.AddYears(-_archiveDataSettings.GetDistanceCacheDataPeriodAvailable).AddDays(1);
				var firstCachedDistance = _cachedDistanceRepository.GetFirstCacheByCreateDate(uow);

				var result = DeleteData(
					uow,
					firstDateInWorkingBase,
					firstCachedDistance.Created,
					_distanceCacheName,
					_cachedDistanceRepository.DeleteCachedDistance);

				if(!result.IsSuccess)
				{
					_deleteDistanceCacheDate = result.SuccessActionDate;
					return;
				}
				_deleteDistanceCacheDate = result.SuccessActionDate;
			}
		}

		private ResultAction ArchiveData(
			IUnitOfWork uow,
			DateTime lastDateToArchive,
			DateTime lastArchiveDataDate,
			string dataName,
			Func<IUnitOfWork, DateTime, bool> ArchiveDataByOneDay)
		{
			var needArchiveDays = (int)(lastDateToArchive - lastArchiveDataDate.Date).TotalDays;
			var result = new ResultAction();

			for(var i = 0; i < needArchiveDays; i++)
			{
				var archivingDate = lastArchiveDataDate.Date.AddDays(i + 1);
				var formattedArchivingDate = $"{archivingDate:dd-MM-yyyy}";
				var tx = uow.Session.BeginTransaction();

				try
				{
					_logger.Debug($"Начинаем архивацию {dataName} за {formattedArchivingDate}");
					if(ArchiveDataByOneDay(uow, archivingDate))
					{
						tx.Commit();
						_logger.Debug($"Архивация {dataName} за {formattedArchivingDate} успешна");
					}
					else
					{
						tx.Commit();
						_logger.Debug($"Нет данных для архивации {dataName} за {formattedArchivingDate}");
					}
				}
				catch(Exception e)
				{
					_logger.Error(e, $"Ошибка при архивации {dataName} за {formattedArchivingDate}");
					if(tx.IsActive)
					{
						tx.Rollback();
					}
					tx.Dispose();
					result.SuccessActionDate = null;
					result.IsSuccess = false;
					return result;
				}
				finally
				{
					tx.Dispose();
				}
			}
			result.SuccessActionDate = DateTime.Today;
			result.IsSuccess = true;
			return result;
		}

		private bool ArchiveMonitoringByOneDay(IUnitOfWork uow, DateTime dateFrom)
		{
			var dateTo = dateFrom.AddDays(1).AddMilliseconds(-1);
			var dataToArchiveExists = HistoryChangesRepository.ChangedEntitiesExists(uow, dateFrom, dateTo);

			if(dataToArchiveExists)
			{
				_oldHistoryChangesRepository.ArchiveChangeSets(uow, dateFrom, dateTo);
				_oldHistoryChangesRepository.ArchiveChangedEntities(uow, dateFrom, dateTo);
				_oldHistoryChangesRepository.ArchiveFieldChanges(uow, dateFrom, dateTo);
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
				_oldTrackPointRepository.ArchiveTrackPoints(uow, dateFrom, dateTo);
				_trackRepository.DeleteTrackPoints(uow, dateFrom, dateTo);

				return true;
			}
			return false;
		}

		private ResultAction DeleteData(
		IUnitOfWork uow,
		DateTime firstDateInWorkingBase,
		DateTime firstDataDate,
		string dataName,
		Action<IUnitOfWork, DateTime, DateTime> DeleteData)
		{
			var needDeleteDays = (int)(firstDateInWorkingBase - firstDataDate.Date).TotalDays;
			var result = new ResultAction();

			for(var i = 0; i < needDeleteDays; i++)
			{
				var deletingDate = firstDataDate.Date.AddDays(i);
				var formattedDeletingDate = $"{deletingDate:dd-MM-yyyy}";
				var tx = uow.Session.BeginTransaction();
				try
				{
					_logger.Debug($"Начинаем удаление {dataName} за {formattedDeletingDate}");
					DeleteData(uow, deletingDate, deletingDate.AddDays(1).AddMilliseconds(-1));
					tx.Commit();
					_logger.Debug($"Удаление {dataName} за {formattedDeletingDate} успешно");
				}
				catch(Exception e)
				{
					_logger.Error(e, $"Ошибка при удалении {dataName} за {formattedDeletingDate}");
					if(tx.IsActive)
					{
						tx.Rollback();
					}
					tx.Dispose();
					result.SuccessActionDate = null;
					result.IsSuccess = false;
					return result;
				}
				finally
				{
					tx.Dispose();
				}
			}
			result.SuccessActionDate = DateTime.Today;
			result.IsSuccess = true;
			return result;
		}

		public class ResultAction
		{
			public bool IsSuccess { get; set; }
			public DateTime? SuccessActionDate { get; set; }
		}
	}
}

using Microsoft.Extensions.Logging;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Vodovoz.CachingRepositories.Common
{
	public abstract class DomainEntityNodeInMemoryCacheRepositoryBase<TEntity>
		: IDomainEntityNodeInMemoryCacheRepository<TEntity>, IDisposable
		where TEntity : IDomainObject
	{
		protected readonly ILogger<IDomainEntityNodeInMemoryCacheRepository<TEntity>> _logger;
		protected readonly IUnitOfWork _unitOfWork;
		protected readonly IDictionary<int, string> _nodesCache = new Dictionary<int,string>();

		protected DomainEntityNodeInMemoryCacheRepositoryBase(
			ILogger<IDomainEntityNodeInMemoryCacheRepository<TEntity>> logger,
			IUnitOfWorkFactory unitOfWorkFactory)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot();
			_unitOfWork.Session.DefaultReadOnly = true;

			NotifyConfiguration.Instance
				.BatchSubscribeOnEntity<TEntity>(EntityChangedHandler);
		}

		public virtual void WarmUpCacheWithIds(IEnumerable<int> ids)
		{
			_logger.LogTrace("Запрошен прогрев кэша значениями: {@ids}, количество {Count}", ids, ids.Count());

			var neededToWarmUp = ids.Except(_nodesCache.Keys).ToArray();

			_logger.LogTrace("Будут прогреты значения с идентификаторами: {@ids}, количество {Count}", neededToWarmUp, neededToWarmUp.Count());

			if(!neededToWarmUp.Any())
			{
				return;
			}

			var newValues = GetTitlesByIdsFromDatabase(neededToWarmUp);

			foreach(var newValue in newValues)
			{
				_nodesCache.Add(newValue);
			}
		}

		public virtual void InvalidateById(int id)
		{
			_logger.LogTrace("Запрошена инвалидация заголовка обьекта с идентификатором {Id}", id);
			_nodesCache.Remove(id);
		}

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			_unitOfWork.Session.Clear();
			_unitOfWork.Dispose();
			_nodesCache.Clear();
		}

		public virtual string GetTitleById(int id)
		{
			_logger.LogTrace("Запрошено значение заголовка обьекта с идентификатором {Id}", id);

			if(_nodesCache.TryGetValue(id, out var title))
			{
				return title;
			}

			var entity = GetEntityById(id);

			title = entity?.GetTitle() ?? string.Empty;

			_unitOfWork.Session.Clear();

			_nodesCache.Add(id, title);

			return title;
		}

		protected abstract TEntity GetEntityById(int id);

		protected abstract IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids);

		protected virtual void EntityChangedHandler(EntityChangeEvent[] changeEvents)
		{
			foreach(var changeEvent in changeEvents)
			{
				var entityId = changeEvent.Entity.GetId();

				if((changeEvent.EventType == TypeOfChangeEvent.Update
						|| changeEvent.EventType == TypeOfChangeEvent.Delete)
					&& _nodesCache.Keys.Contains(entityId))
				{
					_logger.LogTrace(
						"Получено уведомление типа {EventType} для {EntityType} с идентификатором {Id}",
						Enum.GetName(typeof(TypeOfChangeEvent), changeEvent.EventType),
						typeof(TEntity),
						entityId);

					InvalidateById(entityId);
				}
			}
		}
	}
}

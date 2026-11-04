using NHibernate;
using NHibernate.Collection;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Type;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	internal class BulkInsertContext<TItem>
		where TItem : class
	{
		private const int _maxParametersPerCommand = 65535;
		private static readonly PropertyInfo _uowTransactionProperty =
			typeof(UnitOfWorkBase).GetProperty("transaction", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly FieldInfo _postInsertListenersField =
			typeof(SingleUowEventsTracker).GetField("postInsertListeners", BindingFlags.Instance | BindingFlags.NonPublic);

		private readonly ICollection<TItem> _collection;
		private readonly IPersistentCollection _persistentCollection;
		private readonly IUnitOfWork _uow;
		private readonly ISessionImplementor _sessionImplementor;
		private readonly AbstractCollectionPersister _collectionPersister;
		private readonly IList<TItem> _newItems;
		private readonly int _batchSize;
		private readonly object _key;
		private readonly Dictionary<AbstractEntityPersister, InsertPlan> _plans = new Dictionary<AbstractEntityPersister, InsertPlan>();
		private readonly HibernateTracker _historyTracker;
		private HashSet<TItem> _existingItems;
		private IList<TItem> _loadedNewItems;
		private string _stage;

		private BulkInsertContext(
			ICollection<TItem> collection,
			IPersistentCollection persistentCollection,
			IUnitOfWork uow,
			AbstractCollectionPersister collectionPersister,
			object owner,
			object key,
			LockMode parentLockMode,
			IList<TItem> newItems,
			int batchSize)
		{
			_collection = collection;
			_persistentCollection = persistentCollection;
			_uow = uow;
			Session = uow.Session;
			_sessionImplementor = Session.GetSessionImplementation();
			_collectionPersister = collectionPersister;
			Owner = owner;
			_key = key;
			ParentLockMode = parentLockMode;
			_newItems = newItems;
			_batchSize = batchSize;
			_historyTracker = FindHistoryTracker(uow);
		}

		public ISession Session { get; }

		public object Owner { get; }

		public LockMode ParentLockMode { get; }

		public IBatcher Batcher => _sessionImplementor.Batcher;

		public bool UsesMaxPacket => _sessionImplementor.Factory.Dialect is MySQLDialect;

		/// <summary>
		/// Все проверки до начала записи: ни одной команды записи и никаких изменений.
		/// Возвращает null, если вставлять нечего
		/// </summary>
		public static BulkInsertContext<TItem> Create(
			ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize,
			CancellationToken cancellationToken)
		{
			if(collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if(uow == null)
			{
				throw new ArgumentNullException(nameof(uow));
			}

			if(newItems == null)
			{
				throw new ArgumentNullException(nameof(newItems));
			}

			if(batchSize < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "Размер пачки должен быть больше нуля");
			}

			cancellationToken.ThrowIfCancellationRequested();

			var items = newItems.ToList();

			if(items.Count == 0)
			{
				return null;
			}

			ValidateItemsList(items);

			if(!(collection is IPersistentCollection persistentCollection))
			{
				throw new InvalidOperationException(
					"Коллекция не связана с сессией NHibernate. Сохраните родителя в UoW и передайте коллекцию из его свойства после сохранения");
			}

			var session = uow.Session;

			if(!session.IsOpen)
			{
				throw new ObjectDisposedException(nameof(uow), "Сессия переданного UoW уже закрыта");
			}

			var sessionImplementor = session.GetSessionImplementation();
			var persistenceContext = sessionImplementor.PersistenceContext;
			var collectionEntry = persistenceContext.GetCollectionEntry(persistentCollection);

			if(collectionEntry == null)
			{
				throw new InvalidOperationException("Коллекция не принадлежит сессии переданного UoW");
			}

			ResolveOwner(persistentCollection, collectionEntry, persistenceContext, out var owner, out var role);
			EnsureOwnerAlive(owner, role, persistenceContext);

			var collectionPersister = GetCollectionPersister(sessionImplementor, role);
			var key = collectionPersister.CollectionType.GetKeyOfOwner(owner, sessionImplementor);

			if(key == null)
			{
				throw new InvalidOperationException($"Не удалось определить ключ родителя коллекции {role}. Родитель должен быть сохранен");
			}

			var ownerPersister = persistenceContext.GetEntry(owner).Persister;
			var parentLockMode = ownerPersister.IsVersioned ? LockMode.Force : LockMode.Upgrade;
			var context = new BulkInsertContext<TItem>(
				collection, persistentCollection, uow, collectionPersister, owner, key, parentLockMode, items, batchSize);

			context.ValidateItems();
			context.EnsureCurrentUserForHistory();
			context.EnsureNoForeignTransaction();

			return context;
		}

		/// <summary>
		/// Открывает транзакцию UoW, если ее нет. Выполняется после всех проверок, поэтому неправильный вызов транзакцию не открывает
		/// </summary>
		public void OpenTransaction()
		{
			try
			{
				_uow.OpenTransaction();
			}
			catch(TransactionException ex)
			{
				throw new InvalidOperationException(
					"Не удалось открыть транзакцию UoW. Вероятно, на соединении сессии уже открыта транзакция в обход NHibernate", ex);
			}
		}

		public void SetStage(string stage) => _stage = stage;

		/// <summary>
		/// Запоминает элементы коллекции до вставки: новыми считается все, чего среди них нет
		/// </summary>
		public void RememberExistingItems() => _existingItems = new HashSet<TItem>(_collection, ReferenceComparer.Instance);

		public IEnumerable<DbCommand> CreateCommands(long? maxPacket)
		{
			// Запас на служебную часть пакета и неточность оценки размера строки
			var bytesLimit = maxPacket * 9 / 10;
			var groups = _newItems
				.GroupBy(GetElementPersister)
				.ToList();

			foreach(var group in groups)
			{
				var plan = _plans[group.Key];
				var rowsLimit = Math.Max(1, Math.Min(_batchSize, _maxParametersPerCommand / plan.ParametersPerRow));
				var batch = new List<TItem>();
				long batchBytes = plan.HeaderBytes;

				foreach(var row in group)
				{
					var rowBytes = plan.EstimateRowBytes(row);

					if(batch.Count > 0 && (batch.Count >= rowsLimit || batchBytes + rowBytes > bytesLimit))
					{
						yield return plan.CreateCommand(batch, _key);
						batch = new List<TItem>();
						batchBytes = plan.HeaderBytes;
					}

					batch.Add(row);
					batchBytes += rowBytes;
				}

				yield return plan.CreateCommand(batch, _key);
			}
		}

		/// <summary>
		/// Сверка числа строк, которое вернула БД. Выполняется один раз, после всех пачек
		/// </summary>
		public void EnsureAffectedRows(int affectedRows)
		{
			if(affectedRows != _newItems.Count)
			{
				throw new CountMismatchException($"БД сообщила о вставке {affectedRows} строк, а вставлялось {_newItems.Count}");
			}
		}

		/// <summary>
		/// Запрос Id строк коллекции в порядке из маппинга. В загрузке через фильтр OrderBy коллекции не участвует
		/// </summary>
		public IQuery CreateOrderQuery()
		{
			if(!_collectionPersister.HasOrdering
				|| !(_collection is IList<TItem>)
				|| _collectionPersister.KeyColumnNames.Length != 1)
			{
				return null;
			}

			var elementPersister = (AbstractEntityPersister)_collectionPersister.ElementPersister;
			var idColumn = elementPersister.IdentifierColumnNames[0];
			var sql = $"SELECT t.{idColumn} AS id FROM {elementPersister.TableName} t "
				+ $"WHERE t.{_collectionPersister.KeyColumnNames[0]} = ? "
				+ $"ORDER BY {_collectionPersister.GetSQLOrderByString("t")}";

			return Session.CreateSQLQuery(sql)
				.AddScalar("id", elementPersister.IdentifierType)
				.SetParameter(0, _key, _collectionPersister.KeyType);
		}

		public void SynchronizeCollection(IEnumerable<TItem> itemsFromDatabase, IList<object> orderedIds)
		{
			_loadedNewItems = itemsFromDatabase.Where(item => !_existingItems.Contains(item)).Distinct(ReferenceComparer.Instance).Cast<TItem>().ToList();

			if(_loadedNewItems.Count != _newItems.Count)
			{
				throw new CountMismatchException($"Вставлено {_newItems.Count} строк, а загружено новых {_loadedNewItems.Count}");
			}

			foreach(var item in _loadedNewItems)
			{
				_collection.Add(item);
			}

			if(orderedIds != null)
			{
				ApplyOrder(orderedIds);
			}

			// Коллекция теперь совпадает с БД: обновляем снимок и снимаем признак изменения,
			// чтобы при Flush не было лишних UPDATE внешнего ключа и корректно работал delete-orphan
			var collectionEntry = _sessionImplementor.PersistenceContext.GetCollectionEntry(_persistentCollection);
			collectionEntry.PostInitialize(_persistentCollection, _sessionImplementor.PersistenceContext);
			_persistentCollection.ClearDirty();
		}

		/// <summary>
		/// Поднимает для вставленных строк те же события PostInsert, что и NHibernate при обычном Save.
		/// Значения берутся из загруженных сущностей, поэтому содержат и то, что заполнила БД
		/// </summary>
		public void RaisePostInsertEvents()
		{
			var listeners = _sessionImplementor.Listeners.PostInsertEventListeners;

			if(listeners.Length == 0)
			{
				return;
			}

			var eventSource = (IEventSource)_sessionImplementor;

			foreach(var item in _loadedNewItems)
			{
				var persister = _sessionImplementor.GetEntityPersister(null, item);
				var postInsertEvent = new PostInsertEvent(item, persister.GetIdentifier(item), persister.GetPropertyValues(item), persister, eventSource);

				foreach(var listener in listeners)
				{
					listener.OnPostInsert(postInsertEvent);
				}
			}
		}

		/// <summary>
		/// Откатывает транзакцию UoW. Возвращает ошибку отката, чтобы она не подменила исходное исключение
		/// </summary>
		public Exception Rollback()
		{
			try
			{
				// Строки откатываются, поэтому записи истории о них в трекере оставлять нельзя
				_historyTracker?.Reset();

				var transaction = Session.GetCurrentTransaction();

				if(transaction?.IsActive == true)
				{
					transaction.Rollback();
				}

				return null;
			}
			catch(Exception ex)
			{
				return ex;
			}
		}

		public async Task<Exception> RollbackAsync()
		{
			try
			{
				// Строки откатываются, поэтому записи истории о них в трекере оставлять нельзя
				_historyTracker?.Reset();

				var transaction = Session.GetCurrentTransaction();

				if(transaction?.IsActive == true)
				{
					// Токен вызывающего кода здесь не передается: отмена не должна мешать откату
					await transaction.RollbackAsync(CancellationToken.None);
				}

				return null;
			}
			catch(Exception ex)
			{
				return ex;
			}
		}

		public BulkSqlInsertException CreateFailure(Exception error, Exception rollbackError)
		{
			var message = new StringBuilder()
				.Append($"Массовая вставка в коллекцию {_collectionPersister.Role} (родитель {_key}, строк {_newItems.Count}, batchSize {_batchSize}) ")
				.Append($"не выполнена на этапе «{_stage}»: {error.Message}. ");

			if(rollbackError == null)
			{
				message.Append("Транзакция UoW принудительно откатывается из-за ошибки массовой вставки вместе с остальными изменениями этого UoW.");
			}
			else
			{
				message.Append($"Транзакцию UoW нужно принудительно откатить из-за ошибки массовой вставки, но откат не удался: {rollbackError.Message}. ")
					.Append("При обрыве соединения БД откатывает транзакцию сама.");
			}

			return new BulkSqlInsertException(message.ToString(), error is CountMismatchException ? null : error);
		}

		private static void ValidateItemsList(IList<TItem> items)
		{
			var uniqueItems = new HashSet<TItem>(ReferenceComparer.Instance);

			foreach(var item in items)
			{
				if(item == null)
				{
					throw new ArgumentException("Список новых элементов содержит null", "newItems");
				}

				if(!uniqueItems.Add(item))
				{
					throw new ArgumentException("Один и тот же объект передан в списке новых элементов несколько раз", "newItems");
				}
			}
		}

		/// <summary>
		/// Определяет родителя и роль коллекции без Flush. У коллекции только что сохраненного родителя их еще нет,
		/// поэтому родитель ищется в сессии по свойству, которое ссылается на этот экземпляр коллекции
		/// </summary>
		private static void ResolveOwner(
			IPersistentCollection collection,
			CollectionEntry collectionEntry,
			IPersistenceContext persistenceContext,
			out object owner,
			out string role)
		{
			owner = collection.Owner;
			role = collectionEntry.Role ?? collection.Role;

			if(owner != null && role != null)
			{
				return;
			}

			foreach(DictionaryEntry entry in persistenceContext.EntityEntries)
			{
				var persister = ((EntityEntry)entry.Value).Persister;

				if(!persister.HasCollections)
				{
					continue;
				}

				var values = persister.GetPropertyValues(entry.Key);

				for(var i = 0; i < values.Length; i++)
				{
					if(ReferenceEquals(values[i], collection))
					{
						owner = entry.Key;
						role = ((CollectionType)persister.PropertyTypes[i]).Role;
						return;
					}
				}
			}

			throw new InvalidOperationException("Не удалось определить родителя коллекции. Родитель должен находиться в сессии переданного UoW");
		}

		private static void EnsureOwnerAlive(object owner, string role, IPersistenceContext persistenceContext)
		{
			var status = persistenceContext.GetEntry(owner)?.Status;

			switch(status)
			{
				case null:
					throw new InvalidOperationException($"Родитель коллекции {role} не находится в сессии переданного UoW");
				case Status.Deleted:
				case Status.Gone:
					throw new InvalidOperationException($"Родитель коллекции {role} помечен на удаление");
				case Status.ReadOnly:
					throw new InvalidOperationException($"Родитель коллекции {role} находится в режиме только для чтения");
			}
		}

		private static AbstractCollectionPersister GetCollectionPersister(ISessionImplementor session, string role)
		{
			var persister = session.Factory.GetCollectionPersister(role) as AbstractCollectionPersister;

			if(persister == null || !persister.IsOneToMany)
			{
				throw new NotSupportedException(
					$"Коллекция {role} не является one-to-many. Массовая вставка поддерживает только one-to-many коллекции");
			}

			if(persister.HasIndex)
			{
				throw new NotSupportedException(
					$"Коллекция {role} индексированная (list/map). Массовая вставка поддерживает только bag/set коллекции");
			}

			if(persister.HasWhere)
			{
				throw new NotSupportedException($"У коллекции {role} есть Where: вставленные строки могут не загрузиться");
			}

			if(!persister.IsMutable)
			{
				throw new NotSupportedException($"Коллекция {role} неизменяемая");
			}

			if(persister.CollectionType.LHSPropertyName != null)
			{
				throw new NotSupportedException(
					$"Ключ коллекции {role} ссылается на свойство {persister.CollectionType.LHSPropertyName}, а не на Id родителя");
			}

			return persister;
		}

		private void ValidateItems()
		{
			// В незагруженной коллекции могут быть отложенные Add. Они станут видны только после загрузки
			if(_persistentCollection.HasQueuedOperations)
			{
				NHibernateUtil.Initialize(_collection);
			}

			var existingItems = _persistentCollection.WasInitialized
				? new HashSet<TItem>(_collection, ReferenceComparer.Instance)
				: new HashSet<TItem>(ReferenceComparer.Instance);

			foreach(var item in _newItems)
			{
				var persister = GetElementPersister(item);

				if(Session.Contains(item) || existingItems.Contains(item))
				{
					throw new ArgumentException(
						"Элемент уже находится в сессии или в коллекции. Массовая вставка принимает только новые объекты, которые не добавлены в коллекцию",
						"newItems");
				}

				if(!_plans.TryGetValue(persister, out var plan))
				{
					plan = new InsertPlan(persister, _collectionPersister, _sessionImplementor);
					_plans.Add(persister, plan);
				}

				plan.ValidateItem(item, _key);
			}
		}

		/// <summary>
		/// Трекер истории этого UoW. Если его нет, история в приложении не включена, как и для обычного Save.
		/// У <see cref="SingleUowEventsTracker"/> нет способа узнать подключенных слушателей
		/// </summary>
		private static HibernateTracker FindHistoryTracker(IUnitOfWork uow)
		{
			var eventsTracker = (uow as UnitOfWorkBase)?.EventsTracker;

			if(eventsTracker == null)
			{
				return null;
			}

			var listeners = (IEnumerable<IUowPostInsertEventListener>)_postInsertListenersField.GetValue(eventsTracker);

			return listeners.OfType<HibernateTracker>().FirstOrDefault();
		}

		/// <summary>
		/// Трекер берет текущего пользователя уже после коммита строк, поэтому его отсутствие нужно обнаружить до записи
		/// </summary>
		private void EnsureCurrentUserForHistory()
		{
			if(_historyTracker == null
				|| UserRepository.GetCurrentUserId != null
				|| !_plans.Keys.Any(persister => HibernateTracker.NeedTrace(persister.MappedClass)))
			{
				return;
			}

			throw new InvalidOperationException(
				"История изменений включена, но текущий пользователь не задан (UserRepository.GetCurrentUserId). Без него историю записать нельзя");
		}

		/// <summary>
		/// Транзакция должна быть транзакцией UoW или ее должен открыть сам UoW: иначе uow.Commit() ее не закоммитит
		/// </summary>
		private void EnsureNoForeignTransaction()
		{
			if(System.Transactions.Transaction.Current != null)
			{
				throw new InvalidOperationException(
					"Массовая вставка не работает внутри TransactionScope: вставка должна идти в транзакции UoW");
			}

			var sessionTransaction = Session.GetCurrentTransaction();

			if(sessionTransaction?.IsActive != true || !(_uow is UnitOfWorkBase))
			{
				return;
			}

			// У IUnitOfWork нет способа узнать его транзакцию, а без этой проверки вставка молча пропадет при Dispose
			if(!ReferenceEquals(_uowTransactionProperty.GetValue(_uow), sessionTransaction))
			{
				throw new InvalidOperationException(
					"Транзакция сессии открыта в обход UoW, uow.Commit() ее не закоммитит. Откройте транзакцию через UoW");
			}
		}

		private void ApplyOrder(IList<object> orderedIds)
		{
			var elementPersister = _collectionPersister.ElementPersister;
			var positions = new Dictionary<object, int>();

			for(var i = 0; i < orderedIds.Count; i++)
			{
				positions[orderedIds[i]] = i;
			}

			var ordered = _collection
				.OrderBy(item => positions.TryGetValue(elementPersister.GetIdentifier(item), out var position) ? position : int.MaxValue)
				.ToList();

			_collection.Clear();

			foreach(var item in ordered)
			{
				_collection.Add(item);
			}
		}

		private AbstractEntityPersister GetElementPersister(TItem item)
		{
			var elementPersister = (AbstractEntityPersister)_collectionPersister.ElementPersister;
			AbstractEntityPersister itemPersister;

			try
			{
				itemPersister = _sessionImplementor.GetEntityPersister(null, item) as AbstractEntityPersister;
			}
			catch(MappingException ex)
			{
				throw new ArgumentException($"Тип {item.GetType().FullName} не смаплен", "newItems", ex);
			}

			if(itemPersister == null
				|| (itemPersister.EntityName != elementPersister.EntityName
					&& !elementPersister.IsSubclassEntityName(itemPersister.EntityName)))
			{
				throw new ArgumentException(
					$"Тип {item.GetType().FullName} не является элементом коллекции {_collectionPersister.Role}",
					"newItems");
			}

			return itemPersister;
		}
	}
}

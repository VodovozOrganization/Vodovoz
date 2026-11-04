using NHibernate;
using NHibernate.Collection;
using NHibernate.Dialect;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Id;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Массовая вставка новых дочерних сущностей в one-to-many коллекцию родителя многострочными INSERT
	/// с загрузкой вставленных строк в сессию и в коллекцию
	/// </summary>
	/// <remarks>
	/// Единственный разрешенный сценарий: родитель находится в сессии переданного UoW, коллекция смаплена как one-to-many
	/// bag или set без индекса и без Where, дочерняя сущность хранится в одной таблице, не версионируется,
	/// ее Id смаплен как GeneratedBy.Native() (GeneratedBy.Identity() во время работы от него неотличим и тоже допускается).
	/// Все остальное заканчивается исключением. Пустой список новых элементов: метод ничего не делает.
	/// <para>Порядок работы: проверки → открытие транзакции UoW → Flush → блокировка родителя → INSERT → загрузка → сверка количества.</para>
	/// <para>Ссылка элемента на родителя:</para>
	/// <list type="bullet">
	/// <item>пустая: элемент привязывается к родителю коллекции, в FK пишется его Id, а после загрузки ссылка указывает на него;</item>
	/// <item>на этого же родителя (в том числе его прокси): допустимо;</item>
	/// <item>на другого родителя: <see cref="ArgumentException"/> до первого INSERT.</item>
	/// </list>
	/// <para>Транзакция:</para>
	/// <list type="bullet">
	/// <item>если у UoW нет открытой транзакции, метод открывает ее через <see cref="IUnitOfWork.OpenTransaction"/>. Строки без транзакции не вставляются никогда;</item>
	/// <item>транзакция, открытая в обход UoW (на сессии, на соединении или TransactionScope), дает <see cref="InvalidOperationException"/>:
	/// uow.Commit() такую транзакцию не закоммитит;</item>
	/// <item>метод сам не коммитит. Строки видны другим сессиям только после общего uow.Commit().</item>
	/// </list>
	/// <para>Блокировка родителя: до первого INSERT выполняется session.Lock. Родитель с Version: LockMode.Force,
	/// версия с проверкой увеличивается на 1 (повторный вызов в той же транзакции версию уже не меняет).
	/// Родитель без Version: LockMode.Upgrade, то есть SELECT ... FOR UPDATE. Строка родителя остается заблокированной до конца транзакции UoW.
	/// Причина: другая транзакция не должна одновременно менять родителя или массово вставлять в его коллекцию.
	/// Вторая такая вставка ждет конца первой, а для родителя с Version завершается ошибкой устаревшей версии.
	/// От вставки строк в обход этого метода блокировка не защищает.</para>
	/// <para>Ошибки:</para>
	/// <list type="bullet">
	/// <item>проверки до начала записи бросают стандартные исключения: <see cref="ArgumentNullException"/>, <see cref="ArgumentOutOfRangeException"/>,
	/// <see cref="ArgumentException"/>, <see cref="InvalidOperationException"/>, <see cref="NotSupportedException"/>, <see cref="TransientObjectException"/>.
	/// Транзакция при этом не откатывается, а незаписанные изменения сессии не уходят в БД;</item>
	/// <item>любая ошибка начиная с Flush и расхождение количества после всех вставок: транзакция UoW принудительно откатывается
	/// вместе с остальными изменениями этого UoW и бросается <see cref="BulkSqlInsertException"/> с исходным исключением внутри.
	/// После этого UoW нужно закрыть: uow.Commit() молча ничего не сделает;</item>
	/// <item>отмена токена после начала записи тоже откатывает транзакцию, но остается <see cref="OperationCanceledException"/>.</item>
	/// </list>
	/// <para>История изменений: после загрузки строк метод поднимает те же события PostInsert, что и NHibernate при обычном Save,
	/// поэтому для сущностей с [HistoryTrace] историю записывает трекер QS: после коммита, отдельным соединением, одним набором изменений
	/// на коммит вместе с остальными изменениями этого UoW. Если история в приложении не включена, ее нет, как и при обычном Save.
	/// Если история включена, а текущий пользователь не задан, до начала записи бросается <see cref="InvalidOperationException"/>,
	/// иначе трекер упал бы уже после коммита строк.</para>
	/// <para>Важно:</para>
	/// <list type="bullet">
	/// <item>Каскады и SessionScopeEntitySaved для новых строк не срабатывают.</item>
	/// <item>Переданные объекты не меняются и остаются транзиентными, после вставки работать нужно с элементами коллекции.</item>
	/// <item>Повторная передача тех же объектов вставит строки еще раз: метод не отличает дубль от намеренной копии, за это отвечает вызывающий код.</item>
	/// </list>
	/// </remarks>
	public static class CollectionBulkSqlInsertExtensions
	{
		private const int _defaultBatchSize = 1000;
		private const int _maxParametersPerCommand = 65535;
		private const string _maxPacketQuery = "SELECT CAST(@@max_allowed_packet AS SIGNED)";

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и загружает их в сессию и в коллекцию.
		/// Поведение для ссылки на родителя, транзакции, блокировки и ошибок описано у <see cref="CollectionBulkSqlInsertExtensions"/>
		/// </summary>
		/// <param name="collection">Коллекция родителя из сессии <paramref name="uow"/>, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT. Фактическая пачка может быть меньше из-за лимитов БД</param>
		public static void BulkSqlInsert<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize, CancellationToken.None);

			if(context == null)
			{
				return;
			}

			context.OpenTransaction();

			try
			{
				context.SetStage("запись изменений сессии (Flush)");
				context.Session.Flush();

				context.SetStage("блокировка родителя");
				context.Session.Lock(context.Owner, context.ParentLockMode);

				context.SetStage("загрузка коллекции перед вставкой");
				NHibernateUtil.Initialize(collection);
				context.RememberExistingItems();

				context.SetStage("определение лимита размера пачки");
				var maxPacket = context.UsesMaxPacket
					? (long?)Convert.ToInt64(context.Session.CreateSQLQuery(_maxPacketQuery).UniqueResult())
					: null;

				context.SetStage("вставка строк");
				var affectedRows = 0;

				foreach(var command in context.CreateCommands(maxPacket))
				{
					try
					{
						affectedRows += context.Batcher.ExecuteNonQuery(command);
					}
					finally
					{
						context.Batcher.CloseCommand(command, null);
					}
				}

				context.EnsureAffectedRows(affectedRows);

				context.SetStage("загрузка вставленных строк");
				var items = context.Session.CreateFilter(collection, string.Empty).List<TItem>();
				var orderedIds = context.CreateOrderQuery()?.List<object>();

				context.SetStage("сверка количества");
				context.SynchronizeCollection(items, orderedIds);

				context.SetStage("передача записей в трекер истории");
				context.RaisePostInsertEvents();
			}
			catch(OperationCanceledException)
			{
				context.Rollback();
				throw;
			}
			catch(Exception ex)
			{
				var rollbackError = context.Rollback();
				throw context.CreateFailure(ex, rollbackError);
			}
		}

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и загружает их в сессию и в коллекцию.
		/// Поведение для ссылки на родителя, транзакции, блокировки и ошибок описано у <see cref="CollectionBulkSqlInsertExtensions"/>
		/// </summary>
		/// <param name="collection">Коллекция родителя из сессии <paramref name="uow"/>, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT. Фактическая пачка может быть меньше из-за лимитов БД</param>
		/// <param name="cancellationToken">Токен отмены. Отмена после начала записи откатывает транзакцию UoW</param>
		public static async Task BulkSqlInsertAsync<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize,
			CancellationToken cancellationToken = default)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize, cancellationToken);

			if(context == null)
			{
				return;
			}

			context.OpenTransaction();

			try
			{
				context.SetStage("запись изменений сессии (Flush)");
				await context.Session.FlushAsync(cancellationToken);

				context.SetStage("блокировка родителя");
				await context.Session.LockAsync(context.Owner, context.ParentLockMode, cancellationToken);

				context.SetStage("загрузка коллекции перед вставкой");
				await NHibernateUtil.InitializeAsync(collection, cancellationToken);
				context.RememberExistingItems();

				context.SetStage("определение лимита размера пачки");
				var maxPacket = context.UsesMaxPacket
					? (long?)Convert.ToInt64(await context.Session.CreateSQLQuery(_maxPacketQuery).UniqueResultAsync(cancellationToken))
					: null;

				context.SetStage("вставка строк");
				var affectedRows = 0;

				foreach(var command in context.CreateCommands(maxPacket))
				{
					try
					{
						cancellationToken.ThrowIfCancellationRequested();
						affectedRows += await context.Batcher.ExecuteNonQueryAsync(command, cancellationToken);
					}
					finally
					{
						context.Batcher.CloseCommand(command, null);
					}
				}

				context.EnsureAffectedRows(affectedRows);

				context.SetStage("загрузка вставленных строк");
				var items = await context.Session.CreateFilter(collection, string.Empty).ListAsync<TItem>(cancellationToken);
				var orderQuery = context.CreateOrderQuery();
				var orderedIds = orderQuery == null ? null : await orderQuery.ListAsync<object>(cancellationToken);

				context.SetStage("сверка количества");
				context.SynchronizeCollection(items, orderedIds);

				context.SetStage("передача записей в трекер истории");
				context.RaisePostInsertEvents();
			}
			catch(OperationCanceledException)
			{
				await context.RollbackAsync();
				throw;
			}
			catch(Exception ex)
			{
				var rollbackError = await context.RollbackAsync();

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}

				throw context.CreateFailure(ex, rollbackError);
			}
		}

		/// <summary>
		/// Расхождение количества. Наружу выходит только как <see cref="BulkSqlInsertException"/> без вложенного исключения
		/// </summary>
		private class CountMismatchException : Exception
		{
			public CountMismatchException(string message) : base(message)
			{
			}
		}

		private class BulkInsertContext<TItem>
			where TItem : class
		{
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

		/// <summary>
		/// Описание вставки строк одного типа сущности: проверка маппинга и элементов, таблица, колонки, типы параметров
		/// </summary>
		private class InsertPlan
		{
			private readonly ISessionImplementor _session;
			private readonly AbstractEntityPersister _persister;
			private readonly IType _keyType;
			private readonly int[] _propertyIndexes;
			private readonly int[] _parentReferenceIndexes;
			private readonly int[] _entityReferenceIndexes;
			private readonly int[] _collectionIndexes;
			private readonly SqlString _insertHeader;
			private readonly SqlString _rowValues;
			private readonly SqlType[] _rowSqlTypes;
			private readonly string _discriminatorSqlValue;

			public InsertPlan(AbstractEntityPersister persister, AbstractCollectionPersister collectionPersister, ISessionImplementor session)
			{
				_session = session;
				_persister = persister;
				_keyType = collectionPersister.KeyType;

				ValidatePersister(persister);

				var factory = session.Factory;
				var keyColumns = collectionPersister.KeyColumnNames;
				var columns = new List<string>(keyColumns);
				var sqlTypes = new List<SqlType>(_keyType.SqlTypes(factory));
				var propertyIndexes = new List<int>();
				var parentReferenceIndexes = new List<int>();
				var entityReferenceIndexes = new List<int>();
				var collectionIndexes = new List<int>();

				for(var i = 0; i < persister.PropertyTypes.Length; i++)
				{
					var propertyType = persister.PropertyTypes[i];
					var propertyColumns = persister.GetPropertyColumnNames(i);

					if(propertyType.IsCollectionType)
					{
						collectionIndexes.Add(i);
						continue;
					}

					// Ссылка с Not.Insert() в INSERT не попадает, но проверяться должна
					if(propertyType.IsEntityType)
					{
						if(MatchesKey(propertyColumns, keyColumns))
						{
							parentReferenceIndexes.Add(i);
						}
						else
						{
							entityReferenceIndexes.Add(i);
						}
					}

					if(!persister.PropertyInsertability[i] || propertyColumns.Length == 0)
					{
						continue;
					}

					if(IsKeyColumns(propertyColumns, keyColumns))
					{
						// Обратная ссылка на родителя (References на ту же колонку): значение берется из ключа коллекции
						continue;
					}

					columns.AddRange(propertyColumns);
					sqlTypes.AddRange(propertyType.SqlTypes(factory));
					propertyIndexes.Add(i);
				}

				_propertyIndexes = propertyIndexes.ToArray();
				_parentReferenceIndexes = parentReferenceIndexes.ToArray();
				_entityReferenceIndexes = entityReferenceIndexes.ToArray();
				_collectionIndexes = collectionIndexes.ToArray();

				var discriminatorColumn = persister.DiscriminatorColumnName;
				var discriminatorSqlValue = persister.DiscriminatorSQLValue;

				if(!string.IsNullOrEmpty(discriminatorColumn)
					&& !string.IsNullOrEmpty(discriminatorSqlValue)
					&& discriminatorSqlValue != InFragment.Null
					&& discriminatorSqlValue != InFragment.NotNull)
				{
					columns.Add(discriminatorColumn);
					_discriminatorSqlValue = discriminatorSqlValue;
				}

				var duplicateColumn = columns
					.GroupBy(column => column, StringComparer.OrdinalIgnoreCase)
					.FirstOrDefault(group => group.Count() > 1);

				if(duplicateColumn != null)
				{
					throw new NotSupportedException(
						$"Колонка {duplicateColumn.Key} сущности {persister.EntityName} заполняется несколькими свойствами. Массовая вставка не поддерживает такой маппинг");
				}

				_rowSqlTypes = sqlTypes.ToArray();
				ParametersPerRow = Math.Max(1, _rowSqlTypes.Length);

				var header = $"INSERT INTO {persister.TableName} ({string.Join(", ", columns)}) VALUES ";
				HeaderBytes = Encoding.UTF8.GetByteCount(header);
				_insertHeader = new SqlString(header);
				_rowValues = BuildRowValues(_rowSqlTypes.Length, _discriminatorSqlValue);
			}

			public int ParametersPerRow { get; }

			public long HeaderBytes { get; }

			/// <summary>
			/// Проверяет элемент до первого INSERT: новый объект, ссылка на родителя, ссылки на другие сущности, собственные коллекции
			/// </summary>
			public void ValidateItem(object item, object key)
			{
				if(_persister.IsTransient(item, _session) != true)
				{
					throw new ArgumentException(
						$"Элемент {_persister.EntityName} с Id {_persister.GetIdentifier(item)} не новый. Массовая вставка принимает только объекты с незаполненным Id",
						"newItems");
				}

				var values = _persister.GetPropertyValues(item);

				foreach(var index in _collectionIndexes)
				{
					if(values[index] is IEnumerable collection && collection.Cast<object>().Any())
					{
						throw new NotSupportedException(
							$"У элемента {_persister.EntityName} непустая коллекция {_persister.PropertyNames[index]}. Массовая вставка ее не сохранит");
					}
				}

				foreach(var index in _parentReferenceIndexes)
				{
					EnsureSameParent(values[index], key, index);
				}

				foreach(var index in _entityReferenceIndexes)
				{
					EnsureReferenceAvailable(values[index], index);
				}
			}

			public long EstimateRowBytes(object row)
			{
				var bytes = 32L + (_discriminatorSqlValue?.Length ?? 0);
				var values = _persister.GetPropertyValues(row);

				foreach(var index in _propertyIndexes)
				{
					bytes += EstimateValueBytes(values[index], _persister.PropertyTypes[index]);
				}

				return bytes;
			}

			public DbCommand CreateCommand<TItem>(IList<TItem> rows, object key)
				where TItem : class
			{
				var sqlBuilder = new SqlStringBuilder();
				sqlBuilder.Add(_insertHeader);

				for(var i = 0; i < rows.Count; i++)
				{
					if(i > 0)
					{
						sqlBuilder.Add(", ");
					}

					sqlBuilder.Add(_rowValues);
				}

				var sqlTypes = new SqlType[_rowSqlTypes.Length * rows.Count];

				for(var i = 0; i < rows.Count; i++)
				{
					Array.Copy(_rowSqlTypes, 0, sqlTypes, i * _rowSqlTypes.Length, _rowSqlTypes.Length);
				}

				var command = _session.Batcher.PrepareCommand(CommandType.Text, sqlBuilder.ToSqlString(), sqlTypes);

				try
				{
					var index = 0;

					foreach(var row in rows)
					{
						index = BindRow(command, row, key, index);
					}
				}
				catch
				{
					_session.Batcher.CloseCommand(command, null);
					throw;
				}

				return command;
			}

			private int BindRow(DbCommand command, object row, object key, int index)
			{
				_keyType.NullSafeSet(command, key, index, _session);
				index += _keyType.GetColumnSpan(_session.Factory);

				var values = _persister.GetPropertyValues(row);

				foreach(var propertyIndex in _propertyIndexes)
				{
					var propertyType = _persister.PropertyTypes[propertyIndex];
					propertyType.NullSafeSet(command, values[propertyIndex], index, _session);
					index += propertyType.GetColumnSpan(_session.Factory);
				}

				return index;
			}

			private void EnsureSameParent(object reference, object key, int propertyIndex)
			{
				if(reference == null)
				{
					return;
				}

				var referenceId = reference is INHibernateProxy proxy
					? proxy.HibernateLazyInitializer.Identifier
					: _session.GetEntityPersister(null, reference).GetIdentifier(reference);

				if(!_keyType.IsEqual(referenceId, key, _session.Factory))
				{
					throw new ArgumentException(
						$"Свойство {_persister.PropertyNames[propertyIndex]} элемента {_persister.EntityName} ссылается на другого родителя (Id {referenceId}), а вставка идет в родителя с Id {key}",
						"newItems");
				}
			}

			private void EnsureReferenceAvailable(object reference, int propertyIndex)
			{
				if(reference == null || reference is INHibernateProxy)
				{
					return;
				}

				var propertyName = _persister.PropertyNames[propertyIndex];
				var entry = _session.PersistenceContext.GetEntry(reference);

				if(entry != null)
				{
					if(entry.Status == Status.Deleted || entry.Status == Status.Gone)
					{
						throw new InvalidOperationException(
							$"Свойство {propertyName} элемента {_persister.EntityName} ссылается на сущность, помеченную на удаление");
					}

					return;
				}

				var entityName = _session.GetEntityPersister(null, reference).EntityName;

				if(ForeignKeys.IsTransientSlow(entityName, reference, _session))
				{
					throw new TransientObjectException(
						$"Свойство {propertyName} элемента {_persister.EntityName} ссылается на несохраненную сущность {entityName}");
				}
			}

			private long EstimateValueBytes(object value, IType type)
			{
				switch(value)
				{
					case null:
						return 8;
					case string text:
						// Символы при передаче экранируются, поэтому с запасом
						return Encoding.UTF8.GetByteCount(text) * 2L + 8;
					case byte[] bytes:
						return bytes.Length * 2L + 8;
				}

				if(type is IAbstractComponentType componentType)
				{
					var values = componentType.GetPropertyValues(value, _session);
					var total = 0L;

					for(var i = 0; i < values.Length; i++)
					{
						total += EstimateValueBytes(values[i], componentType.Subtypes[i]);
					}

					return total;
				}

				return 32;
			}

			private static void ValidatePersister(AbstractEntityPersister persister)
			{
				if(persister.IsMultiTable)
				{
					throw new NotSupportedException(
						$"Сущность {persister.EntityName} хранится в нескольких таблицах. Массовая вставка поддерживает только сущности одной таблицы");
				}

				if(persister.IsVersioned)
				{
					throw new NotSupportedException(
						$"Сущность {persister.EntityName} версионируется. Массовая вставка не поддерживает версионные сущности");
				}

				if(!(persister.IdentifierGenerator is IdentityGenerator))
				{
					throw new NotSupportedException(
						$"Id сущности {persister.EntityName} смаплен не как GeneratedBy.Native(). Массовая вставка поддерживает только Id, который выдает БД");
				}
			}

			private static bool MatchesKey(string[] propertyColumns, string[] keyColumns) =>
				propertyColumns.Length == keyColumns.Length
				&& propertyColumns.Intersect(keyColumns, StringComparer.OrdinalIgnoreCase).Count() == keyColumns.Length;

			private static bool IsKeyColumns(string[] propertyColumns, string[] keyColumns)
			{
				var overlap = propertyColumns.Intersect(keyColumns, StringComparer.OrdinalIgnoreCase).Count();

				if(overlap == 0)
				{
					return false;
				}

				if(overlap == keyColumns.Length && propertyColumns.Length == keyColumns.Length)
				{
					return true;
				}

				throw new NotSupportedException(
					$"Колонки свойства ({string.Join(", ", propertyColumns)}) частично совпадают с ключом коллекции ({string.Join(", ", keyColumns)})");
			}

			private static SqlString BuildRowValues(int parametersCount, string discriminatorSqlValue)
			{
				var builder = new SqlStringBuilder();
				builder.Add("(");

				for(var i = 0; i < parametersCount; i++)
				{
					if(i > 0)
					{
						builder.Add(", ");
					}

					builder.Add(Parameter.Placeholder);
				}

				if(discriminatorSqlValue != null)
				{
					builder.Add(", ");
					builder.Add(discriminatorSqlValue);
				}

				builder.Add(")");

				return builder.ToSqlString();
			}
		}

		private class ReferenceComparer : IEqualityComparer<object>
		{
			public static readonly ReferenceComparer Instance = new ReferenceComparer();

			public new bool Equals(object x, object y) => ReferenceEquals(x, y);

			public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
		}
	}
}

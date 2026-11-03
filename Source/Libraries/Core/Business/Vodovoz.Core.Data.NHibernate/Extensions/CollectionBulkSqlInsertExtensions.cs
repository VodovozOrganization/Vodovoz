using NHibernate;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Id;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Массовая вставка элементов в one-to-many коллекцию одним SQL запросом на пачку строк
	/// </summary>
	/// <remarks>
	/// Сценарий: в коллекцию родителя нужно добавить большое количество новых строк.
	/// Вместо отдельного INSERT на каждую сущность (Id через AUTO_INCREMENT не дает NHibernate склеивать вставки)
	/// формируется многострочный INSERT по метаданным маппинга, после чего коллекция синхронизируется с БД одним запросом.
	/// <para>Важно:</para>
	/// <list type="bullet">
	/// <item>Перед вставкой выполняется Flush сессии, родитель должен быть сохранен, транзакция UoW открыта.</item>
	/// <item>Вставка выполняется в текущей транзакции UoW и откатывается вместе с ней.</item>
	/// <item>События NHibernate (PostInsert, история изменений [HistoryTrace]), каскады и SessionScopeEntitySaved для новых строк не срабатывают.</item>
	/// <item>Переданные объекты остаются транзиентными, их нельзя сохранять или добавлять в коллекцию.
	/// После вставки работать нужно с элементами коллекции.</item>
	/// <item>Если коллекция была загружена, она дополняется вставленными элементами из БД.
	/// Если не была загружена, она загрузится из БД вместе с новыми строками при первом обращении.</item>
	/// </list>
	/// </remarks>
	public static class CollectionBulkSqlInsertExtensions
	{
		private const int _defaultBatchSize = 1000;
		private const int _maxParametersPerCommand = 65535;

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и синхронизирует коллекцию с БД
		/// </summary>
		/// <param name="collection">Коллекция сохраненного родителя, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT</param>
		public static void BulkSqlInsert<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize);

			if(context == null)
			{
				return;
			}

			context.Session.Flush();
			context.ResolveCollection();

			foreach(var command in context.CreateCommands())
			{
				try
				{
					context.Batcher.ExecuteNonQuery(command);
				}
				finally
				{
					context.Batcher.CloseCommand(command, null);
				}
			}

			if(context.NeedSynchronizeCollection)
			{
				var items = context.Session.CreateFilter(collection, string.Empty).List<TItem>();
				context.SynchronizeCollection(items);
			}
		}

		/// <summary>
		/// Вставляет новые элементы в one-to-many коллекцию многострочными INSERT и синхронизирует коллекцию с БД
		/// </summary>
		/// <param name="collection">Коллекция сохраненного родителя, например <c>parent.Items</c></param>
		/// <param name="uow">UoW, в сессии которого находится родитель</param>
		/// <param name="newItems">Новые, еще не сохраненные элементы</param>
		/// <param name="batchSize">Максимальное количество строк в одном INSERT</param>
		/// <param name="cancellationToken">Токен отмены</param>
		public static async Task BulkSqlInsertAsync<TItem>(
			this ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = _defaultBatchSize,
			CancellationToken cancellationToken = default)
			where TItem : class
		{
			var context = BulkInsertContext<TItem>.Create(collection, uow, newItems, batchSize);

			if(context == null)
			{
				return;
			}

			await context.Session.FlushAsync(cancellationToken);
			context.ResolveCollection();

			foreach(var command in context.CreateCommands())
			{
				try
				{
					await context.Batcher.ExecuteNonQueryAsync(command, cancellationToken);
				}
				finally
				{
					context.Batcher.CloseCommand(command, null);
				}
			}

			if(context.NeedSynchronizeCollection)
			{
				var items = await context.Session.CreateFilter(collection, string.Empty).ListAsync<TItem>(cancellationToken);
				context.SynchronizeCollection(items);
			}
		}

		private class BulkInsertContext<TItem>
			where TItem : class
		{
			private readonly ICollection<TItem> _collection;
			private readonly IPersistentCollection _persistentCollection;
			private readonly ISessionImplementor _sessionImplementor;
			private AbstractCollectionPersister _collectionPersister;
			private readonly IList<TItem> _newItems;
			private readonly int _batchSize;
			private object _key;

			private BulkInsertContext(
				ICollection<TItem> collection,
				IPersistentCollection persistentCollection,
				ISession session,
				IList<TItem> newItems,
				int batchSize)
			{
				_collection = collection;
				_persistentCollection = persistentCollection;
				Session = session;
				_sessionImplementor = session.GetSessionImplementation();
				_newItems = newItems;
				_batchSize = batchSize;
			}

			public ISession Session { get; }

			public IBatcher Batcher => _sessionImplementor.Batcher;

			/// <summary>
			/// Коллекция уже загружена в память и ее нужно дополнить вставленными строками.
			/// Незагруженная коллекция подтянет новые строки сама при первом обращении.
			/// </summary>
			public bool NeedSynchronizeCollection => _persistentCollection.WasInitialized;

			public static BulkInsertContext<TItem> Create(
				ICollection<TItem> collection,
				IUnitOfWork uow,
				IEnumerable<TItem> newItems,
				int batchSize)
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

				var items = newItems.ToList();

				if(items.Count == 0)
				{
					return null;
				}

				if(!(collection is IPersistentCollection persistentCollection))
				{
					throw new InvalidOperationException(
						"Коллекция не связана с сессией NHibernate. Сохраните родителя в UoW и передайте коллекцию из его свойства после сохранения");
				}

				var session = uow.Session;
				var sessionImplementor = session.GetSessionImplementation();

				if(sessionImplementor.PersistenceContext.GetCollectionEntry(persistentCollection) == null)
				{
					throw new InvalidOperationException(
						"Коллекция не принадлежит сессии переданного UoW");
				}

				var transaction = session.GetCurrentTransaction();

				if(transaction == null || !transaction.IsActive)
				{
					throw new InvalidOperationException(
						"Массовая вставка выполняется только в открытой транзакции UoW. Сохраните родителя через UoW перед вставкой");
				}

				var existingItems = persistentCollection.WasInitialized
					? new HashSet<TItem>(collection, ReferenceComparer.Instance)
					: new HashSet<TItem>(ReferenceComparer.Instance);
				var uniqueItems = new HashSet<TItem>(ReferenceComparer.Instance);

				foreach(var item in items)
				{
					if(item == null)
					{
						throw new ArgumentException("Список новых элементов содержит null", nameof(newItems));
					}

					if(!uniqueItems.Add(item))
					{
						throw new ArgumentException("Один и тот же объект передан в списке новых элементов несколько раз", nameof(newItems));
					}

					if(session.Contains(item) || existingItems.Contains(item))
					{
						throw new ArgumentException(
							"Элемент уже находится в сессии или в коллекции. Массовая вставка принимает только новые объекты, которые не добавлены в коллекцию",
							nameof(newItems));
					}
				}

				return new BulkInsertContext<TItem>(collection, persistentCollection, session, items, batchSize);
			}

			/// <summary>
			/// Определяет маппинг коллекции и ключ родителя. Вызывается после Flush:
			/// у коллекции только что сохраненного родителя роль и ключ появляются только после него
			/// </summary>
			public void ResolveCollection()
			{
				var collectionEntry = _sessionImplementor.PersistenceContext.GetCollectionEntry(_persistentCollection);
				var role = collectionEntry?.Role ?? _persistentCollection.Role;

				if(role == null)
				{
					throw new InvalidOperationException(
						"Не удалось определить маппинг коллекции. Родитель должен быть сохранен в UoW");
				}

				_collectionPersister = _sessionImplementor.Factory.GetCollectionPersister(role) as AbstractCollectionPersister;

				if(_collectionPersister == null || !_collectionPersister.IsOneToMany)
				{
					throw new NotSupportedException(
						$"Коллекция {role} не является one-to-many. Массовая вставка поддерживает только one-to-many коллекции");
				}

				if(_collectionPersister.HasIndex)
				{
					throw new NotSupportedException(
						$"Коллекция {role} индексированная (list/map). Массовая вставка поддерживает только bag/set коллекции");
				}

				_key = collectionEntry?.LoadedKey ?? _persistentCollection.Key;

				if(_key == null)
				{
					throw new InvalidOperationException(
						$"Не удалось определить ключ родителя коллекции {role}. Родитель должен быть сохранен");
				}
			}

			public IEnumerable<DbCommand> CreateCommands()
			{
				var groups = _newItems
					.GroupBy(item => GetElementPersister(item))
					.ToList();

				foreach(var group in groups)
				{
					var plan = new InsertPlan(group.Key, _collectionPersister, _sessionImplementor);
					var rows = group.ToList();
					var batchSize = Math.Max(1, Math.Min(_batchSize, _maxParametersPerCommand / plan.ParametersPerRow));

					for(var offset = 0; offset < rows.Count; offset += batchSize)
					{
						var batch = rows.Skip(offset).Take(batchSize).ToList();
						yield return plan.CreateCommand(batch, _key);
					}
				}
			}

			public void SynchronizeCollection(IEnumerable<TItem> itemsFromDatabase)
			{
				var currentItems = new HashSet<TItem>(_collection, ReferenceComparer.Instance);

				foreach(var item in itemsFromDatabase)
				{
					if(currentItems.Add(item))
					{
						_collection.Add(item);
					}
				}

				// Коллекция теперь совпадает с БД: обновляем снимок и снимаем признак изменения,
				// чтобы при Flush не было лишних UPDATE внешнего ключа и корректно работал delete-orphan
				var collectionEntry = _sessionImplementor.PersistenceContext.GetCollectionEntry(_persistentCollection);
				collectionEntry.PostInitialize(_persistentCollection, _sessionImplementor.PersistenceContext);
				_persistentCollection.ClearDirty();
			}

			private AbstractEntityPersister GetElementPersister(TItem item)
			{
				var elementPersister = (AbstractEntityPersister)_collectionPersister.ElementPersister;
				var itemPersister = _sessionImplementor.GetEntityPersister(null, item) as AbstractEntityPersister;

				if(itemPersister == null
					|| (itemPersister.EntityName != elementPersister.EntityName
						&& !elementPersister.IsSubclassEntityName(itemPersister.EntityName)))
				{
					throw new ArgumentException(
						$"Тип {item.GetType().FullName} не является элементом коллекции {_collectionPersister.Role}");
				}

				return itemPersister;
			}
		}

		/// <summary>
		/// Описание вставки строк одного типа сущности: таблица, колонки, типы параметров
		/// </summary>
		private class InsertPlan
		{
			private readonly ISessionImplementor _session;
			private readonly AbstractEntityPersister _persister;
			private readonly IType _keyType;
			private readonly IIdentifierGenerator _identifierGenerator;
			private readonly int[] _propertyIndexes;
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

				_identifierGenerator = persister.IdentifierGenerator is IdentityGenerator
					? null
					: persister.IdentifierGenerator;

				if(_identifierGenerator != null)
				{
					columns.AddRange(persister.IdentifierColumnNames);
					sqlTypes.AddRange(persister.IdentifierType.SqlTypes(factory));
				}

				var propertyIndexes = new List<int>();

				for(var i = 0; i < persister.PropertyTypes.Length; i++)
				{
					var propertyType = persister.PropertyTypes[i];
					var propertyColumns = persister.GetPropertyColumnNames(i);

					if(!persister.PropertyInsertability[i] || propertyType.IsCollectionType || propertyColumns.Length == 0)
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

				_insertHeader = new SqlString($"INSERT INTO {persister.TableName} ({string.Join(", ", columns)}) VALUES ");
				_rowValues = BuildRowValues(_rowSqlTypes.Length, _discriminatorSqlValue);
			}

			public int ParametersPerRow { get; }

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

				if(_identifierGenerator != null)
				{
					var id = _identifierGenerator.Generate(_session, row);
					_persister.IdentifierType.NullSafeSet(command, id, index, _session);
					index += _persister.IdentifierType.GetColumnSpan(_session.Factory);
				}

				var values = _persister.GetPropertyValues(row);

				foreach(var propertyIndex in _propertyIndexes)
				{
					var propertyType = _persister.PropertyTypes[propertyIndex];
					propertyType.NullSafeSet(command, values[propertyIndex], index, _session);
					index += propertyType.GetColumnSpan(_session.Factory);
				}

				return index;
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
			}

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

using NHibernate;
using NHibernate.Engine;
using NHibernate.Id;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;
using NHibernate.Type;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Описание вставки строк одного типа сущности: проверка маппинга и элементов, таблица, колонки, типы параметров
	/// </summary>
	internal class InsertPlan
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
}

using MySqlConnector;
using NHibernate;
using NHibernate.Collection;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Общее для тестов с БД: подготовка данных, вызов метода, проверка «ничего не изменилось».
	/// </summary>
	[Collection(BulkSqlInsertCollection.Name)]
	public abstract class BulkSqlInsertTestBase : IDisposable
	{
		protected const string ChildrenTable = "bulk_children";

		private readonly List<IUnitOfWork> _unitsOfWork = new List<IUnitOfWork>();
		private int _nextCode;

		protected BulkSqlInsertTestBase(BulkSqlInsertFixture fixture)
		{
			Fixture = fixture;
			Fixture.Database.TruncateAll();
			SqlLog.Reset();
			AffectedRowsBatcherFactory.AffectedRows = null;
		}

		protected BulkSqlInsertFixture Fixture { get; }

		protected TestDatabase Database => Fixture.Database;

		public void Dispose()
		{
			SqlLog.Reset();
			AffectedRowsBatcherFactory.AffectedRows = null;

			foreach(var uow in _unitsOfWork)
			{
				try
				{
					uow.Dispose();
				}
				catch(Exception)
				{
					// UoW уже закрыт самим тестом или его соединение оборвано: результат теста от этого не зависит
				}
			}
		}

		protected IUnitOfWork NewUow()
		{
			var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Тест массовой вставки");
			_unitsOfWork.Add(uow);

			return uow;
		}

		protected static async Task Insert<TItem>(
			InsertMode mode,
			ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> newItems,
			int batchSize = 1000,
			CancellationToken cancellationToken = default)
			where TItem : class
		{
			if(mode == InsertMode.Sync)
			{
				collection.BulkSqlInsert(uow, newItems, batchSize);
			}
			else
			{
				await collection.BulkSqlInsertAsync(uow, newItems, batchSize, cancellationToken);
			}
		}

		/// <summary>
		/// Создаёт и коммитит родителя с уже существующими элементами. Журнал SQL после этого чист.
		/// </summary>
		protected int CreateParent(int existingItems = 0)
		{
			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка родителя"))
			{
				var parent = new BulkParent { Code = Interlocked.Increment(ref _nextCode) };
				uow.Save(parent);

				foreach(var child in NewChildren(existingItems, "old"))
				{
					child.Parent = parent;
					uow.Save(child);
				}

				uow.Commit();
				SqlLog.Clear();

				return parent.Id;
			}
		}

		protected static List<BulkChild> NewChildren(int count, string prefix = "new")
		{
			return Enumerable.Range(1, count)
				.Select(i => new BulkChild
				{
					Name = $"{prefix}-{i}",
					Amount = 10.5m + i,
					OccurredAt = new DateTime(2026, 1, 1).AddMinutes(i),
					IsActive = i % 2 == 0,
					Kind = BulkChildKind.Second,
					Address = new BulkAddress { City = "Город", Street = $"Улица {i}" }
				})
				.ToList();
		}

		protected long CountChildren(int parentId, string table = ChildrenTable) =>
			Database.Count(table, $"parent_id = {parentId}");

		/// <summary>
		/// Подсчёт через соединение и транзакцию сессии, мимо журнала SQL.
		/// </summary>
		protected static long CountInSession(IUnitOfWork uow, string table, int parentId)
		{
			using(var command = uow.Session.Connection.CreateCommand())
			{
				uow.Session.GetCurrentTransaction()?.Enlist(command);
				command.CommandText = $"SELECT COUNT(*) FROM `{table}` WHERE parent_id = {parentId}";

				return Convert.ToInt64(command.ExecuteScalar());
			}
		}

		protected static void ExecuteInSession(IUnitOfWork uow, string sql)
		{
			using(var command = uow.Session.Connection.CreateCommand())
			{
				uow.Session.GetCurrentTransaction()?.Enlist(command);
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}

		protected static bool IsTransactionActive(IUnitOfWork uow) =>
			uow.Session.GetCurrentTransaction()?.IsActive == true;

		/// <summary>
		/// Инвариант «ничего не изменилось» для негативных тестов.
		/// </summary>
		protected StateSnapshot<TItem> Snapshot<TItem>(IUnitOfWork uow, ICollection<TItem> collection, IEnumerable<TItem> newItems, string table, int parentId)
			where TItem : class
		{
			SqlLog.Clear();

			return new StateSnapshot<TItem>
			{
				Uow = uow,
				Collection = collection,
				NewItems = newItems.ToList(),
				Table = table,
				ParentId = parentId,
				WasInitialized = NHibernateUtil.IsInitialized(collection),
				Items = NHibernateUtil.IsInitialized(collection) ? collection.ToList() : null,
				RowsBefore = CountInSession(uow, table, parentId),
				TransactionWasActive = IsTransactionActive(uow)
			};
		}

		protected class StateSnapshot<TItem>
			where TItem : class
		{
			public IUnitOfWork Uow { get; set; }
			public ICollection<TItem> Collection { get; set; }
			public List<TItem> NewItems { get; set; }
			public string Table { get; set; }
			public int ParentId { get; set; }
			public bool WasInitialized { get; set; }
			public List<TItem> Items { get; set; }
			public long RowsBefore { get; set; }
			public bool TransactionWasActive { get; set; }

			public void AssertNothingChanged()
			{
				Assert.Equal(0, SqlLog.Inserts(Table));
				Assert.Equal(TransactionWasActive, IsTransactionActive(Uow));
				Assert.Equal(RowsBefore, CountInSession(Uow, Table, ParentId));
				Assert.Equal(WasInitialized, NHibernateUtil.IsInitialized(Collection));

				if(WasInitialized)
				{
					Assert.Equal(Items, Collection.ToList());
				}

				foreach(var item in NewItems.Where(x => x != null))
				{
					Assert.False(Uow.Session.Contains(item));
				}
			}
		}

		/// <summary>
		/// Проверяет отказ массовой вставки после начала работы с БД: тип, вложенное исключение, текст и закрытие UoW.
		/// </summary>
		protected static void AssertFailedWithRollback(BulkSqlInsertException exception, IUnitOfWork uow, Type innerType = null)
		{
			if(innerType == null)
			{
				Assert.Null(exception.InnerException);
			}
			else
			{
				Assert.IsAssignableFrom(innerType, exception.InnerException);
			}

			Assert.Contains("откат", exception.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Contains("UoW закрыт", exception.Message, StringComparison.OrdinalIgnoreCase);
			AssertUowClosed(uow);
		}

		/// <summary>
		/// После ошибки записи метод закрывает UoW: сессия закрыта, транзакция откатывается вместе с ней.
		/// Что строк нет, тесты проверяют ещё и по данным в БД
		/// </summary>
		protected static void AssertUowClosed(IUnitOfWork uow)
		{
			Assert.False(uow.IsAlive);
			Assert.False(uow.Session.IsOpen);
		}

		protected long ScalarFromOtherConnection(string sql, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
		{
			using(var connection = Database.OpenConnection())
			{
				return TestDatabase.Scalar(connection, isolationLevel, sql);
			}
		}

		protected static bool IsInsertInto(System.Data.Common.DbCommand command, string table) =>
			command.CommandText.TrimStart().StartsWith($"INSERT INTO {table}", StringComparison.OrdinalIgnoreCase);

		protected static bool IsSelectFrom(System.Data.Common.DbCommand command, string table) =>
			command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
			&& command.CommandText.IndexOf(table, StringComparison.OrdinalIgnoreCase) >= 0;

		protected static IPersistentCollection AsPersistent<TItem>(ICollection<TItem> collection) =>
			(IPersistentCollection)collection;

		protected static MySqlConnection SessionConnection(IUnitOfWork uow) =>
			(MySqlConnection)uow.Session.Connection;
	}
}

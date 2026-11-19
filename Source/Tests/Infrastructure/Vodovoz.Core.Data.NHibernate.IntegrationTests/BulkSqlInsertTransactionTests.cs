using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;
using IsolationLevel = System.Data.IsolationLevel;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа G: вставка идёт в одной сессии и одной транзакции UoW и видна другим только после общего коммита.
	/// </summary>
	public class BulkSqlInsertTransactionTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertTransactionTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		/// <summary>
		/// G-01. Все команды метода идут в соединении и транзакции сессии, новых соединений нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenExecuted_UsesSessionConnectionAndTransaction(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			var sessionConnection = uow.Session.Connection;
			System.Data.Common.DbTransaction sessionTransaction;

			using(var probe = sessionConnection.CreateCommand())
			{
				uow.Session.GetCurrentTransaction().Enlist(probe);
				sessionTransaction = probe.Transaction;
			}

			using(var monitor = Database.OpenConnection())
			{
				var connectionsBefore = ServerConnections(monitor);
				SqlLog.Clear();

				await Insert(mode, parent.Items, uow, NewChildren(5), 2);

				Assert.NotEmpty(SqlLog.Commands);
				Assert.All(SqlLog.Commands, command =>
				{
					Assert.Same(sessionConnection, command.Connection);
					Assert.Same(sessionTransaction, command.Transaction);
				});
				Assert.Equal(connectionsBefore, ServerConnections(monitor));
			}
		}

		/// <summary>
		/// G-02. До коммита строки не видны другой сессии NHibernate, после коммита видны все.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_BeforeCommit_RowsInvisibleToOtherSession(InsertMode mode)
		{
			var parentId = CreateParent(1);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			using(var other = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Другая сессия"))
			{
				Assert.Equal(1, other.GetById<BulkParent>(parentId).Items.Count);
			}

			uow.Commit();

			using(var other = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Другая сессия"))
			{
				Assert.Equal(4, other.GetById<BulkParent>(parentId).Items.Count);
			}
		}

		/// <summary>
		/// G-03. До коммита строки не видны прямому соединению на READ COMMITTED и REPEATABLE READ.
		/// </summary>
		[DatabaseTheory]
		[InlineData(InsertMode.Sync, IsolationLevel.ReadCommitted)]
		[InlineData(InsertMode.Sync, IsolationLevel.RepeatableRead)]
		[InlineData(InsertMode.Async, IsolationLevel.ReadCommitted)]
		[InlineData(InsertMode.Async, IsolationLevel.RepeatableRead)]
		public async Task BulkSqlInsert_BeforeCommit_RowsInvisibleToDirectConnection(InsertMode mode, IsolationLevel isolationLevel)
		{
			var parentId = CreateParent(2);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var sql = $"SELECT COUNT(*) FROM {ChildrenTable} WHERE parent_id = {parentId}";

			await Insert(mode, parent.Items, uow, NewChildren(3), 2);

			Assert.Equal(2, ScalarFromOtherConnection(sql, isolationLevel));

			uow.Commit();

			Assert.Equal(5, ScalarFromOtherConnection(sql, isolationLevel));
		}

		/// <summary>
		/// G-04. Общий коммит: новый родитель, правка другой сущности и дети становятся видны одновременно.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WithOtherChangesInUow_AllBecomeVisibleTogether(InsertMode mode)
		{
			var otherParentId = CreateParent();
			var uow = NewUow();
			var other = uow.GetById<BulkParent>(otherParentId);
			other.Name = "правка";
			uow.Save(other);
			var parent = new BulkParent { Code = 100, Name = "новый" };
			uow.Save(parent);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			Assert.Equal(0, Database.Count("bulk_parents", "name = 'новый'"));
			Assert.Equal(0, Database.Count("bulk_parents", "name = 'правка'"));
			Assert.Equal(0, Database.Count(ChildrenTable));

			uow.Commit();

			Assert.Equal(1, Database.Count("bulk_parents", "name = 'новый'"));
			Assert.Equal(1, Database.Count("bulk_parents", "name = 'правка'"));
			Assert.Equal(3, CountChildren(parent.Id));
		}

		/// <summary>
		/// G-05. Явный откат после успешного вызова: нет ни детей, ни родителя, ни других изменений.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenRolledBackAfterCall_NothingRemains(InsertMode mode)
		{
			var uow = NewUow();
			var parent = new BulkParent { Code = 100, Name = "новый" };
			uow.Save(parent);

			await Insert(mode, parent.Items, uow, NewChildren(3));
			uow.Session.GetCurrentTransaction().Rollback();

			Assert.Equal(0, Database.Count("bulk_parents", "name = 'новый'"));
			Assert.Equal(0, Database.Count(ChildrenTable));
		}

		/// <summary>
		/// G-05. UoW закрыт без коммита после успешного вызова: нет ни детей, ни родителя.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenUowDisposedAfterCall_NothingRemains(InsertMode mode)
		{
			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Без коммита"))
			{
				var parent = new BulkParent { Code = 100, Name = "новый" };
				uow.Save(parent);

				await Insert(mode, parent.Items, uow, NewChildren(3));
			}

			Assert.Equal(0, Database.Count("bulk_parents", "name = 'новый'"));
			Assert.Equal(0, Database.Count(ChildrenTable));
		}

		/// <summary>
		/// G-06. Общий коммит падает из-за другой сущности: строк этого вызова нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCommitFails_RowsAbsent(InsertMode mode)
		{
			var parentId = CreateParent();

			using(var setup = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка тегов"))
			{
				setup.Save(new BulkTag { Name = "занято" });
				setup.Save(new BulkTag { Name = "свободно" });
				setup.Commit();
			}

			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var tag = uow.Session.QueryOver<BulkTag>().Where(x => x.Name == "свободно").SingleOrDefault();

			await Insert(mode, parent.Items, uow, NewChildren(3));

			tag.Name = "занято";
			uow.Save(tag);

			Assert.ThrowsAny<Exception>(() => uow.Commit());
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// G-07. Два вызова в одном UoW, второй падает: UoW закрыт, нет строк ни первого, ни второго.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSecondCallFails_NoRowsFromEither(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var failing = NewChildren(2, "второй");
			failing[0].UniqueCode = "дубль";
			failing[1].UniqueCode = "дубль";

			await Insert(mode, parent.Items, uow, NewChildren(2, "первый"));
			await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, failing, 1));

			AssertUowClosed(uow);
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// G-08. Транзакция открыта мимо NHibernate прямо на соединении сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTransactionOpenedOnConnection_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;

			using(var external = uow.Session.Connection.BeginTransaction())
			{
				SqlLog.Clear();

				await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

				Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
				external.Rollback();
			}
		}

		/// <summary>
		/// G-08. Ambient TransactionScope без транзакции NHibernate.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenAmbientTransactionScope_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();

			using(var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
			{
				var uow = NewUow();
				var parent = uow.GetById<BulkParent>(parentId);
				_ = parent.Items.Count;
				SqlLog.Clear();

				await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

				Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			}
		}

		/// <summary>
		/// G-09. Метод не коммитит сам: коммит в БД ровно один, от вызывающего кода.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenExecuted_DoesNotCommit(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;

			using(var monitor = Database.OpenConnection())
			{
				var commitsBefore = ServerCommits(monitor);

				await Insert(mode, parent.Items, uow, NewChildren(4), 2);

				Assert.True(IsTransactionActive(uow));
				Assert.Equal(commitsBefore, ServerCommits(monitor));

				uow.Commit();

				Assert.Equal(commitsBefore + 1, ServerCommits(monitor));
			}
		}

		/// <summary>
		/// G-10. Транзакция открыта на сессии в обход UoW: метод вставляет в неё же, а uow.Commit() коммитит строки.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTransactionOpenedOnSession_UowCommitsRows(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			var sessionTransaction = uow.Session.BeginTransaction();

			await Insert(mode, parent.Items, uow, NewChildren(2));

			Assert.Same(sessionTransaction, uow.Session.GetCurrentTransaction());
			Assert.Equal(0, CountChildren(parentId));

			uow.Commit();

			Assert.True(sessionTransaction.WasCommitted);
			Assert.Equal(2, CountChildren(parentId));
		}

		/// <summary>
		/// G-11. Транзакция открыта на сессии в обход UoW: NHibernate отдаёт UoW ту же транзакцию, и uow.Commit() её коммитит.
		/// </summary>
		[DatabaseFact]
		public void Save_WhenTransactionOpenedOnSession_UowCommitsSameTransaction()
		{
			var uow = NewUow();
			var sessionTransaction = uow.Session.BeginTransaction();

			uow.Save(new BulkParent { Code = 100, Name = "через сессию" });

			Assert.Same(sessionTransaction, uow.Session.GetCurrentTransaction());

			uow.Commit();

			Assert.True(sessionTransaction.WasCommitted);
			Assert.Equal(1, Database.Count("bulk_parents", "name = 'через сессию'"));
		}

		private static long ServerConnections(MySqlConnector.MySqlConnection monitor) =>
			StatusValue(monitor, "Connections");

		private static long ServerCommits(MySqlConnector.MySqlConnection monitor) =>
			StatusValue(monitor, "Com_commit");

		private static long StatusValue(MySqlConnector.MySqlConnection monitor, string name)
		{
			using(var command = monitor.CreateCommand())
			{
				command.CommandText = $"SHOW GLOBAL STATUS LIKE '{name}'";

				using(var reader = command.ExecuteReader())
				{
					reader.Read();
					return Convert.ToInt64(reader.GetString(1));
				}
			}
		}
	}
}

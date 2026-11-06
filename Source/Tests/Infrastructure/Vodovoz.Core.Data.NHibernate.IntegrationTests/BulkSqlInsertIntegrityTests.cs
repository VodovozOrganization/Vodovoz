using MySqlConnector;
using NHibernate;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа E: загружено ровно столько, сколько вставлено; при любой ошибке после начала работы с БД транзакция откачена.
	/// Триггеров нет: расхождение моделируется фильтром сессии, вмешательством перед командой и подменой числа строк.
	/// </summary>
	public class BulkSqlInsertIntegrityTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertIntegrityTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		/// <summary>
		/// E-01. Загружено меньше, чем вставлено: фильтр сессии отсекает новые строки.
		/// Транзакция откачена вместе с другими изменениями UoW.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenLoadedLessThanInserted_RollsBackAndThrows(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			parent.Name = "изменён";
			uow.Save(parent);
			uow.Session.EnableFilter(OnlyActiveFilter.FilterName);
			var items = new[]
			{
				new FilteredChild { Name = "активный" },
				new FilteredChild { Name = "архивный", IsArchived = true }
			};

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.FilteredItems, uow, items));

			AssertFailedWithRollback(exception, uow);
			Assert.Equal(0, CountChildren(parentId, "bulk_filtered_children"));
			Assert.Equal(0, Database.Count("bulk_parents", $"id = {parentId} AND name = 'изменён'"));
		}

		/// <summary>
		/// E-02. Загружено больше, чем вставлено: перед загрузкой в той же транзакции появляется лишняя строка.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenLoadedMoreThanInserted_RollsBackAndThrows(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var insertsSeen = 0;
			var extraAdded = false;

			SqlLog.BeforeCommand = command =>
			{
				if(IsInsertInto(command, ChildrenTable))
				{
					insertsSeen++;
				}
				else if(insertsSeen > 0 && !extraAdded && IsSelectFrom(command, ChildrenTable))
				{
					extraAdded = true;
					AddRowInSameTransaction(command, parentId);
				}
			};

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, NewChildren(3)));

			Assert.True(extraAdded);
			AssertFailedWithRollback(exception, uow);
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// E-03. Версионного родителя изменила другая закоммиченная транзакция: ни одного INSERT, чужая правка сохранена.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenVersionedParentStale_RollsBackWithoutInsert(InsertMode mode)
		{
			var parentId = CreateVersionedParent();
			var uow = NewUow();
			var parent = uow.GetById<VersionedParent>(parentId);
			_ = parent.Items.Count;

			Database.Execute($"UPDATE bulk_versioned_parents SET version = version + 1, name = 'чужая правка' WHERE id = {parentId}");
			SqlLog.Clear();

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(
				() => Insert(mode, parent.Items, uow, new[] { new VersionedParentChild { Name = "новый" } }));

			AssertFailedWithRollback(exception, uow, typeof(StaleObjectStateException));
			Assert.Equal(0, SqlLog.Inserts("bulk_versioned_parent_children"));
			Assert.Equal(1, Database.Count("bulk_versioned_parents", $"id = {parentId} AND name = 'чужая правка'"));
		}

		/// <summary>
		/// E-04. Сумма изменённых строк не равна числу элементов: исключение после последней пачки, до загрузки.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenAffectedRowsMismatch_RollsBackBeforeLoad(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			SqlLog.Clear();

			AffectedRowsBatcherFactory.AffectedRows = (command, rows) => IsInsertInto(command, ChildrenTable) ? rows - 1 : rows;

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, NewChildren(5), 2));

			AssertFailedWithRollback(exception, uow);
			// Между пачками проверок нет: выполнены все три пачки, а загрузки не было
			Assert.Equal(3, SqlLog.Inserts(ChildrenTable));
			Assert.Equal(0, SqlLog.Selects(ChildrenTable));
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// E-05. Ошибка БД во второй пачке: первая откачена, исходная ошибка внутри, коллекция без изменений.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenDatabaseErrorInSecondBatch_RollsBackAndWraps(InsertMode mode)
		{
			var parentId = CreateParent(1);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var existing = parent.Items.Count;
			var items = NewChildren(3);
			items[1].UniqueCode = "дубль";
			items[2].UniqueCode = "дубль";

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, items, 2));

			AssertFailedWithRollback(exception, uow, typeof(Exception));
			Assert.Equal(1, CountChildren(parentId));
			Assert.Equal(existing, parent.Items.Count);
		}

		/// <summary>
		/// E-05. Сам Rollback падает, потому что соединение оборвано: внутри всё равно исходная ошибка.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenConnectionLostDuringInsert_KeepsOriginalInnerException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var insertsSeen = 0;

			SqlLog.BeforeCommand = command =>
			{
				if(IsInsertInto(command, ChildrenTable) && ++insertsSeen == 2)
				{
					Database.Execute($"KILL {((MySqlConnection)command.Connection).ServerThread}");
				}
			};

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, NewChildren(4), 2));

			SqlLog.BeforeCommand = null;

			Assert.NotNull(exception.InnerException);
			Assert.IsNotType<TransactionException>(exception.InnerException);
			Assert.Contains("откат", exception.Message, StringComparison.OrdinalIgnoreCase);
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// E-06. Токен отменён до вызова: отмена без отката, транзакция остаётся.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsertAsync_WhenTokenCancelledBeforeCall_ThrowsWithoutRollback()
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			using(var cancellation = new CancellationTokenSource())
			{
				cancellation.Cancel();

				await Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => Insert(InsertMode.Async, parent.Items, uow, items, cancellationToken: cancellation.Token));
			}

			state.AssertNothingChanged();
		}

		/// <summary>
		/// E-06. Токен отменён во время вставки: транзакция откачена, тип исключения остаётся OperationCanceledException.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsertAsync_WhenTokenCancelledDuringInsert_RollsBackAndKeepsExceptionType()
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			using(var cancellation = new CancellationTokenSource())
			{
				var insertsSeen = 0;

				SqlLog.BeforeCommand = command =>
				{
					if(IsInsertInto(command, ChildrenTable) && ++insertsSeen == 1)
					{
						cancellation.Cancel();
					}
				};

				await Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => Insert(InsertMode.Async, parent.Items, uow, NewChildren(4), 2, cancellation.Token));
			}

			AssertRolledBack(uow);
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// E-07. Метод сам не коммитит: транзакция активна, строки не видны второму соединению.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSucceeded_LeavesTransactionOpen(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			Assert.True(IsTransactionActive(uow));
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// E-08. Пачка больше max_allowed_packet: метод либо сам делит её, либо сообщает, что уменьшить batchSize.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenBatchExceedsMaxAllowedPacket_SplitsOrExplains(InsertMode mode)
		{
			const int productionPacket = 134217728;
			const int reducedPacket = 1048576;

			var parentId = CreateParent();
			var items = NewChildren(300);

			foreach(var item in items)
			{
				item.Notes = new string('я', 8000);
			}

			SetMaxAllowedPacket(reducedPacket);

			try
			{
				var uow = NewUow();
				var parent = uow.GetById<BulkParent>(parentId);

				var exception = await Record.ExceptionAsync(() => Insert(mode, parent.Items, uow, items));

				if(exception == null)
				{
					uow.Commit();
					Assert.Equal(300, CountChildren(parentId));
				}
				else
				{
					var failure = Assert.IsType<BulkSqlInsertException>(exception);
					Assert.Contains("batchSize", failure.Message);
					Assert.Equal(0, CountChildren(parentId));
				}
			}
			finally
			{
				SetMaxAllowedPacket(productionPacket);
			}
		}

		/// <summary>
		/// E-09. Две параллельные вставки в одного версионного родителя: вторая ждёт блокировку и падает, не сделав ни одного INSERT.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTwoConcurrentInsertsToVersionedParent_SecondFailsWithoutInsert(InsertMode mode)
		{
			var parentId = CreateVersionedParent();
			var firstUow = NewUow();
			var secondUow = NewUow();
			var firstParent = firstUow.GetById<VersionedParent>(parentId);
			var secondParent = secondUow.GetById<VersionedParent>(parentId);
			_ = firstParent.Items.Count;
			_ = secondParent.Items.Count;

			await Insert(mode, firstParent.Items, firstUow, new[] { new VersionedParentChild { Name = "первый" } });

			var second = Task.Run(() => Insert(mode, secondParent.Items, secondUow, new[] { new VersionedParentChild { Name = "второй" } }));

			await Task.Delay(TimeSpan.FromSeconds(2));
			Assert.False(second.IsCompleted, "Вторая вставка должна ждать блокировку строки родителя");

			firstUow.Commit();

			var exception = await Assert.ThrowsAsync<BulkSqlInsertException>(() => second);

			Assert.IsType<StaleObjectStateException>(exception.InnerException);
			AssertRolledBack(secondUow);
			Assert.Equal(1, CountChildren(parentId, "bulk_versioned_parent_children"));
			Assert.Equal(1, Database.Count("bulk_versioned_parent_children", "name = 'первый'"));
			Assert.Equal(1, Database.Count("bulk_versioned_parents", $"id = {parentId} AND version = 2"));
		}

		private int CreateVersionedParent()
		{
			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка версионного родителя"))
			{
				var parent = new VersionedParent();
				uow.Save(parent);
				uow.Commit();
				SqlLog.Clear();

				return parent.Id;
			}
		}

		private static void AddRowInSameTransaction(DbCommand trigger, int parentId)
		{
			using(var command = trigger.Connection.CreateCommand())
			{
				command.Transaction = trigger.Transaction;
				command.CommandText = $"INSERT INTO {ChildrenTable} (child_type, parent_id, name, amount, occurred_at, is_active, kind) "
					+ $"VALUES ('base', {parentId}, 'лишняя', 0, '2026-01-01', 0, 'First')";
				command.ExecuteNonQuery();
			}
		}

		private void SetMaxAllowedPacket(int value)
		{
			using(var connection = new MySqlConnection(MariaDbServer.Instance.GetConnectionString()))
			using(var command = connection.CreateCommand())
			{
				connection.Open();
				command.CommandText = $"SET GLOBAL max_allowed_packet = {value}";
				command.ExecuteNonQuery();
			}

			// Значение применяется только к новым соединениям
			MySqlConnection.ClearAllPools();
		}
	}
}

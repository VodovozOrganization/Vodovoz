using NHibernate;
using MySqlConnector;
using QS.DomainModel.UoW;
using QS.Project.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Common;
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
	/// Группа H: если у дочерней сущности есть [HistoryTrace], массовая вставка попадает в историю тем же способом,
	/// что и обычный Save: через трекер QS, после коммита, отдельным соединением и в том же формате.
	/// </summary>
	[Collection(BulkSqlInsertHistoryCollection.Name)]
	public class BulkSqlInsertHistoryTests : IDisposable
	{
		private const string ChildrenTable = "history_test_children";
		private const string EntitiesTable = "history_changed_entities";
		private const string ChangesTable = "history_changes";
		private const string ChangeSetsTable = "history_changeset";
		private const string ActionTitle = "Тест истории";
		private const string FixedDateText = "CreatedAt";

		private readonly BulkSqlInsertHistoryFixture _fixture;
		private readonly List<IUnitOfWork> _unitsOfWork = new List<IUnitOfWork>();

		public BulkSqlInsertHistoryTests(BulkSqlInsertHistoryFixture fixture)
		{
			_fixture = fixture;
			Database.TruncateAll();
			Database.Execute($"INSERT INTO users (id, name, login, deactivated, admin) VALUES ({BulkSqlInsertHistoryFixture.TestUserId}, 'Тестовый', 'test', 0, 0)");
			SqlLog.Reset();
			AffectedRowsBatcherFactory.AffectedRows = null;
		}

		private TestDatabase Database => _fixture.Database;

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

		private IUnitOfWork NewUow()
		{
			var uow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle);
			_unitsOfWork.Add(uow);

			return uow;
		}

		private static async Task Insert<TItem>(
			InsertMode mode,
			ICollection<TItem> collection,
			IUnitOfWork uow,
			IEnumerable<TItem> items,
			int batchSize = 1000,
			CancellationToken cancellationToken = default)
			where TItem : class
		{
			if(mode == InsertMode.Sync)
			{
				collection.BulkSqlInsert(uow, items, batchSize);
			}
			else
			{
				await collection.BulkSqlInsertAsync(uow, items, batchSize, cancellationToken);
			}
		}

		private int CreateParent()
		{
			using(var uow = _fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка родителя"))
			{
				var parent = new HistoryParent();
				uow.Save(parent);
				uow.Commit();
				SqlLog.Clear();

				return parent.Id;
			}
		}

		private int CreateTag(string name)
		{
			using(var uow = _fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка тега"))
			{
				var tag = new BulkTag { Name = name };
				uow.Save(tag);
				uow.Commit();
				SqlLog.Clear();

				return tag.Id;
			}
		}

		private static HistoryChild NewChild(string name, BulkTag tag = null, int? nullableCount = 7) =>
			new HistoryChild
			{
				Name = name,
				Amount = 12.50m,
				OccurredAt = new DateTime(2026, 3, 4, 5, 6, 7),
				NullableCount = nullableCount,
				IsActive = true,
				Kind = BulkChildKind.Second,
				Tag = tag,
				IgnoredNote = "не писать в историю"
			};

		private static List<HistoryChild> NewChildren(int count, string prefix = "новый") =>
			Enumerable.Range(1, count).Select(i => NewChild($"{prefix}-{i}")).ToList();

		private long Count(string table, string where = null) => Database.Count(table, where);

		private List<string> Query(string sql)
		{
			var rows = new List<string>();

			using(var connection = Database.OpenConnection())
			using(var command = connection.CreateCommand())
			{
				command.CommandText = sql;

				using(var reader = command.ExecuteReader())
				{
					while(reader.Read())
					{
						rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount)
							.Select(i => reader.IsDBNull(i) ? "NULL" : Convert.ToString(reader.GetValue(i)))));
					}
				}
			}

			return rows;
		}

		/// <summary>
		/// История одной сущности без Id, времени и ссылки на родителя, чтобы сравнивать её с эталоном.
		/// Время создания заполняет БД, поэтому его значение заменено на признак наличия
		/// </summary>
		private List<string> ReadEntityHistory(string entityClass, int entityId)
		{
			var entity = Query($"SELECT id, operation, entity_class, entity_title FROM {EntitiesTable} WHERE entity_class = '{entityClass}' AND entity_id = {entityId}");
			var row = Assert.Single(entity);
			var entityRowId = row.Split('|')[0];
			var result = new List<string> { row.Substring(entityRowId.Length + 1) };

			result.AddRange(Query($"SELECT field_name, type, old_value, old_id, new_value, new_id FROM {ChangesTable} WHERE changed_entity_id = {entityRowId} ORDER BY field_name")
				.Select(x => x.StartsWith(FixedDateText + "|") ? FixedDateText : x));

			return result;
		}

		private List<int> ChildIds(int parentId) =>
			Query($"SELECT id FROM {ChildrenTable} WHERE parent_id = {parentId} ORDER BY id").Select(int.Parse).ToList();

		private void AssertNothingWritten(int parentId)
		{
			Assert.Equal(0, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(0, Count(EntitiesTable, $"entity_class = '{nameof(HistoryChild)}'"));
			Assert.Equal(0, Count(ChangesTable));
		}

		/// <summary>
		/// Команды к таблицам истории через сессию UoW. Трекер пишет отдельным соединением, поэтому их быть не должно
		/// </summary>
		private static int HistoryCommandsInSession() =>
			SqlLog.Inserts(ChangeSetsTable) + SqlLog.Inserts(EntitiesTable) + SqlLog.Inserts(ChangesTable);

		private static bool IsInsertInto(DbCommand command, string table) =>
			command.CommandText.TrimStart().StartsWith($"INSERT INTO {table}", StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// H-01. На один вызов одна запись набора изменений и по записи на каждую строку с операцией Create и настоящим Id из БД.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenEntityHasHistoryTrace_WritesChangedEntityForEachRow(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(5));
			uow.Commit();

			var ids = ChildIds(parentId);
			var historyIds = Query($"SELECT entity_id FROM {EntitiesTable} WHERE entity_class = '{nameof(HistoryChild)}' AND operation = 'Create' ORDER BY entity_id")
				.Select(int.Parse)
				.ToList();

			Assert.Equal(5, ids.Count);
			Assert.Equal(ids, historyIds);
			Assert.Equal(1, Count(ChangeSetsTable));
			Assert.Equal(5, Count(EntitiesTable, $"datetime IS NOT NULL AND changeset_id = (SELECT MAX(id) FROM {ChangeSetsTable})"));
		}

		/// <summary>
		/// H-02. Эталон: для одинаковой сущности история совпадает с той, что QS пишет при обычном Save:
		/// все типы полей, ссылка на родителя и заполненная ссылка на другую сущность.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenComparedToSave_WritesSameHistory(InsertMode mode)
		{
			var tagId = CreateTag("тег");
			var saveParentId = CreateParent();
			var bulkParentId = CreateParent();
			int savedId;

			using(var saveUow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
			{
				var saved = NewChild("одинаковый", saveUow.GetById<BulkTag>(tagId));
				saved.Parent = saveUow.GetById<HistoryParent>(saveParentId);
				saveUow.Save(saved);
				saveUow.Commit();
				savedId = saved.Id;
			}

			var expected = NormalizeParent(ReadEntityHistory(nameof(HistoryChild), savedId), saveParentId);

			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(bulkParentId);
			await Insert(mode, parent.Items, uow, new[] { NewChild("одинаковый", uow.GetById<BulkTag>(tagId)) });
			uow.Commit();

			var actual = NormalizeParent(ReadEntityHistory(nameof(HistoryChild), ChildIds(bulkParentId).Single()), bulkParentId);

			Assert.Equal(expected, actual);
		}

		/// <summary>
		/// H-03. Эталон для пустых значений: пустая ссылка, null и пустая строка записываются так же, как при Save.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenValuesEmpty_WritesSameHistoryAsSave(InsertMode mode)
		{
			var saveParentId = CreateParent();
			var bulkParentId = CreateParent();
			int savedId;

			using(var saveUow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
			{
				var saved = new HistoryChild { Name = string.Empty, OccurredAt = new DateTime(2026, 1, 1) };
				saved.Parent = saveUow.GetById<HistoryParent>(saveParentId);
				saveUow.Save(saved);
				saveUow.Commit();
				savedId = saved.Id;
			}

			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(bulkParentId);
			await Insert(mode, parent.Items, uow, new[] { new HistoryChild { Name = string.Empty, OccurredAt = new DateTime(2026, 1, 1) } });
			uow.Commit();

			Assert.Equal(
				NormalizeParent(ReadEntityHistory(nameof(HistoryChild), savedId), saveParentId),
				NormalizeParent(ReadEntityHistory(nameof(HistoryChild), ChildIds(bulkParentId).Single()), bulkParentId));
		}

		/// <summary>
		/// H-04. Набор изменений заполняется так же, как при Save: пользователь, логин БД, название действия.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenHistoryWritten_ChangeSetSameAsSave(InsertMode mode)
		{
			var saveParentId = CreateParent();
			var bulkParentId = CreateParent();

			using(var saveUow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
			{
				var saved = NewChild("первый");
				saved.Parent = saveUow.GetById<HistoryParent>(saveParentId);
				saveUow.Save(saved);
				saveUow.Commit();
			}

			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(bulkParentId);
			await Insert(mode, parent.Items, uow, new[] { NewChild("второй") });
			uow.Commit();

			var changeSets = Query($"SELECT user_login, action_name, user_id FROM {ChangeSetsTable} ORDER BY id");

			Assert.Equal(2, changeSets.Count);
			Assert.Equal(changeSets[0], changeSets[1]);
			Assert.Contains(ActionTitle, changeSets[1]);
		}

		/// <summary>
		/// H-05. Сущность без [HistoryTrace]: ни одной записи истории и ни одной команды к таблицам истории.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenEntityWithoutHistoryTrace_WritesNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			SqlLog.Clear();

			await Insert(mode, parent.NoHistoryItems, uow, new[] { new NoHistoryChild { Name = "без истории" } });
			uow.Commit();

			Assert.Equal(1, Count("history_test_no_history_children", $"parent_id = {parentId}"));
			Assert.Equal(0, Count(EntitiesTable));
			Assert.Equal(0, SqlLog.Inserts("history_changeset") + SqlLog.Inserts(EntitiesTable) + SqlLog.Inserts(ChangesTable));
		}

		/// <summary>
		/// H-06. Несколько пачек: ровно по одной записи истории на строку, без дублей и без пропусков.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSeveralBatches_WritesExactlyOneRecordPerRow(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(23), 5);
			uow.Commit();

			Assert.Equal(23, Count(EntitiesTable, $"entity_class = '{nameof(HistoryChild)}'"));
			Assert.Equal(ChildIds(parentId), Query($"SELECT DISTINCT entity_id FROM {EntitiesTable} ORDER BY entity_id").Select(int.Parse));
			Assert.Equal(1, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-07. До коммита истории нигде нет, а после коммита она есть вместе со строками.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_BeforeCommit_HistoryNotWritten(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));

			uow.Commit();

			Assert.Equal(3, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(3, Count(EntitiesTable));
			Assert.Equal(1, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-08. История идёт через трекер, а не через сессию UoW: ни при вызове, ни при коммите в сессии нет команд к таблицам истории.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenHistoryWritten_GoesThroughTrackerNotSession(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(3));
			uow.Commit();

			Assert.Equal(0, HistoryCommandsInSession());
			Assert.Equal(3, Count(EntitiesTable));
		}

		/// <summary>
		/// H-09. Откат после успешного вызова: нет ни строк, ни истории.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenRolledBackAfterCall_NoRowsAndNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));
			uow.Session.GetCurrentTransaction().Rollback();
			uow.Commit();

			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-10. Количество не совпало: откат, нет ни строк, ни истории.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCountMismatch_NoRowsAndNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			AffectedRowsBatcherFactory.AffectedRows = (command, rows) => IsInsertInto(command, ChildrenTable) ? rows - 1 : rows;

			await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, NewChildren(3)));

			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-11. Ошибка БД во второй пачке строк: откат, нет ни строк, ни истории.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenDatabaseError_NoRowsAndNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			var items = NewChildren(3);
			items[1].UniqueCode = "дубль";
			items[2].UniqueCode = "дубль";

			await Assert.ThrowsAsync<BulkSqlInsertException>(() => Insert(mode, parent.Items, uow, items, 2));

			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-12. Сбой записи истории после коммита ведёт себя как у обычного Save: Commit бросает исключение,
		/// строки уже закоммичены, истории нет. Таблица истории на время теста переименована.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenHistoryWriteFails_BehavesLikeSave(InsertMode mode)
		{
			var saveParentId = CreateParent();
			var bulkParentId = CreateParent();
			Database.Execute($"RENAME TABLE {ChangesTable} TO {ChangesTable}_off");

			try
			{
				using(var saveUow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
				{
					var saved = NewChild("обычный");
					saved.Parent = saveUow.GetById<HistoryParent>(saveParentId);
					saveUow.Save(saved);

					Assert.ThrowsAny<Exception>(() => saveUow.Commit());
				}

				var uow = NewUow();
				var parent = uow.GetById<HistoryParent>(bulkParentId);

				await Insert(mode, parent.Items, uow, NewChildren(3));

				Assert.ThrowsAny<Exception>(() => uow.Commit());
			}
			finally
			{
				Database.Execute($"RENAME TABLE {ChangesTable}_off TO {ChangesTable}");
			}

			Assert.Equal(1, Count(ChildrenTable, $"parent_id = {saveParentId}"));
			Assert.Equal(3, Count(ChildrenTable, $"parent_id = {bulkParentId}"));
			Assert.Equal(0, Count(EntitiesTable));
		}

		/// <summary>
		/// H-13. Отмена токена во время вставки строк: откат, истории нет ни сразу, ни после коммита.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsertAsync_WhenCancelledDuringInsert_NoHistory()
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			using(var cancellation = new CancellationTokenSource())
			{
				SqlLog.BeforeCommand = command =>
				{
					if(IsInsertInto(command, ChildrenTable))
					{
						cancellation.Cancel();
					}
				};

				await Assert.ThrowsAnyAsync<OperationCanceledException>(
					() => Insert(InsertMode.Async, parent.Items, uow, NewChildren(30), 5, cancellation.Token));
			}

			SqlLog.BeforeCommand = null;
			uow.Commit();

			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-14. История не ломает пакетность вставки: 2000 строк идут теми же пачками, ни одного отдельного INSERT на строку,
		/// а записей истории ровно 2000.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenManyRows_KeepsInsertBatches(InsertMode mode)
		{
			const int count = 2000;

			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			_ = parent.Items.Count;
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(count), 500);

			Assert.Equal(4, SqlLog.Inserts(ChildrenTable));

			uow.Commit();

			Assert.Equal(count, Count(EntitiesTable));
		}

		/// <summary>
		/// H-15. Sync и async пишут одинаковую историю.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsert_WhenSyncAndAsyncCompared_SameHistory()
		{
			var syncParentId = CreateParent();
			var asyncParentId = CreateParent();

			var syncUow = NewUow();
			await Insert(InsertMode.Sync, syncUow.GetById<HistoryParent>(syncParentId).Items, syncUow, new[] { NewChild("одинаковый") });
			syncUow.Commit();

			var asyncUow = NewUow();
			await Insert(InsertMode.Async, asyncUow.GetById<HistoryParent>(asyncParentId).Items, asyncUow, new[] { NewChild("одинаковый") });
			asyncUow.Commit();

			Assert.Equal(
				NormalizeParent(ReadEntityHistory(nameof(HistoryChild), ChildIds(syncParentId).Single()), syncParentId),
				NormalizeParent(ReadEntityHistory(nameof(HistoryChild), ChildIds(asyncParentId).Single()), asyncParentId));
		}

		/// <summary>
		/// H-16. Свойство с [IgnoreHistoryTrace] и коллекция в историю не попадают.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenIgnoredPropertyAndCollection_NotWritten(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, new[] { NewChild("с заметкой") });
			uow.Commit();

			Assert.True(Count(ChangesTable) > 0);
			Assert.Equal(0, Count(ChangesTable, "field_name = 'IgnoredNote'"));
			Assert.Equal(0, Count(ChangesTable, "field_name = 'SubItems'"));
		}

		/// <summary>
		/// H-17. Заголовок записи истории считается с настоящим Id из БД.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTitleUsesId_WritesRealId(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.IdTitleItems, uow, new[] { new HistoryIdTitleChild { Name = "первый" }, new HistoryIdTitleChild { Name = "второй" } });
			uow.Commit();

			var rows = Query($"SELECT entity_id, entity_title FROM {EntitiesTable} WHERE entity_class = '{nameof(HistoryIdTitleChild)}'");

			Assert.Equal(2, rows.Count);
			Assert.All(rows, row =>
			{
				var parts = row.Split('|');
				Assert.NotEqual("0", parts[0]);
				Assert.Equal($"Запись {parts[0]}", parts[1]);
			});
		}

		/// <summary>
		/// H-18. Длинный заголовок обрезается до размера колонки так же, как в трекере QS.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTitleTooLong_TruncatesLikeTracker(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, new[] { NewChild(new string('д', 300)) });
			uow.Commit();

			var title = Query($"SELECT entity_title FROM {EntitiesTable}").Single();

			Assert.Equal(200, title.Length);
			Assert.EndsWith("...", title);
		}

		/// <summary>
		/// H-19. Подклассы вперемешку: в истории имя конкретного класса, атрибут наследуется, поля подкласса записаны.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenMixedSubclasses_WritesConcreteClassName(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			var items = NewChildren(1);
			items.Add(new HistoryChildSpecial { Name = "особый", Extra = "доп", OccurredAt = new DateTime(2026, 1, 1) });

			await Insert(mode, parent.Items, uow, items);
			uow.Commit();

			Assert.Equal(1, Count(EntitiesTable, $"entity_class = '{nameof(HistoryChild)}'"));
			Assert.Equal(1, Count(EntitiesTable, $"entity_class = '{nameof(HistoryChildSpecial)}'"));
			Assert.Equal(1, Count(ChangesTable, "field_name = 'Extra' AND new_value = 'доп'"));
		}

		/// <summary>
		/// H-20. UoW без трекера: как и при обычном Save, история не пишется и ошибки нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenUowNotTracked_WritesNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();

			using(var uow = _fixture.UntrackedUnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
			{
				var parent = uow.GetById<HistoryParent>(parentId);

				await Insert(mode, parent.Items, uow, NewChildren(2));
				uow.Commit();
			}

			Assert.Equal(2, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(0, Count(EntitiesTable));
		}

		/// <summary>
		/// H-21. Обычный Save в том же UoW: один набор изменений на коммит, в нём и обычная запись, и массовая вставка, дублей нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WithOrdinarySaveInSameUow_OneChangeSetWithoutDuplicates(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			var ordinary = NewChild("обычный");
			ordinary.Parent = parent;
			uow.Save(ordinary);

			await Insert(mode, parent.Items, uow, NewChildren(3));
			uow.Commit();

			Assert.Equal(4, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(1, Count(ChangeSetsTable));
			Assert.Equal(4, Count(EntitiesTable, $"entity_class = '{nameof(HistoryChild)}'"));
			Assert.Equal(ChildIds(parentId), Query($"SELECT entity_id FROM {EntitiesTable} ORDER BY entity_id").Select(int.Parse));
		}

		/// <summary>
		/// H-22. Два вызова до одного коммита: один набор изменений, как у обычного Save.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCalledTwiceBeforeCommit_WritesOneChangeSet(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(2, "первый"));
			await Insert(mode, parent.Items, uow, NewChildren(3, "второй"));
			uow.Commit();

			Assert.Equal(1, Count(ChangeSetsTable));
			Assert.Equal(5, Count(EntitiesTable));
		}

		/// <summary>
		/// H-22. Два коммита в одном UoW: по набору изменений на коммит, записи не повторяются.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCommittedTwice_WritesChangeSetPerCommit(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(2, "первый"));
			uow.Commit();
			await Insert(mode, parent.Items, uow, NewChildren(3, "второй"));
			uow.Commit();

			Assert.Equal(2, Count(ChangeSetsTable));
			Assert.Equal(5, Count(EntitiesTable));
		}

		/// <summary>
		/// H-23. Отклонённый проверками вызов: истории нет, транзакция жива.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenValidationFails_WritesNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			var otherParentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<HistoryParent>(parentId);
			var items = NewChildren(2);
			items[1].Parent = uow.GetById<HistoryParent>(otherParentId);
			SqlLog.Clear();

			await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			Assert.Equal(0, SqlLog.Inserts(EntitiesTable));
			Assert.True(uow.Session.GetCurrentTransaction()?.IsActive == true);
		}

		/// <summary>
		/// H-24. UoW с трекером, но история в приложении не включена (AddTrackedUoW без AddStaticHistoryTracker):
		/// как и при обычном Save, история не пишется и ошибки нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenHistoryTrackerDisabled_WritesNoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			_fixture.DisableHistoryTracker();

			try
			{
				using(var uow = _fixture.UnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
				{
					var parent = uow.GetById<HistoryParent>(parentId);
					var ordinary = NewChild("обычный");
					ordinary.Parent = parent;
					uow.Save(ordinary);

					await Insert(mode, parent.Items, uow, NewChildren(2));
					uow.Commit();
				}
			}
			finally
			{
				_fixture.EnableHistoryTracker();
			}

			Assert.Equal(3, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(0, Count(EntitiesTable));
		}

		/// <summary>
		/// H-25. История включена, а текущий пользователь не задан: исключение до записи.
		/// Иначе строки закоммитились бы, а трекер упал бы уже после коммита.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenHistoryEnabledAndNoCurrentUser_ThrowsBeforeWrite(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<HistoryParent>(parentId);
			var currentUser = UserRepository.GetCurrentUserId;
			UserRepository.GetCurrentUserId = null;
			SqlLog.Clear();

			try
			{
				await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));
			}
			finally
			{
				UserRepository.GetCurrentUserId = currentUser;
			}

			Assert.Equal(0, SqlLog.Writes());
			Assert.True(uow.Session.GetCurrentTransaction()?.IsActive == true);
		}

		/// <summary>
		/// H-25. Текущий пользователь не задан, но истории нет: сущность без атрибута или UoW без трекера вставляются как обычно.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenNoCurrentUserAndNoHistoryNeeded_Inserts(InsertMode mode)
		{
			var parentId = CreateParent();
			var currentUser = UserRepository.GetCurrentUserId;
			UserRepository.GetCurrentUserId = null;

			try
			{
				var uow = NewUow();
				var parent = uow.GetById<HistoryParent>(parentId);
				await Insert(mode, parent.NoHistoryItems, uow, new[] { new NoHistoryChild { Name = "без истории" } });
				uow.Commit();

				using(var untracked = _fixture.UntrackedUnitOfWorkFactory.CreateWithoutRoot(ActionTitle))
				{
					var untrackedParent = untracked.GetById<HistoryParent>(parentId);
					await Insert(mode, untrackedParent.Items, untracked, NewChildren(1));
					untracked.Commit();
				}
			}
			finally
			{
				UserRepository.GetCurrentUserId = currentUser;
			}

			Assert.Equal(1, Count("history_test_no_history_children", $"parent_id = {parentId}"));
			Assert.Equal(1, Count(ChildrenTable, $"parent_id = {parentId}"));
			Assert.Equal(0, Count(EntitiesTable));
		}

		/// <summary>
		/// H-26. Больше 10 000 записей: трекер пишет несколькими пакетами, записей ровно столько, сколько строк.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsert_WhenMoreThanTrackerBatch_WritesAllRecords()
		{
			const int count = 10001;

			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);

			await Insert(InsertMode.Async, parent.Items, uow, NewChildren(count));
			uow.Commit();

			Assert.Equal(count, Count(EntitiesTable));
			Assert.Equal(1, Count(ChangeSetsTable));
		}

		/// <summary>
		/// H-27. Сам коммит упал из-за другой сущности: истории нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCommitFails_NoHistory(InsertMode mode)
		{
			var parentId = CreateParent();
			CreateTag("занято");
			var freeTagId = CreateTag("свободно");
			var uow = NewUow();
			var parent = uow.GetById<HistoryParent>(parentId);
			var tag = uow.GetById<BulkTag>(freeTagId);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			tag.Name = "занято";
			uow.Save(tag);

			Assert.ThrowsAny<Exception>(() => uow.Commit());
			AssertNothingWritten(parentId);
			Assert.Equal(0, Count(ChangeSetsTable));
		}

		/// <summary>
		/// Заменяет Id родителя в истории: у эталона и у проверяемой строки родители разные.
		/// </summary>
		private static List<string> NormalizeParent(List<string> history, int parentId) =>
			history.Select(x => x.StartsWith("Parent|") ? x.Replace($"|{parentId}", "|PARENT") : x).ToList();
	}
}

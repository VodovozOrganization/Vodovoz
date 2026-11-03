using MySqlConnector;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа F: прямая цель метода работает. Вставка, загрузка в сессию и в коллекцию родителя со всеми полями из БД.
	/// </summary>
	public class BulkSqlInsertTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		private void AssertLoadedFromDb(IUnitOfWork uow, BulkChild item, int parentId)
		{
			Assert.True(uow.Session.Contains(item));
			Assert.True(item.Id > 0);

			using(var connection = Database.OpenConnection())
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
				command.ExecuteNonQuery();
				command.CommandText = $"SELECT name, parent_id, amount, is_active, kind, address_city, address_street FROM {ChildrenTable} WHERE id = {item.Id}";

				using(var reader = command.ExecuteReader())
				{
					Assert.True(reader.Read());
					Assert.Equal(reader.GetString("name"), item.Name);
					Assert.Equal(parentId, reader.GetInt32("parent_id"));
					Assert.Equal(reader.GetDecimal("amount"), item.Amount);
					Assert.Equal(reader.GetBoolean("is_active"), item.IsActive);
					Assert.Equal(reader.GetString("kind"), item.Kind.ToString());
					Assert.Equal(reader.GetString("address_city"), item.Address.City);
					Assert.Equal(reader.GetString("address_street"), item.Address.Street);
				}
			}
		}

		/// <summary>
		/// F-01. Новый родитель после Save, inverse-коллекция, 2500 элементов по 1000: три INSERT и одна загрузка.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenNewSavedParent_InsertsBatchesAndLoadsAll(InsertMode mode)
		{
			var uow = NewUow();
			var parent = new BulkParent { Code = 100 };
			uow.Save(parent);
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(2500), 1000);

			Assert.Equal(3, SqlLog.Inserts(ChildrenTable));
			Assert.Equal(1, SqlLog.Selects(ChildrenTable));
			Assert.Equal(2500, parent.Items.Count);

			foreach(var item in parent.Items.Take(20))
			{
				AssertLoadedFromDb(uow, item, parent.Id);
			}

			Assert.All(parent.Items, item => Assert.True(uow.Session.Contains(item) && item.Id > 0));
		}

		/// <summary>
		/// F-02. Значения в БД совпадают с обычным Save: все типы колонок, попарно для 30 строк.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenComparedToSave_WritesSameValues(InsertMode mode)
		{
			var bulkParentId = CreateParent();
			var saveParentId = CreateParent();
			var tagId = CreateTag("тег");

			var uow = NewUow();
			var bulkParent = uow.GetById<BulkParent>(bulkParentId);
			var saveParent = uow.GetById<BulkParent>(saveParentId);
			var tag = uow.GetById<BulkTag>(tagId);

			var bulkItems = NewChildren(30);
			var saveItems = NewChildren(30);

			for(var i = 0; i < 30; i++)
			{
				bulkItems[i].Tag = saveItems[i].Tag = i % 3 == 0 ? tag : null;
				bulkItems[i].NullableCount = saveItems[i].NullableCount = i % 2 == 0 ? i : (int?)null;
				saveItems[i].Parent = saveParent;
				uow.Save(saveItems[i]);
			}

			await Insert(mode, bulkParent.Items, uow, bulkItems, 7);
			uow.Commit();

			Assert.Equal(ReadRows(saveParentId), ReadRows(bulkParentId));
		}

		/// <summary>
		/// F-03. Родитель загружен, коллекция не загружена: после вызова она загружена и не требует новых запросов.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCollectionLazy_InitializesCollection(InsertMode mode)
		{
			var parentId = CreateParent(2);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(3);

			await Insert(mode, parent.Items, uow, items);

			Assert.True(NHibernateUtil.IsInitialized(parent.Items));

			SqlLog.Clear();
			Assert.Equal(5, parent.Items.Count);
			Assert.Equal(0, SqlLog.Selects(ChildrenTable));
		}

		/// <summary>
		/// F-04. Коллекция уже загружена: старые элементы остаются теми же экземплярами.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCollectionInitialized_KeepsExistingInstances(InsertMode mode)
		{
			var parentId = CreateParent(3);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var existing = parent.Items.ToList();

			await Insert(mode, parent.Items, uow, NewChildren(4));

			Assert.Equal(7, parent.Items.Count);

			foreach(var item in existing)
			{
				Assert.Contains(parent.Items, x => ReferenceEquals(x, item));
			}
		}

		/// <summary>
		/// F-05. Значения, которые заполняет БД: DEFAULT и Formula, сразу после вызова.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenDatabaseFillsValues_ReturnsThem(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(2, "значение"));

			Assert.All(parent.Items, item =>
			{
				Assert.NotEqual(default, item.CreatedAt);
				Assert.Equal(item.Name.Length, item.NameLength);
			});
		}

		/// <summary>
		/// F-06. Новых элементов в коллекции = строк в БД = newItems.Count, множества Id совпадают.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSucceeded_CountsAndIdsMatch(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(11);

			await Insert(mode, parent.Items, uow, items);
			uow.Commit();

			var databaseIds = new List<int>();

			using(var connection = Database.OpenConnection())
			using(var command = connection.CreateCommand())
			{
				command.CommandText = $"SELECT id FROM {ChildrenTable} WHERE parent_id = {parentId}";

				using(var reader = command.ExecuteReader())
				{
					while(reader.Read())
					{
						databaseIds.Add(reader.GetInt32(0));
					}
				}
			}

			Assert.Equal(items.Count, parent.Items.Count);
			Assert.Equal(items.Count, databaseIds.Count);
			Assert.Equal(databaseIds.OrderBy(x => x), parent.Items.Select(x => x.Id).OrderBy(x => x));
		}

		/// <summary>
		/// F-07. Пустая ссылка на родителя: в FK записан Id родителя из вызова, ссылка указывает на него.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemParentNull_BindsToCollectionParent(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));
			uow.Commit();

			Assert.Equal(3, CountChildren(parentId));
			Assert.All(parent.Items, item => Assert.Same(parent, item.Parent));
		}

		/// <summary>
		/// F-07. Ссылка на того же родителя: норма, в том числе прокси этого родителя.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemParentSame_Inserts(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			items[0].Parent = parent;
			items[1].Parent = uow.Session.Load<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, items);
			uow.Commit();

			Assert.Equal(2, CountChildren(parentId));
			Assert.All(parent.Items, item => Assert.Same(parent, item.Parent));
		}

		/// <summary>
		/// F-08. Переданные объекты не попали ни в коллекцию, ни в сессию.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSucceeded_PassedObjectsStayOutOfSession(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(3);

			await Insert(mode, parent.Items, uow, items);

			Assert.All(items, item =>
			{
				Assert.False(uow.Session.Contains(item));
				Assert.DoesNotContain(parent.Items, x => ReferenceEquals(x, item));
				Assert.Equal(0, item.Id);
				Assert.Null(item.Parent);
			});
		}

		/// <summary>
		/// F-09. Коммит после вызова для inverse-коллекции не даёт лишних команд.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCommittedInverse_NoExtraCommands(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(5));
			SqlLog.Clear();
			uow.Commit();

			Assert.Equal(0, SqlLog.Writes());
			Assert.Equal(5, ReloadCount(parentId, x => x.Items.Count));
		}

		/// <summary>
		/// F-09. Коммит после вызова для не-inverse коллекции не даёт лишних команд и не трогает FK.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCommittedNonInverse_NoExtraCommands(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = Enumerable.Range(1, 5).Select(i => new NonInverseChild { Name = $"новый-{i}" }).ToList();

			await Insert(mode, parent.NonInverseItems, uow, items);
			SqlLog.Clear();
			uow.Commit();

			Assert.Equal(0, SqlLog.Writes());
			Assert.Equal(5, ReloadCount(parentId, x => x.NonInverseItems.Count));
		}

		/// <summary>
		/// F-10. Delete-orphan: удалённый из коллекции новый элемент удаляется из БД при коммите.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemRemovedAfterInsert_DeleteOrphanWorks(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));
			var removed = parent.Items.First();
			parent.Items.Remove(removed);
			uow.Commit();

			Assert.Equal(2, CountChildren(parentId));
			Assert.Equal(0, Database.Count(ChildrenTable, $"id = {removed.Id}"));
		}

		/// <summary>
		/// F-11. UoW закрыт без коммита: строк нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenUowDisposedWithoutCommit_RowsAbsent(InsertMode mode)
		{
			var parentId = CreateParent();

			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Без коммита"))
			{
				var parent = uow.GetById<BulkParent>(parentId);
				await Insert(mode, parent.Items, uow, NewChildren(3));
			}

			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// F-12. Подклассы в одной таблице вперемешку: дискриминатор и типы правильные в той же и в новой сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenMixedSubclasses_WritesDiscriminatorAndLoadsTypes(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			items.Add(new BulkChildSpecial { Name = "особый", Extra = "доп", OccurredAt = new DateTime(2026, 1, 1) });

			await Insert(mode, parent.Items, uow, items);
			uow.Commit();

			Assert.Equal(2, Database.Count(ChildrenTable, $"parent_id = {parentId} AND child_type = 'base'"));
			Assert.Equal(1, Database.Count(ChildrenTable, $"parent_id = {parentId} AND child_type = 'special'"));
			Assert.Single(parent.Items.OfType<BulkChildSpecial>());

			using(var otherUow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Новая сессия"))
			{
				var special = Assert.Single(otherUow.GetById<BulkParent>(parentId).Items.OfType<BulkChildSpecial>());
				Assert.Equal("доп", special.Extra);
			}
		}

		/// <summary>
		/// F-13. Set-коллекция работает как bag.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSetCollection_Inserts(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = Enumerable.Range(1, 3).Select(i => new SetChild { Name = $"новый-{i}" }).ToList();

			await Insert(mode, parent.SetItems, uow, items);
			uow.Commit();

			Assert.Equal(3, parent.SetItems.Count);
			Assert.Equal(3, CountChildren(parentId, "bulk_set_children"));
		}

		/// <summary>
		/// F-13. Коллекция с OrderBy: порядок как в маппинге и в той же, и в новой сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenOrderedCollection_KeepsMappedOrder(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = new[] { "б", "в", "а" }.Select(x => new OrderedChild { Name = x }).ToList();

			await Insert(mode, parent.OrderedItems, uow, items);
			uow.Commit();

			Assert.Equal(new[] { "в", "б", "а" }, parent.OrderedItems.Select(x => x.Name));

			using(var otherUow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Новая сессия"))
			{
				Assert.Equal(new[] { "в", "б", "а" }, otherUow.GetById<BulkParent>(parentId).OrderedItems.Select(x => x.Name));
			}
		}

		/// <summary>
		/// F-14. Границы пачки: число INSERT = ceil(N / batchSize).
		/// </summary>
		[DatabaseTheory]
		[InlineData(InsertMode.Sync, 1, 10, 1)]
		[InlineData(InsertMode.Sync, 10, 10, 1)]
		[InlineData(InsertMode.Sync, 11, 10, 2)]
		[InlineData(InsertMode.Sync, 3, 1, 3)]
		[InlineData(InsertMode.Async, 1, 10, 1)]
		[InlineData(InsertMode.Async, 10, 10, 1)]
		[InlineData(InsertMode.Async, 11, 10, 2)]
		[InlineData(InsertMode.Async, 3, 1, 3)]
		public async Task BulkSqlInsert_WhenBatchBoundary_ExecutesExpectedInserts(InsertMode mode, int count, int batchSize, int expectedInserts)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(count), batchSize);

			Assert.Equal(expectedInserts, SqlLog.Inserts(ChildrenTable));
			Assert.Equal(count, parent.Items.Count);
		}

		/// <summary>
		/// F-15. Параметров в пачке больше 65535: пачка автоматически уменьшена, ошибки нет.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParametersExceedLimit_ReducesBatch(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = Enumerable.Range(1, 7000).Select(i => new WideChild { C1 = i }).ToList();
			_ = parent.WideItems.Count;
			SqlLog.Clear();

			await Insert(mode, parent.WideItems, uow, items, 7000);

			Assert.True(SqlLog.Inserts("bulk_wide_children") > 1);
			Assert.Equal(7000, parent.WideItems.Count);
		}

		/// <summary>
		/// F-16. Два вызова с разными объектами в одной транзакции.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCalledTwice_AddsBothSets(InsertMode mode)
		{
			var parentId = CreateParent(1);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(2, "первый"));
			await Insert(mode, parent.Items, uow, NewChildren(3, "второй"));
			uow.Commit();

			Assert.Equal(6, parent.Items.Count);
			Assert.Equal(6, CountChildren(parentId));
		}

		/// <summary>
		/// F-17. Родитель — неинициализированный прокси: работает как F-03.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentIsProxy_Inserts(InsertMode mode)
		{
			var parentId = CreateParent(1);
			var uow = NewUow();
			var parent = uow.Session.Load<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(2));

			Assert.True(NHibernateUtil.IsInitialized(parent.Items));
			Assert.Equal(3, parent.Items.Count);

			uow.Commit();

			Assert.Equal(3, CountChildren(parentId));
		}

		/// <summary>
		/// F-18. Sync и async дают одинаковые данные и одинаковый набор команд.
		/// </summary>
		[DatabaseFact]
		public async Task BulkSqlInsert_WhenSyncAndAsyncCompared_SameRowsAndCommands()
		{
			var (syncParentId, syncCommands) = await RunAndCapture(InsertMode.Sync);
			var (asyncParentId, asyncCommands) = await RunAndCapture(InsertMode.Async);

			Assert.Equal(ReadRows(syncParentId), ReadRows(asyncParentId));
			Assert.Equal(syncCommands, asyncCommands);
		}

		/// <summary>
		/// F-19. Smoke на продовом маппинге TransferOrder.Items: вставлено и загружено N, коммит без лишних команд.
		/// Таблицы созданы только нужные, ссылки на остальные сущности в этих строках пустые.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenProductionTransferOrderMapping_InsertsAndLoads(InsertMode mode)
		{
			using(var production = new ProductionMappingDatabase())
			{
				var orderId = production.CreateTransferOrder();

				using(var uow = production.UnitOfWorkFactory.CreateWithoutRoot("Продовый маппинг"))
				{
					var order = uow.GetById<TransferOrder>(orderId);
					var items = Enumerable.Range(1, 5).Select(i => new TransferOrderTrueMarkCode { Quantity = i }).ToList();

					await Insert(mode, order.Items, uow, items, 2);

					Assert.Equal(5, order.Items.Count);
					Assert.All(order.Items, item => Assert.Same(order, item.TransferOrder));

					SqlLog.Clear();
					uow.Commit();

					Assert.Equal(0, SqlLog.Writes());
				}

				Assert.Equal(5, production.Database.Count("edo_transfer_order_codes", $"transfer_order_id = {orderId}"));
			}
		}

		/// <summary>
		/// F-19. Продовый маппинг со ссылками на номенклатуру и коды, как в TransferSendPreparer: идентификаторы записаны в строки,
		/// а после загрузки ссылка на заказ указывает на родителя.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenProductionItemsHaveReferences_WritesReferenceIds(InsertMode mode)
		{
			using(var production = new ProductionMappingDatabase())
			{
				var orderId = production.CreateTransferOrder();

				using(var uow = production.UnitOfWorkFactory.CreateWithoutRoot("Продовый маппинг"))
				{
					var order = uow.GetById<TransferOrder>(orderId);
					var items = new[]
					{
						new TransferOrderTrueMarkCode
						{
							TransferOrder = order,
							Quantity = 1,
							Nomenclature = uow.Session.Load<NomenclatureEntity>(10),
							IndividualCode = uow.Session.Load<TrueMarkWaterIdentificationCode>(20)
						},
						new TransferOrderTrueMarkCode
						{
							TransferOrder = order,
							Quantity = 40,
							Nomenclature = uow.Session.Load<NomenclatureEntity>(10),
							GroupCode = uow.Session.Load<TrueMarkWaterGroupCode>(30)
						}
					};

					await Insert(mode, order.Items, uow, items);
					uow.Commit();

					Assert.Equal(2, order.Items.Count);
					Assert.All(order.Items, item => Assert.Same(order, item.TransferOrder));
				}

				Assert.Equal(1, production.Database.Count("edo_transfer_order_codes",
					$"transfer_order_id = {orderId} AND quantity = 1 AND nomenclature_id = 10 AND individual_code_id = 20 AND group_code_id IS NULL"));
				Assert.Equal(1, production.Database.Count("edo_transfer_order_codes",
					$"transfer_order_id = {orderId} AND quantity = 40 AND nomenclature_id = 10 AND group_code_id = 30 AND individual_code_id IS NULL"));
			}
		}

		/// <summary>
		/// F-20. Те же объекты во втором вызове: исключения нет, вставлены два набора. Дубли контролирует вызывающий код.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenSameObjectsPassedAgain_InsertsTwice(InsertMode mode)
		{
			var parentId = CreateParent(1);
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);

			await Insert(mode, parent.Items, uow, items);
			await Insert(mode, parent.Items, uow, items);
			uow.Commit();

			Assert.Equal(5, parent.Items.Count);
			Assert.Equal(5, CountChildren(parentId));
		}

		/// <summary>
		/// F-21. Версионный родитель: версия +1 в БД и в объекте, один UPDATE с условием на старую версию, коммит без лишних UPDATE.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentVersioned_IncrementsVersionOnce(InsertMode mode)
		{
			int parentId;

			using(var setupUow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка версионного родителя"))
			{
				var created = new VersionedParent();
				setupUow.Save(created);
				setupUow.Commit();
				parentId = created.Id;
			}

			var uow = NewUow();
			var parent = uow.GetById<VersionedParent>(parentId);
			var versionBefore = parent.Version;
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, new[] { new VersionedParentChild { Name = "новый" } });

			Assert.Equal(versionBefore + 1, parent.Version);
			Assert.Equal(1, SqlLog.Updates("bulk_versioned_parents"));

			var firstInsert = SqlLog.Commands.ToList().FindIndex(x => x.Text.StartsWith("INSERT INTO bulk_versioned_parent_children", StringComparison.OrdinalIgnoreCase));
			var update = SqlLog.Commands.ToList().FindIndex(x => x.Text.StartsWith("UPDATE bulk_versioned_parents", StringComparison.OrdinalIgnoreCase));
			Assert.True(update < firstInsert, "Блокировка родителя должна быть до первого INSERT");

			SqlLog.Clear();
			uow.Commit();

			Assert.Equal(0, SqlLog.Updates("bulk_versioned_parents"));
			Assert.Equal(1, Database.Count("bulk_versioned_parents", $"id = {parentId} AND version = {versionBefore + 1}"));
		}

		/// <summary>
		/// F-22. Родитель без версии: ни одного UPDATE родителя, один SELECT ... FOR UPDATE до первого INSERT.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentNotVersioned_LocksWithSelectForUpdate(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(2));

			var commands = SqlLog.Commands.ToList();
			var locks = commands.FindAll(x => x.Text.IndexOf("bulk_parents", StringComparison.OrdinalIgnoreCase) >= 0
				&& x.Text.IndexOf("FOR UPDATE", StringComparison.OrdinalIgnoreCase) >= 0);
			var firstInsert = commands.FindIndex(x => x.Text.StartsWith($"INSERT INTO {ChildrenTable}", StringComparison.OrdinalIgnoreCase));

			Assert.Equal(0, SqlLog.Updates("bulk_parents"));
			Assert.Single(locks);
			Assert.True(commands.IndexOf(locks[0]) < firstInsert);
		}

		private async Task<(int ParentId, List<string> Commands)> RunAndCapture(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			SqlLog.Clear();

			await Insert(mode, parent.Items, uow, NewChildren(5), 2);
			var commands = SqlLog.Commands.Select(x => x.Text).ToList();
			uow.Commit();

			return (parentId, commands);
		}

		private int CreateTag(string name)
		{
			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Подготовка тега"))
			{
				var tag = new BulkTag { Name = name };
				uow.Save(tag);
				uow.Commit();
				SqlLog.Clear();

				return tag.Id;
			}
		}

		private int ReloadCount(int parentId, Func<BulkParent, int> selectCount)
		{
			using(var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Новая сессия"))
			{
				return selectCount(uow.GetById<BulkParent>(parentId));
			}
		}

		/// <summary>
		/// Все колонки строк родителя, кроме Id, родителя и времени создания, в порядке имени.
		/// </summary>
		private List<string> ReadRows(int parentId)
		{
			var rows = new List<string>();

			using(var connection = Database.OpenConnection())
			using(var command = connection.CreateCommand())
			{
				command.CommandText = "SELECT child_type, name, unique_code, notes, amount, occurred_at, nullable_count, is_active, kind, "
					+ $"address_city, address_street, tag_id, extra FROM {ChildrenTable} WHERE parent_id = {parentId} ORDER BY name";

				using(var reader = command.ExecuteReader())
				{
					while(reader.Read())
					{
						rows.Add(string.Join("|", Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "NULL" : Convert.ToString(reader.GetValue(i)))));
					}
				}
			}

			return rows;
		}
	}
}

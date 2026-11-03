using NHibernate;
using NHibernate.Collection.Generic;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа B: метод работает только с коллекцией живого родителя в сессии переданного UoW и в транзакции этого UoW.
	/// </summary>
	public class BulkSqlInsertSessionStateTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertSessionStateTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		/// <summary>
		/// B-01. Родитель создан, но не сохранён.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentNotSaved_ThrowsInvalidOperationException(InsertMode mode)
		{
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = new BulkParent();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.True(IsTransactionActive(uow));
		}

		/// <summary>
		/// B-02. После сохранения свойству коллекции присвоили новый список.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCollectionReplacedAfterSave_ThrowsInvalidOperationException(InsertMode mode)
		{
			var uow = NewUow();
			var parent = new BulkParent { Code = 1 };
			uow.Save(parent);
			parent.Items = new ObservableList<BulkChild>();
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.True(IsTransactionActive(uow));
		}

		/// <summary>
		/// B-03. Коллекция родителя из одного UoW, а передан другой.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCollectionFromOtherUow_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow1 = NewUow();
			var uow2 = NewUow();
			uow2.OpenTransaction();
			var parent = uow1.GetById<BulkParent>(parentId);
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow2, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// B-04. Родитель из уже закрытого UoW, передан новый.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentDetached_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			BulkParent parent;

			using(var closedUow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Закрываемый UoW"))
			{
				parent = closedUow.GetById<BulkParent>(parentId);
				_ = parent.Items.Count;
			}

			var uow = NewUow();
			uow.OpenTransaction();
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// B-05. Родитель вытеснен из сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentEvicted_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			uow.Session.Evict(parent);
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.True(IsTransactionActive(uow));
		}

		/// <summary>
		/// B-06. Родитель помечен на удаление: исключение до Flush, родитель не удалён.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentMarkedDeleted_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var collection = parent.Items;
			_ = collection.Count;
			uow.Delete(parent);
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, collection, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Writes());
			Assert.Equal(1, Database.Count("bulk_parents", $"id = {parentId}"));
		}

		/// <summary>
		/// B-07. Родитель в режиме только чтения.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenParentReadOnly_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			uow.Session.SetReadOnly(parent, true);
			var items = NewChildren(2);
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, items));

			state.AssertNothingChanged();
		}

		/// <summary>
		/// B-08. Коллекция-обёртка NHibernate, созданная вручную, без владельца.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenPersistentBagCreatedManually_ThrowsInvalidOperationException(InsertMode mode)
		{
			var uow = NewUow();
			uow.OpenTransaction();
			var bag = new PersistentGenericBag<BulkChild>(uow.Session.GetSessionImplementation());
			SqlLog.Clear();

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, bag, uow, NewChildren(2)));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
		}

		/// <summary>
		/// B-09. Родитель только загружен, транзакции нет: метод открывает транзакцию UoW сам.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenNoTransaction_OpensUowTransaction(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.Items, uow, NewChildren(3));

			Assert.True(IsTransactionActive(uow));
			Assert.Equal(0, CountChildren(parentId));

			uow.Commit();

			Assert.Equal(3, CountChildren(parentId));
		}

		/// <summary>
		/// B-09. Транзакцию открыл метод: без Commit строки откатываются при закрытии UoW.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenNoTransactionAndNoCommit_RowsRolledBackOnDispose(InsertMode mode)
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
		/// B-10. Транзакция уже закоммичена: метод открывает новую, прежний коммит не затронут.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTransactionCommitted_OpensNewTransaction(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			parent.Name = "закоммичено";
			uow.Save(parent);
			uow.Commit();

			await Insert(mode, parent.Items, uow, NewChildren(3));

			Assert.True(IsTransactionActive(uow));
			Assert.Equal(0, CountChildren(parentId));
			Assert.Equal(1, Database.Count("bulk_parents", $"id = {parentId} AND name = 'закоммичено'"));

			uow.Commit();

			Assert.Equal(3, CountChildren(parentId));
		}

		/// <summary>
		/// B-11. Транзакция откачена: метод открывает новую, откаченные данные не возвращаются.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenTransactionRolledBack_OpensNewTransaction(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			_ = parent.Items.Count;
			uow.OpenTransaction();
			ExecuteInSession(uow, $"UPDATE bulk_parents SET name = 'откачено' WHERE id = {parentId}");
			uow.Session.GetCurrentTransaction().Rollback();

			await Insert(mode, parent.Items, uow, NewChildren(3));

			Assert.True(IsTransactionActive(uow));

			uow.Commit();

			Assert.Equal(3, CountChildren(parentId));
			Assert.Equal(0, Database.Count("bulk_parents", $"id = {parentId} AND name = 'откачено'"));
		}

		/// <summary>
		/// B-12. UoW уже освобождён.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenUowDisposed_ThrowsObjectDisposedOrInvalidOperation(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Закрываемый UoW");
			var parent = uow.GetById<BulkParent>(parentId);
			var collection = parent.Items;
			_ = collection.Count;
			uow.Dispose();

			await Assert.ThrowsAnyAsync<InvalidOperationException>(() => Insert(mode, collection, uow, NewChildren(2)));

			Assert.Equal(0, CountChildren(parentId));
		}

		/// <summary>
		/// B-13. Проверки идут до Flush: при ошибке несохранённые правки в сессии не уходят в БД.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenCheckFails_DoesNotFlushPendingChanges(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			parent.Name = "не должно записаться";
			_ = parent.Tags.Count;
			SqlLog.Clear();

			await Assert.ThrowsAsync<NotSupportedException>(() => Insert(mode, parent.Tags, uow, new[] { new BulkTag { Name = "тег" } }));

			Assert.Equal(0, SqlLog.Writes());
			Assert.Equal(0, CountInSessionByName(uow, parentId));
		}

		private static long CountInSessionByName(IUnitOfWork uow, int parentId)
		{
			using(var command = uow.Session.Connection.CreateCommand())
			{
				uow.Session.GetCurrentTransaction()?.Enlist(command);
				command.CommandText = $"SELECT COUNT(*) FROM bulk_parents WHERE id = {parentId} AND name = 'не должно записаться'";

				return Convert.ToInt64(command.ExecuteScalar());
			}
		}
	}
}

using NHibernate;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа D: в метод можно передать только новые объекты типа элемента коллекции.
	/// Все объекты проверяются до первого INSERT.
	/// </summary>
	public class BulkSqlInsertNewItemsTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertNewItemsTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		/// <summary>
		/// D-01. Элемент уже в сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemAlreadyInSession_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var saved = NewChildren(1, "saved")[0];
			uow.Save(saved);
			SqlLog.Clear();
			var items = NewChildren(2);
			items.Add(saved);

			var exception = await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			Assert.Equal("newItems", exception.ParamName);
			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.True(IsTransactionActive(uow));
		}

		/// <summary>
		/// D-02. Элемент уже добавлен в коллекцию через Add.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemAlreadyInCollection_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			parent.Items.Add(items[1]);
			SqlLog.Clear();

			var exception = await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			Assert.Equal("newItems", exception.ParamName);
			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
		}

		/// <summary>
		/// D-03. Detached-объект: загружен в другом UoW, Id заполнен.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemDetached_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent(1);
			BulkChild detached;

			using(var otherUow = Fixture.UnitOfWorkFactory.CreateWithoutRoot("Другой UoW"))
			{
				detached = otherUow.GetById<BulkParent>(parentId).Items.Single();
			}

			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var state = Snapshot(uow, parent.Items, new[] { detached }, ChildrenTable, parentId);

			await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, new[] { detached }));

			state.AssertNothingChanged();
		}

		/// <summary>
		/// D-04. Новый объект с вручную заполненным Id при identity-генераторе.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemHasManualId_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			items[1].Id = 777;
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			state.AssertNothingChanged();
			Assert.Equal(777, items[1].Id);
		}

		/// <summary>
		/// D-06. Объект несмапленного наследника типа элемента.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemTypeNotMapped_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(1);
			items.Add(new UnmappedChild { Name = "несмапленный" });
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			state.AssertNothingChanged();
		}

		/// <summary>
		/// D-07. Ссылка элемента на другого родителя: исключение до первого INSERT.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemReferencesOtherParent_ThrowsArgumentException(InsertMode mode)
		{
			var parentId = CreateParent();
			var otherParentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(3);
			items[2].Parent = uow.GetById<BulkParent>(otherParentId);
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			var exception = await Assert.ThrowsAsync<ArgumentException>(() => Insert(mode, parent.Items, uow, items));

			Assert.Equal("newItems", exception.ParamName);
			state.AssertNothingChanged();
			Assert.Equal(0, CountChildren(otherParentId));
		}

		/// <summary>
		/// D-08. Ссылка на несохранённую сущность: ни один INSERT не выполнен.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemReferencesTransientEntity_ThrowsWithoutInsert(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			items[1].Tag = new BulkTag { Name = "несохранённый" };
			SqlLog.Clear();

			await Assert.ThrowsAsync<TransientObjectException>(() => Insert(mode, parent.Items, uow, items));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.False(uow.Session.Contains(items[0]));
		}

		/// <summary>
		/// D-09. Ссылка на сущность, помеченную на удаление в этой сессии.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemReferencesDeletedEntity_ThrowsInvalidOperationException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);
			var tag = new BulkTag { Name = "удаляемый" };
			uow.Save(tag);
			uow.Session.Flush();
			uow.Delete(tag);
			var items = NewChildren(2);
			items[1].Tag = tag;
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			await Assert.ThrowsAsync<InvalidOperationException>(() => Insert(mode, parent.Items, uow, items));

			state.AssertNothingChanged();
		}

		/// <summary>
		/// D-10. Некорректный элемент в последней пачке: не вставлена ни одна пачка.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenInvalidItemInLastBatch_InsertsNoBatch(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(25);
			items[24].Tag = new BulkTag { Name = "несохранённый" };
			SqlLog.Clear();

			await Assert.ThrowsAnyAsync<Exception>(() => Insert(mode, parent.Items, uow, items, 10));

			Assert.Equal(0, SqlLog.Inserts(ChildrenTable));
			Assert.Empty(parent.Items);
		}
	}
}

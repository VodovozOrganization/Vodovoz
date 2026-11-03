using NHibernate;
using NHibernate.Cfg;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Группа C: допускается только one-to-many bag или set без индекса и фильтра, с дочерней сущностью в одной таблице.
	/// </summary>
	public class BulkSqlInsertMappingTests : BulkSqlInsertTestBase
	{
		public BulkSqlInsertMappingTests(BulkSqlInsertFixture fixture) : base(fixture)
		{
		}

		private async Task AssertNotSupported<TItem>(InsertMode mode, Func<BulkParent, ICollection<TItem>> selectCollection, TItem item)
			where TItem : class
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var collection = selectCollection(parent);
			parent.Name = "не должно записаться";
			SqlLog.Clear();

			await Assert.ThrowsAsync<NotSupportedException>(() => Insert(mode, collection, uow, new[] { item }));

			// C-16: все проверки группы идут до Flush, поэтому записей в БД нет
			Assert.Equal(0, SqlLog.Writes());
			Assert.True(IsTransactionActive(uow));
			Assert.False(uow.Session.Contains(item));
		}

		/// <summary>
		/// C-01, C-16. Many-to-many.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenManyToMany_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.Tags, new BulkTag { Name = "тег" });

		/// <summary>
		/// C-02, C-16. Коллекция значений.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenElementCollection_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.Values, "значение");

		/// <summary>
		/// C-03, C-16. Коллекция компонентов.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenComponentCollection_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.Components, new ChildComponent { Value = "значение" });

		/// <summary>
		/// C-04, C-16. Индексированная коллекция. Map сюда не попадает: IDictionary не приводится к ICollection ссылочного типа,
		/// такой вызов не компилируется из-за ограничения where TItem : class.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenIndexedList_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.ListItems, new ListChild { Name = "элемент" });

		/// <summary>
		/// C-05, C-16. Коллекция с Where: вставленные строки могут не загрузиться.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenCollectionHasWhere_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.WhereItems, new WhereChild { Name = "элемент" });

		/// <summary>
		/// C-06, C-16. Immutable-коллекция.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenCollectionImmutable_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.ImmutableItems, new ImmutableChild { Name = "элемент" });

		/// <summary>
		/// C-07, C-16. Дочерняя сущность хранится в нескольких таблицах (joined-subclass).
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenChildJoinedSubclass_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported<JoinedChildBase>(mode, x => x.JoinedItems, new JoinedChild { Name = "элемент", Extra = "доп" });

		/// <summary>
		/// C-08, C-16. У дочерней сущности есть вторичная таблица.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenChildHasSecondaryTable_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.SecondaryTableItems, new SecondaryTableChild { Name = "элемент", Extra = "доп" });

		/// <summary>
		/// C-09, C-16. Версионная дочерняя сущность.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenChildVersioned_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.VersionedItems, new VersionedChild { Name = "элемент" });

		/// <summary>
		/// C-10. У переданного элемента непустая собственная коллекция: её данные были бы потеряны.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenItemHasNonEmptyOwnCollection_ThrowsNotSupportedException(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = uow.GetById<BulkParent>(parentId);
			var items = NewChildren(2);
			items[1].SubItems.Add(new BulkSubItem { Child = items[1] });
			var state = Snapshot(uow, parent.Items, items, ChildrenTable, parentId);

			await Assert.ThrowsAsync<NotSupportedException>(() => Insert(mode, parent.Items, uow, items));

			state.AssertNothingChanged();
		}

		/// <summary>
		/// C-11. Два свойства пишут в одну колонку. NHibernate отклоняет такой маппинг при старте,
		/// поэтому до метода он не дойдёт. Тест фиксирует эту защиту.
		/// </summary>
		[Fact]
		public void Configuration_WhenTwoPropertiesWriteSameColumn_ThrowsMappingException()
		{
			const string mapping = @"<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2' assembly='Vodovoz.Core.Data.NHibernate.IntegrationTests' namespace='Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain'>
				<class name='SimpleChild' table='bulk_duplicate_children'>
					<id name='Id' column='id'><generator class='native' /></id>
					<property name='Name' column='name' />
				</class>
				<class name='BulkTag' table='bulk_duplicate_tags'>
					<id name='Id' column='id'><generator class='native' /></id>
					<property name='Name' column='same_column' />
					<property name='Name' access='field.camelcase' column='same_column' />
				</class>
			</hibernate-mapping>";

			var configuration = new Configuration();
			configuration.SetProperty(global::NHibernate.Cfg.Environment.Dialect, typeof(global::NHibernate.Dialect.MySQL5Dialect).AssemblyQualifiedName);
			configuration.SetProperty(global::NHibernate.Cfg.Environment.Hbm2ddlKeyWords, "none");

			Assert.ThrowsAny<HibernateException>(() =>
			{
				configuration.AddXmlString(mapping);
				configuration.BuildSessionFactory();
			});
		}

		/// <summary>
		/// C-12. Свойство дочерней сущности частично пересекается с составным ключом коллекции.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenPropertyOverlapsCompositeKey_ThrowsNotSupportedException(InsertMode mode)
		{
			var uow = NewUow();
			uow.OpenTransaction();
			var parent = new CompositeParent { KeyA = 1, KeyB = 2 };
			uow.Save(parent);
			uow.Session.Flush();
			SqlLog.Clear();

			await Assert.ThrowsAsync<NotSupportedException>(
				() => Insert(mode, parent.Items, uow, new[] { new CompositeChild { Name = "элемент", KeyA = 1 } }));

			Assert.Equal(0, SqlLog.Inserts("bulk_composite_children"));
			Assert.True(IsTransactionActive(uow));
		}

		/// <summary>
		/// C-13. Ключ коллекции через PropertyRef, а не по Id родителя.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenKeyUsesPropertyRef_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.PropertyRefItems, new PropertyRefChild { Name = "элемент" });

		/// <summary>
		/// C-15, C-16. Генератор Assigned.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenGeneratorAssigned_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.AssignedItems, new AssignedChild { Name = "элемент" });

		/// <summary>
		/// C-15, C-16. Генератор Guid.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenGeneratorGuid_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.GuidItems, new GuidChild { Name = "элемент" });

		/// <summary>
		/// C-15, C-16. Генератор GuidComb.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenGeneratorGuidComb_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.GuidCombItems, new GuidCombChild { Name = "элемент" });

		/// <summary>
		/// C-15, C-16. Генератор HiLo.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenGeneratorHiLo_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.HiLoItems, new HiLoChild { Name = "элемент" });

		/// <summary>
		/// C-15, C-16. Генератор Increment. Sequence проверить нельзя: диалект MySQL не даёт построить такой маппинг.
		/// </summary>
		[DatabaseTheory, BothModes]
		public Task BulkSqlInsert_WhenGeneratorIncrement_ThrowsNotSupportedException(InsertMode mode) =>
			AssertNotSupported(mode, x => x.IncrementItems, new IncrementChild { Name = "элемент" });

		/// <summary>
		/// C-15. GeneratedBy.Identity() неотличим от Native (оба IdentityGenerator), поэтому проходит наравне с ним.
		/// </summary>
		[DatabaseTheory, BothModes]
		public async Task BulkSqlInsert_WhenGeneratorIdentity_Inserts(InsertMode mode)
		{
			var parentId = CreateParent();
			var uow = NewUow();
			var parent = uow.GetById<BulkParent>(parentId);

			await Insert(mode, parent.IdentityItems, uow, new[] { new IdentityChild { Name = "элемент" } });
			uow.Commit();

			Assert.Equal(1, CountChildren(parentId, "bulk_identity_children"));
		}
	}
}

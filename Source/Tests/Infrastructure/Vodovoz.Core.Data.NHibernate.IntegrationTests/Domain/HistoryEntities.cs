using FluentNHibernate.Mapping;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	/// <summary>
	/// Родитель без истории: его записи не должны мешать проверке истории дочерних сущностей.
	/// </summary>
	public class HistoryParent : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; } = "родитель";
		public virtual IObservableList<HistoryChild> Items { get; set; } = new ObservableList<HistoryChild>();
		public virtual IList<HistoryIdTitleChild> IdTitleItems { get; set; } = new List<HistoryIdTitleChild>();
		public virtual IList<NoHistoryChild> NoHistoryItems { get; set; } = new List<NoHistoryChild>();
	}

	/// <summary>
	/// Сущность с историей: все виды полей, которые умеет записывать трекер QS.
	/// </summary>
	[HistoryTrace]
	public class HistoryChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }
		public virtual string UniqueCode { get; set; }
		public virtual decimal Amount { get; set; }
		public virtual DateTime OccurredAt { get; set; }
		public virtual int? NullableCount { get; set; }
		public virtual bool IsActive { get; set; }
		public virtual BulkChildKind Kind { get; set; }
		public virtual BulkTag Tag { get; set; }
		public virtual DateTime CreatedAt { get; set; }

		[IgnoreHistoryTrace]
		public virtual string IgnoredNote { get; set; }

		public virtual IList<HistorySubItem> SubItems { get; set; } = new List<HistorySubItem>();

		public virtual string Title => $"Строка {Name}";
	}

	public class HistoryChildSpecial : HistoryChild
	{
		public virtual string Extra { get; set; }
	}

	public class HistorySubItem : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryChild Child { get; set; }
	}

	/// <summary>
	/// Заголовок записи истории содержит Id: проверяет, что берётся настоящий Id из БД.
	/// </summary>
	[HistoryTrace]
	public class HistoryIdTitleChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }

		public virtual string Title => $"Запись {Id}";
	}

	/// <summary>
	/// Сущность без атрибута истории.
	/// </summary>
	public class NoHistoryChild : IDomainObject
	{
		public virtual int Id { get; set; }
		public virtual HistoryParent Parent { get; set; }
		public virtual string Name { get; set; }
	}

	public class HistoryParentMap : ClassMap<HistoryParent>
	{
		public HistoryParentMap()
		{
			Table("history_test_parents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			HasMany(x => x.Items).KeyColumn("parent_id").Inverse().Cascade.AllDeleteOrphan();
			HasMany(x => x.IdTitleItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.NoHistoryItems).KeyColumn("parent_id").Inverse();
		}
	}

	public class HistoryChildMap : ClassMap<HistoryChild>
	{
		public HistoryChildMap()
		{
			Table("history_test_children");
			DiscriminateSubClassesOnColumn("child_type", "base").Not.Nullable();
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name").Length(500);
			Map(x => x.UniqueCode).Column("unique_code").Unique();
			Map(x => x.Amount).Column("amount").Precision(16).Scale(2);
			Map(x => x.OccurredAt).Column("occurred_at");
			Map(x => x.NullableCount).Column("nullable_count");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.Kind).Column("kind");
			References(x => x.Tag).Column("tag_id");
			Map(x => x.CreatedAt).Column("created_at")
				.Generated.Insert().Not.Insert().Not.Update().Default("CURRENT_TIMESTAMP");
			Map(x => x.IgnoredNote).Column("ignored_note");
			HasMany(x => x.SubItems).KeyColumn("child_id").Inverse().Cascade.All();
		}
	}

	public class HistoryChildSpecialMap : SubclassMap<HistoryChildSpecial>
	{
		public HistoryChildSpecialMap()
		{
			DiscriminatorValue("special");
			Map(x => x.Extra).Column("extra");
		}
	}

	public class HistorySubItemMap : ClassMap<HistorySubItem>
	{
		public HistorySubItemMap()
		{
			Table("history_test_sub_items");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Child).Column("child_id");
		}
	}

	public class HistoryIdTitleChildMap : ClassMap<HistoryIdTitleChild>
	{
		public HistoryIdTitleChildMap()
		{
			Table("history_test_id_title_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name");
		}
	}

	public class NoHistoryChildMap : ClassMap<NoHistoryChild>
	{
		public NoHistoryChildMap()
		{
			Table("history_test_no_history_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name");
		}
	}
}

using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
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
}

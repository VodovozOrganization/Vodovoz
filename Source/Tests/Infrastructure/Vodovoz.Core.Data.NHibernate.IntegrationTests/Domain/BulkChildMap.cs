using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkChildMap : ClassMap<BulkChild>
	{
		public BulkChildMap()
		{
			Table("bulk_children");
			DiscriminateSubClassesOnColumn("child_type", "base").Not.Nullable();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Parent).Column("parent_id");
			Map(x => x.Name).Column("name");
			Map(x => x.UniqueCode).Column("unique_code").Unique();
			Map(x => x.Notes).Column("notes").Length(20000);
			Map(x => x.Amount).Column("amount").Precision(16).Scale(2);
			Map(x => x.OccurredAt).Column("occurred_at");
			Map(x => x.NullableCount).Column("nullable_count");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.Kind).Column("kind");
			Component(x => x.Address);
			References(x => x.Tag).Column("tag_id");
			Map(x => x.CreatedAt).Column("created_at")
				.Generated.Insert().Not.Insert().Not.Update().Default("CURRENT_TIMESTAMP");
			Map(x => x.NameLength).Formula("CHAR_LENGTH(name)");
			HasMany(x => x.SubItems).KeyColumn("child_id").Inverse().Cascade.All();
		}
	}
}

using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkParentMap : ClassMap<BulkParent>
	{
		public BulkParentMap()
		{
			Table("bulk_parents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Code).Column("code").Unique();

			HasMany(x => x.Items).KeyColumn("parent_id").Inverse().Cascade.AllDeleteOrphan();
			HasMany(x => x.NonInverseItems).KeyColumn("parent_id").Cascade.All();
			HasMany(x => x.SetItems).KeyColumn("parent_id").AsSet().Inverse().Cascade.All();
			HasMany(x => x.OrderedItems).KeyColumn("parent_id").Inverse().OrderBy("name desc").Cascade.All();
			HasMany(x => x.FilteredItems).KeyColumn("parent_id").Inverse().ApplyFilter<OnlyActiveFilter>("is_archived = 0");
			HasMany(x => x.WideItems).KeyColumn("parent_id").Inverse();
			HasManyToMany(x => x.Tags).Table("bulk_parent_tags").ParentKeyColumn("parent_id").ChildKeyColumn("tag_id");
			HasMany(x => x.Values).Table("bulk_parent_values").KeyColumn("parent_id").Element("value", e => e.Type<string>());
			HasMany(x => x.Components).Table("bulk_parent_components").KeyColumn("parent_id").Component(c => c.Map(x => x.Value).Column("value"));
			HasMany(x => x.ListItems).KeyColumn("parent_id").AsList(i => i.Column("position"));
			HasMany(x => x.WhereItems).KeyColumn("parent_id").Inverse().Where("is_archived = 0");
			HasMany(x => x.ImmutableItems).KeyColumn("parent_id").Inverse().ReadOnly();
			HasMany(x => x.JoinedItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.SecondaryTableItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.VersionedItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.PropertyRefItems).KeyColumn("parent_code").PropertyRef("Code").Inverse();
			HasMany(x => x.AssignedItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.GuidItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.GuidCombItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.HiLoItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.IncrementItems).KeyColumn("parent_id").Inverse();
			HasMany(x => x.IdentityItems).KeyColumn("parent_id").Inverse();
		}
	}
}

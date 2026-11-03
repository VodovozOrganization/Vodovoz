using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class OnlyActiveFilter : FilterDefinition
	{
		public const string FilterName = "only_active";

		public OnlyActiveFilter()
		{
			WithName(FilterName);
		}
	}

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

	public class BulkAddressMap : ComponentMap<BulkAddress>
	{
		public BulkAddressMap()
		{
			Map(x => x.City).Column("address_city");
			Map(x => x.Street).Column("address_street");
		}
	}

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

	public class BulkChildSpecialMap : SubclassMap<BulkChildSpecial>
	{
		public BulkChildSpecialMap()
		{
			DiscriminatorValue("special");
			Map(x => x.Extra).Column("extra");
		}
	}

	public class BulkSubItemMap : ClassMap<BulkSubItem>
	{
		public BulkSubItemMap()
		{
			Table("bulk_sub_items");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Child).Column("child_id");
		}
	}

	public class BulkTagMap : ClassMap<BulkTag>
	{
		public BulkTagMap()
		{
			Table("bulk_tags");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name").Unique();
		}
	}

	public class NonInverseChildMap : ClassMap<NonInverseChild>
	{
		public NonInverseChildMap()
		{
			Table("bulk_non_inverse_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class SetChildMap : ClassMap<SetChild>
	{
		public SetChildMap()
		{
			Table("bulk_set_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class OrderedChildMap : ClassMap<OrderedChild>
	{
		public OrderedChildMap()
		{
			Table("bulk_ordered_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class FilteredChildMap : ClassMap<FilteredChild>
	{
		public FilteredChildMap()
		{
			Table("bulk_filtered_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}

	public class WhereChildMap : ClassMap<WhereChild>
	{
		public WhereChildMap()
		{
			Table("bulk_where_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}

	public class ListChildMap : ClassMap<ListChild>
	{
		public ListChildMap()
		{
			Table("bulk_list_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class ImmutableChildMap : ClassMap<ImmutableChild>
	{
		public ImmutableChildMap()
		{
			Table("bulk_immutable_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class JoinedChildBaseMap : ClassMap<JoinedChildBase>
	{
		public JoinedChildBaseMap()
		{
			Table("bulk_joined_base");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class JoinedChildMap : SubclassMap<JoinedChild>
	{
		public JoinedChildMap()
		{
			Table("bulk_joined_children");
			KeyColumn("id");
			Map(x => x.Extra).Column("extra");
		}
	}

	public class SecondaryTableChildMap : ClassMap<SecondaryTableChild>
	{
		public SecondaryTableChildMap()
		{
			Table("bulk_secondary_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Join("bulk_secondary_ext", join =>
			{
				join.KeyColumn("child_id");
				join.Map(x => x.Extra).Column("extra");
			});
		}
	}

	public class VersionedChildMap : ClassMap<VersionedChild>
	{
		public VersionedChildMap()
		{
			Table("bulk_versioned_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Version(x => x.Version).Column("version");
			Map(x => x.Name).Column("name");
		}
	}

	public class PropertyRefChildMap : ClassMap<PropertyRefChild>
	{
		public PropertyRefChildMap()
		{
			Table("bulk_property_ref_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}

	public class IdentityChildMap : ClassMap<IdentityChild>
	{
		public IdentityChildMap()
		{
			Table("bulk_identity_children");
			Id(x => x.Id).Column("id").GeneratedBy.Identity();
			Map(x => x.Name).Column("name");
		}
	}

	public class AssignedChildMap : ClassMap<AssignedChild>
	{
		public AssignedChildMap()
		{
			Table("bulk_assigned_children");
			Id(x => x.Id).Column("id").GeneratedBy.Assigned();
			Map(x => x.Name).Column("name");
		}
	}

	public class IncrementChildMap : ClassMap<IncrementChild>
	{
		public IncrementChildMap()
		{
			Table("bulk_increment_children");
			Id(x => x.Id).Column("id").GeneratedBy.Increment();
			Map(x => x.Name).Column("name");
		}
	}

	public class HiLoChildMap : ClassMap<HiLoChild>
	{
		public HiLoChildMap()
		{
			Table("bulk_hilo_children");
			Id(x => x.Id).Column("id").GeneratedBy.HiLo("bulk_hilo_keys", "next_hi", "100");
			Map(x => x.Name).Column("name");
		}
	}

	public class GuidChildMap : ClassMap<GuidChild>
	{
		public GuidChildMap()
		{
			Table("bulk_guid_children");
			Id(x => x.Id).Column("id").GeneratedBy.Guid();
			Map(x => x.Name).Column("name");
		}
	}

	public class GuidCombChildMap : ClassMap<GuidCombChild>
	{
		public GuidCombChildMap()
		{
			Table("bulk_guid_comb_children");
			Id(x => x.Id).Column("id").GeneratedBy.GuidComb();
			Map(x => x.Name).Column("name");
		}
	}

	public class WideChildMap : ClassMap<WideChild>
	{
		public WideChildMap()
		{
			Table("bulk_wide_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.C1).Column("c1");
			Map(x => x.C2).Column("c2");
			Map(x => x.C3).Column("c3");
			Map(x => x.C4).Column("c4");
			Map(x => x.C5).Column("c5");
			Map(x => x.C6).Column("c6");
			Map(x => x.C7).Column("c7");
			Map(x => x.C8).Column("c8");
			Map(x => x.C9).Column("c9");
			Map(x => x.C10).Column("c10");
		}
	}

	public class VersionedParentMap : ClassMap<VersionedParent>
	{
		public VersionedParentMap()
		{
			Table("bulk_versioned_parents");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Version(x => x.Version).Column("version");
			Map(x => x.Name).Column("name");
			HasMany(x => x.Items).KeyColumn("parent_id").Inverse();
		}
	}

	public class VersionedParentChildMap : ClassMap<VersionedParentChild>
	{
		public VersionedParentChildMap()
		{
			Table("bulk_versioned_parent_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			References(x => x.Parent).Column("parent_id");
		}
	}

	public class CompositeParentMap : ClassMap<CompositeParent>
	{
		public CompositeParentMap()
		{
			Table("bulk_composite_parents");
			CompositeId()
				.KeyProperty(x => x.KeyA, "key_a")
				.KeyProperty(x => x.KeyB, "key_b");
			HasMany(x => x.Items).KeyColumns.Add("key_a", "key_b").Inverse();
		}
	}

	public class CompositeChildMap : ClassMap<CompositeChild>
	{
		public CompositeChildMap()
		{
			Table("bulk_composite_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.KeyA).Column("key_a");
		}
	}
}

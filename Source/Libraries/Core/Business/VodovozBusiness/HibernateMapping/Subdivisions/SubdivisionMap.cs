﻿using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap()
		{
			Table("subdivisions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.SubdivisionType).Column("type").CustomType<SubdivisionTypeStringType>();
			Map(x => x.Address).Column("address");
			References(x => x.Chief).Column("chief_id");
			References(x => x.ParentSubdivision).Column("parent_subdivision_id");
			References(x => x.GeographicGroup).Column("geo_group_id");
			References(x => x.DefaultSalesPlan).Column("default_sales_plan_id");
			HasMany(x => x.ChildSubdivisions).Cascade.AllDeleteOrphan().Inverse().KeyColumn("parent_subdivision_id");
			HasManyToMany(x => x.DocumentTypes).Table("subdivisions_documents_types")
									  .ParentKeyColumn("subdivision_id")
									  .ChildKeyColumn("type_of_entity_id")
									  .LazyLoad();
		}
	}
}


using FluentNHibernate.Mapping;

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
			References(x => x.Chief).Column("chief_id");
			References(x => x.ParentSubdivision).Column("parent_subdivision_id");
			References(x => x.GeographicGroup).Column("geographic_group_id");
			References(x => x.DefaultWageParameter).Column("default_wage_calculation_id");
			HasMany(x => x.ChildSubdivisions).Cascade.AllDeleteOrphan().Inverse().KeyColumn("parent_subdivision_id");
			HasManyToMany(x => x.DocumentTypes).Table("subdivisions_documents_types")
									  .ParentKeyColumn("subdivision_id")
									  .ChildKeyColumn("type_of_entity_id")
									  .LazyLoad();
		}
	}
}


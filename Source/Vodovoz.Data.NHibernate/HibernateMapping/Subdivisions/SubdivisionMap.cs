using FluentNHibernate.Mapping;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Subdivisions
{
	public class SubdivisionMap : ClassMap<Subdivision>
	{
		public SubdivisionMap()
		{
			Table("subdivisions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.ShortName).Column("short_name");
			Map(x => x.SubdivisionType).Column("type");
			Map(x => x.Address).Column("address");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.PacsTimeManagementEnabled).Column("pacs_time_management_enabled");
			Map(x => x.FinancialResponsibilityCenterId).Column("financial_responsibility_center_id");
			Map(x => x.ChiefId).Column("chief_id").ReadOnly();

			References(x => x.Chief).Column("chief_id");
			References(x => x.ParentSubdivision).Column("parent_subdivision_id");
			References(x => x.GeographicGroup).Column("geo_group_id");
			References(x => x.DefaultSalesPlan).Column("default_sales_plan_id");

			HasMany(x => x.ChildSubdivisions).Cascade.AllDeleteOrphan().Inverse().KeyColumn("parent_subdivision_id");
			HasManyToMany(x => x.DocumentTypes)
				.Table("subdivisions_documents_types")
				.ParentKeyColumn("subdivision_id")
				.ChildKeyColumn("type_of_entity_id")
				.LazyLoad();
		}
	}
}

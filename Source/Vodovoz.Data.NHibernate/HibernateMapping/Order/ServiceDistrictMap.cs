using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class ServiceDistrictMap : ClassMap<ServiceDistrict>
	{
		public ServiceDistrictMap()
		{
			Table("service_districts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ServiceDistrictName).Column("district_name");
			Map(x => x.ServiceDistrictBorder).Column("district_border").CustomType<MySQL57GeometryType>();

			References(x => x.GeographicGroup).Column("geo_group_id");
			References(x => x.ServiceDistrictsSet).Column("service_districts_set_id");
			References(x => x.CopyOf).Column("copy_of");

			HasMany(x => x.ServiceDistrictCopyItems)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("service_district_id");

			HasMany(x => x.AllServiceDistrictRules)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("service_district_id");

			HasMany(x => x.AllServiceDeliveryScheduleRestrictions)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("service_district_id");
		}
	}
}

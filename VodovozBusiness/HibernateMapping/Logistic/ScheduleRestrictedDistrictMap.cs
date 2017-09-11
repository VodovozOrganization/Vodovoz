using FluentNHibernate.Mapping;
using NHibernate.Spatial.Type;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping
{
	public class ScheduleRestrictedDistrictMap: ClassMap<ScheduleRestrictedDistrict>
	{
		public ScheduleRestrictedDistrictMap()
		{
			Table("schedule_restricted_districts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DistrictName).Column("district_name");
			Map(x => x.MinBottles).Column("min_bottles");
			Map(x => x.DistrictBorder).Column("district_border").CustomType<GeometryType>();

			HasMany(x => x.ScheduleRestrictions).Cascade.AllDeleteOrphan().KeyColumn("district_id").Inverse();
		}
	}
}

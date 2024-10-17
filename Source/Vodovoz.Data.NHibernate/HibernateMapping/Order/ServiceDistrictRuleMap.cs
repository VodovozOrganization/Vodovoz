using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class ServiceDistricRuletMap : ClassMap<ServiceDistrictRule>
	{
		public ServiceDistricRuletMap()
		{
			Table("service_district_rules");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.ServiceType).Column("service_district_type");
			Map(x => x.Price).Column("price");

			References(x => x.ServiceDistrict).Column("service_district_id");

		}

		public class CommonServiceDistrictRuleMap : SubclassMap<CommonServiceDistrictRule>
		{
			public CommonServiceDistrictRuleMap()
			{
				DiscriminatorValue(nameof(ServiceDistrictRule.ServiceDistrictRuleType.Common));
			}
		}

		public class WeekDayServiceDistrictRuleMap : SubclassMap<WeekDayServiceDistrictRule>
		{
			public WeekDayServiceDistrictRuleMap()
			{
				DiscriminatorValue(nameof(ServiceDistrictRule.ServiceDistrictRuleType.WeekDay));

				Map(x => x.WeekDay).Column("week_day");
			}
		}
	}
}

using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class ServiceDeliveryScheduleRestrictionMap : ClassMap<ServiceDeliveryScheduleRestriction>
	{
		public ServiceDeliveryScheduleRestrictionMap()
		{
			Table("service_delivery_schedule_restrictions");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.WeekDay).Column("week_day");

			References(x => x.ServiceDistrict).Column("service_district_id");
			References(x => x.DeliverySchedule).Column("delivery_schedule_id");
			References(x => x.AcceptBefore).Column("accept_before_id").Not.LazyLoad();
		}
	}
}

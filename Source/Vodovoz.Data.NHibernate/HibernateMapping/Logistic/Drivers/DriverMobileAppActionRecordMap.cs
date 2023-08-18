using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Drivers
{
	public class DriverMobileAppActionRecordMap : ClassMap<DriverMobileAppActionRecord>
	{
		public DriverMobileAppActionRecordMap()
		{
			Table("driver_mobile_app_action_records");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Action).Column("type");
			Map(x => x.Result).Column("result");
			Map(x => x.ActionDatetime).Column("action_datetime");
			Map(x => x.RecievedDatetime).Column("recieved_datetime");

			References(x => x.Driver).Column("employee_id");
		}
	}
}

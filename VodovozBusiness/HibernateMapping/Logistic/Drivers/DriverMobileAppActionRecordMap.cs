using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Drivers;

namespace Vodovoz.HibernateMapping.Logistic.Drivers
{
	public class DriverMobileAppActionRecordMap : ClassMap<DriverMobileAppActionRecord>
	{
		public DriverMobileAppActionRecordMap()
		{
			Table("driver_mobile_app_action_records");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Action).Column("type").CustomType<DriverMobileAppActionStringType>();
			Map(x => x.Result).Column("result");
			Map(x => x.ActionDatetime).Column("action_datetime");
			Map(x => x.RecievedDatetime).Column("recieved_datetime");

			References(x => x.Driver).Column("employee_id");
		}
	}

	public class DriverMobileAppActionStringType : NHibernate.Type.EnumStringType
	{
		public DriverMobileAppActionStringType() : base(typeof(DriverMobileAppActionType))
		{
		}
	}
}

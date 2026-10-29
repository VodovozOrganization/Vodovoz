using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Mango;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Mango
{
	public class DriverMangoEmployeeRegistrationRequestMap : ClassMap<DriverMangoEmployeeRegistrationRequest>
	{
		public DriverMangoEmployeeRegistrationRequestMap()
		{
			Table("driver_mango_employee_registration_requests");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.DriverId).Column("driver_id");
			Map(x => x.Status).Column("status");
			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.ProcessedAt).Column("processed_at");
			Map(x => x.ErrorMessage).Column("error_message");
		}
	}
}

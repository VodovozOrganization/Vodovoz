using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class DriverComplaintReasonMap : ClassMap<DriverComplaintReason>
	{
		public DriverComplaintReasonMap()
		{
			Table("driver_complaint_reason");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsPopular).Column("is_popular");
		}
	}
}

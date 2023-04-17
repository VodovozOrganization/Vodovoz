using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintLogisticsRequirementsMap : ClassMap<ComplaintLogisticsRequirements>
	{
		public ComplaintLogisticsRequirementsMap()
		{
			Table("complaint_logistics_requirement");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ForwarderRequired).Column("forwarder_required");
			References(x => x.DocumentsRequired).Column("documents_required");
			References(x => x.RussianDriverRequired).Column("russian_driver_required");
			References(x => x.PassRequired).Column("pass_required");
			References(x => x.LagrusRequired).Column("lagrus_required");
			References(x => x.Complaint).Column("complaint_id");
		}
	}
}

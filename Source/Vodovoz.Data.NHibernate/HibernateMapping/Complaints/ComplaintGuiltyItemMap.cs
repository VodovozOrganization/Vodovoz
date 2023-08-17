using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Complaints
{
	public class ComplaintGuiltyItemMap : ClassMap<ComplaintGuiltyItem>
	{
		public ComplaintGuiltyItemMap()
		{
			Table("complaint_guilty_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Complaint).Column("complaint_id");
			References(x => x.Responsible).Column("responsible_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}

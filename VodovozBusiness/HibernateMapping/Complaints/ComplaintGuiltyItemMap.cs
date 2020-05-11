using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintGuiltyItemMap : ClassMap<ComplaintGuiltyItem>
	{
		public ComplaintGuiltyItemMap()
		{
			Table("complaint_guilty_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Complaint).Column("complaint_id");
			Map(x => x.GuiltyType).Column("guilty_type").CustomType<ComplaintGuiltyTypesStringType>();
			References(x => x.Employee).Column("employee_id");
			References(x => x.Subdivision).Column("subdivision_id");
		}
	}
}

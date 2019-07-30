using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.HibernateMapping.Complaints
{
	public class ComplaintMap : ClassMap<Complaint>
	{
		public ComplaintMap()
		{
			Table("complaints");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ComplainantName).Column("complainant_name");
			Map(x => x.ComplaintText).Column("complaint_text");
			Map(x => x.Phone).Column("phone");
			Map(x => x.Status).Column("status").CustomType<ComplaintStatusesStringType>();

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Order).Column("order_id");
			References(x => x.ComplaintSource).Column("complaint_source_id");
		}
	}
}

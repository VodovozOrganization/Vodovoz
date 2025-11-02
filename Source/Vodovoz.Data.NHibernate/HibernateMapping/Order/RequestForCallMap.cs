using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class RequestForCallMap : ClassMap<RequestForCall>
	{
		public RequestForCallMap()
		{
			Table("requests_for_call");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Author).Column("author");
			Map(x => x.Source).Column("source");
			Map(x => x.Created).Column("created");
			Map(x => x.Phone).Column("phone");
			Map(x => x.RequestForCallStatus).Column("request_for_call_status");

			References(x => x.Order).Column("order_id");
			References(x => x.EmployeeWorkWith).Column("employee_work_with_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.ClosedReason).Column("closed_reason_id");
		}
	}
}

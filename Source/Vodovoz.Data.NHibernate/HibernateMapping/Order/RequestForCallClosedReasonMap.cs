using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class RequestForCallClosedReasonMap : ClassMap<RequestForCallClosedReason>
	{
		public RequestForCallClosedReasonMap()
		{
			Table("requests_for_call_closed_reasons");
			
			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}

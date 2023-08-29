using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class RouteListAddressKeepingDocumentItemMap : ClassMap<RouteListAddressKeepingDocumentItem>
	{
		public RouteListAddressKeepingDocumentItemMap()
		{
			Table("route_list_address_keeping_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");

			References(x => x.RouteListAddressKeepingDocument).Column("route_list_keeping_document_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.DeliveryFreeBalanceOperation).Cascade.All().Column("delivery_free_balance_operation_id");
		}
	}
}

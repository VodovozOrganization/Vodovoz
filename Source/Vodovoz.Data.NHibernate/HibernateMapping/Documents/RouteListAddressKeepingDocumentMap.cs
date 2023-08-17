using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class RouteListAddressKeepingDocumentMap : ClassMap<RouteListAddressKeepingDocument>
	{
		public RouteListAddressKeepingDocumentMap()
		{
			Table("route_list_address_keeping_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.RouteListItem).Column("route_list_address_id");
			References(x => x.Author).Column("author_id");

			HasMany(x => x.Items).KeyColumn("route_list_keeping_document_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}

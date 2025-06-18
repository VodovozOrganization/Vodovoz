using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class RouteListAddressKeepingDocumentMap : ClassMap<RouteListAddressKeepingDocument>
	{
		public RouteListAddressKeepingDocumentMap()
		{
			Table("route_list_address_keeping_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.AuthorId).Column("author_id");

			References(x => x.RouteListItem).Column("route_list_address_id");

			HasMany(x => x.Items).KeyColumn("route_list_keeping_document_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();
		}
	}
}

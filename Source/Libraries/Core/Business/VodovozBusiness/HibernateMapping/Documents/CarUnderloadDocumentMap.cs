using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class CarUnderloadDocumentMap : ClassMap<CarUnderloadDocument>
    {
        public CarUnderloadDocumentMap()
        {
            Table ("store_car_underload_documents");
			
            Id(x => x.Id).Column("id").GeneratedBy.Native();

            References(x => x.RouteList).Column("route_list_id");
			References(x => x.Author).Column("author_id");

			HasMany(x => x.Items).KeyColumn("car_underload_document_id")
				.Cascade.AllDeleteOrphan().Inverse().LazyLoad();

		}
    }
}

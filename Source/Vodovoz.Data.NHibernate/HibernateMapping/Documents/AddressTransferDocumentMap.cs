using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
    public class AddressTransferDocumentMap : ClassMap<AddressTransferDocument>
    {
        public AddressTransferDocumentMap()
        {
            Table("address_transfer_documents");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.TimeStamp).Column("time_stamp");
            Map(x => x.LastEditedTime).Column("last_edit_time");

            References(x => x.Author).Column("author_id");
            References(x => x.LastEditor).Column("last_editor_id");
            References(x => x.RouteListFrom).Column("route_list_from_id");
            References(x => x.RouteListTo).Column("route_list_to_id");

            HasMany(x => x.AddressTransferDocumentItems)
                .Cascade.AllDeleteOrphan().Inverse().KeyColumn("address_transfer_document_id");
        }
    }
}
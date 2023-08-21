﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
    public class DeliveryDocumentMap : ClassMap<DeliveryDocument>
    {
        public DeliveryDocumentMap()
        {
            Table("delivery_documents");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Map(x => x.TimeStamp).Column("time_stamp");
            Map(x => x.LastEditedTime).Column("last_edit_time");

            References(x => x.Author).Column("author_id");
            References(x => x.LastEditor).Column("last_editor_id");
            References(x => x.RouteListItem).Column("route_list_address_id");

            HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("delivery_document_id");
        }
    }
}

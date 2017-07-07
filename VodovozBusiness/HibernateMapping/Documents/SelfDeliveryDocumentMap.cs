using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class SelfDeliveryDocumentMap : ClassMap<SelfDeliveryDocument>
	{
		public SelfDeliveryDocumentMap ()
		{
			Table ("store_self_delivery_document");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.Comment).Column("comment");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.Order).Column ("order_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("store_self_delivery_document_id");
			HasMany (x => x.ReturnedItems).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("store_self_delivery_document_id");
		}
	}
}


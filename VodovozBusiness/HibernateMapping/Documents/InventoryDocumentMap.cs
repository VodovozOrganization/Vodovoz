using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class InventoryDocumentMap : ClassMap<InventoryDocument>
	{
		public InventoryDocumentMap ()
		{
			Table ("store_inventory");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Comment).Column ("comment");
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("store_inventory_id");
		}
	}
}
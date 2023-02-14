using FluentNHibernate;
using FluentNHibernate.Mapping;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class InventoryDocumentMap : ClassMap<InventoryDocument>
	{
		public InventoryDocumentMap ()
		{
			Table ("store_inventory");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Map (x => x.Comment).Column ("comment");
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			Component(x => x.Items,
				part => {
					part.HasMany<InventoryDocumentItem>(
							Reveal.Member<GenericObservableList<InventoryDocumentItem>>("items"))
						.KeyColumn("store_inventory_id")
						.Cascade.AllDeleteOrphan().Inverse();
				});
		}
	}
}

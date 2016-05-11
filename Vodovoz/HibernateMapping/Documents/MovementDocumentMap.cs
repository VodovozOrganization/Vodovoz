using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class MovementDocumentMap : ClassMap<MovementDocument>
	{
		public MovementDocumentMap ()
		{
			Table ("movement_document");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map (x => x.Comment).Column ("comment");
			Map (x => x.Category).Column ("category").CustomType<MovementDocumentCategoryStringType> ();
			Map(x => x.LastEditedTime).Column("last_edit_time");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.ResponsiblePerson).Column ("responsible_person_id");
			References (x => x.FromDeliveryPoint).Column ("delivery_point_from_id");
			References (x => x.FromClient).Column ("counterparty_from_id");
			References (x => x.FromWarehouse).Column ("warehouse_from_id");
			References (x => x.ToDeliveryPoint).Column ("delivery_point_to_id");
			References (x => x.ToClient).Column ("counterparty_to_id");
			References (x => x.ToWarehouse).Column ("warehouse_to_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("movement_document_id");
		}
	}
}
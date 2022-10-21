using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class MovementDocumentMap : ClassMap<MovementDocument>
	{
		public MovementDocumentMap ()
		{
			Table ("store_movement_document");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column ("id").GeneratedBy.Native ();

			Map(x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.Comment).Column ("comment");
			Map(x => x.DocumentType).Column ("category").CustomType<MovementDocumentCategoryStringType> ();
			Map(x => x.Status).Column("status").CustomType<MovementDocumentStatusStringType>();
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.SendTime).Column("send_time");
			Map(x => x.ReceiveTime).Column("delivered_time");
			Map(x => x.DiscrepancyAcceptTime).Column("discrepancy_accept_time");
			Map(x => x.HasDiscrepancy).Column("has_discrepancy");

			References(x => x.Author).Column ("author_id");
			References(x => x.LastEditor).Column("last_editor_id");
			References(x => x.Receiver).Column("receiver_id");
			References(x => x.Sender).Column ("sender_id");
			References(x => x.DiscrepancyAccepter).Column("discrepancy_accepter_id");
			References(x => x.MovementWagon).Column ("transportation_wagon_id");
			References(x => x.FromWarehouse).Column ("warehouse_from_id");
			References(x => x.ToWarehouse).Column ("warehouse_to_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("movement_document_id");
		}
	}
}
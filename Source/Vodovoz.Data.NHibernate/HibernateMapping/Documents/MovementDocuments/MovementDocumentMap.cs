using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments
{
	public class MovementDocumentMap : ClassMap<MovementDocument>
	{
		public MovementDocumentMap()
		{
			Table("store_movement_document");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.Comment).Column("comment");
			Map(x => x.DocumentType).Column("category");
			Map(x => x.Status).Column("status");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.SendTime).Column("send_time");
			Map(x => x.ReceiveTime).Column("delivered_time");
			Map(x => x.DiscrepancyAcceptTime).Column("discrepancy_accept_time");
			Map(x => x.HasDiscrepancy).Column("has_discrepancy");
			Map(x => x.MovementDocumentTypeByStorage).Column("document_type_by_storage");
			Map(x => x.StorageFrom).Column("storage_from");
			Map(x => x.TransporterBill).Column("transporter_bill");
			Map(x => x.TranporterSum).Column("tranporter_sum");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.Receiver).Column("receiver_id");
			References(x => x.Sender).Column("sender_id");
			References(x => x.DiscrepancyAccepter).Column("discrepancy_accepter_id");
			References(x => x.MovementWagon).Column("transportation_wagon_id");
			References(x => x.FromWarehouse).Column("warehouse_from_id");
			References(x => x.ToWarehouse).Column("warehouse_to_id");
			References(x => x.FromEmployee).Column("employee_from_id");
			References(x => x.ToEmployee).Column("employee_to_id");
			References(x => x.FromCar).Column("car_from_id");
			References(x => x.ToCar).Column("car_to_id");
			References(x => x.TransporterCounterparty).Column("transporter_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("movement_document_id");
		}
	}
}

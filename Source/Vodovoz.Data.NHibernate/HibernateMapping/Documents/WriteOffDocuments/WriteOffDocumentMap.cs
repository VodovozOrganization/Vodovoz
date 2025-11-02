using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class WriteOffDocumentMap : ClassMap<WriteOffDocument>
	{
		public WriteOffDocumentMap()
		{
			Table("store_writeoff_document");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Comment).Column("comment");
			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.WriteOffType).Column("write_off_type");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.ResponsibleEmployee).Column("responsible_employee_id");
			References(x => x.WriteOffFromWarehouse).Column("write_off_from_warehouse_id");
			References(x => x.WriteOffFromEmployee).Column("write_off_from_employee_id");
			References(x => x.WriteOffFromCar).Column("write_off_from_car_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("write_off_document_id");
		}
	}
}

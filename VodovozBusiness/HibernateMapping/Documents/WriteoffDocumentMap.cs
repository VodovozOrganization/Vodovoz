using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;

namespace Vodovoz.HibernateMapping
{
	public class WriteoffDocumentMap : ClassMap<WriteoffDocument>
	{
		public WriteoffDocumentMap ()
		{
			Table ("store_writeoff_document");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Comment).Column ("comment");
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.ResponsibleEmployee).Column ("responsible_employee_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.Client).Column ("counterparty_id");
			References (x => x.WriteoffWarehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("writeoff_document_id");
		}
	}
}
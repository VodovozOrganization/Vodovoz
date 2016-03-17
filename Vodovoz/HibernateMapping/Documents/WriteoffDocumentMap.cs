using Vodovoz.Domain.Documents;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class WriteoffDocumentMap : ClassMap<WriteoffDocument>
	{
		public WriteoffDocumentMap ()
		{
			Table ("writeoff_document");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Comment).Column ("comment");
			Map (x => x.TimeStamp).Column ("time_stamp");
			References (x => x.ResponsibleEmployee).Column ("responsible_employee_id");
			References (x => x.DeliveryPoint).Column ("delivery_point_id");
			References (x => x.Client).Column ("counterparty_id");
			References (x => x.WriteoffWarehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("writeoff_document_id");
		}
	}
}
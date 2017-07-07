using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class IncomingInvoiceMap : ClassMap<IncomingInvoice>
	{
		public IncomingInvoiceMap ()
		{
			Table ("store_incoming_invoice");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.TimeStamp).Column ("time_stamp");
			Map (x => x.InvoiceNumber).Column ("invoice_number");
			Map (x => x.WaybillNumber).Column ("waybill_number");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map (x => x.Comment).Column ("comment");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.Contractor).Column ("counterparty_id");
			References (x => x.Warehouse).Column ("warehouse_id");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("incoming_invoice_id");
		}
	}
}
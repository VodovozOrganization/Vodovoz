using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.IncomingInvoices
{
	public class IncomingInvoiceMap : ClassMap<IncomingInvoice>
	{
		public IncomingInvoiceMap()
		{
			Table("store_incoming_invoice");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TimeStamp).Column("time_stamp");
			Map(x => x.InvoiceNumber).Column("invoice_number");
			Map(x => x.WaybillNumber).Column("waybill_number");
			Map(x => x.LastEditedTime).Column("last_edit_time");
			Map(x => x.Comment).Column("comment");
			Map(x => x.AuthorId).Column("author_id");
			Map(x => x.LastEditorId).Column("last_editor_id");

			References(x => x.Contractor).Column("counterparty_id");
			References(x => x.Warehouse).Column("warehouse_id");

			HasMany(x => x.Items).Cascade.AllDeleteOrphan().Inverse().KeyColumn("incoming_invoice_id");
		}
	}
}

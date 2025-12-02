using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.IncomingInvoices;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.IncomingInvoices
{
	public class IncomingInvoiceItemMap : ClassMap<IncomingInvoiceItem>
	{
		public IncomingInvoiceItemMap()
		{
			Table("store_incoming_invoice_items");
			DiscriminateSubClassesOnColumn("accounting_type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.PrimeCost).Column("price");

			References(x => x.Document).Column("incoming_invoice_id").Not.Nullable();
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			References(x => x.GoodsAccountingOperation).Column("good_move_operation_id").Not.Nullable().Cascade.All();
			References(x => x.VatRate).Column("vat_rate_id").Not.Nullable();
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelIncomeInvoiceMap : ClassMap<FuelIncomeInvoice>
	{
		public FuelIncomeInvoiceMap()
		{
			Table("fuel_income_invoices");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.СreationTime).Column("creation_time");
			Map(x => x.InvoiceDoc).Column("invoice_doc");
			Map(x => x.InvoiceBillDoc).Column("invoice_bill_doc");
			Map(x => x.Comment).Column("comment");

			References(x => x.Author).Column("author_id");
			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.Subdivision).Column("subdivision_id");

			HasMany(x => x.FuelIncomeInvoiceItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("fuel_income_invoice_id");
		}
	}
}

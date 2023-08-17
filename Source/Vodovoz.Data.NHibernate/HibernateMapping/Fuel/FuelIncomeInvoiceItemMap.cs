using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.HibernateMapping.Fuel
{
	public class FuelIncomeInvoiceItemMap : ClassMap<FuelIncomeInvoiceItem>
	{
		public FuelIncomeInvoiceItemMap()
		{
			Table("fuel_income_invoice_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Liters).Column("liters");
			Map(x => x.Price).Column("price");

			References(x => x.FuelIncomeInvoice).Column("fuel_income_invoice_id");
			References(x => x.FuelIncomeOperation).Column("fuel_income_operation_id").Cascade.All();
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}

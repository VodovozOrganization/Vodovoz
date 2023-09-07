using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class FuelIncomeOperationMap : ClassMap<FuelIncomeOperation>
	{
		public FuelIncomeOperationMap()
		{
			Table("fuel_income_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.СreationTime).Column("creation_time");
			Map(x => x.FuelLiters).Column("fuel_liters");

			References(x => x.FuelTransferDocument).Column("fuel_transfer_document_id");
			References(x => x.FuelIncomeInvoiceItem).Column("fuel_income_invoice_item_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.RelatedToSubdivision).Column("related_to_subdivision_id");
		}
	}
}

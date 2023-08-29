using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelWriteoffDocumentItemMap : ClassMap<FuelWriteoffDocumentItem>
	{
		public FuelWriteoffDocumentItemMap()
		{
			Table("fuel_writeoff_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Liters).Column("liters");


			References(x => x.FuelWriteoffDocument).Column("fuel_writeoff_document_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.FuelExpenseOperation).Column("fuel_expense_operation_id").Cascade.All();
		}
	}
}

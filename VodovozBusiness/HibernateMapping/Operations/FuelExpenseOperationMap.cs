using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.HibernateMapping.Operations
{
	public class FuelExpenseOperationMap : ClassMap<FuelExpenseOperation>
	{
		public FuelExpenseOperationMap()
		{
			Table("fuel_expense_operations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.СreationTime).Column("creation_time");
			Map(x => x.FuelLiters).Column("fuel_liters");

			References(x => x.FuelWriteoffDocumentItem).Column("fuel_writeoff_document_item_id");
			References(x => x.FuelTransferDocument).Column("fuel_transfer_document_id");
			References(x => x.FuelDocument).Column("fuel_document_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.RelatedToSubdivision).Column("related_to_subdivision_id");
		}
	}
}

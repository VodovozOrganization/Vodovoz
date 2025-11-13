using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class DriverDiscrepancyDocumentItemMap : ClassMap<DriverDiscrepancyDocumentItem>
	{
		public DriverDiscrepancyDocumentItemMap()
		{
			Table("driver_discrepancy_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.DiscrepancyReason).Column("discrepancy_reason");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Document).Column("driver_discrepancy_document_id");
			References(x => x.EmployeeNomenclatureMovementOperation).Cascade.All().Column("employee_nomenclature_movement_operation_id");
		}
	}
}

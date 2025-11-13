using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class DeliveryDocumentItemMap : ClassMap<DeliveryDocumentItem>
	{
		public DeliveryDocumentItemMap()
		{
			Table("delivery_document_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Amount).Column("amount");
			Map(x => x.Direction).Column("direction");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.Document).Column("delivery_document_id");
			References(x => x.EmployeeNomenclatureMovementOperation).Cascade.All().Column("employee_nomenclature_movement_operation_id");
		}
	}
}

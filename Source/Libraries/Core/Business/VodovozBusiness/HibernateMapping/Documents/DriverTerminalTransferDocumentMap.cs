using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class DriverTerminalTransferDocumentMap : ClassMap<DriverTerminalTransferDocument>
	{
		public DriverTerminalTransferDocumentMap()
		{
			Table("driver_terminal_transfer_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate).Column("create_date");

			References(x => x.Author).Column("author_id");

			References(x => x.RouteListFrom).Column("routelist_from_id");
			References(x => x.RouteListTo).Column("routelist_to_id");

			References(x => x.DriverFrom).Column("driver_from_id");
			References(x => x.DriverTo).Column("driver_to_id");

			References(x => x.EmployeeNomenclatureMovementOperationFrom)
				.Cascade.All().Column("employee_nomenclature_movement_operation_from_id");
			References(x => x.EmployeeNomenclatureMovementOperationTo)
				.Cascade.All().Column("employee_nomenclature_movement_operation_to_id");
		}
	}
}

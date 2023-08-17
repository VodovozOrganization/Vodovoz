using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.DriverTerminalTransfer;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class DriverTerminalTransferDocumentBaseMap : ClassMap<DriverTerminalTransferDocumentBase>
	{
		public DriverTerminalTransferDocumentBaseMap()
		{
			Table("driver_terminal_transfer_documents");

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CreateDate).Column("create_date");

			References(x => x.Author).Column("author_id");

			References(x => x.RouteListFrom).Column("routelist_from_id");
			References(x => x.RouteListTo).Column("routelist_to_id");

			References(x => x.DriverFrom).Column("driver_from_id");
			References(x => x.DriverTo).Column("driver_to_id");
		}

		public class DriverTerminalTransferDocumentMap : SubclassMap<AnotherDriverTerminalTransferDocument>
		{
			public DriverTerminalTransferDocumentMap()
			{
				DiscriminatorValue(DriverTerminalTransferDocumentType.AnotherDriver.ToString());

				References(x => x.EmployeeNomenclatureMovementOperationFrom)
					.Cascade.All().Column("employee_nomenclature_movement_operation_from_id");

				References(x => x.EmployeeNomenclatureMovementOperationTo)
					.Cascade.All().Column("employee_nomenclature_movement_operation_to_id");
			}
		}

		public class SelfDriverTerminalTransferDocumentMap : SubclassMap<SelfDriverTerminalTransferDocument>
		{
			public SelfDriverTerminalTransferDocumentMap()
			{
				DiscriminatorValue(DriverTerminalTransferDocumentType.SelfDriver.ToString());
			}
		}
	}
}

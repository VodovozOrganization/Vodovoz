using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.DriverTerminal;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.DriverTerminal
{
	public class DriverAttachedTerminalDocumentsMap : ClassMap<DriverAttachedTerminalDocumentBase>
	{
		public DriverAttachedTerminalDocumentsMap()
		{
			Table("driver_attached_terminal_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("type");

			Map(x => x.CreationDate).Column("creation_date");
			References(x => x.Author).Column("author_id").Cascade.All();
			References(x => x.Driver).Column("driver_id").Cascade.All();
			References(x => x.GoodsAccountingOperation).Cascade.All()
				.Column("warehouse_movement_operation_id");
			References(x => x.EmployeeNomenclatureMovementOperation).Cascade.All()
				.Column("employee_nomenclature_movement_operation_id");
		}
	}

	public class DriverAttachedTerminalReturnDocumentMap : SubclassMap<DriverAttachedTerminalReturnDocument>
	{
		public DriverAttachedTerminalReturnDocumentMap()
		{
			DiscriminatorValue(AttachedTerminalDocumentType.Return.ToString());
		}
	}

	public class DriverAttachedTerminalGiveoutDocumentMap : SubclassMap<DriverAttachedTerminalGiveoutDocument>
	{
		public DriverAttachedTerminalGiveoutDocumentMap()
		{
			DiscriminatorValue(AttachedTerminalDocumentType.Giveout.ToString());
		}
	}
}

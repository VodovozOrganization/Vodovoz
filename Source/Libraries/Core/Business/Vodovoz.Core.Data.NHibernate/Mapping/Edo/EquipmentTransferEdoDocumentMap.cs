using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EquipmentTransferEdoDocumentMap : SubclassMap<EquipmentTransferEdoDocument>
	{
		public EquipmentTransferEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.EquipmentTransfer));

			Map(x => x.DocumentTaskId)
				.Column("document_task_id");
		}
	}
}


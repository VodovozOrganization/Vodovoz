using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EquipmentTransferEdoRequestMap : SubclassMap<EquipmentTransferEdoRequest>
	{
		public EquipmentTransferEdoRequestMap()
		{
			DiscriminatorValue(nameof(CustomerEdoRequestType.EquipmentTransfer));

			Map(x => x.EquipmentTransferId)
				.Column("equipment_transfer_id");
		}
	}
}

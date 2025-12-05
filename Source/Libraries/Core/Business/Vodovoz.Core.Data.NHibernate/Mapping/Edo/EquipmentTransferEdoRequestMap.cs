using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EquipmentTransferEdoRequestMap : SubclassMap<EquipmentTransferEdoRequest>
	{
		public EquipmentTransferEdoRequestMap()
		{
			DiscriminatorValue(nameof(OrderDocumentType.EquipmentTransfer));
		}
	}
}

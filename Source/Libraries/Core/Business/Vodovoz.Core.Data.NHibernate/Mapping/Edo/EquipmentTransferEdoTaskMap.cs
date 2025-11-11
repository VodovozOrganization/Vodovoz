using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EquipmentTransferEdoTaskMap : SubclassMap<EquipmentTransferEdoTask>
	{
		public EquipmentTransferEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.EquipmentTransfer));

			Extends(typeof(OrderEdoTask));
		}
	}
}

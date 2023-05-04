using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromWarehouseToWarehouseItemMap : SubclassMap<InstanceMovementDocumentFromWarehouseToWarehouseItem>
	{
		public InstanceMovementDocumentFromWarehouseToWarehouseItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromWarehouseToWarehouseItem));
		}
	}
}

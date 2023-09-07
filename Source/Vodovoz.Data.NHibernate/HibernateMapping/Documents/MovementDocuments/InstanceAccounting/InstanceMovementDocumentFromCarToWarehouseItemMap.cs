using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromCarToWarehouseItemMap : SubclassMap<InstanceMovementDocumentFromCarToWarehouseItem>
	{
		public InstanceMovementDocumentFromCarToWarehouseItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromCarToWarehouseItem));
		}
	}
}

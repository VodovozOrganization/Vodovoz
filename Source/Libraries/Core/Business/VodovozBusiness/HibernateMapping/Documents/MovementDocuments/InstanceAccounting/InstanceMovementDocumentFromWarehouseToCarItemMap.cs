using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromWarehouseToCarItemMap : SubclassMap<InstanceMovementDocumentFromWarehouseToCarItem>
	{
		public InstanceMovementDocumentFromWarehouseToCarItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromWarehouseToCarItem));
		}
	}
}

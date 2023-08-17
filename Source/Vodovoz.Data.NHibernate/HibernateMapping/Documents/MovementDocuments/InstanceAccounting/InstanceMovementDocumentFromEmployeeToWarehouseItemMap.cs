using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromEmployeeToWarehouseItemMap : SubclassMap<InstanceMovementDocumentFromEmployeeToWarehouseItem>
	{
		public InstanceMovementDocumentFromEmployeeToWarehouseItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromEmployeeToWarehouseItem));
		}
	}
}

using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromWarehouseToEmployeeItemMap : SubclassMap<InstanceMovementDocumentFromWarehouseToEmployeeItem>
	{
		public InstanceMovementDocumentFromWarehouseToEmployeeItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromWarehouseToEmployeeItem));
		}
	}
}

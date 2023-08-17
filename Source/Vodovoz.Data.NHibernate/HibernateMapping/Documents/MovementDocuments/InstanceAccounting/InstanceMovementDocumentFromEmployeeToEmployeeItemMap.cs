using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentFromEmployeeToEmployeeItemMap : SubclassMap<InstanceMovementDocumentFromEmployeeToEmployeeItem>
	{
		public InstanceMovementDocumentFromEmployeeToEmployeeItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.InstanceMovementDocumentFromEmployeeToEmployeeItem));
		}
	}
}

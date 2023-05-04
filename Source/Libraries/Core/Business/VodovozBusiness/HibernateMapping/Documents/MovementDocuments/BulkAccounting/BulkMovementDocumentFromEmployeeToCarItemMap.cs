using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromEmployeeToCarItemMap : SubclassMap<BulkMovementDocumentFromEmployeeToCarItem>
	{
		public BulkMovementDocumentFromEmployeeToCarItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromEmployeeToCarItem));
		}
	}
}

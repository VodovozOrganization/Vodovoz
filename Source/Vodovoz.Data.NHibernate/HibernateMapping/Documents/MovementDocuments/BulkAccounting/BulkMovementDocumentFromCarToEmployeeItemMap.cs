﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromCarToEmployeeItemMap : SubclassMap<BulkMovementDocumentFromCarToEmployeeItem>
	{
		public BulkMovementDocumentFromCarToEmployeeItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromCarToEmployeeItem));
		}
	}
}

using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class DocumentTwoWarehousesBoundedIdsSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : ITwoWarhousesBindedDocument
	{
		private readonly IEnumerable<int> _warehouseIds;

		public DocumentTwoWarehousesBoundedIdsSpecification(IEnumerable<int> warehouseIds)
		{
			_warehouseIds = warehouseIds;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => !_warehouseIds.Any() || _warehouseIds.Contains(doc.FromWarehouse.Id) || _warehouseIds.Contains(doc.ToWarehouse.Id);
		}
	}
}

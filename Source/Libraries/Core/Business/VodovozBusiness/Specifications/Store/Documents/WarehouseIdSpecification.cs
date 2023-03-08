using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class WarehouseIdSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : Document
	{
		private readonly int? _warehouseId;

		public WarehouseIdSpecification(int? warehouseId)
		{
			_warehouseId = warehouseId;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => _warehouseId == null || doc.Id == _warehouseId;
		}
	}
}

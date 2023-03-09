using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;

namespace Vodovoz.Specifications.Store.Documents
{
	public class DocumentTwoWarehousesBoundedIdSpecification<TDocument> : ISpecification<TDocument>
		where TDocument : ITwoWarhousesBindedDocument
	{
		private readonly int? _warehouseId;

		public DocumentTwoWarehousesBoundedIdSpecification(int? warehouseId)
		{
			_warehouseId = warehouseId;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => _warehouseId == null || doc.FromWarehouse.Id == _warehouseId || doc.ToWarehouse.Id == _warehouseId;
		}
	}
}

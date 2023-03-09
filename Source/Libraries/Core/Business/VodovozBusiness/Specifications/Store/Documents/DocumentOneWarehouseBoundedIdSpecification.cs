using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz.Specifications.Store.Documents
{
	public class DocumentOneWarehouseBoundedIdSpecification<TDocument> : ISpecification<TDocument>
	where TDocument : IWarehouseBoundedDocument
	{
		private readonly int? _warehouseId;

		public DocumentOneWarehouseBoundedIdSpecification(int? warehouseId)
		{
			_warehouseId = warehouseId;
		}

		public Expression<Func<TDocument, bool>> IsSatisfiedBy()
		{
			return (doc) => _warehouseId == null || doc.Warehouse.Id == _warehouseId;
		}
	}
}

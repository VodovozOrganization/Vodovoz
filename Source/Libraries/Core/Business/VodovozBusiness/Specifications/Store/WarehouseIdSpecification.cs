using System;
using System.Linq.Expressions;
using Vodovoz.Domain.Store;

namespace Vodovoz.Specifications.Store
{
	public class WarehouseIdSpecification : ISpecification<Warehouse>
	{
		private readonly int? _warehouseId;

		public WarehouseIdSpecification(int? warehouseId)
		{
			_warehouseId = warehouseId;
		}

		public Expression<Func<Warehouse, bool>> IsSatisfiedBy()
		{
			return (warehouse) => _warehouseId == null || warehouse.Id == _warehouseId;
		}
	}
}

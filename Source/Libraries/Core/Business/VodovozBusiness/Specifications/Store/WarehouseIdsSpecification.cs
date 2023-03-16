using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Domain.Store;

namespace Vodovoz.Specifications.Store
{
	public class WarehouseIdsSpecification : ISpecification<Warehouse>
	{
		private readonly IEnumerable<int> _warehouseIds;

		public WarehouseIdsSpecification(IEnumerable<int> warehouseId)
		{
			_warehouseIds = warehouseId;
		}

		public Expression<Func<Warehouse, bool>> IsSatisfiedBy()
		{
			return (warehouse) => !_warehouseIds.Any() || _warehouseIds.Contains(warehouse.Id);
		}
	}
}

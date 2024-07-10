using NHibernate.Criterion;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Infrastructure.Persistance.Operations
{
	internal sealed class GoodsAccountingOperationRepository
	{
		public static ICriterion GetGoodsAccountingOperationCriterionByStorage(OperationType operationType, int storageId)
		{
			switch(operationType)
			{
				case OperationType.WarehouseInstanceGoodsAccountingOperation:
					return Restrictions.Eq(nameof(WarehouseInstanceGoodsAccountingOperation.Warehouse.Id), storageId);
				case OperationType.EmployeeInstanceGoodsAccountingOperation:
					return Restrictions.Eq(nameof(EmployeeInstanceGoodsAccountingOperation.Employee.Id), storageId);
				case OperationType.CarInstanceGoodsAccountingOperation:
					return Restrictions.Eq(nameof(CarInstanceGoodsAccountingOperation.Car.Id), storageId);
				case OperationType.EmployeeBulkGoodsAccountingOperation:
					return Restrictions.Eq(nameof(EmployeeBulkGoodsAccountingOperation.Employee.Id), storageId);
				case OperationType.CarBulkGoodsAccountingOperation:
					return Restrictions.Eq(nameof(CarBulkGoodsAccountingOperation.Car.Id), storageId);
				default:
					return Restrictions.Eq(nameof(WarehouseBulkGoodsAccountingOperation.Warehouse.Id), storageId);
			}
		}
	}
}

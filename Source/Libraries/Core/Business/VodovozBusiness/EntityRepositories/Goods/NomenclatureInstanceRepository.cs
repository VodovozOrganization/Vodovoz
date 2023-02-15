using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Goods
{
	public class NomenclatureInstanceRepository : INomenclatureInstanceRepository
	{
		public InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id)
			=> uow.GetById<InventoryNomenclatureInstance>(id);

		public IList<NomenclatureInstanceBalanceNode> GetInventoryInstancesByStorage(
			IUnitOfWork uow, OperationTypeByStorage operationTypeByStorage, int storageId)
		{
			InstanceGoodsAccountingOperation instanceGoodsAccountingOperationAlias = null;
			InventoryNomenclatureInstance inventoryNomenclatureInstanceAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclatureInstanceBalanceNode resultAlias = null;

			var query = uow.Session
				.QueryOver(() => instanceGoodsAccountingOperationAlias)
				.JoinAlias(
					() => instanceGoodsAccountingOperationAlias.InventoryNomenclatureInstance,
					() => inventoryNomenclatureInstanceAlias)
				.JoinAlias(() => inventoryNomenclatureInstanceAlias.Nomenclature, () => nomenclatureAlias)
				.Where(GetCriterionByStorage(operationTypeByStorage, storageId))
				.SelectList(list => list
					.SelectGroup(() => inventoryNomenclatureInstanceAlias.Id).WithAlias(() => resultAlias.InstanceId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.InstanceName)
					.Select(() => inventoryNomenclatureInstanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(Projections.Entity(() => inventoryNomenclatureInstanceAlias)).WithAlias(() => resultAlias.InventoryNomenclatureInstance)
					.SelectSum(() => instanceGoodsAccountingOperationAlias.Amount).WithAlias(() => resultAlias.Balance))
				.Where(Restrictions.Gt(Projections.Sum<WarehouseInstanceGoodsAccountingOperation>(w => w.Amount), 0))
				.TransformUsing(Transformers.AliasToBean<NomenclatureInstanceBalanceNode>())
				.List<NomenclatureInstanceBalanceNode>();

			return query;
		}
		
		public IList<FindingInfoInventoryInstanceNode> GetFindingInfoInventoryInstance(IUnitOfWork uow, int instanceId)
		{
			var resultList = GetFindingInfoInventoryInstanceOnWarehouses(uow, instanceId);

			foreach(var item in GetFindingInfoInventoryInstanceOnEmployees(uow, instanceId))
			{
				resultList.Add(item);
			}

			foreach(var item in GetFindingInfoInventoryInstanceOnCars(uow, instanceId))
			{
				resultList.Add(item);
			}
			
			return resultList;
		}

		public IList<FindingInfoInventoryInstanceNode> GetFindingInfoInventoryInstanceOnWarehouses(IUnitOfWork uow, int instanceId)
		{
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceGoodsAccountingOperationAlias = null;
			InventoryNomenclatureInstance inventoryNomenclatureInstanceAlias = null;
			Warehouse warehouseAlias = null;
			FindingInfoInventoryInstanceNode resultAlias = null;

			var query = uow.Session
				.QueryOver(() => warehouseInstanceGoodsAccountingOperationAlias)
				.JoinAlias(
					() => warehouseInstanceGoodsAccountingOperationAlias.InventoryNomenclatureInstance,
					() => inventoryNomenclatureInstanceAlias)
				.JoinAlias(() => warehouseInstanceGoodsAccountingOperationAlias.Warehouse, () => warehouseAlias)
				.Where(() => inventoryNomenclatureInstanceAlias.Id == instanceId)
				.SelectList(list => list
					.SelectGroup(() => inventoryNomenclatureInstanceAlias.Id).WithAlias(() => resultAlias.InstanceId)
					.SelectGroup(() => warehouseAlias.Id).WithAlias(() => resultAlias.StorageId)
					.Select(() => warehouseAlias.Name).WithAlias(() => resultAlias.StorageName)
					.Select(() => typeof(Warehouse)).WithAlias(() => resultAlias.StorageType)
					.SelectSum(() => warehouseInstanceGoodsAccountingOperationAlias.Amount).WithAlias(() => resultAlias.Balance))
				.Where(Restrictions.Gt(Projections.Sum<WarehouseInstanceGoodsAccountingOperation>(w => w.Amount), 0))
				.TransformUsing(Transformers.AliasToBean<FindingInfoInventoryInstanceNode>())
				.List<FindingInfoInventoryInstanceNode>();

			return query;
		}
		
		public IList<FindingInfoInventoryInstanceNode> GetFindingInfoInventoryInstanceOnEmployees(IUnitOfWork uow, int instanceId)
		{
			EmployeeInstanceGoodsAccountingOperation employeeInstanceGoodsAccountingOperationAlias = null;
			InventoryNomenclatureInstance inventoryNomenclatureInstanceAlias = null;
			Employee employeeAlias = null;
			FindingInfoInventoryInstanceNode resultAlias = null;

			var query = uow.Session
				.QueryOver(() => employeeInstanceGoodsAccountingOperationAlias)
				.JoinAlias(
					() => employeeInstanceGoodsAccountingOperationAlias.InventoryNomenclatureInstance,
					() => inventoryNomenclatureInstanceAlias)
				.JoinAlias(() => employeeInstanceGoodsAccountingOperationAlias.Employee, () => employeeAlias)
				.Where(() => inventoryNomenclatureInstanceAlias.Id == instanceId)
				.SelectList(list => list
					.SelectGroup(() => inventoryNomenclatureInstanceAlias.Id).WithAlias(() => resultAlias.InstanceId)
					.SelectGroup(() => employeeAlias.Id).WithAlias(() => resultAlias.StorageId)
					.Select(() => employeeAlias.Name).WithAlias(() => resultAlias.StorageName)
					.Select(() => typeof(Employee)).WithAlias(() => resultAlias.StorageType)
					.SelectSum(() => employeeInstanceGoodsAccountingOperationAlias.Amount).WithAlias(() => resultAlias.Balance))
				.Where(Restrictions.Gt(Projections.Sum<EmployeeInstanceGoodsAccountingOperation>(w => w.Amount), 0))
				.TransformUsing(Transformers.AliasToBean<FindingInfoInventoryInstanceNode>())
				.List<FindingInfoInventoryInstanceNode>();

			return query;
		}
		
		public IList<FindingInfoInventoryInstanceNode> GetFindingInfoInventoryInstanceOnCars(IUnitOfWork uow, int instanceId)
		{
			CarInstanceGoodsAccountingOperation carInstanceGoodsAccountingOperationAlias = null;
			InventoryNomenclatureInstance inventoryNomenclatureInstanceAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;
			FindingInfoInventoryInstanceNode resultAlias = null;

			var query = uow.Session
				.QueryOver(() => carInstanceGoodsAccountingOperationAlias)
				.JoinAlias(
					() => carInstanceGoodsAccountingOperationAlias.InventoryNomenclatureInstance,
					() => inventoryNomenclatureInstanceAlias)
				.JoinAlias(() => carInstanceGoodsAccountingOperationAlias.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Where(() => inventoryNomenclatureInstanceAlias.Id == instanceId)
				.SelectList(list => list
					.SelectGroup(() => inventoryNomenclatureInstanceAlias.Id).WithAlias(() => resultAlias.InstanceId)
					.SelectGroup(() => carAlias.Id).WithAlias(() => resultAlias.StorageId)
					.Select(() => carModelAlias.Name).WithAlias(() => resultAlias.StorageName)
					.Select(() => typeof(Car)).WithAlias(() => resultAlias.StorageType)
					.SelectSum(() => carInstanceGoodsAccountingOperationAlias.Amount).WithAlias(() => resultAlias.Balance))
				.Where(Restrictions.Gt(Projections.Sum<CarInstanceGoodsAccountingOperation>(w => w.Amount), 0))
				.TransformUsing(Transformers.AliasToBean<FindingInfoInventoryInstanceNode>())
				.List<FindingInfoInventoryInstanceNode>();

			return query;
		}

		private ICriterion GetCriterionByStorage(OperationTypeByStorage operationTypeByStorage, int storageId)
		{
			switch(operationTypeByStorage)
			{
				case OperationTypeByStorage.Warehouse:
					return Restrictions.Where<WarehouseInstanceGoodsAccountingOperation>(o => o.Warehouse.Id == storageId);
				case OperationTypeByStorage.Employee:
					return Restrictions.Where<EmployeeInstanceGoodsAccountingOperation>(o => o.Employee.Id == storageId);
				case OperationTypeByStorage.Car:
					return Restrictions.Where<CarInstanceGoodsAccountingOperation>(o => o.Car.Id == storageId);
				default:
					throw new ArgumentOutOfRangeException(nameof(operationTypeByStorage), operationTypeByStorage, null);
			}
		}

		public class NomenclatureInstanceBalanceNode
		{
			public int InstanceId { get; set; }
			public string InstanceName { get; set; }
			public string InventoryNumber { get; set; }
			public InventoryNomenclatureInstance InventoryNomenclatureInstance { get; set; }
			public decimal Balance { get; set; }
		}
		
		public class FindingInfoInventoryInstanceNode
		{
			public int InstanceId { get; set; }
			public int StorageId { get; set; }
			public string StorageType { get; set; }
			public string StorageName { get; set; }
			public decimal Balance { get; set; }
		}
	}
}

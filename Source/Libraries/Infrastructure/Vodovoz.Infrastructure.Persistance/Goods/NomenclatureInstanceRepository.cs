using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Operations;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.Infrastructure.Persistance.Goods
{
	internal sealed class NomenclatureInstanceRepository : INomenclatureInstanceRepository
	{
		public InventoryNomenclatureInstance GetInventoryNomenclatureInstance(IUnitOfWork uow, int id)
			=> uow.GetById<InventoryNomenclatureInstance>(id);

		public IList<NomenclatureInstanceBalanceNode> GetInventoryInstancesByStorage(
			IUnitOfWork uow,
			StorageType storageType,
			int storageId,
			IEnumerable<int> instancesToInclude,
			IEnumerable<int> instancesToExclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToInclude,
			IEnumerable<NomenclatureCategory> nomenclatureTypeToExclude,
			IEnumerable<int> productGroupToInclude,
			IEnumerable<int> productGroupToExclude,
			DateTime? onDate = null)
		{
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			Nomenclature nomenclatureAlias = null;
			NomenclatureInstanceBalanceNode resultAlias = null;

			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(() => instanceAlias.Nomenclature, () => nomenclatureAlias);

			IProjection balanceProjection = null;
			IProjection operationTimeProjection = null;

			switch(storageType)
			{
				case StorageType.Employee:
					query.JoinEntityAlias(() => employeeInstanceOperationAlias,
						() => instanceAlias.Id == employeeInstanceOperationAlias.InventoryNomenclatureInstance.Id
							&& employeeInstanceOperationAlias.Employee.Id == storageId);

					balanceProjection = Projections.Sum(() => employeeInstanceOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => employeeInstanceOperationAlias.OperationTime);
					break;
				case StorageType.Car:
					query.JoinEntityAlias(() => carInstanceOperationAlias,
						() => instanceAlias.Id == carInstanceOperationAlias.InventoryNomenclatureInstance.Id
							&& carInstanceOperationAlias.Car.Id == storageId);

					balanceProjection = Projections.Sum(() => carInstanceOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => carInstanceOperationAlias.OperationTime);
					break;
				default:
					query.JoinEntityAlias(() => warehouseInstanceOperationAlias,
						() => instanceAlias.Id == warehouseInstanceOperationAlias.InventoryNomenclatureInstance.Id
							&& warehouseInstanceOperationAlias.Warehouse.Id == storageId);

					balanceProjection = Projections.Sum(() => warehouseInstanceOperationAlias.Amount);
					operationTimeProjection = Projections.Property(() => warehouseInstanceOperationAlias.OperationTime);
					break;
			}

			if(instancesToInclude != null && instancesToInclude.Any())
			{
				query.AndRestrictionOn(() => instanceAlias.Id).IsInG(instancesToInclude);
			}

			if(instancesToExclude != null && instancesToExclude.Any())
			{
				query.AndRestrictionOn(() => instanceAlias.Id).Not.IsInG(instancesToExclude);
			}

			if(nomenclatureTypeToInclude != null && nomenclatureTypeToInclude.Any())
			{
				query.AndRestrictionOn(() => nomenclatureAlias.Category).IsInG(nomenclatureTypeToInclude);
			}

			if(nomenclatureTypeToExclude != null && nomenclatureTypeToExclude.Any())
			{
				query.AndRestrictionOn(() => nomenclatureAlias.Category).Not.IsInG(nomenclatureTypeToExclude);
			}

			if(productGroupToInclude != null && productGroupToInclude.Any())
			{
				query.AndRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).IsInG(productGroupToInclude);
			}

			if(productGroupToExclude != null && productGroupToExclude.Any())
			{
				query.AndRestrictionOn(() => nomenclatureAlias.ProductGroup.Id).Not.IsInG(productGroupToExclude);
			}

			if(onDate.HasValue)
			{
				query.Where(Restrictions.Lt(operationTimeProjection, onDate.Value));
			}

			var result = query.SelectList(list => list
					.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.InstanceId)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.InstanceName)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(Projections.Entity(() => instanceAlias))
					.WithAlias(() => resultAlias.InventoryNomenclatureInstance)
					.Select(balanceProjection).WithAlias(() => resultAlias.Balance))
				.Where(Restrictions.Gt(balanceProjection, 0))
				.TransformUsing(Transformers.AliasToBean<NomenclatureInstanceBalanceNode>())
				.List<NomenclatureInstanceBalanceNode>();

			return result;
		}

		public decimal GetNomenclatureInstanceBalance(IUnitOfWork uow, int instanceId)
		{
			InstanceGoodsAccountingOperation operationAlias = null;

			return uow.Session.QueryOver(() => operationAlias)
				.Where(igao => igao.InventoryNomenclatureInstance.Id == instanceId)
				.Select(Projections.Sum(() => operationAlias.Amount))
				.SingleOrDefault<decimal>();
		}

		public IList<InstanceOnStorageData> GetOtherInstancesOnStorageBalance(
			IUnitOfWork uow, StorageType storageType, int storageId, int[] instanceIds, DateTime? date = null)
		{
			InventoryNomenclatureInstance instanceAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseInstanceGoodsAccountingOperation warehouseOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeOperationAlias = null;
			CarInstanceGoodsAccountingOperation carOperationAlias = null;
			InstanceOnStorageData resultAlias = null;

			var query = uow.Session.QueryOver(() => instanceAlias)
				.JoinAlias(() => instanceAlias.Nomenclature, () => nomenclatureAlias);

			IProjection balanceProjection = null;
			switch(storageType)
			{
				case StorageType.Employee:
					query.JoinEntityAlias(
						() => employeeOperationAlias,
						() => instanceAlias.Id == employeeOperationAlias.InventoryNomenclatureInstance.Id
							&& employeeOperationAlias.Employee.Id == storageId);

					if(date.HasValue)
					{
						query.Where(() => employeeOperationAlias.OperationTime <= date);
					}

					balanceProjection = Projections.Sum(() => employeeOperationAlias.Amount);
					break;
				case StorageType.Car:
					query.JoinEntityAlias(
						() => carOperationAlias,
						() => instanceAlias.Id == carOperationAlias.InventoryNomenclatureInstance.Id
							&& carOperationAlias.Car.Id == storageId);

					if(date.HasValue)
					{
						query.Where(() => carOperationAlias.OperationTime <= date);
					}

					balanceProjection = Projections.Sum(() => carOperationAlias.Amount);
					break;
				default:
					query.JoinEntityAlias(
						() => warehouseOperationAlias,
						() => instanceAlias.Id == warehouseOperationAlias.InventoryNomenclatureInstance.Id
							&& warehouseOperationAlias.Warehouse.Id == storageId);

					if(date.HasValue)
					{
						query.Where(() => warehouseOperationAlias.OperationTime <= date);
					}

					balanceProjection = Projections.Sum(() => warehouseOperationAlias.Amount);
					break;
			}

			return query.Where(Restrictions.Gt(balanceProjection, 0))
				.AndRestrictionOn(() => instanceAlias.Id).Not.IsIn(instanceIds)
				.SelectList(list => list
					.SelectGroup(i => i.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(() => instanceAlias.IsUsed).WithAlias(() => resultAlias.IsUsed)
				)
				.TransformUsing(Transformers.AliasToBean<InstanceOnStorageData>())
				.List<InstanceOnStorageData>();
		}

		public ILookup<int, InstanceOnStorageData> GetCurrentInstancesOnOtherStorages(
			IUnitOfWork uow, StorageType storageType, int storageId, int[] instanceIds, DateTime? date = null)
		{
			WarehouseInstanceGoodsAccountingOperation warehouseInstanceOperationAlias = null;
			EmployeeInstanceGoodsAccountingOperation employeeInstanceOperationAlias = null;
			CarInstanceGoodsAccountingOperation carInstanceOperationAlias = null;
			InventoryNomenclatureInstance instanceAlias = null;
			Nomenclature nomenclatureAlias = null;
			Employee employeeStorageAlias = null;
			Warehouse warehouseAlias = null;
			Car carStorageAlias = null;
			CarModel carModelAlias = null;
			InstanceOnStorageData resultAlias = null;

			var warehouseProjection = CustomProjections.Concat(
				Projections.Constant("Склад: "),
				Projections.Property(() => warehouseAlias.Name));

			var employeeStorageProjection = CustomProjections.Concat(
				Projections.Constant("Сотрудник: "),
				Projections.Property(() => employeeStorageAlias.Id),
				Projections.Constant(" "),
				Projections.Property(() => employeeStorageAlias.LastName),
				Projections.Constant(" "),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => employeeStorageAlias.Name)),
				Projections.Constant("."),
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.String, "LEFT(?1, 1)"),
					NHibernateUtil.String,
					Projections.Property(() => employeeStorageAlias.Patronymic)),
				Projections.Constant(".")
			);

			var carStorageProjection = CustomProjections.Concat(
				Projections.Constant("Автомобиль: "),
				Projections.Property(() => carStorageAlias.Id),
				Projections.Constant(" "),
				Projections.Property(() => carModelAlias.Name),
				Projections.Constant(" "),
				Projections.Property(() => carStorageAlias.RegistrationNumber));

			var instanceBalanceByWarehousesQuery = uow.Session.QueryOver(() => warehouseInstanceOperationAlias)
				.JoinAlias(o => o.InventoryNomenclatureInstance, () => instanceAlias)
				.JoinAlias(() => instanceAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(o => o.Warehouse, () => warehouseAlias)
				.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instanceIds)
				.SelectList(list => list
					.SelectGroup(() => warehouseAlias.Id).WithAlias(() => resultAlias.StorageId)
					.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(() => instanceAlias.IsUsed).WithAlias(() => resultAlias.IsUsed)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(warehouseProjection).WithAlias(() => resultAlias.StorageName)
					.SelectSum(o => o.Amount).WithAlias(() => resultAlias.Balance)
				)
				.Where(Restrictions.Gt(Projections.Sum(() => warehouseInstanceOperationAlias.Amount), 0))
				.OrderBy(() => instanceAlias.Id).Asc
				.ThenBy(() => warehouseInstanceOperationAlias.Warehouse.Id).Asc
				.TransformUsing(Transformers.AliasToBean<InstanceOnStorageData>());

			var instanceBalanceByEmployeesQuery = uow.Session.QueryOver(() => employeeInstanceOperationAlias)
				.JoinAlias(o => o.InventoryNomenclatureInstance, () => instanceAlias)
				.JoinAlias(() => instanceAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(o => o.Employee, () => employeeStorageAlias)
				.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instanceIds)
				.SelectList(list => list
					.SelectGroup(() => employeeStorageAlias.Id).WithAlias(() => resultAlias.StorageId)
					.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(() => instanceAlias.IsUsed).WithAlias(() => resultAlias.IsUsed)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(employeeStorageProjection).WithAlias(() => resultAlias.StorageName)
					.SelectSum(o => o.Amount).WithAlias(() => resultAlias.Balance)
				)
				.Where(Restrictions.Gt(Projections.Sum(() => employeeInstanceOperationAlias.Amount), 0))
				.OrderBy(() => instanceAlias.Id).Asc
				.ThenBy(() => employeeInstanceOperationAlias.Employee.Id).Asc
				.TransformUsing(Transformers.AliasToBean<InstanceOnStorageData>());

			var instanceBalanceByCarsQuery = uow.Session.QueryOver(() => carInstanceOperationAlias)
				.JoinAlias(o => o.InventoryNomenclatureInstance, () => instanceAlias)
				.JoinAlias(() => instanceAlias.Nomenclature, () => nomenclatureAlias)
				.JoinAlias(o => o.Car, () => carStorageAlias)
				.JoinAlias(() => carStorageAlias.CarModel, () => carModelAlias)
				.WhereRestrictionOn(() => instanceAlias.Id).IsIn(instanceIds)
				.SelectList(list => list
					.SelectGroup(() => carStorageAlias.Id).WithAlias(() => resultAlias.StorageId)
					.SelectGroup(() => instanceAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => instanceAlias.InventoryNumber).WithAlias(() => resultAlias.InventoryNumber)
					.Select(() => instanceAlias.IsUsed).WithAlias(() => resultAlias.IsUsed)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(carStorageProjection).WithAlias(() => resultAlias.StorageName)
					.SelectSum(o => o.Amount).WithAlias(() => resultAlias.Balance)
				)
				.Where(Restrictions.Gt(Projections.Sum(() => carInstanceOperationAlias.Amount), 0))
				.OrderBy(() => instanceAlias.Id).Asc
				.ThenBy(() => carInstanceOperationAlias.Car.Id).Asc
				.TransformUsing(Transformers.AliasToBean<InstanceOnStorageData>());

			if(date.HasValue)
			{
				instanceBalanceByWarehousesQuery.Where(o => o.OperationTime <= date);
				instanceBalanceByEmployeesQuery.Where(o => o.OperationTime <= date);
				instanceBalanceByCarsQuery.Where(o => o.OperationTime <= date);
			}

			switch(storageType)
			{
				case StorageType.Warehouse:
					instanceBalanceByWarehousesQuery.Where(() => warehouseInstanceOperationAlias.Warehouse.Id != storageId);
					break;
				case StorageType.Employee:
					instanceBalanceByEmployeesQuery.Where(() => employeeInstanceOperationAlias.Employee.Id != storageId);
					break;
				case StorageType.Car:
					instanceBalanceByCarsQuery.Where(() => carInstanceOperationAlias.Car.Id != storageId);
					break;
			}

			var warehousesResult = instanceBalanceByWarehousesQuery.List<InstanceOnStorageData>();
			var employeesResult = instanceBalanceByEmployeesQuery.List<InstanceOnStorageData>();
			var carsResult = instanceBalanceByCarsQuery.List<InstanceOnStorageData>();

			var result =
				warehousesResult
					.Concat(employeesResult)
					.Concat(carsResult)
					.ToLookup(x => x.Id);

			return result;
		}
	}
}

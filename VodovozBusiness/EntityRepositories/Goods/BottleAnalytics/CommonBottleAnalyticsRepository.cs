using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB.EntityMappingConfig;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class CommonBottleAnalyticsRepository : ICommonBottleAnalyticsRepository
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEntityMappingConfigProvider _entityMappingConfigProvider;

		public CommonBottleAnalyticsRepository(IUnitOfWorkFactory unitOfWorkFactory,
			IEntityMappingConfigProvider entityMappingConfigProvider)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_entityMappingConfigProvider =
				entityMappingConfigProvider ?? throw new ArgumentNullException(nameof(entityMappingConfigProvider));
		}

		#region BottleAnalyticsNomenclatureIds

		public IList<int> GetBottleAnalyticsNomenclatureIds()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				return GetBottleAnalyticsNomenclatureIdsFuture(uow).ToList();
			}
		}

		public IFutureEnumerable<int> GetBottleAnalyticsNomenclatureIdsFuture(IUnitOfWork uow)
		{
			var nomenclatureConfig = _entityMappingConfigProvider.GetEntityMappingConfig<Nomenclature>();

			var nomenclature = nomenclatureConfig.TableName;
			var id = nomenclatureConfig.PropertyNames[nameof(Nomenclature.Id)];
			var isShabbyBottle = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsShabbyBottle)];
			var isDefectiveBottle = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDefectiveBottle)];
			var isArchive = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsArchive)];
			var isDiler = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDiler)];
			var isDisposableTare = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDisposableTare)];
			var category = nomenclatureConfig.PropertyNames[nameof(Nomenclature.Category)];
			var tareVolume = nomenclatureConfig.PropertyNames[nameof(Nomenclature.TareVolume)];

			var sql =
				$"SELECT n.{id} AS id " +
				$"FROM {nomenclature} n " +
				$"WHERE !n.{isShabbyBottle} " +
				$"    AND !n.{isDefectiveBottle} " +
				$"    AND !n.{isArchive} " +
				$"    AND !n.{isDiler} " +
				$"    AND !n.{isDisposableTare} " +
				$"    AND (n.{category} = :bottle OR (n.{category} = :water AND n.{tareVolume} = :Vol19L)) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("id", NHibernateUtil.Int32)
				.SetParameter("bottle", "bottle", NHibernateUtil.String)
				.SetParameter("water", "water", NHibernateUtil.String)
				.SetParameter("Vol19L", "Vol19L", NHibernateUtil.String)
				.Future<int>();
		}

		public IFutureEnumerable<NomenclatureNode> GetBottleAnalyticsNomenclaturesWithShabbyBottlesFuture(IUnitOfWork uow)
		{
			var nomenclatureConfig = _entityMappingConfigProvider.GetEntityMappingConfig<Nomenclature>();

			var nomenclature = nomenclatureConfig.TableName;
			var id = nomenclatureConfig.PropertyNames[nameof(Nomenclature.Id)];
			var isDefectiveBottle = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDefectiveBottle)];
			var isArchive = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsArchive)];
			var isDiler = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDiler)];
			var isDisposableTare = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsDisposableTare)];
			var isShabbyBottle = nomenclatureConfig.PropertyNames[nameof(Nomenclature.IsShabbyBottle)];
			var category = nomenclatureConfig.PropertyNames[nameof(Nomenclature.Category)];
			var tareVolume = nomenclatureConfig.PropertyNames[nameof(Nomenclature.TareVolume)];
			var name = nomenclatureConfig.PropertyNames[nameof(Nomenclature.Name)];
			var veryShortName = nomenclatureConfig.PropertyNames[nameof(Nomenclature.VeryShortName)];

			var sql =
				$"SELECT " +
				$"    n.{id} AS {nameof(NomenclatureNode.Id)}, " +
				$"    n.{name} AS {nameof(NomenclatureNode.Name)}, " +
				$"    n.{veryShortName} AS {nameof(NomenclatureNode.VeryShortName)}, " +
				$"    n.{isDefectiveBottle} AS {nameof(NomenclatureNode.IsDefectiveBottle)}, " +
				$"    n.{isShabbyBottle} AS {nameof(NomenclatureNode.IsShabbyBottle)}, " +
				$"    n.{category} AS {nameof(NomenclatureNode.CategoryAsString)} " +
				$"FROM {nomenclature} n " +
				$"WHERE !n.{isDefectiveBottle} " +
				$"    AND !n.{isArchive} " +
				$"    AND !n.{isDiler} " +
				$"    AND !n.{isDisposableTare} " +
				$"    AND (n.{category} = :bottle OR (n.{category} = :water AND n.{tareVolume} = :Vol19L)) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar(nameof(NomenclatureNode.Id), NHibernateUtil.Int32)
				.AddScalar(nameof(NomenclatureNode.Name), NHibernateUtil.String)
				.AddScalar(nameof(NomenclatureNode.VeryShortName), NHibernateUtil.String)
				.AddScalar(nameof(NomenclatureNode.CategoryAsString), NHibernateUtil.String)
				.AddScalar(nameof(NomenclatureNode.IsShabbyBottle), NHibernateUtil.Boolean)
				.AddScalar(nameof(NomenclatureNode.IsDefectiveBottle), NHibernateUtil.Boolean)
				.SetParameter("bottle", "bottle", NHibernateUtil.String)
				.SetParameter("water", "water", NHibernateUtil.String)
				.SetParameter("Vol19L", "Vol19L", NHibernateUtil.String)
				.SetResultTransformer(Transformers.AliasToBean<NomenclatureNode>())
				.Future<NomenclatureNode>();
		}

		#endregion

		#region Актив складов

		public IFutureValue<int?> GetIncomeWarehouseAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var incomingWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.IncomingWarehouse)];
			var writeoffWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.WriteoffWarehouse)];

			var sql =
				$"SELECT CAST(SUM(wmo.{amount}) as SIGNED) AS amount " +
				$"FROM {wmo} wmo " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"    AND wmo.{incomingWarehouseId} IS NOT NULL " +
				$"    AND wmo.{writeoffWarehouseId} IS NULL ";
			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureValue<int?> GetWriteoffWarehouseAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var incomingWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.IncomingWarehouse)];
			var writeoffWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.WriteoffWarehouse)];

			var sql =
				$"SELECT CAST(SUM(wmo.{amount}) as SIGNED) AS amount " +
				$"FROM {wmo} wmo " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"    AND wmo.{incomingWarehouseId} IS NULL " +
				$"    AND wmo.{writeoffWarehouseId} IS NOT NULL ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureEnumerable<AmountOnDateNode> GetIncomeWarehouseAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var incomingWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.IncomingWarehouse)];
			var writeoffWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.WriteoffWarehouse)];

			var sql =
				$"SELECT" +
				$"    DATE(wmo.{operationTime}) as {nameof(AmountOnDateNode.DateTime)}, " +
				$"    CAST(SUM(wmo.{amount}) as SIGNED) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM {wmo} wmo " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} >= :startDate " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"    AND wmo.{incomingWarehouseId} IS NOT NULL " +
				$"    AND wmo.{writeoffWarehouseId} IS NULL " +
				$"GROUP BY DATE(wmo.{operationTime}) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(AmountOnDateNode.DateTime)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(AmountOnDateNode.Amount)}", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		public IFutureEnumerable<AmountOnDateNode> GetWriteoffWarehouseAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();

			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var incomingWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.IncomingWarehouse)];
			var writeoffWarehouseId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.WriteoffWarehouse)];

			var sql =
				$"SELECT " +
				$"    DATE(wmo.{operationTime}) as {nameof(AmountOnDateNode.DateTime)}, " +
				$"    CAST(SUM(wmo.{amount}) as SIGNED) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM {wmo} wmo " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} >= :startDate " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"    AND wmo.{incomingWarehouseId} IS NULL " +
				$"    AND wmo.{writeoffWarehouseId} IS NOT NULL " +
				$"GROUP BY DATE(wmo.{operationTime}) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(AmountOnDateNode.DateTime)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(AmountOnDateNode.Amount)}", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Актив документов перемещения

		public IFutureValue<int?> GetIncomeMovementDocumentAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds)
		{
			var mdiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<MovementDocumentItem>();
			var movementDocumentItems = mdiConfig.TableName;
			var warehouseWriteoffOperationId = mdiConfig.PropertyNames[nameof(MovementDocumentItem.WarehouseWriteoffOperation)];

			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var warehouseMovementOperations = wmoConfig.TableName;
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];

			var sql =
				$"SELECT CAST(SUM(wmo.{amount}) as SIGNED) AS amount " +
				$"FROM {warehouseMovementOperations} wmo " +
				$"INNER JOIN {movementDocumentItems} mdi ON mdi.{warehouseWriteoffOperationId} = wmo.{wmoId} " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} <= :endDate ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureValue<int?> GetWriteoffMovementDocumentAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds)
		{
			var mdiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<MovementDocumentItem>();
			var movementDocumentItems = mdiConfig.TableName;
			var warehouseIncomeOperationId = mdiConfig.PropertyNames[nameof(MovementDocumentItem.WarehouseIncomeOperation)];

			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var warehouseMovementOperations = wmoConfig.TableName;
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];

			var sql =
				$"SELECT CAST(SUM(wmo.{amount}) as SIGNED) AS amount " +
				$"FROM {warehouseMovementOperations} wmo " +
				$"INNER JOIN {movementDocumentItems} mdi ON mdi.{warehouseIncomeOperationId} = wmo.{wmoId} " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} <= :endDate ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureEnumerable<AmountOnDateNode> GetIncomeMovementDocumentAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			var mdiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<MovementDocumentItem>();
			var movementDocumentItems = mdiConfig.TableName;
			var warehouseWriteoffOperationId = mdiConfig.PropertyNames[nameof(MovementDocumentItem.WarehouseWriteoffOperation)];

			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var warehouseMovementOperations = wmoConfig.TableName;
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];

			var sql =
				$"SELECT" +
				$"    DATE(wmo.{operationTime}) as {nameof(AmountOnDateNode.DateTime)}, " +
				$"    CAST(SUM(wmo.{amount}) as SIGNED) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM {warehouseMovementOperations} wmo " +
				$"INNER JOIN {movementDocumentItems} mdi ON mdi.{warehouseWriteoffOperationId} = wmo.{wmoId} " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} >= :startDate " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"GROUP BY DATE(wmo.{operationTime}) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(AmountOnDateNode.DateTime)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(AmountOnDateNode.Amount)}", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		public IFutureEnumerable<AmountOnDateNode> GetWriteoffMovementDocumentAssetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			var mdiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<MovementDocumentItem>();
			var movementDocumentItems = mdiConfig.TableName;
			var warehouseIncomeOperationId = mdiConfig.PropertyNames[nameof(MovementDocumentItem.WarehouseIncomeOperation)];

			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var warehouseMovementOperations = wmoConfig.TableName;
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];

			var sql =
				$"SELECT" +
				$"    DATE(wmo.{operationTime}) as {nameof(AmountOnDateNode.DateTime)}, " +
				$"    CAST(SUM(wmo.{amount}) as SIGNED) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM {warehouseMovementOperations} wmo " +
				$"INNER JOIN {movementDocumentItems} mdi ON mdi.{warehouseIncomeOperationId} = wmo.{wmoId} " +
				$"WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND wmo.{operationTime} >= :startDate " +
				$"    AND wmo.{operationTime} <= :endDate " +
				$"GROUP BY DATE(wmo.{operationTime}) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(AmountOnDateNode.DateTime)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(AmountOnDateNode.Amount)}", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Актив водителей

		public IFutureValue<int?> GetRouteListAssetFuture(IUnitOfWork uow, DateTime endDate, int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();

			var employeeNomenclatureMovementOperations = enmoConfig.TableName;
			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var sql =
				$"SELECT CAST(SUM(enmo.{amount}) as SIGNED) AS amount " +
				$"FROM {employeeNomenclatureMovementOperations} enmo " +
				$"WHERE enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND enmo.{operationTime} <= :endDate ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureEnumerable<AmountOnDateNode> GetRouteListAsseetByDatesFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var sql =
				$"SELECT" +
				$"    DATE(enmo.{operationTime}) as {nameof(AmountOnDateNode.DateTime)}, " +
				$"    CAST(SUM(enmo.{amount}) as SIGNED) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM {enmo} enmo " +
				$"WHERE enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"    AND enmo.{operationTime} >= :startDate " +
				$"    AND enmo.{operationTime} <= :endDate " +
				$"GROUP BY DATE(enmo.{operationTime}) ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(AmountOnDateNode.DateTime)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(AmountOnDateNode.Amount)}", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion
	}
}

using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.DB.EntityMappingConfig;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class GeneralDeltaBottleAnalyticsRepository : IGeneralDeltaBottleAnalyticsRepository
	{
		private readonly IEntityMappingConfigProvider _entityMappingConfigProvider;

		public GeneralDeltaBottleAnalyticsRepository(IEntityMappingConfigProvider entityMappingConfigProvider)
		{
			_entityMappingConfigProvider =
				entityMappingConfigProvider ?? throw new ArgumentNullException(nameof(entityMappingConfigProvider));
		}

		#region Недосдачи клиентами -

		public IFutureEnumerable<AmountOnDateNode> GetCounterpartyReturnLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var enmoId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Id)];
			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var ddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DeliveryDocumentItem>();
			var ddi = ddiConfig.TableName;
			var ddiEnmoId = ddiConfig.PropertyNames[nameof(DeliveryDocumentItem.EmployeeNomenclatureMovementOperation)];
			var ddiDdId = ddiConfig.PropertyNames[nameof(DeliveryDocumentItem.Document)];

			var sql =
				$"SELECT " +
				$"    T.date AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    -SUM(T.amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM" +
				$"    (SELECT " +
				$"        DATE(enmo.{operationTime}) AS date, " +
				$"        SUM(enmo.{amount}) AS amount " +
				$"    FROM {enmo} enmo " +
				$"    INNER JOIN {ddi} ddi on enmo.{enmoId} = ddi.{ddiEnmoId} " +
				$"    WHERE " +
				$"        enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"        AND enmo.{operationTime} >= :startDate " +
				$"        AND enmo.{operationTime} <= :endDate " +
				$"    GROUP BY ddi.{ddiDdId} " +
				$"    ) AS T " +
				$"WHERE T.amount < 0 " +
				$"GROUP BY T.date ";

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

		#region Пересдачи клиентами +

		public IFutureEnumerable<AmountOnDateNode> GetCounterpartyReturnIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var enmoId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Id)];
			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var ddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DeliveryDocumentItem>();
			var ddi = ddiConfig.TableName;
			var ddiEnmoId = ddiConfig.PropertyNames[nameof(DeliveryDocumentItem.EmployeeNomenclatureMovementOperation)];
			var ddiDdId = ddiConfig.PropertyNames[nameof(DeliveryDocumentItem.Document)];

			var sql =
				$"SELECT " +
				$"    T.date AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    SUM(T.amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM" +
				$"    (SELECT " +
				$"        DATE(enmo.{operationTime}) AS date, " +
				$"        SUM(enmo.{amount}) AS amount " +
				$"    FROM {enmo} enmo " +
				$"    INNER JOIN {ddi} ddi on enmo.{enmoId} = ddi.{ddiEnmoId} " +
				$"    WHERE " +
				$"        enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"        AND enmo.{operationTime} >= :startDate " +
				$"        AND enmo.{operationTime} <= :endDate " +
				$"    GROUP BY ddi.{ddiDdId}" +
				$"    ) AS T " +
				$"WHERE T.amount > 0 " +
				$"GROUP BY T.date ";

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

		#region Пересорт +

		public IFutureEnumerable<AmountOnDateNode> GetRegradingIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			RegradingOfGoodsDocumentItem rogdi = null;
			WarehouseMovementOperation wmo1 = null;
			WarehouseMovementOperation wmo2 = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => rogdi)
				.Left.JoinAlias(() => rogdi.WarehouseWriteOffOperation, () => wmo1)
				.Left.JoinAlias(() => rogdi.WarehouseIncomeOperation, () => wmo2)
				.WhereRestrictionOn(() => wmo1.Nomenclature.Id).Not.IsIn(nomenclatureIds)
				.WhereRestrictionOn(() => wmo2.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => wmo1.OperationTime >= startDate)
				.And(() => wmo1.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo1.OperationTime))))
					.WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo1.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Пересорт -

		public IFutureEnumerable<AmountOnDateNode> GetRegradingLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			RegradingOfGoodsDocumentItem rogdi = null;
			WarehouseMovementOperation wmo1 = null;
			WarehouseMovementOperation wmo2 = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => rogdi)
				.Inner.JoinAlias(() => rogdi.WarehouseWriteOffOperation, () => wmo1)
				.Inner.JoinAlias(() => rogdi.WarehouseIncomeOperation, () => wmo2)
				.WhereRestrictionOn(() => wmo1.Nomenclature.Id).IsIn(nomenclatureIds)
				.WhereRestrictionOn(() => wmo2.Nomenclature.Id).Not.IsIn(nomenclatureIds)
				.And(() => wmo1.OperationTime >= startDate)
				.And(() => wmo1.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo1.OperationTime))))
					.WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo1.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Акт списания -

		public IFutureEnumerable<AmountOnDateNode> GetWriteoffLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			WriteoffDocumentItem wdi = null;
			WarehouseMovementOperation wmo = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => wdi)
				.Inner.JoinAlias(() => wdi.WarehouseWriteoffOperation, () => wmo)
				.WhereRestrictionOn(() => wmo.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => wmo.OperationTime >= startDate)
				.And(() => wmo.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo.OperationTime)))
					).WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo.Amount)))
					).WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Инвентаризация -

		public IFutureEnumerable<AmountOnDateNode> GetInventorizationLossByDates(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			InventoryDocumentItem idi = null;
			WarehouseMovementOperation wmo = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => idi)
				.Left.JoinAlias(() => idi.WarehouseChangeOperation, () => wmo)
				.WhereRestrictionOn(() => wmo.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => wmo.WriteoffWarehouse != null)
				.And(() => wmo.OperationTime >= startDate)
				.And(() => wmo.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo.OperationTime))))
					.WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Инвентаризация +

		public IFutureEnumerable<AmountOnDateNode> GetInventorizationIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			InventoryDocumentItem idi = null;
			WarehouseMovementOperation wmo = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => idi)
				.Left.JoinAlias(() => idi.WarehouseChangeOperation, () => wmo)
				.WhereRestrictionOn(() => wmo.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => wmo.IncomingWarehouse != null)
				.And(() => wmo.OperationTime >= startDate)
				.And(() => wmo.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo.OperationTime))))
					.WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Входящая накладная +

		public IFutureEnumerable<AmountOnDateNode> GetIncomingInvoiceIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			IncomingInvoiceItem iii = null;
			WarehouseMovementOperation wmo = null;
			AmountOnDateNode resultAlias = null;

			return uow.Session.QueryOver(() => iii)
				.Left.JoinAlias(() => iii.IncomeGoodsOperation, () => wmo)
				.WhereRestrictionOn(() => wmo.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => wmo.IncomingWarehouse != null)
				.And(() => wmo.OperationTime >= startDate)
				.And(() => wmo.OperationTime <= endDate)
				.SelectList(list => list
					.Select(Projections.GroupProperty(
						CustomProjections.Date(Projections.Property(() => wmo.OperationTime))))
					.WithAlias(() => resultAlias.DateTime)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => wmo.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<AmountOnDateNode>())
				.Future<AmountOnDateNode>();
		}

		#endregion

		#region Недосдачи водителями -

		public IFutureEnumerable<AmountOnDateNode> GetDriverReturnLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var enmoId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Id)];

			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var dddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DriverDiscrepancyDocumentItem>();
			var dddi = dddiConfig.TableName;
			var dddiEnmoId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.EmployeeNomenclatureMovementOperation)];
			var dddiDddId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.Document)];

			var sql =
				$"SELECT " +
				$"    T.date AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    -SUM(T.amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM" +
				$"    (SELECT " +
				$"        DATE(enmo.{operationTime}) AS date, " +
				$"        SUM(enmo.{amount}) AS amount " +
				$"    FROM {enmo} enmo " +
				$"    INNER JOIN {dddi} ddi on enmo.{enmoId} = ddi.{dddiEnmoId} " +
				$"    WHERE " +
				$"        enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"        AND enmo.{operationTime} >= :startDate " +
				$"        AND enmo.{operationTime} <= :endDate " +
				$"    GROUP BY ddi.{dddiDddId}" +
				$"    ) AS T " +
				$"WHERE T.amount < 0 " +
				$"GROUP BY T.date ";

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

		#region Пересдачи водителями +

		public IFutureEnumerable<AmountOnDateNode> GetDriverReturnIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var enmoId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Id)];

			var amount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var nomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var operationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var dddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DriverDiscrepancyDocumentItem>();
			var dddi = dddiConfig.TableName;
			var dddiEnmoId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.EmployeeNomenclatureMovementOperation)];
			var dddiDddId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.Document)];

			var sql =
				$"SELECT " +
				$"    T.date AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    SUM(T.amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM" +
				$"    (SELECT " +
				$"        DATE(enmo.{operationTime}) AS date, " +
				$"        SUM(enmo.{amount}) AS amount " +
				$"    FROM {enmo} enmo " +
				$"    INNER JOIN {dddi} ddi on enmo.{enmoId} = ddi.{dddiEnmoId} " +
				$"    WHERE " +
				$"        enmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"        AND enmo.{operationTime} >= :startDate " +
				$"        AND enmo.{operationTime} <= :endDate " +
				$"    GROUP BY ddi.{dddiDddId}" +
				$"    ) AS T " +
				$"WHERE T.amount > 0 " +
				$"GROUP BY T.date ";

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

		#region Самовывоз +

		public IFutureEnumerable<AmountOnDateNode> GetSelfDeliveryIncome(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];

			var sddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<SelfDeliveryDocumentItem>();
			var sddi = sddiConfig.TableName;
			var sddiWmoId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentItem.WarehouseMovementOperation)];
			var sddiDocumentId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentItem.Document)];

			var sddrConfig = _entityMappingConfigProvider.GetEntityMappingConfig<SelfDeliveryDocumentReturned>();
			var sddr = sddrConfig.TableName;
			var sddrWmoId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentReturned.WarehouseMovementOperation)];
			var sddrDocumentId = sddrConfig.PropertyNames[nameof(SelfDeliveryDocumentReturned.Document)];

			var sql =
				$"SELECT " +
				$"    grouped_by_document.doc_operation_time AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    SUM(grouped_by_document.document_amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM " +
				$"    (SELECT " +
				$"        grouped_by_operation.operation_time as doc_operation_time, " +
				$"        SUM(grouped_by_operation.wmo_amount) as document_amount " +
				$"    FROM " +
				$"        (SELECT " +
				$"            DATE(wmo.{operationTime}) AS operation_time, " +
				$"            -wmo.{amount} AS wmo_amount, " +
				$"            ssddi.{sddiDocumentId} AS document_id " +
				$"        FROM {wmo} wmo " +
				$"        INNER JOIN {sddi} ssddi on wmo.{wmoId} = ssddi.{sddiWmoId} " +
				$"        WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"            AND wmo.{operationTime} >= :startDate " +
				$"            AND wmo.{operationTime} <= :endDate " +
				$"        UNION ALL " +
				$"        SELECT " +
				$"            DATE(wmo.{operationTime}) AS operation_time, " +
				$"            wmo.{amount} AS wmo_amount, " +
				$"            ssddr.{sddrDocumentId} AS document_id " +
				$"        FROM {wmo} wmo " +
				$"        INNER JOIN {sddr} ssddr ON wmo.{wmoId} = ssddr.{sddrWmoId} " +
				$"        WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"            AND wmo.{operationTime} >= :startDate " +
				$"            AND wmo.{operationTime} <= :endDate " +
				$"    ) AS grouped_by_operation " +
				$"    GROUP BY grouped_by_operation.document_id " +
				$") AS grouped_by_document " +
				$"WHERE document_amount > 0 " +
				$"GROUP BY DATE(grouped_by_document.doc_operation_time) ";

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

		#region Самовывоз -

		public IFutureEnumerable<AmountOnDateNode> GetSelfDeliveryLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			var wmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<WarehouseMovementOperation>();
			var wmo = wmoConfig.TableName;
			var amount = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Amount)];
			var nomenclatureId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Nomenclature)];
			var operationTime = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.OperationTime)];
			var wmoId = wmoConfig.PropertyNames[nameof(WarehouseMovementOperation.Id)];

			var sddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<SelfDeliveryDocumentItem>();
			var sddi = sddiConfig.TableName;
			var sddiWmoId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentItem.WarehouseMovementOperation)];
			var sddiDocumentId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentItem.Document)];

			var sddrConfig = _entityMappingConfigProvider.GetEntityMappingConfig<SelfDeliveryDocumentReturned>();
			var sddr = sddrConfig.TableName;
			var sddrWmoId = sddiConfig.PropertyNames[nameof(SelfDeliveryDocumentReturned.WarehouseMovementOperation)];
			var sddrDocumentId = sddrConfig.PropertyNames[nameof(SelfDeliveryDocumentReturned.Document)];

			var sql =
				$"SELECT " +
				$"    grouped_by_document.doc_operation_time AS {nameof(AmountOnDateNode.DateTime)}, " +
				$"    -SUM(grouped_by_document.document_amount) AS {nameof(AmountOnDateNode.Amount)} " +
				$"FROM " +
				$"    (SELECT " +
				$"        grouped_by_operation.operation_time as doc_operation_time, " +
				$"        SUM(grouped_by_operation.wmo_amount) as document_amount " +
				$"    FROM " +
				$"        (SELECT " +
				$"            DATE(wmo.{operationTime}) AS operation_time, " +
				$"            -wmo.{amount} AS wmo_amount, " +
				$"            ssddi.{sddiDocumentId} AS document_id " +
				$"        FROM {wmo} wmo " +
				$"        INNER JOIN {sddi} ssddi on wmo.{wmoId} = ssddi.{sddiWmoId} " +
				$"        WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"            AND wmo.{operationTime} >= :startDate " +
				$"            AND wmo.{operationTime} <= :endDate " +
				$"        UNION ALL " +
				$"        SELECT " +
				$"            DATE(wmo.{operationTime}) AS operation_time, " +
				$"            wmo.{amount} AS wmo_amount, " +
				$"            ssddr.{sddrDocumentId} AS document_id " +
				$"        FROM {wmo} wmo " +
				$"        INNER JOIN {sddr} ssddr ON wmo.{wmoId} = ssddr.{sddrWmoId} " +
				$"        WHERE wmo.{nomenclatureId} IN (:nomenclatureIds) " +
				$"            AND wmo.{operationTime} >= :startDate " +
				$"            AND wmo.{operationTime} <= :endDate " +
				$"    ) AS grouped_by_operation " +
				$"    GROUP BY grouped_by_operation.document_id " +
				$") AS grouped_by_document " +
				$"WHERE document_amount < 0 " +
				$"GROUP BY DATE(grouped_by_document.doc_operation_time) ";

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

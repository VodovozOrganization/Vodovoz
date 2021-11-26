using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.DB.EntityMappingConfig;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class DeltaLossBottleAnalyticsRepository : IDeltaLossBottleAnalyticsRepository
	{
		private readonly IEntityMappingConfigProvider _entityMappingConfigProvider;

		public DeltaLossBottleAnalyticsRepository(IEntityMappingConfigProvider entityMappingConfigProvider)
		{
			_entityMappingConfigProvider =
				entityMappingConfigProvider ?? throw new ArgumentNullException(nameof(entityMappingConfigProvider));
		}

		public IFutureEnumerable<SummaryNode> GetWriteoffLossSummaryFuture(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			SummaryNode summaryNode = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			WriteoffDocumentItem writeoffDocumentItemAlias = null;
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver(() => writeoffDocumentItemAlias)
				.Inner.JoinAlias(() => writeoffDocumentItemAlias.WarehouseWriteoffOperation, () => warehouseOperationAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.WriteoffWarehouse, () => warehouseAlias)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime <= endDate)
				.SelectList(list => list
					.SelectGroup(() => warehouseOperationAlias.WriteoffWarehouse.Id)
					.Select(() => warehouseAlias.Name)
					.WithAlias(() => summaryNode.Name)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => summaryNode.Amount))
				.TransformUsing(Transformers.AliasToBean<SummaryNode>())
				.Future<SummaryNode>();
		}

		public IFutureEnumerable<SummaryNode> GetInventarizationLossSummaryFuture(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds)
		{
			SummaryNode summaryNode = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			InventoryDocumentItem inventoryDocumentItemAlias = null;
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver(() => inventoryDocumentItemAlias)
				.Inner.JoinAlias(() => inventoryDocumentItemAlias.WarehouseChangeOperation, () => warehouseOperationAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.WriteoffWarehouse, () => warehouseAlias)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime < endDate)
				.SelectList(list => list
					.SelectGroup(() => warehouseOperationAlias.WriteoffWarehouse.Id)
					.Select(() => warehouseAlias.Name)
					.WithAlias(() => summaryNode.Name)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => summaryNode.Amount))
				.TransformUsing(Transformers.AliasToBean<SummaryNode>())
				.Future<SummaryNode>();
		}

		public IFutureEnumerable<DetailedNode> GetDriverLossByRouteListFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate,
			int[] nomenclatureIds)
		{
			var enmoConfig = _entityMappingConfigProvider.GetEntityMappingConfig<EmployeeNomenclatureMovementOperation>();
			var enmo = enmoConfig.TableName;
			var enmoId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Id)];

			var enmoAmount = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Amount)];
			var enmoNomenclatureId = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.Nomenclature)];
			var enmoOperationTime = enmoConfig.PropertyNames[nameof(EmployeeNomenclatureMovementOperation.OperationTime)];

			var dddiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DriverDiscrepancyDocumentItem>();
			var dddi = dddiConfig.TableName;
			var dddiEnmoId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.EmployeeNomenclatureMovementOperation)];
			var dddiDddId = dddiConfig.PropertyNames[nameof(DriverDiscrepancyDocumentItem.Document)];

			var dddConfig = _entityMappingConfigProvider.GetEntityMappingConfig<DriverDiscrepancyDocument>();
			var ddd = dddConfig.TableName;
			var dddId = dddConfig.PropertyNames[nameof(DriverDiscrepancyDocument.Id)];
			var dddRlId = dddConfig.PropertyNames[nameof(DriverDiscrepancyDocument.RouteList)];

			var rlConfig = _entityMappingConfigProvider.GetEntityMappingConfig<RouteList>();
			var rl = rlConfig.TableName;
			var rlId = rlConfig.PropertyNames[nameof(RouteList.Id)];
			var rlDriverId = rlConfig.PropertyNames[nameof(RouteList.Driver)];
			var rlClosingComment = rlConfig.PropertyNames[nameof(RouteList.ClosingComment)];

			var emplConfig = _entityMappingConfigProvider.GetEntityMappingConfig<Employee>();
			var empl = emplConfig.TableName;
			var emplId = emplConfig.PropertyNames[nameof(Employee.Id)];
			var emplLastName = emplConfig.PropertyNames[nameof(Employee.LastName)];
			var emplName = emplConfig.PropertyNames[nameof(Employee.Name)];
			var emplPatronymic = emplConfig.PropertyNames[nameof(Employee.Patronymic)];

			var fineConfig = _entityMappingConfigProvider.GetEntityMappingConfig<Fine>();
			var fine = fineConfig.TableName;
			var fineId = fineConfig.PropertyNames[nameof(Fine.Id)];
			var fineRlId = fineConfig.PropertyNames[nameof(Fine.RouteList)];

			var fiConfig = _entityMappingConfigProvider.GetEntityMappingConfig<FineItem>();
			var fi = fiConfig.TableName;
			var fiFineId = fiConfig.PropertyNames[nameof(FineItem.Fine)];
			var fiEMplId = fiConfig.PropertyNames[nameof(FineItem.Employee)];
			var fiMoney = fiConfig.PropertyNames[nameof(FineItem.Money)];

			var sql =
				$"SELECT " +
				$"    ddd.{dddRlId} AS {nameof(DetailedNode.DocumentNumber)}, " +
				$"    'МЛ' AS {nameof(DetailedNode.DocumentName)}, " +
				$"    DATE(enmo.{enmoOperationTime}) AS {nameof(DetailedNode.Date)}, " +
				$"    SUM(enmo.{enmoAmount}) AS {nameof(DetailedNode.Amount)}, " +
				$"    GET_PERSON_NAME_WITH_INITIALS(driver.{emplLastName}, driver.{emplName}, driver.{emplPatronymic}) AS {nameof(DetailedNode.AuthorOrDriver)}, " +
				$"    rl.{rlClosingComment} AS {nameof(DetailedNode.Comment)}, " +
				$"    'Бутыль 19л' AS {nameof(DetailedNode.NomenclatureName)}, " +
				$"    (SELECT " +
				$"        GROUP_CONCAT(GET_PERSON_NAME_WITH_INITIALS(e.{emplLastName}, e.{emplName}, e.{emplPatronymic}), ' ', fi.{fiMoney}) " +
				$"        FROM {fine} fine " +
				$"        INNER JOIN {fi} fi ON fine.{fineId} = fi.{fiFineId} " +
				$"        INNER JOIN {empl} e ON e.{emplId} = fi.{fiEMplId}" +
				$"        WHERE fine.{fineRlId} = rl.{rlId} " +
				$"    ) AS {nameof(DetailedNode.FineString)} " +
				$"FROM {enmo} enmo " +
				$"INNER JOIN {dddi} ddi on enmo.{enmoId} = ddi.{dddiEnmoId} " +
				$"INNER JOIN {ddd} ddd on ddd.{dddId} = ddi.{dddiDddId} " +
				$"INNER JOIN {rl} rl on rl.{rlId} = ddd.{dddRlId} " +
				$"INNER JOIN {empl} driver on driver.{emplId} = rl.{rlDriverId} " +
				$"WHERE " +
				$"    enmo.{enmoNomenclatureId} IN (:nomenclatureIds) " +
				$"    AND enmo.{enmoOperationTime} >= :startDate " +
				$"    AND enmo.{enmoOperationTime} <= :endDate " +
				$"GROUP BY rl.{rlId} " +
				$"HAVING {nameof(DetailedNode.Amount)} < 0 ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar($"{nameof(DetailedNode.DocumentNumber)}", NHibernateUtil.Int32)
				.AddScalar($"{nameof(DetailedNode.DocumentName)}", NHibernateUtil.String)
				.AddScalar($"{nameof(DetailedNode.Date)}", NHibernateUtil.DateTime)
				.AddScalar($"{nameof(DetailedNode.Amount)}", NHibernateUtil.Int32)
				.AddScalar($"{nameof(DetailedNode.AuthorOrDriver)}", NHibernateUtil.String)
				.AddScalar($"{nameof(DetailedNode.Comment)}", NHibernateUtil.String)
				.AddScalar($"{nameof(DetailedNode.NomenclatureName)}", NHibernateUtil.String)
				.AddScalar($"{nameof(DetailedNode.FineString)}", NHibernateUtil.String)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.SetResultTransformer(Transformers.AliasToBean<DetailedNode>())
				.Future<DetailedNode>();
		}

		public IFutureValue<int?> GetDriverReturnSummaryLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate, int[] nomenclatureIds)
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
				$"    -SUM(T.amount) AS amount " +
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
				$"WHERE T.amount < 0 ";

			return uow.Session.CreateSQLQuery(sql)
				.AddScalar("amount", NHibernateUtil.Int32)
				.SetParameter("startDate", startDate, NHibernateUtil.DateTime)
				.SetParameter("endDate", endDate, NHibernateUtil.DateTime)
				.SetParameterList("nomenclatureIds", nomenclatureIds)
				.FutureValue<int?>();
		}

		public IFutureEnumerable<DetailedNode> GetInventorizationLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate,
			int[] nomenclatureIds, int? warehouseId)
		{
			DetailedNode resultALias = null;
			FineItem fineItemAlias = null;
			InventoryDocument inventoryDocumentAlias = null;
			InventoryDocumentItem inventoryDocumentItemAlias = null;
			Employee authorAlias = null;
			Employee finedEmployeeAlias = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;

			var inventoryFinesSubquery = QueryOver.Of(() => fineItemAlias)
				.Inner.JoinAlias(() => fineItemAlias.Employee, () => finedEmployeeAlias)
				.Where(() => fineItemAlias.Fine.Id == inventoryDocumentItemAlias.Fine.Id)
				.Select(CustomProjections.GroupConcat(CustomProjections.Concat(() => finedEmployeeAlias.LastName, () => fineItemAlias.Money),
					separator: ",\n"));

			var query = uow.Session.QueryOver(() => warehouseOperationAlias)
				.JoinEntityAlias(() => inventoryDocumentItemAlias,
					() => inventoryDocumentItemAlias.WarehouseChangeOperation.Id == warehouseOperationAlias.Id)
				.Inner.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Inner.JoinAlias(() => inventoryDocumentItemAlias.Document, () => inventoryDocumentAlias)
				.Inner.JoinAlias(() => inventoryDocumentAlias.Author, () => authorAlias)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime < endDate)
				.And(CustomRestrictions.IsNotNull(() => warehouseOperationAlias.WriteoffWarehouse.Id));

			if(warehouseId != null)
			{
				query.Where(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId);
			}

			return query.SelectList(list => list
					.Select(CustomProjections.Date(() => warehouseOperationAlias.OperationTime))
					.WithAlias(() => resultALias.Date)
					.Select(
						CustomProjections.GetPersonNameWithInitials(
							() => authorAlias.LastName,
							() => authorAlias.Name,
							() => authorAlias.Patronymic))
					.WithAlias(() => resultALias.AuthorOrDriver)
					.Select(() => inventoryDocumentAlias.Id)
					.WithAlias(() => resultALias.DocumentNumber)
					.Select(() => "Инвентаризация")
					.WithAlias(() => resultALias.DocumentName)
					.Select(() => nomenclatureAlias.ShortName)
					.WithAlias(() => resultALias.NomenclatureName)
					.Select(CustomProjections.Negative(NHibernateUtil.Int32, Projections.Cast(NHibernateUtil.Int32,
						Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => resultALias.Amount)
					.SelectSubQuery(inventoryFinesSubquery)
					.WithAlias(() => resultALias.FineString)
					.Select(() => inventoryDocumentItemAlias.Comment)
					.WithAlias(() => resultALias.Comment))
				.TransformUsing(Transformers.AliasToBean<DetailedNode>())
				.Future<DetailedNode>();
		}

		public IFutureEnumerable<DetailedNode> GetWriteoffLoss(IUnitOfWork uow, DateTime startDate, DateTime endDate, int[] nomenclatureIds,
			int? warehouseId)
		{
			DetailedNode resultALias = null;
			FineItem fineItemAlias = null;
			WriteoffDocument writeoffDocumentAlias = null;
			WriteoffDocumentItem writeoffDocumentItemAlias = null;
			Employee employeeAlias = null;
			Employee authorAlias = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			Nomenclature nomenclatureAlias = null;

			var inventoryFinesSubquery = QueryOver.Of<FineItem>(() => fineItemAlias)
				.Inner.JoinAlias(() => fineItemAlias.Employee, () => authorAlias)
				.Where(() => fineItemAlias.Fine.Id == writeoffDocumentItemAlias.Fine.Id)
				.Select(CustomProjections.GroupConcat(CustomProjections.Concat(() => authorAlias.LastName, () => fineItemAlias.Money),
					separator: ",\n"));

			var query = uow.Session.QueryOver<WarehouseMovementOperation>(() => warehouseOperationAlias)
				.JoinEntityAlias(() => writeoffDocumentItemAlias,
					() => writeoffDocumentItemAlias.WarehouseWriteoffOperation.Id == warehouseOperationAlias.Id)
				.Inner.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Inner.JoinAlias(() => writeoffDocumentItemAlias.Document, () => writeoffDocumentAlias)
				.Inner.JoinAlias(() => writeoffDocumentAlias.Author, () => employeeAlias)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => warehouseOperationAlias.OperationTime >= startDate)
				.And(() => warehouseOperationAlias.OperationTime < endDate)
				.And(() => warehouseOperationAlias.Amount > 0);

			if(warehouseId != null)
			{
				query.Where(() => warehouseOperationAlias.WriteoffWarehouse.Id == warehouseId);
			}

			return query.SelectList(list => list
					.Select(
						CustomProjections.Date(() => warehouseOperationAlias.OperationTime))
					.WithAlias(() => resultALias.Date)
					.Select(
						CustomProjections.GetPersonNameWithInitials(
							() => employeeAlias.LastName,
							() => employeeAlias.Name,
							() => employeeAlias.Patronymic))
					.WithAlias(() => resultALias.AuthorOrDriver)
					.Select(() => writeoffDocumentAlias.Id)
					.WithAlias(() => resultALias.DocumentNumber)
					.Select(() => "Акт списания")
					.WithAlias(() => resultALias.DocumentName)
					.Select(() => nomenclatureAlias.ShortName)
					.WithAlias(() => resultALias.NomenclatureName)
					.Select(CustomProjections.Negative(NHibernateUtil.Int32,
						Projections.Cast(NHibernateUtil.Int32, Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => resultALias.Amount)
					.SelectSubQuery(inventoryFinesSubquery)
					.WithAlias(() => resultALias.FineString)
					.Select(() => writeoffDocumentItemAlias.Comment)
					.WithAlias(() => resultALias.Comment))
				.TransformUsing(Transformers.AliasToBean<DetailedNode>())
				.Future<DetailedNode>();
		}
	}
}

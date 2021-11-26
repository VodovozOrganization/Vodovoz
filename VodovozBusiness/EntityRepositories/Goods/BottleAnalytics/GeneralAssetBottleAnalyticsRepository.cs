using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class GeneralAssetBottleAnalyticsRepository : IGeneralAssetBottleAnalyticsRepository
	{
		public IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetDriversLateAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds)
		{
			AmountOnDateNode amountOnDateAlias = null;
			NomenclatureAmountOnWarehouseNode nomenclatureAmountOnWarehouseNode = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeNomenclatureMovementOperation employeeOperationAlias = null;

			return uow.Session.QueryOver(() => employeeOperationAlias)
				.Inner.JoinAlias(() => employeeOperationAlias.Nomenclature, () => nomenclatureAlias)
				.WhereRestrictionOn(() => employeeOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => employeeOperationAlias.OperationTime <= date)
				.SelectList(list => list
					.Select(() => "Водители просрочка")
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.WarehouseName)
					.SelectGroup(() => employeeOperationAlias.Nomenclature.Id)
					.Select(() => nomenclatureAlias.VeryShortName)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.NomenclatureVeryShortName)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => employeeOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureAmountOnWarehouseNode>())
				.Future<NomenclatureAmountOnWarehouseNode>();
		}

		public IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetDriversOnDateAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds)
		{
			AmountOnDateNode amountOnDateAlias = null;
			NomenclatureAmountOnWarehouseNode nomenclatureAmountOnWarehouseNode = null;
			Nomenclature nomenclatureAlias = null;
			EmployeeNomenclatureMovementOperation employeeOperationAlias = null;

			return uow.Session.QueryOver(() => employeeOperationAlias)
				.Inner.JoinAlias(() => employeeOperationAlias.Nomenclature, () => nomenclatureAlias)
				.WhereRestrictionOn(() => employeeOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => employeeOperationAlias.OperationTime >= date.Date)
				.And(() => employeeOperationAlias.OperationTime <= date.Date.AddDays(1).AddMilliseconds(-1))
				.SelectList(list => list
					.Select(() => "Водители сегодня")
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.WarehouseName)
					.SelectGroup(() => employeeOperationAlias.Nomenclature.Id)
					.Select(() => nomenclatureAlias.VeryShortName)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.NomenclatureVeryShortName)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => employeeOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureAmountOnWarehouseNode>())
				.Future<NomenclatureAmountOnWarehouseNode>();
		}

		public IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetWarehouseIncomeAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds)
		{
			AmountOnDateNode amountOnDateAlias = null;
			NomenclatureAmountOnWarehouseNode nomenclatureAmountOnWarehouseNode = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver(() => warehouseOperationAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.IncomingWarehouse, () => warehouseAlias)
				.And(CustomRestrictions.IsNull(() => warehouseOperationAlias.WriteoffWarehouse))
				.And(() => warehouseOperationAlias.OperationTime <= date)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.SelectList(list => list
					.SelectGroup(() => warehouseAlias.Name)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.WarehouseName)
					.SelectGroup(() => warehouseOperationAlias.Nomenclature.Id)
					.Select(() => nomenclatureAlias.VeryShortName)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.NomenclatureVeryShortName)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureAmountOnWarehouseNode>())
				.Future<NomenclatureAmountOnWarehouseNode>();
		}

		public IFutureEnumerable<NomenclatureAmountOnWarehouseNode> GetWarehouseWriteoffAssetFuture(IUnitOfWork uow, DateTime date,
			int[] nomenclatureIds)
		{
			AmountOnDateNode amountOnDateAlias = null;
			NomenclatureAmountOnWarehouseNode nomenclatureAmountOnWarehouseNode = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation warehouseOperationAlias = null;
			Warehouse warehouseAlias = null;

			return uow.Session.QueryOver(() => warehouseOperationAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.Nomenclature, () => nomenclatureAlias)
				.Inner.JoinAlias(() => warehouseOperationAlias.WriteoffWarehouse, () => warehouseAlias)
				.And(CustomRestrictions.IsNull(() => warehouseOperationAlias.IncomingWarehouse))
				.And(() => warehouseOperationAlias.OperationTime <= date)
				.WhereRestrictionOn(() => warehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.SelectList(list => list
					.SelectGroup(() => warehouseAlias.Name)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.WarehouseName)
					.SelectGroup(() => warehouseOperationAlias.Nomenclature.Id)
					.Select(() => nomenclatureAlias.VeryShortName)
					.WithAlias(() => nomenclatureAmountOnWarehouseNode.NomenclatureVeryShortName)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => warehouseOperationAlias.Amount))))
					.WithAlias(() => amountOnDateAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<NomenclatureAmountOnWarehouseNode>())
				.Future<NomenclatureAmountOnWarehouseNode>();
		}
	}

	public class NomenclatureAmountOnWarehouseNode
	{
		public int Amount { get; set; }
		public string NomenclatureVeryShortName { get; set; }
		public string WarehouseName { get; set; }
	}
}

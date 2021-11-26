using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace Vodovoz.EntityRepositories.Goods.BottleAnalytics
{
	public class DeltaShabbyBottleAnalyticsRepository : IDeltaShabbyBottleAnalyticsRepository
	{
		public IFutureEnumerable<SummaryNode> GetRegradingToShabbyBottlesSummaryFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int[] nomenclatureIds)
		{
			SummaryNode resultAlias = null;
			WarehouseMovementOperation writeoffWarehouseWriteoffOperationAlias = null;
			Warehouse warehouseAlias = null;
			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			WarehouseMovementOperation incomeWarehouseOperationAlias = null;
			Nomenclature incomeNomenclatureAlias = null;
			Nomenclature writeoffNomenclatureAlias = null;

			return uow.Session.QueryOver(() => regradingOfGoodsDocumentItemAlias)
				.Inner.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseWriteOffOperation,
					() => writeoffWarehouseWriteoffOperationAlias)
				.Inner.JoinAlias(() => writeoffWarehouseWriteoffOperationAlias.WriteoffWarehouse, () => warehouseAlias)
				.Inner.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseIncomeOperation, () => incomeWarehouseOperationAlias)
				.Inner.JoinAlias(() => writeoffWarehouseWriteoffOperationAlias.Nomenclature, () => writeoffNomenclatureAlias)
				.Inner.JoinAlias(() => incomeWarehouseOperationAlias.Nomenclature, () => incomeNomenclatureAlias)
				.WhereRestrictionOn(() => writeoffWarehouseWriteoffOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => !writeoffNomenclatureAlias.IsShabbyBottle)
				.And(() => incomeNomenclatureAlias.IsShabbyBottle)
				.And(() => !incomeNomenclatureAlias.IsDefectiveBottle)
				.And(() => writeoffWarehouseWriteoffOperationAlias.OperationTime >= startDate)
				.And(() => writeoffWarehouseWriteoffOperationAlias.OperationTime <= endDate)
				.And(() => incomeWarehouseOperationAlias.Amount > 0)
				.SelectList(list => list
					.SelectGroup(() => writeoffWarehouseWriteoffOperationAlias.WriteoffWarehouse.Id)
					.Select(() => warehouseAlias.Name)
					.WithAlias(() => resultAlias.Name)
					.Select(Projections.Cast(NHibernateUtil.Int32,
						Projections.Sum(Projections.Property(() => writeoffWarehouseWriteoffOperationAlias.Amount))))
					.WithAlias(() => resultAlias.Amount))
				.TransformUsing(Transformers.AliasToBean<SummaryNode>())
				.Future<SummaryNode>();
		}

		public IFutureEnumerable<DetailedNode> GetRegradingToShabbyBottlesDetailedFuture(IUnitOfWork uow, DateTime startDate,
			DateTime endDate, int? warehouseId, int[] nomenclatureIds)
		{
			DetailedNode resultAlias = null;
			Nomenclature writeoffNomenclatureAlias = null;
			WarehouseMovementOperation writeoffWarehouseOperationAlias = null;
			RegradingOfGoodsDocumentItem regradingOfGoodsDocumentItemAlias = null;
			RegradingOfGoodsDocument regradingOfGoodsDocumentAlias = null;
			WarehouseMovementOperation incomeWarehouseOperationAlias = null;
			Nomenclature incomeNomenclatureAlias = null;
			Employee authorAlias = null;
			Employee finedEmployeeAlias = null;
			Fine fineAlias = null;
			FineItem fineItemAlias = null;

			var finesSubquery = QueryOver.Of(() => fineItemAlias)
				.Inner.JoinAlias(() => fineItemAlias.Employee, () => finedEmployeeAlias)
				.Inner.JoinAlias(() => fineItemAlias.Fine, () => fineAlias)
				.Where(() => fineAlias.Id == regradingOfGoodsDocumentItemAlias.Fine.Id)
				.Select(CustomProjections.GroupConcat(
					CustomProjections.Concat(() => finedEmployeeAlias.LastName, () => fineItemAlias.Money), separator: ",\n"));

			var query = uow.Session.QueryOver(() => writeoffWarehouseOperationAlias)
				.JoinEntityAlias(() => regradingOfGoodsDocumentItemAlias,
					() => regradingOfGoodsDocumentItemAlias.WarehouseWriteOffOperation.Id == writeoffWarehouseOperationAlias.Id)
				.Inner.JoinAlias(() => regradingOfGoodsDocumentItemAlias.Document, () => regradingOfGoodsDocumentAlias)
				.Inner.JoinAlias(() => writeoffWarehouseOperationAlias.Nomenclature, () => writeoffNomenclatureAlias)
				.Inner.JoinAlias(() => regradingOfGoodsDocumentAlias.Author, () => authorAlias)
				.Inner.JoinAlias(() => regradingOfGoodsDocumentItemAlias.WarehouseIncomeOperation, () => incomeWarehouseOperationAlias)
				.Inner.JoinAlias(() => incomeWarehouseOperationAlias.Nomenclature, () => incomeNomenclatureAlias)
				.WhereRestrictionOn(() => writeoffWarehouseOperationAlias.Nomenclature.Id).IsIn(nomenclatureIds)
				.And(() => incomeNomenclatureAlias.IsShabbyBottle)
				.And(() => writeoffWarehouseOperationAlias.OperationTime >= startDate)
				.And(() => writeoffWarehouseOperationAlias.OperationTime <= endDate)
				.And(() => incomeWarehouseOperationAlias.Amount > 0);

			if(warehouseId != null)
			{
				query.Where(() => writeoffWarehouseOperationAlias.WriteoffWarehouse.Id == warehouseId);
			}

			return query.SelectList(list => list
					.Select(CustomProjections.Date(() => writeoffWarehouseOperationAlias.OperationTime))
					.WithAlias(() => resultAlias.Date)
					.Select(
						CustomProjections.GetPersonNameWithInitials(
							() => authorAlias.LastName,
							() => authorAlias.Name,
							() => authorAlias.Patronymic))
					.WithAlias(() => resultAlias.AuthorOrDriver)
					.Select(() => regradingOfGoodsDocumentAlias.Id)
					.WithAlias(() => resultAlias.DocumentNumber)
					.Select(() => "Документ пересортицы")
					.WithAlias(() => resultAlias.DocumentName)
					.Select(() => writeoffNomenclatureAlias.Name)
					.WithAlias(() => resultAlias.NomenclatureName)
					.Select(CustomProjections.Negative(NHibernateUtil.Int32,
						Projections.Cast(NHibernateUtil.Int32, Projections.Property(() => writeoffWarehouseOperationAlias.Amount))))
					.WithAlias(() => resultAlias.Amount)
					.SelectSubQuery(finesSubquery)
					.WithAlias(() => resultAlias.FineString)
					.Select(() => regradingOfGoodsDocumentItemAlias.Comment)
					.WithAlias(() => resultAlias.Comment))
				.TransformUsing(Transformers.AliasToBean<DetailedNode>())
				.Future<DetailedNode>();
		}
	}
}

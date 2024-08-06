﻿using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Errors;
using Vodovoz.Presentation.ViewModels.Reports;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Presentation.ViewModels.Store.Reports
{
	[Appellative(Nominative = "Оборачиваемость складских остатков")]
	public partial class TurnoverOfWarehouseBalancesReport : IClosedXmlReport
	{
		private const string _noSalesPrefix = "Ост: ";

		private static readonly OrderStatus[] _orderStatuses = new OrderStatus[]
		{
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,
			OrderStatus.Closed
		};

		private TurnoverOfWarehouseBalancesReport(
			DateTime startDate,
			DateTime endDate,
			IList<IDateTimeSlice> slices,
			List<TurnoverOfWarehouseBalancesReportRow> reportRows)
		{
			CreatedAt = DateTime.Now;

			StartDate = startDate.Date;
			EndDate = endDate.LatestDayTime();

			var slicesTitles = new List<string>();

			foreach(var slice in slices)
			{
				slicesTitles.Add($"{slice.StartDate:dd-MM}");
			}

			SliceTitles = slicesTitles;
			ReportRows = reportRows;
		}

		public string TemplatePath => @".\Reports\Store\TurnoverOfWarehouseBalancesReport.xlsx";

		public IList<string> SliceTitles { get; }

		public DateTime CreatedAt { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public List<TurnoverOfWarehouseBalancesReportRow> ReportRows { get; }

		public static async Task<Result<TurnoverOfWarehouseBalancesReport>> Generate(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			DateTimeSliceType dateTimeSliceType,
			int[] includedWarehouseIds,
			int[] excludedWarehouseIds,
			NomenclatureCategory[] includedNomenclatureCategoryIds,
			NomenclatureCategory[] excludedNomenclatureCategoryIds,
			int[] includedNomenclatureIds,
			int[] excludedNomenclatureIds,
			int?[] includedProductGroupIds,
			int?[] excludedProductGroupIds,
			CancellationToken cancellationToken)
		{
			var slices = DateTimeSliceFactory
				.CreateSlices(dateTimeSliceType, startDate, endDate)
				.ToArray();

			var nomenclaturesSmallNodes = await GetNomenclaturesQuery(
				unitOfWork,
				includedNomenclatureCategoryIds,
				excludedNomenclatureCategoryIds,
				includedNomenclatureIds,
				excludedNomenclatureIds,
				includedProductGroupIds,
				excludedProductGroupIds)
				.ToListAsync(cancellationToken);

			var warehousesNodes = await GetWarehousesQuery(
				unitOfWork,
				includedWarehouseIds,
				excludedWarehouseIds)
				.ToListAsync(cancellationToken);

			var warehouseSmallNodesIds = warehousesNodes
				.Select(wsn => wsn.Id)
				.ToArray();

			var nomenclaturesSmallNodesIds = nomenclaturesSmallNodes
				.Select(nsn => nsn.Id)
				.ToArray();

			var salesQuery = GetSalesQuery(
				unitOfWork,
				startDate,
				endDate,
				warehouseSmallNodesIds,
				nomenclaturesSmallNodesIds);

			var selfDeliverySales = GetSelfDeliverySalesQuery(
				unitOfWork,
				startDate,
				endDate,
				warehouseSmallNodesIds,
				nomenclaturesSmallNodesIds);

			var salesResult = 
				(await salesQuery.ToListAsync(cancellationToken))
				.Concat(await selfDeliverySales.ToListAsync(cancellationToken));

			var reportRows = new List<TurnoverOfWarehouseBalancesReportRow>();

			var grouppedSalesResult = salesResult
				.GroupBy(sr => (sr.WarehouseId, sr.NomenclatureId));

			var residuesAtDates = new Dictionary<DateTime, WarehouseResidueNode[]>();

			foreach(var slice in slices)
			{
				for(DateTime i = slice.StartDate; i < slice.EndDate; i = i.AddDays(1))
				{
					if(slice == slices.First())
					{
						residuesAtDates.Add(
							i,
							GetWarehousesBalanceAtAsync(
									unitOfWork,
									nomenclaturesSmallNodesIds,
									warehouseSmallNodesIds,
									i.LatestDayTime())
								.ToArray());
					}
					else
					{
						var diff = GetWarehouseResidueDiffsQuery(
								unitOfWork,
								nomenclaturesSmallNodesIds,
								warehouseSmallNodesIds,
								i,
								i.LatestDayTime())
							.ToArray();

						var newResidues = (WarehouseResidueNode[])residuesAtDates[i.AddDays(-1)].Clone();

						foreach(var residue in newResidues)
						{
							var currentDiff = diff.FirstOrDefault(dr => dr.NomenclatureId == residue.NomenclatureId && dr.WarehouseId == residue.WarehouseId);

							if(currentDiff is null)
							{
								continue;
							}

							residue.StockAmount += currentDiff.StockAmountDiff;
						}

						residuesAtDates.Add(
						i,
						newResidues);
					}

					if(cancellationToken.IsCancellationRequested)
					{
						throw new OperationCanceledException(cancellationToken);
					}
				}
			}

			foreach(var wsn in warehousesNodes)
			{
				foreach(var nsn in nomenclaturesSmallNodes)
				{
					var sliceValues = new List<string>(slices.Length);

					var warehouseToNomenclatureSalesGroup = grouppedSalesResult
						.FirstOrDefault(gsr =>
							gsr.Key.NomenclatureId == nsn.Id
							&& gsr.Key.WarehouseId == wsn.Id);

					var slicesValue = 0m;
					var slicesSales = 0m;

					foreach(var slice in slices)
					{
						var residueMedianDays = (slice.EndDate.Date - slice.StartDate).TotalDays + 1;

						var residuesInSlice = 0m;

						var residuesAtDate = residuesAtDates
							.Where(pair => pair.Key >= slice.StartDate
								&& pair.Key <= slice.EndDate)
							.SelectMany(x => x.Value)
							.Where(x => x.NomenclatureId == nsn.Id
								&& x.WarehouseId == wsn.Id)
							.Sum(x => x.StockAmount);

						residuesInSlice += residuesAtDate / (decimal)residueMedianDays;
						slicesValue += residuesInSlice * (decimal)residueMedianDays;

						if(warehouseToNomenclatureSalesGroup != null)
						{
							var sliceItems = warehouseToNomenclatureSalesGroup
								.Where(gi => gi.SaleDate <= slice.EndDate
									&& gi.SaleDate >= slice.StartDate);

							var sliceSalesSum = sliceItems
								.Sum(si => si.ActualCount);

							var sliceValue = "";

							if(sliceSalesSum is null
								|| sliceSalesSum == 0m)
							{
								var residueAtEndOfSlice = residuesAtDates
									.Where(pair => pair.Key >= slice.StartDate
										&& pair.Key <= slice.EndDate)
									.LastOrDefault()
									.Value
									.Where(x => x.NomenclatureId == nsn.Id
										&& x.WarehouseId == wsn.Id)
									.LastOrDefault()
									.StockAmount;

								sliceValue = $"{_noSalesPrefix}{residueAtEndOfSlice:##0.000}";
							}
							else
							{
								var medianValue = residuesInSlice * (decimal)residueMedianDays / sliceSalesSum.Value;

								slicesSales += sliceSalesSum.Value;
								sliceValue = medianValue.ToString("##0.000");
							}
							sliceValues.Add(sliceValue);
						}
						else
						{
							sliceValues.Add($"{_noSalesPrefix} {residuesInSlice:0.000}");
						}
					}

					if(slicesSales == 0)
					{
						slicesSales = 1;
					}

					reportRows.Add(new TurnoverOfWarehouseBalancesReportRow
					{
						WarehouseName = wsn.Name,
						NomanclatureName = nsn.Name,
						SliceValues = sliceValues,
						Total = slicesValue / slicesSales / slices.Length
					});
				}
			}

			return new TurnoverOfWarehouseBalancesReport(
				startDate,
				endDate,
				slices,
				reportRows);
		}

		private static IQueryable<SalesGenerationNode> GetSelfDeliverySalesQuery(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			int[] warehouseSmallNodesIds,
			int[] nomenclaturesSmallNodesIds) =>
			from order in unitOfWork.Session.Query<Order>()
			join selfDeliveryDocument in unitOfWork.Session.Query< SelfDeliveryDocument>()
			on order.Id equals selfDeliveryDocument.Order.Id
			join warehouse in unitOfWork.Session.Query<Warehouse>()
			on selfDeliveryDocument.Warehouse.Id equals warehouse.Id
			join orderItem in unitOfWork.Session.Query<OrderItem>()
			on order.Id equals orderItem.Order.Id
			join nomenclature in unitOfWork.Session.Query<Nomenclature>()
			on orderItem.Nomenclature.Id equals nomenclature.Id
			where order.DeliveryDate != null
				&& order.DeliveryDate <= endDate
				&& order.DeliveryDate >= startDate
				&& _orderStatuses.Contains(order.OrderStatus)
				&& warehouseSmallNodesIds.Contains(warehouse.Id)
				&& nomenclaturesSmallNodesIds.Contains(nomenclature.Id)
			select new SalesGenerationNode
			{
				SaleDate = order.DeliveryDate,
				WarehouseId = warehouse.Id,
				NomenclatureId = nomenclature.Id,
				NomenclatureName = nomenclature.Name,
				ActualCount = orderItem.ActualCount
			};

		private static IQueryable<NomenclatureGenerationNode> GetNomenclaturesQuery(
			IUnitOfWork unitOfWork,
			NomenclatureCategory[] includedNomenclatureCategoryIds,
			NomenclatureCategory[] excludedNomenclatureCategoryIds,
			int[] includedNomenclatureIds,
			int[] excludedNomenclatureIds,
			int?[] includedProductGroupIds,
			int?[] excludedProductGroupIds) =>
			from nomenclature in unitOfWork.Session.Query<Nomenclature>()
			join productGroup in unitOfWork.Session.Query<ProductGroup>()
			on nomenclature.ProductGroup.Id equals productGroup.Id
			where ((!includedNomenclatureIds.Any() && !nomenclature.IsArchive)
					|| includedNomenclatureIds.Contains(nomenclature.Id))
				&& (!excludedNomenclatureIds.Any()
					|| !excludedNomenclatureIds.Contains(nomenclature.Id))
				&& (!includedNomenclatureCategoryIds.Any()
					|| includedNomenclatureCategoryIds.Contains(nomenclature.Category))
				&& (!excludedNomenclatureCategoryIds.Any()
					|| !excludedNomenclatureCategoryIds.Contains(nomenclature.Category))
				&& ((!includedProductGroupIds.Any() && !productGroup.IsArchive)
					|| includedProductGroupIds.Contains(nomenclature.ProductGroup.Id))
				&& (!excludedProductGroupIds.Any()
					|| !excludedProductGroupIds.Contains(nomenclature.ProductGroup.Id))
			select new NomenclatureGenerationNode
			{
				Id = nomenclature.Id,
				Name = nomenclature.Name
			};

		private static IQueryable<WarehouseGenerationNode> GetWarehousesQuery(
			IUnitOfWork unitOfWork,
			int[] includedWarehouseIds,
			int[] excludedWarehouseIds) =>
			from warehouse in unitOfWork.Session.Query<Warehouse>()
			where ((!includedWarehouseIds.Any() && !warehouse.IsArchive)
					|| includedWarehouseIds.Contains(warehouse.Id))
				&& (!excludedWarehouseIds.Any()
					|| !excludedWarehouseIds.Contains(warehouse.Id))
			select new WarehouseGenerationNode
			{
				Id = warehouse.Id,
				Name = warehouse.Name
			};

		private static IQueryable<SalesGenerationNode> GetSalesQuery(
			IUnitOfWork unitOfWork,
			DateTime startDate,
			DateTime endDate,
			int[] warehouseSmallNodesIds,
			int[] nomenclaturesSmallNodesIds) =>
			from order in unitOfWork.Session.Query<Order>()
			join orderItem in unitOfWork.Session.Query<OrderItem>()
			on order.Id equals orderItem.Order.Id
			join nomenclature in unitOfWork.Session.Query<Nomenclature>()
			on orderItem.Nomenclature.Id equals nomenclature.Id

			let warehouseId =
				(int?)(from routeListAddress in unitOfWork.Session.Query<RouteListItem>()
				 join carLoadDocument in unitOfWork.Session.Query<CarLoadDocument>()
				 on routeListAddress.RouteList.Id equals carLoadDocument.RouteList.Id
				 where routeListAddress.Order.Id == order.Id
					 && warehouseSmallNodesIds.Contains(carLoadDocument.Warehouse.Id)
				 select carLoadDocument.Warehouse.Id)
				.FirstOrDefault()

			where order.DeliveryDate != null
				&& order.DeliveryDate <= endDate
				&& order.DeliveryDate >= startDate
				&& warehouseId != null
				&& _orderStatuses.Contains(order.OrderStatus)
				&& nomenclaturesSmallNodesIds.Contains(nomenclature.Id)
			select new SalesGenerationNode
			{
				SaleDate = order.DeliveryDate,
				WarehouseId = warehouseId.Value,
				NomenclatureId = nomenclature.Id,
				NomenclatureName = nomenclature.Name,
				ActualCount = orderItem.ActualCount
			};

		//private static async Task<WarehouseResidueNode[]> GetWarehousesBalanceAtAsync(
		//	IUnitOfWork unitOfWork,
		//	int[] nomenclatureIds,
		//	int[] warehouseIds,
		//	DateTime dateTime,
		//	CancellationToken cancellationToken)
		//{
		//	WarehouseBulkGoodsAccountingOperation operationAlias = null;
		//	Nomenclature nomenclatureAlias = null;
		//	WarehouseResidueNode resultAlias = null;

		//	var balanceProjection = Projections.SqlFunction(
		//		new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
		//		NHibernateUtil.Decimal,
		//		Projections.Sum(() => operationAlias.Amount));

		//	var result = await unitOfWork.Session.QueryOver(() => nomenclatureAlias)
		//		.JoinEntityAlias(() => operationAlias,
		//			() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
		//			JoinType.InnerJoin)
		//		.WhereRestrictionOn(() => nomenclatureAlias.Id).IsInG(nomenclatureIds)
		//		.AndRestrictionOn(() => operationAlias.Warehouse.Id).IsInG(warehouseIds)
		//		.And(Restrictions.Le(Projections.Property(() => operationAlias.OperationTime), dateTime))
		//		.SelectList(list => list
		//			.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
		//			.Select(() => operationAlias.Warehouse.Id).WithAlias(() => resultAlias.WarehouseId)
		//			.Select(balanceProjection).WithAlias(() => resultAlias.StockAmount))
		//		.TransformUsing(Transformers.AliasToBean<WarehouseResidueNode>())
		//		.ListAsync<WarehouseResidueNode>(cancellationToken);

		//	return result.ToArray();
		//}

		private static IQueryable<WarehouseResidueNode> GetWarehousesBalanceAtAsync(
			IUnitOfWork unitOfWork,
			int[] nomenclatureIds,
			int[] warehouseIds,
			DateTime endDateTime) =>
			from operation in unitOfWork.Session.Query<WarehouseBulkGoodsAccountingOperation>()
			where operation.OperationTime <= endDateTime
			  && nomenclatureIds.Contains(operation.Nomenclature.Id)
			  && warehouseIds.Contains(operation.Warehouse.Id)
			group operation by new { WarehouseId = operation.Warehouse.Id, NomenclatureId = operation.Nomenclature.Id } into warehouseNomenclatureGroup
			select new WarehouseResidueNode
			{
				NomenclatureId = warehouseNomenclatureGroup.Key.NomenclatureId,
				WarehouseId = warehouseNomenclatureGroup.Key.WarehouseId,
				StockAmount = warehouseNomenclatureGroup.Sum(g => g.Amount)
			};

		private static IQueryable<WarehouseResidueDiffNode> GetWarehouseResidueDiffsQuery(
			IUnitOfWork unitOfWork,
			int[] nomenclatureIds,
			int[] warehouseIds,
			DateTime startDateTime,
			DateTime endDateTime) =>
			from operation in unitOfWork.Session.Query<WarehouseBulkGoodsAccountingOperation>()
			where operation.OperationTime > startDateTime
			  && operation.OperationTime <= endDateTime
			  && nomenclatureIds.Contains(operation.Nomenclature.Id)
			  && warehouseIds.Contains(operation.Warehouse.Id)
			group operation by new { WarehouseId = operation.Warehouse.Id, NomenclatureId = operation.Nomenclature.Id } into warehouseNomenclatureGroup
			select new WarehouseResidueDiffNode
			{
				NomenclatureId = warehouseNomenclatureGroup.Key.NomenclatureId,
				WarehouseId = warehouseNomenclatureGroup.Key.WarehouseId,
				StockAmountDiff = warehouseNomenclatureGroup.Sum(g => g.Amount)
			};
	}
}

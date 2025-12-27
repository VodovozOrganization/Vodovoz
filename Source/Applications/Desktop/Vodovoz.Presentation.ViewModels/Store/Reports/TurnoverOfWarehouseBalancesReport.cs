using DateTimeHelpers;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
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

			SliceTitles = slices.Select(s => s.ToString()).ToList();
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

			slices.First().StartDate = startDate;
			slices.Last().EndDate = endDate.LatestDayTime();

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


			var grouppedSalesResult = salesResult
				.GroupBy(sr => (sr.WarehouseId, sr.NomenclatureId));

			var residuesAtDates = await ProcessResidues(
				unitOfWork,
				slices,
				nomenclaturesSmallNodesIds,
				warehouseSmallNodesIds,
				cancellationToken);

			var reportRows = ProcessReportRows(
				slices,
				nomenclaturesSmallNodes,
				warehousesNodes,
				grouppedSalesResult,
				residuesAtDates,
				cancellationToken);

			return new TurnoverOfWarehouseBalancesReport(
				startDate,
				endDate,
				slices,
				reportRows);
		}

		private static List<TurnoverOfWarehouseBalancesReportRow> ProcessReportRows(
			IDateTimeSlice[] slices,
			List<NomenclatureGenerationNode> nomenclaturesSmallNodes,
			List<WarehouseGenerationNode> warehousesNodes,
			IEnumerable<IGrouping<(int WarehouseId, int NomenclatureId), SalesGenerationNode>> grouppedSalesResult,
			Dictionary<DateTime, WarehouseResidueNode[]> residuesAtDates,
			CancellationToken cancellationToken)
		{
			var reportRows = new List<TurnoverOfWarehouseBalancesReportRow>();

			foreach(var wsn in warehousesNodes)
			{
				ProcessWarehouse(
					reportRows,
					wsn,
					slices,
					nomenclaturesSmallNodes,
					grouppedSalesResult,
					residuesAtDates,
					cancellationToken);


				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}

			return reportRows;
		}

		private static void ProcessWarehouse(
			List<TurnoverOfWarehouseBalancesReportRow> reportRows,
			WarehouseGenerationNode wsn,
			IDateTimeSlice[] slices,
			List<NomenclatureGenerationNode> nomenclaturesSmallNodes,
			IEnumerable<IGrouping<(int WarehouseId, int NomenclatureId), SalesGenerationNode>> grouppedSalesResult,
			Dictionary<DateTime, WarehouseResidueNode[]> residuesAtDates,
			CancellationToken cancellationToken)
		{
			foreach(var nsn in nomenclaturesSmallNodes)
			{
				ProcessNomenclature(
					reportRows,
					wsn,
					slices,
					grouppedSalesResult,
					residuesAtDates,
					nsn,
					cancellationToken);

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}
		}

		private static void ProcessNomenclature(
			List<TurnoverOfWarehouseBalancesReportRow> reportRows,
			WarehouseGenerationNode wsn,
			IDateTimeSlice[] slices,
			IEnumerable<IGrouping<(int WarehouseId, int NomenclatureId), SalesGenerationNode>> grouppedSalesResult,
			Dictionary<DateTime, WarehouseResidueNode[]> residuesAtDates,
			NomenclatureGenerationNode nsn,
			CancellationToken cancellationToken)
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
				ProcessNomenclatureByDateTimeSlice(
					wsn,
					residuesAtDates,
					nsn,
					sliceValues,
					warehouseToNomenclatureSalesGroup,
					ref slicesValue,
					ref slicesSales,
					slice);

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}

			AddNomenclatureTotal(
				reportRows,
				wsn,
				nsn,
				sliceValues,
				slicesValue,
				slicesSales);
		}

		private static void ProcessNomenclatureByDateTimeSlice(
			WarehouseGenerationNode wsn,
			Dictionary<DateTime, WarehouseResidueNode[]> residuesAtDates,
			NomenclatureGenerationNode nsn,
			List<string> sliceValues,
			IGrouping<(int WarehouseId, int NomenclatureId), SalesGenerationNode> warehouseToNomenclatureSalesGroup,
			ref decimal slicesValue,
			ref decimal slicesSales,
			IDateTimeSlice slice)
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

			var lastResidue = residuesAtDates
				.Where(pair => pair.Key >= slice.StartDate
					&& pair.Key <= slice.EndDate)
				.LastOrDefault();

			var residueOfCurrentMomenclatureAndWarehouse = lastResidue
				.Value
				.Where(x => x.NomenclatureId == nsn.Id
					&& x.WarehouseId == wsn.Id)
				.LastOrDefault();

			var residueAtEndOfSlice =
				residueOfCurrentMomenclatureAndWarehouse?.StockAmount ?? 0m;

			if(warehouseToNomenclatureSalesGroup == null)
			{
				sliceValues.Add($"{_noSalesPrefix} {residueAtEndOfSlice:# ##0.000}");
				return;
			}

			var sliceItems = warehouseToNomenclatureSalesGroup
				.Where(gi => gi.SaleDate <= slice.EndDate
					&& gi.SaleDate >= slice.StartDate);

			var sliceSalesSum = sliceItems
				.Sum(si => si.ActualCount);

			var sliceValue = "";

			if(sliceSalesSum is null
				|| sliceSalesSum == 0m)
			{
				sliceValue = $"{_noSalesPrefix}{residueAtEndOfSlice:# ##0.000}";
			}
			else
			{
				var medianValue = residuesInSlice * (decimal)residueMedianDays / sliceSalesSum.Value;

				slicesSales += sliceSalesSum.Value;
				sliceValue = medianValue.ToString("# ##0.000");
			}

			sliceValues.Add(sliceValue);
		}

		private static void AddNomenclatureTotal(
			List<TurnoverOfWarehouseBalancesReportRow> reportRows,
			WarehouseGenerationNode wsn,
			NomenclatureGenerationNode nsn,
			List<string> sliceValues,
			decimal slicesValue,
			decimal slicesSales)
		{
			string total;

			if(slicesSales == 0)
			{
				total = "Продаж не было";
			}
			else
			{
				total = $"{slicesValue / slicesSales:# ##0.000}";
			}

			reportRows.Add(new TurnoverOfWarehouseBalancesReportRow
			{
				WarehouseName = wsn.Name,
				NomanclatureName = nsn.Name,
				SliceValues = sliceValues,
				Total = total
			});
		}

		private static async Task<Dictionary<DateTime, WarehouseResidueNode[]>> ProcessResidues(
			IUnitOfWork unitOfWork,
			IDateTimeSlice[] slices,
			int[] nomenclaturesSmallNodesIds,
			int[] warehouseSmallNodesIds,
			CancellationToken cancellationToken)
		{
			var residuesAtDates = new Dictionary<DateTime, WarehouseResidueNode[]>();

			foreach(var slice in slices)
			{
				ProcessResiduesDateTimeSlice(
					unitOfWork,
					slices,
					nomenclaturesSmallNodesIds,
					warehouseSmallNodesIds,
					residuesAtDates,
					slice,
					cancellationToken);

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}

			return await Task.FromResult(residuesAtDates);
		}

		private static void ProcessResiduesDateTimeSlice(
			IUnitOfWork unitOfWork,
			IDateTimeSlice[] slices,
			int[] nomenclaturesSmallNodesIds,
			int[] warehouseSmallNodesIds,
			Dictionary<DateTime, WarehouseResidueNode[]> residuesAtDates,
			IDateTimeSlice slice,
			CancellationToken cancellationToken)
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

					var newResidues = new List<WarehouseResidueNode>();

					var previousResidues = residuesAtDates[i.AddDays(-1)];

					foreach(var previousResidue in previousResidues)
					{
						newResidues.Add(new WarehouseResidueNode
						{
							NomenclatureId = previousResidue.NomenclatureId,
							WarehouseId = previousResidue.WarehouseId,
							StockAmount = previousResidue.StockAmount
						});

						if(cancellationToken.IsCancellationRequested)
						{
							throw new OperationCanceledException(cancellationToken);
						}
					}

					foreach(var residue in newResidues)
					{
						var currentDiff = diff
							.FirstOrDefault(dr =>
								dr.NomenclatureId == residue.NomenclatureId
								&& dr.WarehouseId == residue.WarehouseId);

						if(currentDiff is null)
						{
							continue;
						}

						residue.StockAmount += currentDiff.StockAmountDiff;

						if(cancellationToken.IsCancellationRequested)
						{
							throw new OperationCanceledException(cancellationToken);
						}
					}

					residuesAtDates.Add(
						i,
						newResidues.ToArray());
				}

				if(cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
			}
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

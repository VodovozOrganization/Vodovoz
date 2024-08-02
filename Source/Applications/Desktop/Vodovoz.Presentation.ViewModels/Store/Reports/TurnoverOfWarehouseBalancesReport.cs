using DateTimeHelpers;
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
				excludedWarehouseIds,
				cancellationToken)
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
					residuesAtDates.Add(i, GetWarehousesBalanceAt(unitOfWork, nomenclaturesSmallNodesIds, warehouseSmallNodesIds, i.LatestDayTime()));

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

					foreach(var slice in slices)
					{
						var residueMedianDays = (slice.EndDate - slice.StartDate).TotalDays;

						var residuesInSlice = 0m;

						var residuesAtDate = residuesAtDates
							.Where(pair => pair.Key >= slice.StartDate
								&& pair.Key <= slice.EndDate)
							.SelectMany(x => x.Value)
							.Where(x => x.NomenclatureId == nsn.Id
								&& x.WarehouseId == wsn.Id)
							.Sum(x => x.StockAmount);

						residuesInSlice += residuesAtDate / (decimal)residueMedianDays;

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
								slicesValue += medianValue;
								sliceValue = medianValue.ToString();
							}

							sliceValues.Add(sliceValue);
						}
						else
						{
							sliceValues.Add($"{_noSalesPrefix} 0.000");
						}
					}

					reportRows.Add(new TurnoverOfWarehouseBalancesReportRow
					{
						WarehouseName = wsn.Name,
						NomanclatureName = nsn.Name,
						SliceValues = sliceValues,
						Total = slicesValue / slices.Length
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
			where (!includedNomenclatureIds.Any()
					|| includedNomenclatureIds.Contains(nomenclature.Id))
				&& (!excludedNomenclatureIds.Any()
					|| !excludedNomenclatureIds.Contains(nomenclature.Id))
				&& (!includedNomenclatureCategoryIds.Any()
					|| includedNomenclatureCategoryIds.Contains(nomenclature.Category))
				&& (!excludedNomenclatureCategoryIds.Any()
					|| !excludedNomenclatureCategoryIds.Contains(nomenclature.Category))
				&& (!includedProductGroupIds.Any()
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
			int[] excludedWarehouseIds,
			CancellationToken cancellationToken) =>
			from warehouse in unitOfWork.Session.Query<Warehouse>()
			where (!includedWarehouseIds.Any()
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
			join routeListAddress in unitOfWork.Session.Query<RouteListItem>()
			on order.Id equals routeListAddress.Order.Id
			join routeList in unitOfWork.Session.Query<RouteList>()
			on routeListAddress.RouteList.Id equals routeList.Id
			join carLoadDocument in unitOfWork.Session.Query<CarLoadDocument>()
			on routeList.Id equals carLoadDocument.RouteList.Id
			join warehouse in unitOfWork.Session.Query<Warehouse>()
			on carLoadDocument.Warehouse.Id equals warehouse.Id
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

		private static WarehouseResidueNode[] GetWarehousesBalanceAt(
			IUnitOfWork unitOfWork,
			int[] nomenclatureIds,
			int[] warehouseIds,
			DateTime dateTime)
		{
			WarehouseBulkGoodsAccountingOperation operationAlias = null;
			Nomenclature nomenclatureAlias = null;
			WarehouseResidueNode resultAlias = null;

			var balanceProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "IFNULL(?1, 0)"),
				NHibernateUtil.Decimal,
				Projections.Sum(() => operationAlias.Amount));

			var result = unitOfWork.Session.QueryOver(() => nomenclatureAlias)
				.JoinEntityAlias(() => operationAlias,
					() => nomenclatureAlias.Id == operationAlias.Nomenclature.Id,
					JoinType.InnerJoin)
				.WhereRestrictionOn(() => nomenclatureAlias.Id).IsInG(nomenclatureIds)
				.AndRestrictionOn(() => operationAlias.Warehouse.Id).IsInG(warehouseIds)
				.And(Restrictions.Le(Projections.Property(() => operationAlias.OperationTime), dateTime))
				.SelectList(list => list
					.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.NomenclatureId)
					.Select(() => operationAlias.Warehouse.Id).WithAlias(() => resultAlias.WarehouseId)
					.Select(balanceProjection).WithAlias(() => resultAlias.StockAmount))
				.TransformUsing(Transformers.AliasToBean<WarehouseResidueNode>())
				.List<WarehouseResidueNode>()
				.ToArray();

			return result;
		}
	}
}

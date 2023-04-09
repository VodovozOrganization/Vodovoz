using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		private readonly IEnumerable<IGrouping<(int NomenclatureId, int SubdivisionId), SalesDataNode>> _sales;
		private readonly IEnumerable<IGrouping<(int NomenclatureId, int WarehouseId), ResidueDataNode>> _residues;
		private readonly HeaderRow _header;
		private readonly SubHeaderRow _subHeader;
		private TotalRow _totalRow;
		private readonly List<DisplayRow> _displayRows = new List<DisplayRow>();
		private readonly List<int> _warehouseIndexes;
		private readonly List<int> _subdivisionIndexes;
		private readonly List<int> _productGroupIndexes;
		private readonly Dictionary<int, IEnumerable<int>> _productGroupNomenclatures;
		private readonly List<Row> _rows = new List<Row>();
		private SalesBySubdivisionsAnalitycsReport(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses,
			IDictionary<int, string> warehouses,
			IDictionary<int, string> nomenclatures,
			IDictionary<int, string> productGroups,
			IDictionary<int, string> subdivisions,
			IEnumerable<SalesDataNode> sales,
			IEnumerable<ResidueDataNode> residues)
		{
			FirstPeriodStartDate = firstPeriodStartDate;
			FirstPeriodEndDate = firstPeriodEndDate;
			SecondPeriodStartDate = secondPeriodStartDate;
			SecondPeriodEndDate = secondPeriodEndDate;
			SplitByNomenclatures = splitByNomenclatures;
			SplitBySubdivisions = splitBySubdivisions;
			SplitByWarehouses = splitByWarehouses;

			Subdivisions = subdivisions;
			_subdivisionIndexes = subdivisions.Keys.ToList();
			Warehouses = warehouses;
			_warehouseIndexes = warehouses.Keys.ToList();
			Nomenclatures = nomenclatures;
			ProductGroups = productGroups;
			_productGroupIndexes = ProductGroups.Keys.ToList();
			_productGroupNomenclatures = new Dictionary<int, IEnumerable<int>>();
			_sales = sales.GroupBy(x => (x.NomenclatureId, x.SubdivisionId));
			_residues = residues.GroupBy(x => (x.NomenclatureId, x.WarehouseId));

			foreach(var productGroupId in _productGroupIndexes)
			{
				_productGroupNomenclatures.Add(
					//productGroupId,
					//_sales.SelectMany(x =>
					//		x.Where(y => y.ProductGroupId == productGroupId)
					//		 .Select(y => y.NomenclatureId).Distinct())
					//	.Concat(_residues.
					//		SelectMany(x =>
					//			x.Where(y => y.ProductGroupId == productGroupId)
					//			.Select(y => y.NomenclatureId).Distinct()))
					//	.Distinct());
					productGroupId,
					_sales.SelectMany(x =>
							x.Where(y => y.ProductGroupId == productGroupId)
							 .Select(y => y.NomenclatureId).Distinct()));
			}

			var subdivisionsList = new List<string>
			{
				string.Empty,
				string.Empty
			};

			foreach(var subdivision in Subdivisions)
			{
				subdivisionsList.Add(subdivision.Value);
				subdivisionsList.Add(string.Empty);
			}

			var warehousesList = new List<string>
			{
				"Общий остаток на складах"
			};

			warehousesList.AddRange(Warehouses.Values);

			_header = new HeaderRow
			{
				Title = "Общий отчет по продажам",
				SubdivisionsTitles = subdivisionsList,
				WarehousesTitles = warehousesList
			};

			var dynamicsSubheaderRows = new List<string>();

			for(int i = 0; i < Subdivisions.Count + 1; i++)
			{
				dynamicsSubheaderRows.Add("Количество");
				dynamicsSubheaderRows.Add("Сумма");
			}

			for(int i = 0; i < Warehouses.Count + 1; i++)
			{
				dynamicsSubheaderRows.Add("Количество");
			}

			_subHeader = new SubHeaderRow
			{
				Title = "Группы товаров",
				DynamicColumns = dynamicsSubheaderRows
			};

			_displayRows.Add(_header);
			_displayRows.Add(_subHeader);

			Process();

			CreatedAt = DateTime.Now;
		}

		private void Process()
		{
			if(SplitByNomenclatures)
			{
				_totalRow = new TotalRow()
				{
					SubTotalRows = new List<SubTotalRow>(),
					SalesBySubdivision = Enumerable.Repeat(new AmountPricePair { Amount = 0m, Price = 0m }, Subdivisions.Count + 1).ToList(),
					ResiduesByWarehouse = Enumerable.Repeat(0m, Warehouses.Count + 1).ToList()
				};

				for(var i = 0; i < _productGroupIndexes.Count; i++)
				{
					_totalRow.SubTotalRows.Add(ProcessProductGroup(_productGroupIndexes[i]));
				}

				for(int i = 0; i < _subdivisionIndexes.Count + 1; i++)
				{
					_totalRow.SalesBySubdivision[i].Amount = _totalRow.SubTotalRows.Sum(x => x.SalesBySubdivision[i].Amount);
					_totalRow.SalesBySubdivision[i].Price = _totalRow.SubTotalRows.Sum(x => x.SalesBySubdivision[i].Price);
				}

				for(int i = 0; i < _warehouseIndexes.Count + 1; i++)
				{
					_totalRow.ResiduesByWarehouse[i] = _totalRow.SubTotalRows.Sum(x => x.ResiduesByWarehouse[i]);
				}

				_rows.Add(_totalRow);
				_displayRows.Add(_totalRow);
			}
		}

		private SubTotalRow ProcessProductGroup(int productGroupId)
		{
			SubTotalRow result = new SubTotalRow
			{
				Title = ProductGroups[productGroupId],
				SalesBySubdivision = Enumerable.Repeat(new AmountPricePair { Amount = 0m, Price = 0m }, Subdivisions.Count + 1).ToList(),
				ResiduesByWarehouse = Enumerable.Repeat(0m, Warehouses.Count + 1).ToList()
			};

			var nomenclatureRows = new List<Row>();

			if(SplitBySubdivisions)
			{
				nomenclatureRows = ProcessNomenclatureGroup(productGroupId);
				result.NomenclatureRows = nomenclatureRows;

				for(int i = 0; i < _subdivisionIndexes.Count + 1; i++)
				{
					result.SalesBySubdivision[i].Amount = nomenclatureRows.Sum(x => x.SalesBySubdivision[i].Amount);
					result.SalesBySubdivision[i].Price = nomenclatureRows.Sum(x => x.SalesBySubdivision[i].Price);
				}
			}

			if(SplitByWarehouses)
			{
				for(int i = 0; i < _warehouseIndexes.Count + 1; i++)
				{
					result.ResiduesByWarehouse[i] = nomenclatureRows.Sum(x => x.ResiduesByWarehouse[i]);
				}
			}

			_rows.Add(result);
			_rows.AddRange(nomenclatureRows);
			_displayRows.Add(result);
			_displayRows.AddRange(nomenclatureRows);

			return result;
		}

		private List<Row> ProcessNomenclatureGroup(int productGroupId)
		{
			var nomenclatureRows = new List<Row>();

			var nomenclatureIds = _productGroupNomenclatures[productGroupId];

			foreach(var nomenclatureId in nomenclatureIds)
			{
				nomenclatureRows.Add(new Row
				{
					Title = Nomenclatures[nomenclatureId],
					SalesBySubdivision = ProcessSubdivisionsNomenclaturesSales(nomenclatureId),
					ResiduesByWarehouse = ProcessWarehousesResidues(nomenclatureId)
				});
			}

			return nomenclatureRows;
		}

		private IList<AmountPricePair> ProcessSubdivisionsNomenclaturesSales(int nomenclatureId)
		{
			var result = Enumerable.Repeat(new AmountPricePair { Amount = 0m, Price = 0m }, Subdivisions.Count + 1).ToList();

			var salesCount = _sales.Count();

			foreach(var group in _sales)
			{
				if(group.Key.NomenclatureId == nomenclatureId)
				{
					result[_subdivisionIndexes.IndexOf(group.Key.SubdivisionId) + 1].Amount = group.Sum(x => x.Amount);
					result[_subdivisionIndexes.IndexOf(group.Key.SubdivisionId) + 1].Price = group.Sum(x => x.Price);
				}
			}

			result[0] = new AmountPricePair
			{
				Amount = result.Skip(1).Sum(x => x.Amount),
				Price = result.Skip(1).Sum(x => x.Price)
			};

			return result;
		}

		private IList<decimal> ProcessWarehousesResidues(int nomenclatureId)
		{
			var result = Enumerable.Repeat(0m, Warehouses.Count + 1).ToList();

			foreach(var group in _residues)
			{
				if(group.Key.NomenclatureId == nomenclatureId)
				{
					result[_warehouseIndexes.IndexOf(group.Key.WarehouseId) + 1] = group.Sum(x => x.Residue);
				}
			}

			result[0] = result.Skip(1).Sum();

			return result;
		}

		public DateTime FirstPeriodStartDate { get; }

		public DateTime FirstPeriodEndDate { get; }

		public DateTime? SecondPeriodStartDate { get; }

		public DateTime? SecondPeriodEndDate { get; }

		public bool SplitByNomenclatures { get; }

		public bool SplitBySubdivisions { get; }

		public bool SplitByWarehouses { get; }

		public IDictionary<int, string> Subdivisions { get; set; }

		public IDictionary<int, string> Warehouses { get; }

		public IDictionary<int, string> Nomenclatures { get; }

		public IDictionary<int, string> ProductGroups { get; }

		public DateTime CreatedAt { get; }

		public TotalRow Total => _totalRow;

		public List<DisplayRow> DisplayRows => _displayRows;

		public List<Row> Rows => _rows;

		public static async Task<SalesBySubdivisionsAnalitycsReport> Create(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses,
			Func<DateTime, DateTime, DateTime?, DateTime?, bool, bool, bool, IEnumerable<SalesDataNode>> retrieveFunction,
			Func<DateTime, IEnumerable<ResidueDataNode>> warehouseResiduesFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getNomenclaturesFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetProductGroupsFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetSubdivisionsFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetWarehousesFunc)
		{
			if(retrieveFunction is null)
			{
				throw new ArgumentNullException(nameof(retrieveFunction));
			}

			ValidateParameters(
				firstPeriodStartDate,
				firstPeriodEndDate,
				secondPeriodStartDate,
				secondPeriodEndDate,
				splitByWarehouses);

			IEnumerable<SalesDataNode> dataNodes = retrieveFunction(
				firstPeriodStartDate,
				firstPeriodEndDate,
				secondPeriodStartDate,
				secondPeriodEndDate,
				splitByNomenclatures,
				splitBySubdivisions,
				splitByWarehouses);

			IEnumerable<ResidueDataNode> residueDataNodes = warehouseResiduesFunc(firstPeriodEndDate);

			IDictionary<int, string> warehouses = await getGetWarehousesFunc(
				residueDataNodes.Where(x => dataNodes.Any(y => y.NomenclatureId == x.NomenclatureId)).Select(x => x.WarehouseId).Distinct());
			//IDictionary<int, string> nomenclatures = await getNomenclaturesFunc(
			//	dataNodes.Select(x => x.NomenclatureId)
			//		.Concat(residueDataNodes.Select(x => x.NomenclatureId))
			//		.Distinct());
			IDictionary<int, string> nomenclatures = await getNomenclaturesFunc(
				dataNodes.Select(x => x.NomenclatureId)
					.Concat(residueDataNodes.Select(x => x.NomenclatureId))
					.Distinct());
			//IDictionary<int, string> productGroups = await getGetProductGroupsFunc(
			//	dataNodes.Select(x => x.ProductGroupId)
			//		.Concat(residueDataNodes.Select(x => x.ProductGroupId))
			//		.Distinct());
			IDictionary<int, string> productGroups = await getGetProductGroupsFunc(dataNodes.Select(x => x.ProductGroupId).Distinct());
			IDictionary<int, string> subdivisions = await getGetSubdivisionsFunc(dataNodes.Select(x => x.SubdivisionId).Distinct());

			return new SalesBySubdivisionsAnalitycsReport(
				firstPeriodStartDate,
				firstPeriodEndDate,
				secondPeriodStartDate,
				secondPeriodEndDate,
				splitByNomenclatures,
				splitBySubdivisions,
				splitByWarehouses,
				warehouses,
				nomenclatures,
				productGroups,
				subdivisions,
				dataNodes,
				residueDataNodes);
		}

		private static void ValidateParameters(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByWarehouses)
		{
			if(splitByWarehouses && (secondPeriodStartDate != null || secondPeriodEndDate != null))
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с двумя периодами",
					nameof(splitByWarehouses));
			}

			if(splitByWarehouses
				&& (firstPeriodEndDate - firstPeriodStartDate).TotalDays > 1)
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с интервалом более одного дня",
					nameof(splitByWarehouses));
			}
		}

		public class AmountPricePair
		{
			public decimal Amount { get; set; }

			public decimal Price { get; set; }
		}
	}
}

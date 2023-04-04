using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		private readonly IEnumerable<SalesDataNode> _sales;
		private readonly IEnumerable<ResidueDataNode> _residues;
		private readonly HeaderRow _header;
		private readonly SubHeaderRow _subHeader;
		private TotalRow _totalRow;
		private readonly List<DisplayRow> _displayRows = new List<DisplayRow>();

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
			Warehouses = warehouses;
			Nomenclatures = nomenclatures;
			ProductGroups = productGroups;
			_sales = sales;
			_residues = residues;

			var subdivisionsList = new List<string>
			{
				"Итого"
			};

			subdivisionsList.AddRange(Subdivisions.Values);

			var warehousesList = new List<string>
			{
				"Остатки"
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
				DynamicRows = dynamicsSubheaderRows
			};

			_displayRows.Add(_header);
			_displayRows.Add(_subHeader);

			Process();

			CreatedAt = DateTime.Now;
		}

		private void Process()
		{
			_totalRow = new TotalRow()
			{
				SubTotalRows = Enumerable.Repeat<SubTotalRow>(null, ProductGroups.Count).ToList()
			};

			for(var i = 0; i < ProductGroups.Count; i++)
			{
				_totalRow.SubTotalRows[i] = ProcessProductGroup(ProductGroups.Keys.ElementAt(i));
			}
		}

		private SubTotalRow ProcessProductGroup(int productGroupId)
		{
			var nomenclatureGroup = _sales.Where(x => x.ProductGroupId == productGroupId);
			var nomenclatureIds = nomenclatureGroup.Select(x => x.NomenclatureId).Distinct();

			SubTotalRow result = new SubTotalRow
			{
				Title = ProductGroups[productGroupId],
				SalesBySubdivision = ProcessSubdivisionsProductGroupSales(productGroupId),
				ResiduesByWarehouse = Enumerable.Repeat(0m, Warehouses.Count + 1).ToList()
			};

			List<Row> nomenclatureRows = ProcessNomenclatureGroup();

			for(int i = 0; i < Warehouses.Count; i++)
			{
				var warehouseId = Warehouses.Keys.ElementAt(i);

				result.ResiduesByWarehouse[i] = 0;
			}

			result.ResiduesByWarehouse[0] = result.ResiduesByWarehouse.Sum();

			result.NomenclatureRows = nomenclatureRows;
			_displayRows.Add(result);
			_displayRows.AddRange(nomenclatureRows);

			return result;
		}

		private List<Row> ProcessNomenclatureGroup()
		{
			var nomenclatureRows = Enumerable.Repeat<Row>(null, Nomenclatures.Count).ToList();

			for(int i = 0; i < Nomenclatures.Count; i++)
			{
				var nomenclatureId = Nomenclatures.Keys.ElementAt(i);

				nomenclatureRows[i] = new Row
				{
					Title = Nomenclatures[nomenclatureId],
					SalesBySubdivision = ProcessSubdivisionsNomenclaturesSales(nomenclatureId),
					ResiduesByWarehouse = ProcessWarehousesResidues(nomenclatureId)
				};
			}

			return nomenclatureRows;
		}

		private IList<(decimal Amount, decimal Price)> ProcessSubdivisionsProductGroupSales(int productGroupId)
		{
			var result = Enumerable.Repeat<(decimal Amount, decimal Price)>((0m, 0m), Subdivisions.Count + 1).ToList();

			for(var i = 0; i < Subdivisions.Count; i++)
			{
				result[i + 1] = ProcessSubdivisionProductGroupSales(productGroupId, Subdivisions.Keys.ElementAt(i));
			}

			result[0] = (result.Sum(x => x.Amount), result.Sum(x => x.Price));

			return result;
		}

		private (decimal Amount, decimal Price) ProcessSubdivisionProductGroupSales(int productGroupId, int subdivisionId)
		{
			var rows = _sales.Where(x => x.SubdivisionId == subdivisionId && x.ProductGroupId == productGroupId);

			return (rows.Sum(x => x.Amount), rows.Sum(x => x.Price));
		}

		private IList<(decimal Amount, decimal Price)> ProcessSubdivisionsNomenclaturesSales(int nomenclatureId)
		{
			var result = Enumerable.Repeat<(decimal Amount, decimal Price)>((0m, 0m), Subdivisions.Count + 1).ToList();

			for(var i = 0; i < Subdivisions.Count; i++)
			{
				var salesCount = _sales.Count();

				var subdivisionId = Subdivisions.Keys.ElementAt(i);

				foreach(var sale in _sales)
				{
					if(sale.NomenclatureId == nomenclatureId && sale.SubdivisionId == subdivisionId)
					{
						result[i + 1] = (result[i + 1].Amount + sale.Amount, result[i + 1].Price + sale.Price);
					}
				}
			}

			result[0] = (result.Sum(x => x.Amount), result.Sum(x => x.Price));

			return result;
		}

		private IList<decimal> ProcessWarehousesResidues(int nomenclatureId)
		{
			var result = Enumerable.Repeat(0m, Warehouses.Count + 1).ToList();

			for(int i = 0; i < Warehouses.Count; i++)
			{
				var warehouseId = Warehouses.Keys.ElementAt(i);

				foreach(var residue in _residues)
				{
					if(residue.WarehouseId == warehouseId && residue.NomenclatureId == nomenclatureId)
					{
						result[i] += residue.Residue;
					}
				}
			}

			result[0] = result.Sum();

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

		public List<DisplayRow> DisplayRows => _displayRows;

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

			//IDictionary<int, string> warehouses = null;
			//IDictionary<int, string> nomenclatures = null;
			//IDictionary<int, string> productGroups = null;
			//IDictionary<int, string> subdivisions = null;

			//await Task.WhenAll(
			//	Task.Run(async () => warehouses = await getGetWarehousesFunc(residueDataNodes.Select(x => x.WarehouseId).Distinct())),
			//	Task.Run(async () => nomenclatures = await getNomenclaturesFunc(dataNodes.Select(x => x.NomenclatureId).Distinct())),
			//	Task.Run(async () => productGroups = await getGetProductGroupsFunc(dataNodes.Select(x => x.ProductGroupId).Distinct())),
			//	Task.Run(async () => subdivisions = await getGetSubdivisionsFunc(dataNodes.Select(x => x.SubdivisionId).Distinct())));

			IDictionary<int, string> warehouses = await getGetWarehousesFunc(residueDataNodes.Select(x => x.WarehouseId).Distinct());
			IDictionary<int, string> nomenclatures = await getNomenclaturesFunc(dataNodes.Select(x => x.NomenclatureId).Distinct());
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
	}
}

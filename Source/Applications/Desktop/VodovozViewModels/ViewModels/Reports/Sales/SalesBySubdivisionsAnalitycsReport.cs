using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReportViewModel;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsReport
	{
		private readonly IEnumerable<SalesDataNode> _sales;
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
			SplitByNomenclatures = splitByNomenclatures;
			SplitBySubdivisions = splitBySubdivisions;
			SplitByWarehouses = splitByWarehouses;

			Subdivisions = new Dictionary<int, string>
			{
				{ 0, string.Empty }
			};

			foreach(var subdivision in subdivisions)
			{
				Subdivisions.Add(subdivision);
			}

			_subdivisionIndexes = Subdivisions.Keys.ToList();

			ShowResidues = (firstPeriodEndDate - firstPeriodStartDate).TotalDays < 1;

			var warehousesList = new List<string>();

			if(ShowResidues)
			{
				Warehouses = new Dictionary<int, string>
				{
					{ 0, "Общий остаток на складах" }
				};

				foreach(var warehouse in warehouses)
				{
					Warehouses.Add(warehouse);
				}

				_warehouseIndexes = Warehouses.Keys.ToList();

				warehousesList.AddRange(Warehouses.Values);
			}
			else
			{
				Warehouses = new Dictionary<int, string>();
			}

			Nomenclatures = nomenclatures;
			ProductGroups = productGroups;
			ProductGroups.Add(0, "Без группы");
			_productGroupIndexes = ProductGroups.Keys.ToList();
			_productGroupNomenclatures = new Dictionary<int, IEnumerable<int>>();
			_sales = sales;
			_residues = residues.GroupBy(x => (x.NomenclatureId, x.WarehouseId));

			foreach(var productGroupId in _productGroupIndexes)
			{
				_productGroupNomenclatures.Add(
					productGroupId,
					_sales
						.Where(y => y.ProductGroupId == productGroupId)
						.Select(y => y.NomenclatureId)
						.Distinct());
			}

			var subdivisionsList = new List<string>();

			foreach(var subdivision in Subdivisions)
			{
				subdivisionsList.Add(subdivision.Value);
				subdivisionsList.Add(string.Empty);
			}

			_header = new HeaderRow
			{
				Title = "Общий отчет по продажам",
				SubdivisionsTitles = subdivisionsList,
				WarehousesTitles = warehousesList
			};

			var dynamicsSubheaderRows = new List<string>();

			for(int i = 0; i < Subdivisions.Count; i++)
			{
				dynamicsSubheaderRows.Add("Количество");
				dynamicsSubheaderRows.Add("Сумма");
			}

			if(ShowResidues)
			{
				for(int i = 0; i < Warehouses.Count; i++)
				{
					dynamicsSubheaderRows.Add("Количество");
				}
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

		public string Title => "Аналитика продаж КБ";

		public DateTime FirstPeriodStartDate { get; }

		public DateTime FirstPeriodEndDate { get; }

		public bool SplitByNomenclatures { get; }

		public bool SplitBySubdivisions { get; }

		public bool SplitByWarehouses { get; }

		public bool ShowResidues { get; }

		public IDictionary<int, string> Subdivisions { get; set; }

		public IDictionary<int, string> Warehouses { get; }

		public IDictionary<int, string> Nomenclatures { get; }

		public IDictionary<int, string> ProductGroups { get; }

		public DateTime CreatedAt { get; }

		public TotalRow Total => _totalRow;

		public List<DisplayRow> DisplayRows => _displayRows;

		public List<Row> Rows => _rows;

		private void Process()
		{
			_totalRow = new TotalRow()
			{
				SubTotalRows = new List<SubTotalRow>(),
				SalesBySubdivision = CreateSalesBySubdivisionEmptyList(),
				ResiduesByWarehouse = Enumerable.Repeat(0m, Warehouses.Count).ToList()
			};

			for(var i = 0; i < _productGroupIndexes.Count; i++)
			{
				_totalRow.SubTotalRows.Add(ProcessProductGroup(_productGroupIndexes[i]));
			}

			for(int i = 0; i < Subdivisions.Count; i++)
			{
				_totalRow.SalesBySubdivision[i].Amount = _totalRow.SubTotalRows.Sum(x => x.SalesBySubdivision[i].Amount);
				_totalRow.SalesBySubdivision[i].Price = _totalRow.SubTotalRows.Sum(x => x.SalesBySubdivision[i].Price);
			}

			if(ShowResidues)
			{
				for(int i = 0; i < Warehouses.Count; i++)
				{
					_totalRow.ResiduesByWarehouse[i] = _totalRow.SubTotalRows.Sum(x => x.ResiduesByWarehouse[i]);
				}
			}
			else
			{
				_totalRow.ResiduesByWarehouse = new List<decimal>();
			}

			_rows.Add(_totalRow);
			_displayRows.Add(_totalRow);
		}

		private SubTotalRow ProcessProductGroup(int productGroupId)
		{
			SubTotalRow result = new SubTotalRow
			{
				Title = ProductGroups[productGroupId],
				SalesBySubdivision = CreateSalesBySubdivisionEmptyList(),
				ResiduesByWarehouse = Enumerable.Repeat(0m, Warehouses.Count).ToList()
			};

			if(SplitByNomenclatures)
			{
				var salesGroups = _sales.GroupBy(x => (x.NomenclatureId, x.SubdivisionId));

				var nomenclatureRows = ProcessNomenclatureGroup(salesGroups, productGroupId);
				result.NomenclatureRows = nomenclatureRows;

				if(SplitBySubdivisions)
				{
					for(int i = 0; i < Subdivisions.Count; i++)
					{
						result.SalesBySubdivision[i].Amount = nomenclatureRows.Sum(x => x.SalesBySubdivision[i].Amount);
						result.SalesBySubdivision[i].Price = nomenclatureRows.Sum(x => x.SalesBySubdivision[i].Price);
					}
				}
				else
				{
					result.SalesBySubdivision[0].Amount = nomenclatureRows.Sum(x => x.SalesBySubdivision.Sum(y => y.Amount));
					result.SalesBySubdivision[0].Price = nomenclatureRows.Sum(x => x.SalesBySubdivision.Sum(y => y.Price));
				}

				if(ShowResidues)
				{
					if(SplitByWarehouses)
					{
						for(int i = 0; i < Warehouses.Count; i++)
						{
							result.ResiduesByWarehouse[i] = nomenclatureRows.Sum(x => x.ResiduesByWarehouse[i]);
						}

						result.ResiduesByWarehouse[0] = result.ResiduesByWarehouse.Skip(1).Sum();
					}
					else
					{
						result.ResiduesByWarehouse[0] = _residues.Sum(x =>
							x.Where(y => y.ProductGroupId == productGroupId
								&& _productGroupNomenclatures[productGroupId].Contains(y.NomenclatureId))
							 .Sum(y => y.Residue));
					}
				}
				else
				{
					result.ResiduesByWarehouse = new List<decimal>();
				}

				_rows.Add(result);
				_displayRows.Add(result);
				_rows.AddRange(nomenclatureRows);
				_displayRows.AddRange(nomenclatureRows);
			}
			else
			{
				if(SplitBySubdivisions)
				{
					result.NomenclatureRows = Enumerable.Empty<Row>().ToList();

					for(int i = 1; i < _subdivisionIndexes.Count; i++)
					{
						result.SalesBySubdivision[i].Amount =
							_sales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i]
									&& x.ProductGroupId == productGroupId)
								.Sum(x => x.Amount);
						result.SalesBySubdivision[i].Price =
							_sales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i]
									&& x.ProductGroupId == productGroupId)
								.Sum(x => x.Price);
					}

					result.SalesBySubdivision[0].Amount =
						result.SalesBySubdivision.Skip(1).Sum(x => x.Amount);
					result.SalesBySubdivision[0].Price =
						result.SalesBySubdivision.Skip(1).Sum(x => x.Price);
				}
				else
				{
					result.SalesBySubdivision[0].Amount =
						_sales
							.Where(x => x.ProductGroupId == productGroupId)
							.Sum(x => x.Amount);
					result.SalesBySubdivision[0].Price =
						_sales
							.Where(x => x.ProductGroupId == productGroupId)
							.Sum(x => x.Price);
				}

				if(ShowResidues)
				{
					if(SplitByWarehouses)
					{
						for(int i = 1; i < _warehouseIndexes.Count; i++)
						{
							result.ResiduesByWarehouse[i] =
								_residues
									.Where(x => x.Key.WarehouseId == _warehouseIndexes[i]
										&& _productGroupNomenclatures.ContainsKey(productGroupId)
										&& _productGroupNomenclatures[productGroupId].Contains(x.Key.NomenclatureId))
									.Sum(x => x.Sum(y => y.Residue));
						}

						result.ResiduesByWarehouse[0] = result.ResiduesByWarehouse.Skip(1).Sum();
					}
					else
					{
						result.ResiduesByWarehouse[0] = _residues.Sum(x =>
							x.Where(y => y.ProductGroupId == productGroupId
								&& _productGroupNomenclatures[productGroupId].Contains(y.NomenclatureId))
							 .Sum(y => y.Residue));
					}
				}
				else
				{
					result.ResiduesByWarehouse = new List<decimal>();
				}

				_rows.Add(result);
				_displayRows.Add(result);
			}

			return result;
		}

		private List<Row> ProcessNomenclatureGroup(IEnumerable<IGrouping<(int NomenclatureId, int SubdivisionId), SalesDataNode>> salesGroups, int productGroupId)
		{
			var nomenclatureRows = new List<Row>();

			var nomenclatureIds = _productGroupNomenclatures[productGroupId];

			foreach(var nomenclatureId in nomenclatureIds)
			{
				IList<decimal> residues;

				if(ShowResidues)
				{
					residues = ProcessWarehousesResidues(nomenclatureId);
				}
				else
				{
					residues = new List<decimal>();
				}

				nomenclatureRows.Add(new Row
				{
					Title = Nomenclatures[nomenclatureId],
					SalesBySubdivision = ProcessSubdivisionsNomenclaturesSales(salesGroups, nomenclatureId),
					ResiduesByWarehouse = residues
				});
			}

			return nomenclatureRows;
		}

		private IList<AmountPricePair> ProcessSubdivisionsNomenclaturesSales(
			IEnumerable<IGrouping<(int NomenclatureId, int SubdivisionId), SalesDataNode>> salesGroups,
			int nomenclatureId)
		{
			var result = CreateSalesBySubdivisionEmptyList();

			var salesCount = salesGroups.Count();

			if(SplitBySubdivisions)
			{
				foreach(var group in salesGroups)
				{
					if(group.Key.NomenclatureId == nomenclatureId)
					{
						result[_subdivisionIndexes.IndexOf(group.Key.SubdivisionId)].Amount = group.Sum(x => x.Amount);
						result[_subdivisionIndexes.IndexOf(group.Key.SubdivisionId)].Price = group.Sum(x => x.Price);
					}
				}

				result[0].Amount = result.Skip(1).Sum(x => x.Amount);
				result[0].Price = result.Skip(1).Sum(x => x.Price);
			}
			else
			{
				foreach(var group in salesGroups)
				{
					if(group.Key.NomenclatureId == nomenclatureId)
					{
						result[0].Amount += group.Sum(x => x.Amount);
						result[0].Price += group.Sum(x => x.Price);
					}
				}
			}

			return result;
		}

		private IList<decimal> ProcessWarehousesResidues(int nomenclatureId)
		{
			if(!ShowResidues)
			{
				return new List<decimal>();
			}

			var result = Enumerable.Repeat(0m, Warehouses.Count).ToList();

			if(SplitByWarehouses)
			{
				foreach(var group in _residues)
				{
					if(group.Key.NomenclatureId == nomenclatureId)
					{
						result[_warehouseIndexes.IndexOf(group.Key.WarehouseId) + 1] = group.Sum(x => x.Residue);
					}
				}

				result[0] = result.Skip(1).Sum();
			}
			else
			{
				result[0] = _residues.Where(x => x.Key.NomenclatureId == nomenclatureId).Sum(x => x.Sum(y => y.Residue));
			}

			return result;
		}

		public List<AmountPricePair> CreateSalesBySubdivisionEmptyList()
		{
			var result = new List<AmountPricePair>();

			for(int i = 0; i < Subdivisions.Count; i++)
			{
				result.Add(new AmountPricePair
				{
					Amount = 0m,
					Price = 0m
				});
			}

			return result;
		}

		public static async Task<SalesBySubdivisionsAnalitycsReport> Create(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses,
			int[] subdivisionsIds,
			int[] warehousesIds,
			Func<DateTime, DateTime, int[], IEnumerable<SalesDataNode>> retrieveFunction,
			Func<DateTime, int[], IEnumerable<ResidueDataNode>> warehouseResiduesFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getNomenclaturesFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetProductGroupsFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetSubdivisionsFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetWarehousesFunc)
		{
			if(retrieveFunction is null)
			{
				throw new ArgumentNullException(nameof(retrieveFunction));
			}

			if(warehouseResiduesFunc is null)
			{
				throw new ArgumentNullException(nameof(warehouseResiduesFunc));
			}

			if(getNomenclaturesFunc is null)
			{
				throw new ArgumentNullException(nameof(getNomenclaturesFunc));
			}

			if(getGetProductGroupsFunc is null)
			{
				throw new ArgumentNullException(nameof(getGetProductGroupsFunc));
			}

			if(getGetSubdivisionsFunc is null)
			{
				throw new ArgumentNullException(nameof(getGetSubdivisionsFunc));
			}

			if(getGetWarehousesFunc is null)
			{
				throw new ArgumentNullException(nameof(getGetWarehousesFunc));
			}

			IEnumerable<SalesDataNode> dataNodes = retrieveFunction(
				firstPeriodStartDate,
				firstPeriodEndDate,
				subdivisionsIds);

			IEnumerable<ResidueDataNode> residueDataNodes;

			IDictionary<int, string> warehouses = new Dictionary<int, string>();

			if((firstPeriodEndDate - firstPeriodStartDate).TotalDays < 1)
			{
				residueDataNodes = warehouseResiduesFunc(firstPeriodEndDate, warehousesIds);

				if(splitByWarehouses)
				{
					warehouses = await getGetWarehousesFunc(warehousesIds);
				}
			}
			else
			{
				residueDataNodes = Enumerable.Empty<ResidueDataNode>();
			}

			IDictionary<int, string> nomenclatures;

			if(splitByNomenclatures)
			{
				nomenclatures = await getNomenclaturesFunc(
				dataNodes.Select(x => x.NomenclatureId).Distinct());
			}
			else
			{
				nomenclatures = new Dictionary<int, string>();
			}
			
			IDictionary<int, string> productGroups = await getGetProductGroupsFunc(dataNodes.Select(x => x.ProductGroupId).Distinct());

			IDictionary<int, string> subdivisions;

			if(splitBySubdivisions)
			{
				subdivisions = await getGetSubdivisionsFunc(subdivisionsIds);
			}
			else
			{
				subdivisions = new Dictionary<int, string>();
			}

			return new SalesBySubdivisionsAnalitycsReport(
				firstPeriodStartDate,
				firstPeriodEndDate,
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
	}
}

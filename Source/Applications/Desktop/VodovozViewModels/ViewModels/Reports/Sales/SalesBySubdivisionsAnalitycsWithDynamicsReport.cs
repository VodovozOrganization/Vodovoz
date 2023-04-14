using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Vodovoz.ViewModels.ViewModels.Reports.Sales.SalesBySubdivisionsAnalitycsReportViewModel;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public partial class SalesBySubdivisionsAnalitycsWithDynamicsReport
	{
		private readonly List<DisplayRow> _displayRows = new List<DisplayRow>();
		private readonly List<Row> _rows = new List<Row>();
		private readonly IEnumerable<SalesDataNode> _firstPeriodSales;
		private readonly IEnumerable<SalesDataNode> _secondPeriodSales;
		private readonly List<int> _subdivisionIndexes;
		private readonly List<int> _productGroupIndexes;
		private readonly Dictionary<int, IEnumerable<int>> _productGroupNomenclatures;
		private HeaderRow _header;
		private SubHeaderRow _subHeader;
		private SubHeaderRow _subHeader2;
		private TotalRow _totalRow;

		private SalesBySubdivisionsAnalitycsWithDynamicsReport(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			IDictionary<int, string> nomenclatures,
			IDictionary<int, string> productGroups,
			IDictionary<int, string> subdivisions,
			IEnumerable<SalesDataNode> firstPeriodSales,
			IEnumerable<SalesDataNode> secondPeriodSales)
		{
			FirstPeriodStartDate = firstPeriodStartDate;
			FirstPeriodEndDate = firstPeriodEndDate;
			SecondPeriodStartDate = secondPeriodStartDate;
			SecondPeriodEndDate = secondPeriodEndDate;
			SplitByNomenclatures = splitByNomenclatures;
			SplitBySubdivisions = splitBySubdivisions;
			Nomenclatures = nomenclatures;
			ProductGroups = productGroups;
			_firstPeriodSales = firstPeriodSales;
			_secondPeriodSales = secondPeriodSales;

			Subdivisions = new Dictionary<int, string>
				{
					{ 0, string.Empty }
				};

			foreach(var subdivision in subdivisions)
			{
				Subdivisions.Add(subdivision);
			}

			_subdivisionIndexes = Subdivisions.Keys.ToList();
			ProductGroups.Add(0, "Без группы");
			_productGroupIndexes = ProductGroups.Keys.ToList();
			_productGroupNomenclatures = new Dictionary<int, IEnumerable<int>>();

			foreach(var productGroupId in _productGroupIndexes)
			{
				_productGroupNomenclatures.Add(
					productGroupId,
					_firstPeriodSales
						.Concat(_secondPeriodSales)
						.Where(y => y.ProductGroupId == productGroupId)
						.Select(y => y.NomenclatureId)
						.Distinct());
			}

			var subdivisionsList = new List<string>();

			foreach(var subdivision in Subdivisions)
			{
				subdivisionsList.Add(subdivision.Value);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
				subdivisionsList.Add(string.Empty);
			}

			_header = new HeaderRow
			{
				Title = "Общий отчет по продажам",
				SubdivisionsTitles = subdivisionsList,
			};

			var dynamicsSubheaderRows = new List<string>();

			foreach(var subdivision in Subdivisions)
			{
				dynamicsSubheaderRows.Add(FirstPeriodStartDate.ToString("d"));
				dynamicsSubheaderRows.Add("-" + FirstPeriodEndDate.ToString("d"));
				dynamicsSubheaderRows.Add(SecondPeriodStartDate.Value.ToString("d"));
				dynamicsSubheaderRows.Add("-" + SecondPeriodEndDate.Value.ToString("d"));
				dynamicsSubheaderRows.Add("динамика");
				dynamicsSubheaderRows.Add(string.Empty);
				dynamicsSubheaderRows.Add(string.Empty);
				dynamicsSubheaderRows.Add(string.Empty);
			}

			_subHeader = new SubHeaderRow
			{
				Title = string.Empty,
				DynamicColumns = dynamicsSubheaderRows
			};

			var dynamicsSubheader2Rows = new List<string>();

			for(int i = 0; i < Subdivisions.Count; i++)
			{
				dynamicsSubheader2Rows.Add("Количество");
				dynamicsSubheader2Rows.Add("Сумма");
				dynamicsSubheader2Rows.Add("Количество");
				dynamicsSubheader2Rows.Add("Сумма");
				dynamicsSubheader2Rows.Add("Количество/шт");
				dynamicsSubheader2Rows.Add("Количество %");
				dynamicsSubheader2Rows.Add("Сумма/руб");
				dynamicsSubheader2Rows.Add("Сумма %");
			}

			_subHeader2 = new SubHeaderRow
			{
				Title = string.Empty,
				DynamicColumns = dynamicsSubheader2Rows
			};

			_displayRows.Add(_header);
			_displayRows.Add(_subHeader);
			_displayRows.Add(_subHeader2);

			Process();

			CreatedAt = DateTime.Now;
		}

		public string Title => "Аналитика продаж КБ с динамикой";

		public DateTime FirstPeriodStartDate { get; }

		public DateTime FirstPeriodEndDate { get; }

		public DateTime? SecondPeriodStartDate { get; }

		public DateTime? SecondPeriodEndDate { get; }

		public bool SplitByNomenclatures { get; }

		public bool SplitBySubdivisions { get; }

		public IDictionary<int, string> Subdivisions { get; set; }

		public IDictionary<int, string> Nomenclatures { get; }

		public IDictionary<int, string> ProductGroups { get; }

		public DateTime CreatedAt { get; }

		public List<DisplayRow> DisplayRows => _displayRows;

		public List<Row> Rows => _rows;

		private void Process()
		{
			_totalRow = new TotalRow()
			{
				SubTotalRows = ProcessSubTotalRows(),
				SalesBySubdivision = CreateSalesBySubdivisionEmptyList(),
			};

			for(int i = 0; i < _totalRow.SalesBySubdivision.Count; i++)
			{
				_totalRow.SalesBySubdivision[i].FirstPeriodAmount = _totalRow.SubTotalRows
					.Sum(x => x.SalesBySubdivision[i].FirstPeriodAmount);
				_totalRow.SalesBySubdivision[i].FirstPeriodPrice = _totalRow.SubTotalRows
					.Sum(x => x.SalesBySubdivision[i].FirstPeriodPrice);
				_totalRow.SalesBySubdivision[i].SecondPeriodAmount = _totalRow.SubTotalRows
					.Sum(x => x.SalesBySubdivision[i].SecondPeriodAmount);
				_totalRow.SalesBySubdivision[i].SecondPeriodPrice = _totalRow.SubTotalRows
					.Sum(x => x.SalesBySubdivision[i].SecondPeriodPrice);
			}

			_rows.Add(_totalRow);
			_displayRows.Add(_totalRow);
		}

		private IList<SubTotalRow> ProcessSubTotalRows()
		{
			var result = new List<SubTotalRow>();

			foreach(var productGroup in ProductGroups)
			{
				var subTotalRow = new SubTotalRow
				{
					Title = productGroup.Value,
					SalesBySubdivision = CreateSalesBySubdivisionEmptyList()
				};

				if(SplitByNomenclatures)
				{
					subTotalRow.NomenclatureRows = ProceedNomenclatureRowsForProductGroup(productGroup.Key);

					for(int i = 0; i < Subdivisions.Count; i++)
					{
						subTotalRow.SalesBySubdivision[i].FirstPeriodAmount = subTotalRow.NomenclatureRows
							.Sum(x => x.SalesBySubdivision[i].FirstPeriodAmount);
						subTotalRow.SalesBySubdivision[i].FirstPeriodPrice = subTotalRow.NomenclatureRows
							.Sum(x => x.SalesBySubdivision[i].FirstPeriodPrice);
						subTotalRow.SalesBySubdivision[i].SecondPeriodAmount = subTotalRow.NomenclatureRows
							.Sum(x => x.SalesBySubdivision[i].SecondPeriodAmount);
						subTotalRow.SalesBySubdivision[i].SecondPeriodPrice = subTotalRow.NomenclatureRows
							.Sum(x => x.SalesBySubdivision[i].SecondPeriodPrice);
					}
				}
				else
				{
					subTotalRow.NomenclatureRows = new List<Row>();

					if(SplitBySubdivisions)
					{
						for(int i = 1; i < Subdivisions.Count; i++)
						{
							var currentSubdivisionId = _subdivisionIndexes[i - 1];

							subTotalRow.SalesBySubdivision[i].FirstPeriodAmount =
								_firstPeriodSales
									.Where(x => x.SubdivisionId == currentSubdivisionId
										&& x.ProductGroupId == productGroup.Key)
									.Sum(x => x.Amount);

							subTotalRow.SalesBySubdivision[i].FirstPeriodPrice =
								_firstPeriodSales
									.Where(x => x.SubdivisionId == currentSubdivisionId
										&& x.ProductGroupId == productGroup.Key)
									.Sum(x => x.Price);

							subTotalRow.SalesBySubdivision[i].SecondPeriodAmount =
								_secondPeriodSales
									.Where(x => x.SubdivisionId == currentSubdivisionId
										&& x.ProductGroupId == productGroup.Key)
									.Sum(x => x.Amount);

							subTotalRow.SalesBySubdivision[i].SecondPeriodPrice =
								_secondPeriodSales
									.Where(x => x.SubdivisionId == currentSubdivisionId
										&& x.ProductGroupId == productGroup.Key)
									.Sum(x => x.Price);
						}

						subTotalRow.SalesBySubdivision[0].FirstPeriodAmount =
							subTotalRow.SalesBySubdivision.Skip(1).Sum(x => x.FirstPeriodAmount);
						subTotalRow.SalesBySubdivision[0].FirstPeriodPrice =
							subTotalRow.SalesBySubdivision.Skip(1).Sum(x => x.FirstPeriodPrice);
						subTotalRow.SalesBySubdivision[0].SecondPeriodAmount =
							subTotalRow.SalesBySubdivision.Skip(1).Sum(x => x.SecondPeriodAmount);
						subTotalRow.SalesBySubdivision[0].SecondPeriodPrice =
							subTotalRow.SalesBySubdivision.Skip(1).Sum(x => x.SecondPeriodPrice);
					}
					else
					{
						subTotalRow.SalesBySubdivision[0].FirstPeriodAmount =
							_firstPeriodSales
								.Where(x => x.ProductGroupId == productGroup.Key)
								.Sum(x => x.Amount);

						subTotalRow.SalesBySubdivision[0].FirstPeriodPrice =
							_firstPeriodSales
								.Where(x => x.ProductGroupId == productGroup.Key)
								.Sum(x => x.Price);

						subTotalRow.SalesBySubdivision[0].SecondPeriodAmount =
							_secondPeriodSales
								.Where(x => x.ProductGroupId == productGroup.Key)
								.Sum(x => x.Amount);

						subTotalRow.SalesBySubdivision[0].SecondPeriodPrice =
							_secondPeriodSales
								.Where(x => x.ProductGroupId == productGroup.Key)
								.Sum(x => x.Price);
					}
				}

				result.Add(subTotalRow);

				_rows.Add(subTotalRow);
				_rows.AddRange(subTotalRow.NomenclatureRows);
				_displayRows.Add(subTotalRow);
				_displayRows.AddRange(subTotalRow.NomenclatureRows);
			}

			return result;
		}

		private IList<Row> ProceedNomenclatureRowsForProductGroup(int productGroupId)
		{
			var result = new List<Row>();

			foreach(var nomanclatureId in _productGroupNomenclatures[productGroupId])
			{
				var nomanclatureRow = new Row()
				{
					Title = Nomenclatures[nomanclatureId],
					SalesBySubdivision = CreateSalesBySubdivisionEmptyList()
				};

				if(SplitBySubdivisions)
				{
					for(int i = 1; i < Subdivisions.Count; i++)
					{
						nomanclatureRow.SalesBySubdivision[i].FirstPeriodAmount =
							_firstPeriodSales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i - 1]
									&& x.NomenclatureId == nomanclatureId)
								.Sum(x => x.Amount);

						nomanclatureRow.SalesBySubdivision[i].FirstPeriodPrice =
							_firstPeriodSales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i - 1]
									&& x.NomenclatureId == nomanclatureId)
								.Sum(x => x.Price);

						nomanclatureRow.SalesBySubdivision[i].SecondPeriodAmount =
							_secondPeriodSales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i - 1]
									&& x.NomenclatureId == nomanclatureId)
								.Sum(x => x.Amount);

						nomanclatureRow.SalesBySubdivision[i].SecondPeriodPrice =
							_secondPeriodSales
								.Where(x => x.SubdivisionId == _subdivisionIndexes[i - 1]
									&& x.NomenclatureId == nomanclatureId)
								.Sum(x => x.Price);
					}

					nomanclatureRow.SalesBySubdivision[0].FirstPeriodAmount =
						nomanclatureRow.SalesBySubdivision.Skip(1).Sum(x => x.FirstPeriodAmount);
					nomanclatureRow.SalesBySubdivision[0].FirstPeriodPrice =
						nomanclatureRow.SalesBySubdivision.Skip(1).Sum(x => x.FirstPeriodPrice);
					nomanclatureRow.SalesBySubdivision[0].SecondPeriodAmount =
						nomanclatureRow.SalesBySubdivision.Skip(1).Sum(x => x.SecondPeriodAmount);
					nomanclatureRow.SalesBySubdivision[0].SecondPeriodPrice =
						nomanclatureRow.SalesBySubdivision.Skip(1).Sum(x => x.SecondPeriodPrice);
				}
				else
				{
					nomanclatureRow.SalesBySubdivision[0].FirstPeriodAmount =
						_firstPeriodSales
							.Where(x => x.NomenclatureId == nomanclatureId)
							.Sum(x => x.Amount);

					nomanclatureRow.SalesBySubdivision[0].FirstPeriodPrice =
						_firstPeriodSales
							.Where(x => x.NomenclatureId == nomanclatureId)
							.Sum(x => x.Price);

					nomanclatureRow.SalesBySubdivision[0].SecondPeriodAmount =
						_secondPeriodSales
							.Where(x => x.NomenclatureId == nomanclatureId)
							.Sum(x => x.Amount);

					nomanclatureRow.SalesBySubdivision[0].SecondPeriodPrice =
						_secondPeriodSales
							.Where(x => x.NomenclatureId == nomanclatureId)
							.Sum(x => x.Price);
				}

				result.Add(nomanclatureRow);
			}

			return result;
		}

		public List<SalesBySubdivisionRowPart> CreateSalesBySubdivisionEmptyList()
		{
			var result = new List<SalesBySubdivisionRowPart>();

			for(int i = 0; i < Subdivisions.Count; i++)
			{
				result.Add(new SalesBySubdivisionRowPart
				{
					FirstPeriodAmount = 0m,
					FirstPeriodPrice = 0m,
					SecondPeriodAmount = 0m,
					SecondPeriodPrice = 0m
				});
			}

			return result;
		}

		public static async Task<SalesBySubdivisionsAnalitycsWithDynamicsReport> Create(
			DateTime firstPeriodStartDate,
			DateTime firstPeriodEndDate,
			DateTime? secondPeriodStartDate,
			DateTime? secondPeriodEndDate,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			int[] subdivisionsIds,
			Func<DateTime, DateTime, int[], IEnumerable<SalesDataNode>> retrieveFunction,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getNomenclaturesFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetProductGroupsFunc,
			Func<IEnumerable<int>, Task<IDictionary<int, string>>> getGetSubdivisionsFunc)
		{
			if(retrieveFunction is null)
			{
				throw new ArgumentNullException(nameof(retrieveFunction));
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

			IEnumerable<SalesDataNode> firstPeriodDataNodes = retrieveFunction(
				firstPeriodStartDate,
				firstPeriodEndDate,
				subdivisionsIds);

			IEnumerable<SalesDataNode> secondPeriodDataNodes = retrieveFunction(
				secondPeriodStartDate.Value,
				secondPeriodEndDate.Value,
				subdivisionsIds);

			IDictionary<int, string> nomenclatures;

			if(splitByNomenclatures)
			{
				nomenclatures = await getNomenclaturesFunc(
					firstPeriodDataNodes
						.Select(x => x.NomenclatureId)
						.Concat(secondPeriodDataNodes.Select(x => x.NomenclatureId))
						.Distinct());
			}
			else
			{
				nomenclatures = new Dictionary<int, string>();
			}

			IDictionary<int, string> productGroups = await getGetProductGroupsFunc(
				firstPeriodDataNodes
					.Select(x => x.ProductGroupId)
					.Distinct()
					.Concat(secondPeriodDataNodes
						.Select(x => x.ProductGroupId)
						.Distinct())
					.Distinct());

			IDictionary<int, string> subdivisions;

			if(splitBySubdivisions)
			{
				subdivisions = await getGetSubdivisionsFunc(subdivisionsIds);
			}
			else
			{
				subdivisions = new Dictionary<int, string>();
			}

			return new SalesBySubdivisionsAnalitycsWithDynamicsReport(
				firstPeriodStartDate,
				firstPeriodEndDate,
				secondPeriodStartDate,
				secondPeriodEndDate,
				splitByNomenclatures,
				splitBySubdivisions,
				nomenclatures,
				productGroups,
				subdivisions,
				firstPeriodDataNodes,
				secondPeriodDataNodes);
		}
	}
}

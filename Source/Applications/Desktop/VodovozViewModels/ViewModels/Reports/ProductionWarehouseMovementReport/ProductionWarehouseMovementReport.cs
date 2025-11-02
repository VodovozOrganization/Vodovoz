using CsvHelper;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Documents.MovementDocuments;

namespace Vodovoz.ViewModels.Reports
{
	public partial class ProductionWarehouseMovementReport
	{
		private readonly IUnitOfWork _uow;
		private readonly DateTime? _startDate;
		private readonly DateTime? _endDate;
		private readonly IList<Warehouse> _warehouseList;
		private readonly Warehouse _warehouse;

		public ProductionWarehouseMovementReport(IUnitOfWork uow, DateTime? startDate, DateTime? endDate, IList<Warehouse> warehouseList, Warehouse warehouse)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_warehouseList = warehouseList ?? throw new ArgumentNullException(nameof(warehouseList));
			_warehouse = warehouse;
			_startDate = startDate;
			_endDate = endDate.Value.AddDays(1).AddMilliseconds(-1);
		}

		private void GenerateBaseReport(List<ProductionWarehouseMovementReportDataBaseNode> dataBaseNodeList)
		{
			var nomenclatureResultList = new List<ProductionWarehouseMovementReportNomenclature>();

			var nomenclatureIds = dataBaseNodeList.Select(x => x.NomenclatureId).Distinct();

			foreach(var nomenclatureId in nomenclatureIds)
			{
				var nomenclatureDocuments = dataBaseNodeList.Where(x => x.NomenclatureId == nomenclatureId);

				var nomenclatureRangePrices = nomenclatureDocuments
					.FirstOrDefault()
					.PurchasePrices
					.Where(x => (x.StartDate <= _endDate)
						&& (x.EndDate == null || x.EndDate >= _startDate));

				foreach(var nomenclatureRangePrice in nomenclatureRangePrices)
				{
					var documentsInPriceRange = nomenclatureDocuments
						.Where(x => (nomenclatureRangePrice.StartDate <= x.MovementDocumentDate)
							&& (nomenclatureRangePrice.EndDate == null || nomenclatureRangePrice.EndDate >= x.MovementDocumentDate));

					var nomenclature = new ProductionWarehouseMovementReportNomenclature
					{
						PurchasePriceStartDate = nomenclatureRangePrice.StartDate,
						PurchasePriceEndDate = nomenclatureRangePrice.EndDate ?? _endDate,
						NomenclatureName = nomenclatureDocuments.FirstOrDefault().NomenclatureName,
						Amount = decimal.Round(documentsInPriceRange.Sum(a => a.Amount)),
						PurchasePrice = nomenclatureRangePrice.PurchasePrice,
						Sum = decimal.Round(documentsInPriceRange.Sum(a => a.Amount) * nomenclatureRangePrice.PurchasePrice, 2)
					};

						nomenclatureResultList.Add(nomenclature);
				}

				var documentsNotInPriceRange = new List<ProductionWarehouseMovementReportDataBaseNode>();

				foreach(var nomenclatureDocument in nomenclatureDocuments)
				{
					if(!nomenclatureDocument.PurchasePrices
						.Any(p => (p.StartDate <= nomenclatureDocument.MovementDocumentDate)
							&& (p.EndDate == null || p.EndDate >= nomenclatureDocument.MovementDocumentDate)))
					{
						documentsNotInPriceRange.Add(nomenclatureDocument);
					}
				}

				var nomenclatureWithNullPrice = new ProductionWarehouseMovementReportNomenclature
				{
					NomenclatureName = nomenclatureDocuments.FirstOrDefault().NomenclatureName,
					Amount = decimal.Round(documentsNotInPriceRange.Sum(a => a.Amount))
				};

				if(nomenclatureWithNullPrice.Amount > 0)
				{
					nomenclatureResultList.Add(nomenclatureWithNullPrice);
				}
			}

			var totalRow = new ProductionWarehouseMovementReportNomenclature
			{
				NomenclatureName = "ИТОГО",
				Amount = decimal.Round(nomenclatureResultList.Sum(n => n.Amount)),
				Sum = nomenclatureResultList.Sum(n => n.Sum),
				IsTotal = true
			};

			var reportNode = new ProductionWarehouseMovementReportNode
			{
				NomenclatureColumns = (nomenclatureResultList
					.OrderBy(x => x.NomenclatureName)
					.ThenBy(x => x.PurchasePriceStartDate)
					.ToList())
			};

			reportNode.NomenclatureColumns.Add(totalRow);

			ResultNodeList.Add(reportNode);
		}

		private void GenerateDetailedReport(List<ProductionWarehouseMovementReportDataBaseNode> dataBaseNodeList)
		{
			Titles = dataBaseNodeList
				.OrderBy(x => x.NomenclatureName)
				.GroupBy(x => x.NomenclatureId)
				.Select(x => new ProductionWarehouseMovementReportTitleNode
				{
					NomenclatureId = x.First().NomenclatureId,
					NomenclatureName = x.First().NomenclatureName
				})
				.ToList();

			var documentDates = dataBaseNodeList
				.Select(x => x.MovementDocumentDate)
				.OrderBy(x => x)
				.Distinct();

			foreach(var documentDate in documentDates)
			{
				var documentsOnDate = dataBaseNodeList
						.Where(d => d.MovementDocumentDate == documentDate)
						.GroupBy(x => x.MovementDocumentId)
						.Select(x => new
						{
							Id = x.First().MovementDocumentId,
							Date = x.First().MovementDocumentDate
						});

				var rowsOnDate = new List<ProductionWarehouseMovementReportNode>();

				#region Формирование строк-документов с колонками-номенклатурами

				foreach(var documentOnDate in documentsOnDate)
				{
					var nomenclatures = dataBaseNodeList
						.Where(d => d.MovementDocumentId == documentOnDate.Id)
						.Select(x =>
						{
							var nomenclature = new ProductionWarehouseMovementReportNomenclature
							{
								Id = x.NomenclatureId,
								NomenclatureName = x.NomenclatureName,
								Amount = decimal.Round(x.Amount, 2)
							};

							nomenclature.PurchasePrice = x.PurchasePrices
								.FirstOrDefault(p => (p.StartDate <= documentOnDate.Date)
									&& (p.EndDate == null || p.EndDate >= documentOnDate.Date))
								?.PurchasePrice ?? 0m;

							nomenclature.Sum = decimal.Round(nomenclature.PurchasePrice * nomenclature.Amount, 2);

							return nomenclature;
						});

					var filledByTitleNomenclatures = FillNomenclatureColumnsByTitles(nomenclatures);

					var filledRow = new ProductionWarehouseMovementReportNode
					{
						NomenclatureColumns = filledByTitleNomenclatures,
						MovementDocumentDate = documentOnDate.Date,
						MovementDocumentName = $"Документ №{documentOnDate.Id}"
					};

					rowsOnDate.Add(filledRow);
				}

				ResultNodeList.AddRange(rowsOnDate);

				#endregion

				#region Формирование строки с итоговым количеством за дату

				var amountTotalNomenclatures = new List<ProductionWarehouseMovementReportNomenclature>();

				foreach(var title in Titles)
				{
					var amountSum = rowsOnDate
						.SelectMany(x => x.NomenclatureColumns)
						.Where(x => x.Id == title.NomenclatureId)
						.Sum(x => x.Amount);

					amountTotalNomenclatures.Add(new ProductionWarehouseMovementReportNomenclature { Id = title.NomenclatureId, Amount = decimal.Round(amountSum, 2) });
				}

				var amountTotalRow = new ProductionWarehouseMovementReportNode
				{
					MovementDocumentName = "Сумма шт:",
					NomenclatureColumns = amountTotalNomenclatures
				};

				ResultNodeList.Add(amountTotalRow);

				#endregion

				#region Формирование строки с ценой закупки на дату

				var purchasePriceNomenclatures = new List<ProductionWarehouseMovementReportNomenclature>();

				foreach(var title in Titles)
				{
					var purchasePrice = rowsOnDate
						.SelectMany(x => x.NomenclatureColumns)
						.Where(x => x.Id == title.NomenclatureId)
						.Max(x => x.PurchasePrice);

					purchasePriceNomenclatures.Add(new ProductionWarehouseMovementReportNomenclature { Id = title.NomenclatureId, Amount = purchasePrice, IsTotal = true });
				}

				var purchasePriceTotalRow = new ProductionWarehouseMovementReportNode
				{
					MovementDocumentName = "Цена:",
					NomenclatureColumns = purchasePriceNomenclatures
				};

				ResultNodeList.Add(purchasePriceTotalRow);

				#endregion

				#region Формирование строки с итоговой суммой за дату

				var sumTotalColumns = new List<ProductionWarehouseMovementReportNomenclature>();

				for(int i = 0; i < Titles.Count; i++)
				{
					sumTotalColumns.Add(new ProductionWarehouseMovementReportNomenclature
					{
						Id = Titles[i].NomenclatureId,
						Amount = decimal.Round(amountTotalNomenclatures[i].Amount * purchasePriceNomenclatures[i].Amount),
						IsTotal = true
					});
				}

				var sumTotalRow = new ProductionWarehouseMovementReportNode
				{
					MovementDocumentName = "Сумма р.:",
					NomenclatureColumns = sumTotalColumns
				};

				ResultNodeList.Add(sumTotalRow);

				#endregion
			}
		}

		private List<ProductionWarehouseMovementReportDataBaseNode> ReadFromDataBase()
		{
			List<Warehouse> warehouses = new List<Warehouse>();

			if(_warehouse == null)
			{
				warehouses.AddRange(_warehouseList);
			}
			else
			{
				warehouses.Add(_warehouse);
			}

			int[] selectedWarehouses = warehouses.Select(x => x.Id).ToArray();

			return _uow.Session.Query<MovementDocumentItem>()
				.Where(x =>
					(x.Document.Status == MovementDocumentStatus.Accepted || x.Document.Status == MovementDocumentStatus.Discrepancy)
					&& (selectedWarehouses.Contains(x.Document.FromWarehouse.Id))
					&& (x.Document.TimeStamp >= _startDate && x.Document.TimeStamp <= (_endDate ?? DateTime.MaxValue))
					&& x.Nomenclature.Category == NomenclatureCategory.water
					&& x.ReceivedAmount > 0)
				.Fetch(x => x.IncomeOperation)
				.Fetch(x => x.WriteOffOperation)
				.Fetch(x => x.Nomenclature)
				.ThenFetch(x => x.Unit)
				.Select(x => new ProductionWarehouseMovementReportDataBaseNode
				{
					MovementDocumentDate = x.Document.TimeStamp.Date,
					MovementDocumentId = x.Document.Id,
					NomenclatureId = x.Nomenclature.Id,
					NomenclatureName = x.Nomenclature.Name,
					Amount = x.ReceivedAmount,
					PurchasePrices = x.Nomenclature.PurchasePrices
				})
				.ToList();
		}

		private List<ProductionWarehouseMovementReportNomenclature> FillNomenclatureColumnsByTitles(IEnumerable<ProductionWarehouseMovementReportNomenclature> nonZeroNomenclatures)
		{
			var resultNomenclatures = new List<ProductionWarehouseMovementReportNomenclature>();

			foreach(var title in Titles)
			{
				var nomenclature = new ProductionWarehouseMovementReportNomenclature();

				if(nonZeroNomenclatures.Any(x => x.Id == title.NomenclatureId))
				{
					nomenclature = nonZeroNomenclatures.SingleOrDefault(x => x.Id == title.NomenclatureId);
				}
				else
				{
					nomenclature = new ProductionWarehouseMovementReportNomenclature
					{
						Id = title.NomenclatureId,
						Amount = 0
					};
				}

				resultNomenclatures.Add(nomenclature);
			}

			return resultNomenclatures;
		}

		public void Fill(bool isDetailed)
		{
			var databaseNodeList = ReadFromDataBase();

			ResultNodeList = new List<ProductionWarehouseMovementReportNode>();

			if(isDetailed)
			{
				GenerateDetailedReport(databaseNodeList);
			}
			else
			{
				GenerateBaseReport(databaseNodeList);
			}
		}

		public void Export(IFileDialogService fileDialogService, string fileName, bool isDetailed, Encoding encoding = null)
		{
			if(fileDialogService is null)
			{
				throw new ArgumentNullException(nameof(fileDialogService));
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".csv";
			dialogSettings.FileName = $"{fileName} {DateTime.Now:yyyy-MM-dd-HH-mm}.csv";

			var result = fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!result.Successful)
			{
				return;
			}

			var writer = new StreamWriter(result.Path, false, encoding ?? Encoding.GetEncoding(1251));

			var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);

			if(isDetailed)
			{
				var columnNames = Titles.Select(x => x.NomenclatureName).ToList();

				var headers = new[] { "Дата", "ТМЦ" }.Concat(columnNames).ToList();

				foreach(var header in headers)
				{
					csv.WriteField(header);
				}

				csv.NextRecord();

				foreach(var row in ResultNodeList)
				{
					csv.WriteField(row.MovementDocumentDate > DateTime.MinValue ? row.MovementDocumentDate.ToShortDateString() : string.Empty);
					csv.WriteField(row.MovementDocumentName);

					foreach(var column in row.NomenclatureColumns)
					{
						csv.WriteField(column.IsTotal? $"{ column.Amount:F2}" : $"{ column.Amount:F0}");
					}

					csv.NextRecord();
				}
			}
			else
			{
				var headers = new[] { "ТМЦ", "Период", "Цена", "Количество", "Сумма" };

				foreach(var header in headers)
				{
					csv.WriteField(header);
				}

				csv.NextRecord();

				foreach(var row in ResultNodeList.SingleOrDefault().NomenclatureColumns)
				{
					csv.WriteField(row.NomenclatureName);
					csv.WriteField(row.DateRange);
					csv.WriteField(row.IsTotal ? "" : row.PurchasePrice.ToString());
					csv.WriteField(row.Amount);
					csv.WriteField(row.Sum);
					csv.NextRecord();
				}
			}

			writer.Close();
		}

		public List<ProductionWarehouseMovementReportNode> ResultNodeList { get; set; }
		public IList<ProductionWarehouseMovementReportTitleNode> Titles { get; set; }
	}
}

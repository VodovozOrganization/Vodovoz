using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.Search;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic.FastDelivery;
using Vodovoz.Domain.Sale;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryFailsReport
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly FastDeliveryAvailabilityFilterViewModel _filterViewModel;
		private readonly IJournalSearch _journalSearch;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IUnitOfWorkFactory _uowFactory;

		public FastDeliveryFailsReport(IUnitOfWorkFactory uowFactory, FastDeliveryAvailabilityFilterViewModel filterViewModel,
			IJournalSearch journalSearch, INomenclatureSettings nomenclatureSettings, IFileDialogService fileDialogService)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_nomenclatureSettings =
				nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_journalSearch = journalSearch ?? throw new ArgumentNullException(nameof(journalSearch));
		}

		public void Export()
		{
			IList<FastDeliveryFailsReportRow> rows;

			using(var uow = _uowFactory.CreateWithoutRoot(GetType().Name))
			{
				rows = GetRows(uow);
			}

			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{GetReportFileName(_filterViewModel.IsNomenclatureNotInStock)} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				if(_filterViewModel.IsNomenclatureNotInStock == null || !_filterViewModel.IsNomenclatureNotInStock.Value)
				{
					SaveReport(result.Path, rows);
				}
				else
				{
					SaveAssortmentAnalysisReport(result.Path, rows);
				}
			}
		}

		private IList<FastDeliveryFailsReportRow> GetRows(IUnitOfWork uow)
		{
			FastDeliveryAvailabilityHistory fastDeliveryAvailabilityHistoryAlias = null;
			FastDeliveryAvailabilityHistoryItem fastDeliveryAvailabilityHistoryItemAlias = null;
			FastDeliveryNomenclatureDistributionHistory fastDeliveryNomenclatureDistributionHistoryAlias = null;
			FastDeliveryOrderItemHistory fastDeliveryOrderItemHistoryAlias = null;
			Employee authorAlias = null;
			Employee logisticianAlias = null;
			Nomenclature nomenclatureAlias = null;
			DeliveryPoint deliveryPointAlias = null;
			District districtAlias = null;
			Domain.Client.Counterparty counterpartyAlias = null;
			FastDeliveryFailsReportRow resultAlias = null;
			Order orderAlias = null;

			var authorProjection = CustomProjections.Concat_WS(
				"",
				Projections.Property(() => authorAlias.LastName),
				Projections.Property(() => authorAlias.Name),
				Projections.Property(() => authorAlias.Patronymic)
			);

			var logisticianProjection = CustomProjections.Concat_WS(
				"",
				Projections.Property(() => logisticianAlias.LastName),
				Projections.Property(() => logisticianAlias.Name),
				Projections.Property(() => logisticianAlias.Patronymic)
			);

			var validFastDeliveryCheckingSubQuery = QueryOver.Of<FastDeliveryAvailabilityHistoryItem>()
				.Where(fhi => fhi.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
				.And(fhi => fhi.IsValidToFastDelivery)
				.Select(fhi => fhi.Id);
			
			var lastFastDeliveryCheckingId = QueryOver.Of<FastDeliveryAvailabilityHistory>()
				.Where(fh => fh.DeliveryPoint.Id == fastDeliveryAvailabilityHistoryAlias.DeliveryPoint.Id)
				.And(fh => fh.VerificationDate.Date == fastDeliveryAvailabilityHistoryAlias.VerificationDate.Date)
				.Select(Projections.Id())
				.OrderBy(fh => fh.Id).Desc
				.Take(1);
				
			var validLastFastDeliveryCheckingSubQuery = QueryOver.Of(() => fastDeliveryAvailabilityHistoryItemAlias)
				.WithSubquery.WhereProperty(() => fastDeliveryAvailabilityHistoryItemAlias.FastDeliveryAvailabilityHistory.Id)
					.Eq(lastFastDeliveryCheckingId)
				.And(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidToFastDelivery)
				.Select(Projections.Property(() => fastDeliveryAvailabilityHistoryItemAlias.Id));

			#region Ассортимент не в запасе

			var nomenclatureDistributionSubquery = QueryOver.Of(() => fastDeliveryNomenclatureDistributionHistoryAlias)
				.Where(() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id ==
							 fastDeliveryAvailabilityHistoryAlias.Id)
				.Where(() => fastDeliveryOrderItemHistoryAlias.Nomenclature.Id ==
							 fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id)
				.Select(Projections.Property(() => fastDeliveryNomenclatureDistributionHistoryAlias.Nomenclature.Id));

			var nomenclatureNotInStockSubquery = QueryOver.Of(() => fastDeliveryOrderItemHistoryAlias)
				.JoinAlias(() => fastDeliveryOrderItemHistoryAlias.Nomenclature, () => nomenclatureAlias)
				.Where(() => fastDeliveryOrderItemHistoryAlias.FastDeliveryAvailabilityHistory.Id == fastDeliveryAvailabilityHistoryAlias.Id)
				.And(() => nomenclatureAlias.ProductGroup.Id != _nomenclatureSettings.PromotionalNomenclatureGroupId)
				.WithSubquery.WhereNotExists(nomenclatureDistributionSubquery)
				.Select(Projections.Conditional(
					Restrictions.Gt(
						Projections.Max(Projections.Property(() => fastDeliveryOrderItemHistoryAlias.Nomenclature.Id)), 0),
					Projections.Constant(true),
					Projections.Constant(false)));

			#endregion

			var orderSubquery = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.DeliveryPoint.Id == deliveryPointAlias.Id)
				.And(() => orderAlias.CreateDate.Value.Date == fastDeliveryAvailabilityHistoryAlias.VerificationDate.Date)
				.And(() => orderAlias.IsFastDelivery == false)
				.Select(Projections.Property(() => orderAlias.Id));

			var timesDiffInMinutesTemplate = new SQLFunctionTemplate(NHibernateUtil.Int32, "TIMESTAMPDIFF(MINUTE, ?1, ?2)");

			var itemsQuery = uow.Session.QueryOver(() => fastDeliveryAvailabilityHistoryAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Items, () => fastDeliveryAvailabilityHistoryItemAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Author, () => authorAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.DeliveryPoint, () => deliveryPointAlias)
				.Left.JoinAlias(() => deliveryPointAlias.District, () => districtAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Logistician, () => logisticianAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(() => fastDeliveryAvailabilityHistoryAlias.OrderItemsHistory, () => fastDeliveryOrderItemHistoryAlias)
				.Left.JoinAlias(() => fastDeliveryOrderItemHistoryAlias.Nomenclature, () => nomenclatureAlias);

			// Отчёт "Анализ ассортимента не в запасе"
			if(_filterViewModel.IsNomenclatureNotInStock != null && _filterViewModel.IsNomenclatureNotInStock.Value)
			{
				var distributionNomenclaturesSubquery = QueryOver.Of(() => fastDeliveryNomenclatureDistributionHistoryAlias)
					.Where(() => fastDeliveryNomenclatureDistributionHistoryAlias.FastDeliveryAvailabilityHistory.Id ==
					             fastDeliveryAvailabilityHistoryAlias.Id)
					.Select(x => x.Nomenclature.Id);

				itemsQuery.WithSubquery.WhereProperty(() => nomenclatureAlias.Id).NotIn(distributionNomenclaturesSubquery);
			}

			itemsQuery.WithSubquery.WhereExists(orderSubquery);

			itemsQuery.WithSubquery.WhereNotExists(validFastDeliveryCheckingSubQuery);
			
			itemsQuery.WithSubquery.WhereNotExists(validLastFastDeliveryCheckingSubQuery);

			if(_filterViewModel.VerificationDateFrom != null && _filterViewModel.VerificationDateTo != null)
			{
				itemsQuery.Where(x => x.VerificationDate >= _filterViewModel.VerificationDateFrom.Value.Date.Add(new TimeSpan(0, 0, 0, 0))
									  && x.VerificationDate <= _filterViewModel.VerificationDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			if(_filterViewModel.Counterparty != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Counterparty.Id == _filterViewModel.Counterparty.Id);
			}

			if(_filterViewModel.District != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.District.Id == _filterViewModel.District.Id);
			}

			if(_filterViewModel.Logistician != null)
			{
				itemsQuery.Where(() => fastDeliveryAvailabilityHistoryAlias.Logistician.Id == _filterViewModel.Logistician.Id);
			}

			// Истина, если есть хотя бы одна номенклатура, которая была в заказе с запросом на ДЗЧ, но при этом отутствовала в дополнительной погрузке (запасе)
			if(_filterViewModel.IsNomenclatureNotInStock != null)
			{
				itemsQuery.Where(Restrictions.Eq(Projections.SubQuery(nomenclatureNotInStockSubquery),
					_filterViewModel.IsNomenclatureNotInStock));
			}

			if(_filterViewModel.LogisticianReactionTimeMinutes > 0)
			{
				var timestampProjection = Projections.SqlFunction(timesDiffInMinutesTemplate, NHibernateUtil.Int32,
					Projections.Property(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate),
					Projections.Property(() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion));

				itemsQuery.Where(Restrictions.Ge(timestampProjection, _filterViewModel.LogisticianReactionTimeMinutes));
			}

			var searchCriterion = new SearchCriterion(_journalSearch);

			itemsQuery.Where(searchCriterion.By(
					() => fastDeliveryAvailabilityHistoryAlias.Id,
					() => fastDeliveryAvailabilityHistoryAlias.Order.Id,
					() => authorProjection,
					() => logisticianProjection,
					() => counterpartyAlias.Name,
					() => deliveryPointAlias.ShortAddress,
					() => districtAlias.DistrictName,
					() => fastDeliveryAvailabilityHistoryAlias.LogisticianComment,
					() => fastDeliveryAvailabilityHistoryAlias.VerificationDate,
					() => fastDeliveryAvailabilityHistoryAlias.LogisticianCommentVersion
				).Finish()
			);

			return itemsQuery
				.SelectList(list => list
					.Select(() => fastDeliveryAvailabilityHistoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fastDeliveryAvailabilityHistoryAlias.VerificationDate).WithAlias(() => resultAlias.VerificationDate)
					.Select(() => deliveryPointAlias.Id).WithAlias(() => resultAlias.DeliveryPointId)
					.Select(() => districtAlias.DistrictName).WithAlias(() => resultAlias.District)
					.Select(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidDistanceByLineToClient).WithAlias(() => resultAlias.IsValidDistanceByLine)
					.Select(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidIsGoodsEnough).WithAlias(() => resultAlias.IsValidIsGoodsEnough)
					.Select(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidLastCoordinateTime).WithAlias(() => resultAlias.IsValidLastCoordinateTime)
					.Select(() => fastDeliveryAvailabilityHistoryItemAlias.IsValidUnclosedFastDeliveries).WithAlias(() => resultAlias.IsValidUnclosedFastDeliveries)
					.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Nomenclature)
				)
				.TransformUsing(Transformers.AliasToBean<FastDeliveryFailsReportRow>())
				.List<FastDeliveryFailsReportRow>();
		}

		private void SaveReport(string path, IList<FastDeliveryFailsReportRow> rows)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Заказы");

				worksheet.Range(worksheet.Cell(1, 1), worksheet.Cell(1, 6)).Merge();
				worksheet.Column(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				worksheet.Cell(1, 1).Value = "Сводный отчет по заказам, не попавшим в доставку за час";

				worksheet.Column(1).Width = 20;
				for(var col = 2; col <= 6; col++)
				{
					worksheet.Column(col).Width = 10;
				}

				worksheet.Row(1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				worksheet.Row(1).Style.Font.SetBold(true);
				worksheet.Cell(3, 1).Value = "Период";
				worksheet.Cell(3, 2).Value = "с";
				worksheet.Cell(3, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
				worksheet.Cell(3, 3).Value = _filterViewModel.VerificationDateFrom;
				worksheet.Cell(3, 4).Value = "по";
				worksheet.Cell(3, 4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
				worksheet.Cell(3, 5).Value = _filterViewModel.VerificationDateTo?.Date;

				worksheet.Row(5).Style.Font.SetBold(true);
				worksheet.Row(5).Style.Alignment.WrapText = true;
				worksheet.Row(5).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				DrawRowBording(worksheet, 5, 6);

				worksheet.Cell(5, 1).Value = "Суммарные значения за период отчета";
				worksheet.Cell(5, 2).Value = "Не доставлено заказов (ТД)";
				worksheet.Cell(5, 3).Value = "Не хватило остатков";
				worksheet.Cell(5, 4).Value = "Много заказов в работе";
				worksheet.Cell(5, 5).Value = "Не приходят координаты";
				worksheet.Cell(5, 6).Value = "Большое расстояние";

				GenerateRows(worksheet, 6, "", rows);

				worksheet.Cell(8, 1).Value = "Детализация по районам";
				worksheet.Range(worksheet.Cell(8, 1), worksheet.Cell(8, 6)).Merge();
				worksheet.Cell(8, 1).Style.Font.SetBold(true);
				worksheet.Cell(8, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

				var rowIndex = 9;

				for(var date = _filterViewModel.VerificationDateFrom.Value.Date;
					date <= _filterViewModel.VerificationDateTo.Value.Date;
					date = date.AddDays(1))
				{
					var dayRows = rows.Where(x => x.VerificationDate.Date == date).ToArray();

					worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
					worksheet.Cell(rowIndex, 1).Style.Font.Bold = true;

					GenerateRows(worksheet, rowIndex, date.ToString(), dayRows);

					var districtsRows = dayRows.OrderBy(x => x.District).GroupBy(x => x.District);

					foreach(var district in districtsRows)
					{
						rowIndex++;
						GenerateRows(worksheet, rowIndex, district.Key, district.ToArray());
					}

					rowIndex += 2;
				}

				workbook.SaveAs(path);
			}
		}

		private void SaveAssortmentAnalysisReport(string path, IList<FastDeliveryFailsReportRow> rows)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Ассортимент не в запасе");

				worksheet.Cell(1, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				worksheet.Cell(1, 2).Value = "Анализ ассортимента не в запасе";
				worksheet.Row(1).Style.Font.SetBold(true);

				worksheet.Column(1).Width = 10;
				worksheet.Column(2).Width = 50;
				worksheet.Column(3).Width = 20;
				worksheet.Column(4).Width = 10;

				worksheet.Cell(2, 2).Value = $"период с {_filterViewModel.VerificationDateFrom:dd.MM.yyyy} по {_filterViewModel.VerificationDateTo?.Date:dd.MM.yyyy}";
				worksheet.Cell(2, 2).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

				worksheet.Row(4).Style.Font.SetBold(true);
				worksheet.Row(4).Style.Alignment.WrapText = true;
				worksheet.Row(4).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				DrawRowBording(worksheet, 4, 3);

				worksheet.Cell(4, 1).Value = "№";
				worksheet.Cell(4, 2).Value = "Номенклатура";
				worksheet.Cell(4, 3).Value = "Кол-во адресов";

				var rowIndex = 5;
				var startRowIndex = rowIndex;

				var groupedByNomenclatureRows = rows.OrderBy(x => x.Nomenclature)
					.GroupBy(x => x.Nomenclature)
					.ToArray();

				var assortmentDictionary = groupedByNomenclatureRows
					.ToDictionary(
						gr => gr.Key,
						gr => gr.GroupBy(x => x.DeliveryPointId).Count());

				foreach(var nomenclatureRow in assortmentDictionary.OrderByDescending(x=>x.Value))
				{
					worksheet.Cell(rowIndex, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
					worksheet.Cell(rowIndex, 1).Value = rowIndex - startRowIndex + 1;
					worksheet.Cell(rowIndex, 2).Value = nomenclatureRow.Key;
					worksheet.Cell(rowIndex, 3).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
					worksheet.Cell(rowIndex, 3).Value = nomenclatureRow.Value;
					DrawRowBording(worksheet, rowIndex, 3);
					rowIndex += 1;
				}

				workbook.SaveAs(path);
			}
		}

		private void DrawRowBording(IXLWorksheet worksheet, int rowIndex, int colCount)
		{
			for(var col = 1; col <= colCount; col++)
			{
				worksheet.Cell(rowIndex, col).Style.Border.SetTopBorder(XLBorderStyleValues.Thin);
				worksheet.Cell(rowIndex, col).Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
				worksheet.Cell(rowIndex, col).Style.Border.SetLeftBorder(XLBorderStyleValues.Thin);
				worksheet.Cell(rowIndex, col).Style.Border.SetRightBorder(XLBorderStyleValues.Thin);
			}
		}

		private void GenerateRows(IXLWorksheet worksheet, int rowIndex, string title, IList<FastDeliveryFailsReportRow> rows)
		{
			worksheet.Cell(rowIndex, 1).Value = title;
			worksheet.Cell(rowIndex, 2).Value = rows.GroupBy(x => x.DeliveryPointId).Count();

			var groupedByDeliveryPoint = rows.GroupBy(x => x.DeliveryPointId);

			int goodsEnough = 0, unclosedFastDeliveries = 0, coordinates = 0, distances = 0;

			foreach(var row in groupedByDeliveryPoint)
			{
				var validByDistance = row.Where(x => x.IsValidDistanceByLine.HasValue && x.IsValidDistanceByLine.Value).ToArray();
				if(validByDistance.Any())
				{
					goodsEnough += validByDistance.Any(x => x.IsValidIsGoodsEnough.HasValue && !x.IsValidIsGoodsEnough.Value) ? 1 : 0;
					unclosedFastDeliveries += validByDistance.Any(x => x.IsValidUnclosedFastDeliveries.HasValue && !x.IsValidUnclosedFastDeliveries.Value) ? 1 : 0;
					coordinates += validByDistance.Any(x => x.IsValidLastCoordinateTime.HasValue && !x.IsValidLastCoordinateTime.Value) ? 1 : 0;
				}
				else
				{
					distances += 1;
				}
			}

			worksheet.Cell(rowIndex, 3).Value = goodsEnough;

			worksheet.Cell(rowIndex, 4).Value = unclosedFastDeliveries;

			worksheet.Cell(rowIndex, 5).Value = coordinates;

			worksheet.Cell(rowIndex, 6).Value = distances;

			DrawRowBording(worksheet, rowIndex, 6);
		}

		public static string GetReportName(bool? isNomenclatureNotInStock) =>
			isNomenclatureNotInStock == null
				? "Отчёт по всем заказам"
				: isNomenclatureNotInStock.Value
					? "Анализ ассортимента не в запасе"
					: "Отчет по заказам с ассортиментом из запаса";

		private string GetReportFileName(bool? isNomenclatureNotInStock) =>
			isNomenclatureNotInStock == null
				? "Заказы, не попавшие в доставку за час"
				: isNomenclatureNotInStock.Value
					? "Анализ ассортимента не в запасе"
					: "Отчет по заказам с ассортиментом из запаса";
	}
}

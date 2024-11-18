using ClosedXML.Excel;
using Dapper;
using DatabaseServiceWorker.Helpers;
using DatabaseServiceWorker.PowerBiWorker.Dto;
using MySqlConnector;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;

namespace DatabaseServiceWorker.PowerBiWorker
{
	internal partial class PowerBiExportWorker
	{		
		private async Task ExportReportsAsync(
			MySqlConnection connectionSource,
			MySqlConnection connectionTarget,
			IUnitOfWorkFactory unitOfWorkFactory,
			DateTime startDate,
			DateTime endDate,
			IGeneralSettings generalSettings,
			IDeliveryRepository deliveryRepository,
			ITrackRepository trackRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			INomenclatureSettings nomenclatureSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			CancellationToken stoppingToken)
		{
			TruncateReportTables(connectionTarget, startDate);

			#region GeneralInformation

			var revenueDaySql = GetRevenuesSql(startDate, endDate);
			var revenueDayList = await connectionSource.GetDataAsync<RevenueDayDto>(revenueDaySql);			
			var deliveredSql = GetDeliveredSql(startDate, endDate);
			var deliveredList = await connectionSource.GetDataAsync<DeliveredDto>(deliveredSql);

			var generalInformationTransaction = connectionTarget.BeginTransaction();

			foreach(var delivered in deliveredList)
			{
				var row = new
				{
					date = delivered.Date,
					revenue_day = revenueDayList.Single(r => r.Date == delivered.Date).RevenueDay,
					shipment_day_plan = delivered.ShipmentDayPlan,
					shipment_day_fact = delivered.ShipmentDayFact,
					delivery_plan = delivered.DeliveryPlan,
					delivery_fact = delivered.DeliveryFact
				};

				connectionTarget.Execute("INSERT INTO general_info (`date`, revenue_day, shipment_day_plan, shipment_day_fact, delivery_plan, delivery_fact)" +
					" VALUES(@date, @revenue_day, @shipment_day_plan, @shipment_day_fact, @delivery_plan, @delivery_fact);",
					row,
					generalInformationTransaction);
			}

			generalInformationTransaction.Commit();

			#endregion

			#region UndeliveriesInformation

			var undeliveredSql = GetUndeliveredSql(startDate, endDate);			
			var undeliveredList = await connectionSource.GetDataAsync<UndeliveredDto>(undeliveredSql);

			var undeliveryTransaction = connectionTarget.BeginTransaction();

			foreach(var undelivery in undeliveredList)
			{
				var row = new
				{
					date = undelivery.Date,
					responsible = undelivery.Responsible,
					quantity = undelivery.Quantity,
					quantity19 = undelivery.Quantity19
				};

				connectionTarget.Execute("INSERT INTO undeliveries_info(`date`, responsible, quantity, quantity19)" +
					" VALUES(@date, @responsible, @quantity, @quantity19);",
					row,
					undeliveryTransaction);
			}

			undeliveryTransaction.Commit();

			#endregion

			#region FastDeliveryInformation

			var fastDeliveryFailsSql = GetFastDeliveryFails(startDate, endDate);
			var fastDeliveryFailsList = await connectionSource.GetDataAsync<FastDeliveryFailDto>(fastDeliveryFailsSql,
					new { product_group_id = nomenclatureSettings.PromotionalNomenclatureGroupId });

			var numberOfFastDeliverySalesSql = GetNumberOfFastDeliverySalesSql(startDate, endDate);
			var numberOfFastDeliverySales = await connectionSource.GetDataAsync<(DateTime date, decimal quantity)>(numberOfFastDeliverySalesSql);

			var fastDeliveryLatesSql = GetLatesSql(startDate, endDate);
			var fastDeliveryLateList = await connectionSource.GetDataAsync<LateDto>(fastDeliveryLatesSql,
				new { interval_select_mode = generalSettings.FastDeliveryIntervalFrom.ToString() });

			var fastDeliveryUndeliveriesSql = GetFastDeliveryUndeliveriesSql(startDate, endDate);
			var fastDeliveryUndeliveriesList = await connectionSource.GetDataAsync<(DateTime date, decimal quantity)>(fastDeliveryUndeliveriesSql);

			var numberOfFastdeliveryComplaintsSql = GeNumberOfFastdeliveryComplaintsSql(startDate, endDate);
			var numberOfFastdeliveryComplaintsList = await connectionSource.GetDataAsync<(DateTime date, decimal quantity)>(numberOfFastdeliveryComplaintsSql);

			var coverageList = new List<CoverageDto>();

			using(var uow = unitOfWorkFactory.CreateWithoutRoot("PowerBiCoverageReport"))
			{
				for(DateTime date = startDate; date < endDate; date = date.AddDays(1))
				{
					var fastDeliveryCoverage = await GetCoverageAsync(deliveryRulesSettings, uow, deliveryRepository, trackRepository, scheduleRestrictionRepository, date, stoppingToken);
					fastDeliveryCoverage.Date = date;
					coverageList.Add(fastDeliveryCoverage);
				}
			}

			var remainingBottleList = new List<RemainingBottlesDto>();

			using(var uow = unitOfWorkFactory.CreateWithoutRoot("PowerBiCoverageReport"))
			{
				for(DateTime date = startDate; date < endDate; date = date.AddDays(1))
				{
					var remainingBottle = GetRemainingBottle(uow, date);
					remainingBottle.Date = date;
					remainingBottleList.Add(remainingBottle);
				}
			}

			TruncateFastDeliveryReportTables(connectionTarget, startDate);

			var fastDeliveryTransaction = connectionTarget.BeginTransaction();

			for(DateTime curDate = startDate; curDate < endDate; curDate = curDate.AddDays(1))
			{
				var row = new
				{
					date = curDate,
					number_of_sales = numberOfFastDeliverySales.Single(x => x.date == curDate).quantity,
					number_of_late_less_5 = fastDeliveryLateList.Single(x => x.Date == curDate).LessThan5Minutes,
					number_of_late_less_30 = fastDeliveryLateList.Single(x => x.Date == curDate).LessThan30Minutes,
					number_of_late_more_30 = fastDeliveryLateList.Single(x => x.Date == curDate).MoreThan30Minutes,
					number_of_fail_delivery = fastDeliveryUndeliveriesList.Single(x => x.date == curDate).quantity,
					number_of_claim = numberOfFastdeliveryComplaintsList.Single(x => x.date == curDate).quantity,
					fill = coverageList.Single(x => x.Date == curDate).Fill.ToString("F2"),
					average_radius = coverageList.Single(x => x.Date == curDate).AverageRadius.ToString("F2"),
					number_of_cars = coverageList.Single(x => x.Date == curDate).NumberOfCars.ToString("F2"),
					not_enough_product = fastDeliveryFailsList.Single(x => x.Date == curDate).IsValidIsGoodsEnoughTotal,
					lot_of_orders = fastDeliveryFailsList.Single(x => x.Date == curDate).IsValidUnclosedFastDeliveriesTotal,
					not_coordinates = fastDeliveryFailsList.Single(x => x.Date == curDate).IsValidLastCoordinateTimeTotal,
					long_distance = fastDeliveryFailsList.Single(x => x.Date == curDate).IsValidDistanceByLineToClientTotal,
					uploaded_19 = remainingBottleList.Single(x => x.Date == curDate).Uploaded19,
					sold_19 = remainingBottleList.Single(x => x.Date == curDate).Sold19,
					return_19 = remainingBottleList.Single(x => x.Date == curDate).Return19,
				};

				var sql =
					"INSERT INTO fast_delivery_info(`date`, number_of_sales, number_of_late_less_5, number_of_late_less_30, number_of_late_more_30, " +
					"number_of_fail_delivery, number_of_claim, fill, average_radius, number_of_cars," +
					"not_enough_product, lot_of_orders, not_coordinates, long_distance, uploaded_19, sold_19, return_19)" +
					" VALUES(@date, @number_of_sales, @number_of_late_less_5, @number_of_late_less_30, @number_of_late_more_30, @number_of_fail_delivery, @number_of_claim, "
					+ "@fill, @average_radius, @number_of_cars, @not_enough_product, @lot_of_orders, @not_coordinates, @long_distance, @uploaded_19, @sold_19, @return_19);";

				connectionTarget.Execute(
					sql,
					row,
				fastDeliveryTransaction);
			}

			fastDeliveryTransaction.Commit();

			#endregion

			// Скоро не понадобится
			#region ExcelFile

			var smbPath = $"smb://{_options.Value.Login}:{_options.Value.Password}@{_options.Value.ExportPath}";

			var file = new SmbFile(smbPath);
			var readStream = file.GetInputStream();
			var memStream = new MemoryStream();
			((Stream)readStream).CopyTo(memStream);
			readStream.Dispose();

			using(var excelWorkbook = new XLWorkbook(memStream))
			{
				if(IsNeedExportToday(excelWorkbook))
				{
					ClearSheetsData(excelWorkbook);

					for(DateTime curDate = startDate /*new DateTime(2024, 03, 01)*/; curDate < endDate; curDate = curDate.AddDays(1))
					{
						AddToGeneralSheet(excelWorkbook.Worksheet(1), curDate, revenueDayList.Single(x => x.Date == curDate).RevenueDay, deliveredList.Single(x => x.Date == curDate));

						AddToUndeliveriesSheet(excelWorkbook.Worksheet(2), curDate, undeliveredList);

						AddToFastDeliverySheet(
							excelWorkbook.Worksheet(3),
							curDate,
							fastDeliveryLateList.Single(x => x.Date == curDate),
							numberOfFastDeliverySales.Single(x => x.date == curDate).quantity,
							fastDeliveryUndeliveriesList.Single(x => x.date == curDate).quantity,
							numberOfFastdeliveryComplaintsList.Single(x => x.date == curDate).quantity,
							coverageList.Single(x => x.Date == curDate),
							fastDeliveryFailsList.Single(x => x.Date == curDate),
							remainingBottleList.Single(x => x.Date == curDate));
					}

					excelWorkbook.Save();
					WriteExcelStreamToFile(file, memStream);
				}
			}

			#endregion
		}

		private void TruncateReportTables(MySqlConnection connectionTarget, DateTime truncateFromDate)
		{
			var truncateTransaction = connectionTarget.BeginTransaction();

			connectionTarget.Execute("delete from general_info where date >= @date;", new { date = truncateFromDate }, truncateTransaction);
			connectionTarget.Execute("delete from undeliveries_info where date >= @date;", new { date = truncateFromDate }, truncateTransaction);

			truncateTransaction.Commit();
		}

		private void TruncateFastDeliveryReportTables(MySqlConnection connectionTarget, DateTime truncateFromDate)
		{
			var truncateTransaction = connectionTarget.BeginTransaction();

			connectionTarget.Execute("delete from fast_delivery_info where date >= @date;", new { date = truncateFromDate }, truncateTransaction);

			truncateTransaction.Commit();
		}

		private void WriteExcelStreamToFile1(SmbFile file, MemoryStream memStream)
		{
			if(file.Exists())
			{
				file.Delete();
			}

			file.CreateNewFile();

			var writeStream = file.GetOutputStream();
			writeStream.Write(memStream.ToArray());

			writeStream.Dispose();
		}

		private void ClearSheetsData1(XLWorkbook excelWorkbook)
		{
			var lastRowNumber1 = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
			excelWorkbook.Worksheet(1).Range($"A2:F{lastRowNumber1}").Clear();
			var lastRowNumber2 = excelWorkbook.Worksheet(2).LastRowUsed().RowNumber();
			excelWorkbook.Worksheet(2).Range($"A2:D{lastRowNumber2}").Clear();
			var lastRowNumber3 = excelWorkbook.Worksheet(3).LastRowUsed().RowNumber();
			excelWorkbook.Worksheet(3).Range($"A2:Q{lastRowNumber3}").Clear();
		}
	}
}

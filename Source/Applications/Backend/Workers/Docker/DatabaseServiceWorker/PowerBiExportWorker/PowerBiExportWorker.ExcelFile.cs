using ClosedXML.Excel;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Vodovoz.Settings.Nomenclature;

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
		private void AddToFastDeliverySheet(IXLWorksheet sheet, DateTime date, LateDto fastDeliveryLates, decimal numberOfFastDeliverySales,
			long fastDeliveryUndeliveries, long numberOfFastDeliveryComplaints, CoverageDto fastDeliveryCoverage, FastDeliveryFailDto fastDeliveryFails,
			RemainingBottlesDto remainingBottle)
		{
			var sheetLastRowNumber = sheet.LastRowUsed().RowNumber() + 1;
			var row = sheet.Row(sheetLastRowNumber);
			row.Cell(1).SetValue(date);
			row.Cell(2).SetValue(numberOfFastDeliverySales);
			row.Cell(3).SetValue(fastDeliveryLates.LessThan5Minutes);
			row.Cell(4).SetValue(fastDeliveryLates.LessThan30Minutes);
			row.Cell(5).SetValue(fastDeliveryLates.MoreThan30Minutes);
			row.Cell(6).SetValue(fastDeliveryUndeliveries);
			row.Cell(7).SetValue(numberOfFastDeliveryComplaints);
			row.Cell(8).SetValue(fastDeliveryCoverage.Fill);
			row.Cell(9).SetValue(fastDeliveryCoverage.AverageRadius);
			row.Cell(10).SetValue(fastDeliveryCoverage.NumberOfCars);
			row.Cell(11).SetValue(fastDeliveryFails.IsValidIsGoodsEnoughTotal);
			row.Cell(12).SetValue(fastDeliveryFails.IsValidUnclosedFastDeliveriesTotal);
			row.Cell(13).SetValue(fastDeliveryFails.IsValidLastCoordinateTimeTotal);
			row.Cell(14).SetValue(fastDeliveryFails.IsValidDistanceByLineToClientTotal);
			row.Cell(15).SetValue(remainingBottle.Uploaded19);
			row.Cell(16).SetValue(remainingBottle.Sold19);
			row.Cell(17).SetValue(remainingBottle.Return19);
		}

		private void AddToUndeliveriesSheet(IXLWorksheet sheet, DateTime date, IList<UndeliveredDto> undelivered)
		{
			var sheetLastRowNumber = sheet.LastRowUsed().RowNumber() + 1;

			for(int n = sheetLastRowNumber; n < sheetLastRowNumber + undelivered.Count; n++)
			{
				var row = sheet.Row(n);
				row.Cell(1).SetValue(date);
				row.Cell(2).SetValue(undelivered[n - sheetLastRowNumber].Responsible);
				row.Cell(3).SetValue(undelivered[n - sheetLastRowNumber].Quantity);
				row.Cell(4).SetValue(undelivered[n - sheetLastRowNumber].Quantity19);
			}
		}

		private void AddToGeneralSheet(IXLWorksheet sheet, DateTime date, decimal revenueDay, DeliveredDto delivered)
		{
			var sheetLastRowNumber = sheet.LastRowUsed().RowNumber() + 1;
			var row = sheet.Row(sheetLastRowNumber);
			row.Cell(1).SetValue(date);
			row.Cell(2).SetValue(revenueDay);
			row.Cell(3).SetValue(delivered.ShipmentDayPlan);
			row.Cell(4).SetValue(delivered.ShipmentDayFact);
			row.Cell(5).SetValue(delivered.DeliveryPlan);
			row.Cell(6).SetValue(delivered.DeliveryFact);
		}

		private bool IsNeedExportToday(XLWorkbook excelWorkbook)
		{
			var lastRow = excelWorkbook.Worksheet(1).LastRowUsed().RowNumber();
			DateTime lastDateTime;
			excelWorkbook.Worksheet(1).Row(lastRow).Cell(1).TryGetValue(out lastDateTime);
			var yesterdayDate = DateTime.Now.Date.AddDays(-1);
			var nowHour = DateTime.Now.Hour;
			return lastDateTime.Date < yesterdayDate && nowHour > 1; // не раньше 1 часа ночи
		}

		private async void ReadDataFromDbAndExportToExcel(
			IUnitOfWork uow,
			XLWorkbook excelWorkbook,
			DateTime date,
			INomenclatureSettings nomenclatureSettings,
			CancellationToken stoppingToken)
		{
			var revenueDay = GetRevenues(uow, date);
			var delivered = GetDelivered(uow, date);
			AddToGeneralSheet(excelWorkbook.Worksheet(1), date, revenueDay, delivered);

			var undelivered = GetUndelivered(uow, date);
			AddToUndeliveriesSheet(excelWorkbook.Worksheet(2), date, undelivered);

			var fastDeliveryLates = GetLates(uow, date);
			var numberOfFastDeliverySales = GetNumberOfFastDeliverySales(uow, date);
			var fastDeliveryUndeliveries = GetFastDeliveryUndeliveries(uow, date);
			var numberOfFastdeliveryComplaints = GeNumberOfFastdeliveryComplaints(uow, date);
			var fastDeliveryCoverage = await GetCoverageAsync(uow, date, stoppingToken);
			var fastDeliveryFails = GetFastDeliveryFails(uow, date);
			var remainingBottle = GetRemainingBottle(uow, date);

			AddToFastDeliverySheet(
				excelWorkbook.Worksheet(3),
				date,
				fastDeliveryLates,
				numberOfFastDeliverySales,
				fastDeliveryUndeliveries,
				numberOfFastdeliveryComplaints,
				fastDeliveryCoverage,
				fastDeliveryFails,
				remainingBottle
				);
		}


		private void WriteExcelStreamToFile(SmbFile file, MemoryStream memStream)
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

		private void ClearSheetsData(XLWorkbook excelWorkbook)
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

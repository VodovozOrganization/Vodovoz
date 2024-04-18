using ClosedXML.Excel;
using QS.DomainModel.UoW;
using SharpCifs.Smb;
using System;
using System.Collections.Generic;
using System.IO;

namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
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

		private void ReadDataFromDbAndExportToExcel(IUnitOfWork uow, XLWorkbook excelWorkbook, DateTime date)
		{
			var revenueDay = GetRevenues(uow, date);
			var delivered = GetDelivered(uow, date);
			var undelivered = GetUndelivered(uow, date);

			AddToGeneralSheet(excelWorkbook.Worksheet(1), date, revenueDay, delivered);
			AddToUndeliveriesSheet(excelWorkbook.Worksheet(2), date, undelivered);
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
		}
	}
}

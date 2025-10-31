using System;
using System.Collections.Generic;
using System.Text;
using ClosedXML.Excel;
using Core.Infrastructure;
using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using VodovozBusiness.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Payments
{
	[Appellative(Nominative = "Выгрузка журнала движений средств по расчетным счетам")]
	public class BankAccountsMovementsJournalReport
	{
		private const string _name = "Движения по расчетным счетам за период";
		private readonly IFileDialogService _fileDialogService;

		public BankAccountsMovementsJournalReport(IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
		}

		private string Title { get; set; }
		private DateTime StartDate { get; set; }
		private DateTime? EndDate { get; set; }

		public void Export(
			DateTime startDate,
			DateTime? endDate,
			IEnumerable<BankAccountsMovementsJournalNode> nodes)
		{
			const string extension = ".xlsx";

			StartDate = startDate;
			EndDate = endDate;
			
			var sb = new StringBuilder();
			Title = sb
				.Append(_name)
				.Append(' ')
				.Append(!EndDate.HasValue
					? $"с {StartDate:dd.MM.yyyy}"
					: $"с {StartDate:dd.MM.yyyy} по {EndDate:dd.MM.yyyy}")
				.ToString();

			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = extension,
				FileName = $"{_name} {DateTime.Now:yyyy-MM-dd-HH-mm}{extension}"
			};

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);

			if(result.Successful)
			{
				using(var wb = new XLWorkbook())
				{
					Generate(wb, nodes);
					wb.SaveAs(result.Path);
				}
			}
		}

		private void Generate(
			IXLWorkbook wb,
			IEnumerable<BankAccountsMovementsJournalNode> nodes)
		{
			var ws = wb.Worksheets.Add("Движения по р_сч");

			var colNames = new[]
			{
				BankAccountsMovementsJournalColumns.Id,
				BankAccountsMovementsJournalColumns.StartDate,
				BankAccountsMovementsJournalColumns.EndDate,
				BankAccountsMovementsJournalColumns.Account,
				BankAccountsMovementsJournalColumns.Bank,
				BankAccountsMovementsJournalColumns.Organization,
				BankAccountsMovementsJournalColumns.Empty,
				BankAccountsMovementsJournalColumns.AmountFromDocument,
				BankAccountsMovementsJournalColumns.AmountFromProgram,
				BankAccountsMovementsJournalColumns.Discrepancy,
				BankAccountsMovementsJournalColumns.DiscrepancyDescription
			};

			var row = 1;
			var col = 1;

			var title = ws.Range(row, col, row, colNames.Length);
			title.Cell(row, col).Value = Title;
			title.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
			title.Merge();
			row += 2;

			foreach(var name in colNames)
			{
				var cell = ws.Cell(row, col);
				cell.Value = name;
				cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
				cell.SetBoldFont();
				col++;
			}

			foreach(var node in nodes)
			{
				row++;
				col = 1;
				ws.Cell(row, col).Value = node.Id;
				ws.Cell(row, ++col).Value = node.StartDate.ToShortDateString();
				ws.Cell(row, ++col).Value = node.EndDate.ToShortDateString();
				ws.Cell(row, ++col).SetValue(node.Account);
				ws.Cell(row, ++col).Value = node.Bank;
				ws.Cell(row, ++col).Value = node.Organization;
				ws.Cell(row, ++col).Value = node.AccountMovementDataType.GetEnumDisplayName();
				GenerateAmountCell(ws.Cell(row, ++col), node.Amount);
				GenerateAmountFromProgramCell(ws.Cell(row, ++col), node.AmountFromProgram);
				GenerateDifferenceCell(ws.Cell(row, ++col), node.Difference);
				ws.Cell(row, ++col).Value = node.GetDiscrepancyDescription();
			}

			ws.Columns().AdjustToContents();
		}

		private void GenerateAmountCell(IXLCell cell, decimal? amount)
		{
			if(amount.HasValue)
			{
				cell.Value = amount.Value;
				cell.SetCurrencyFormat();
			}
			else
			{
				cell.Value = StringConstants.NotSet;
			}
		}

		private void GenerateAmountFromProgramCell(IXLCell cell, decimal? amountFromProgram)
		{
			if(amountFromProgram.HasValue)
			{
				cell.Value = amountFromProgram.Value;
				cell.SetCurrencyFormat();
			}
			else
			{
				cell.Value = null;
			}
		}

		private void GenerateDifferenceCell(IXLCell cell, decimal? difference)
		{
			if(difference.HasValue && difference != 0)
			{
				cell.Value = difference.Value;
				cell.SetCurrencyFormat();
			}
			else
			{
				cell.Value = null;
			}
		}
	}
}

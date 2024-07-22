using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels.Dialog;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Organizations;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class CompanyBalanceByDateViewModel : UowDialogViewModelBase
	{
		//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\56456416.xlsx"
		private readonly IGenericRepository<CompanyBalanceByDay> _companyBalanceByDayRepository;
		private DateTime _date = DateTime.Today;

		public CompanyBalanceByDateViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IGenericRepository<CompanyBalanceByDay> companyBalanceByDayRepository) : base(unitOfWorkFactory, navigation)
		{
			_companyBalanceByDayRepository =
				companyBalanceByDayRepository ?? throw new ArgumentNullException(nameof(companyBalanceByDayRepository));
			UpdateData();
			//ParseData(@"D:\Работа\Программист Веселый Водовоз\файлы выписок\ibc2-20240721-40702810094510024535.xls");
		}
		
		public CompanyBalanceByDay Entity { get; private set; }

		public DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}

		private void UpdateData()
		{
			Entity = _companyBalanceByDayRepository.Get(UoW, e => e.Date == Date).FirstOrDefault();

			if(Entity is null)
			{
				Entity = CompanyBalanceByDay.Create(Date);
				UoW.Save(Entity);
				GenerateNewData();
			}
			
			if(Entity.FundsSummary.Any())
			{
				return;
			}
		}

		private void GenerateNewData()
		{
			var accountsByFunds = UoW.GetAll<BusinessAccount>()
				.OrderBy(x => x.Funds.Id)
				.ThenBy(x => x.BusinessActivity.Id)
				.ToLookup(x => x.Funds);
			
			foreach(var accountsByFund in accountsByFunds)
			{
				var fundSummary = FundsSummary.Create(accountsByFund.Key);
				var activitiesSummary = new Dictionary<int, BusinessActivitySummary>();
				
				foreach(var businessAccount in accountsByFund)
				{
					var businessActivity = businessAccount.BusinessActivity;

					if(!activitiesSummary.TryGetValue(businessActivity.Id, out var activitySummary))
					{
						activitySummary = BusinessActivitySummary.Create(businessActivity, fundSummary);
						activitiesSummary.Add(businessActivity.Id, activitySummary);
						fundSummary.BusinessActivitySummary.Add(activitySummary);
					}
					
					var accountSummary = BusinessAccountSummary.Create(businessAccount, activitySummary);
					activitySummary.BusinessAccountsSummary.Add(accountSummary);
				}
				
				Entity.FundsSummary.Add(fundSummary);
			}
		}

		private IEnumerable<IEnumerable<string>> ParseData(string path)
		{
			/*if(!IsXlsxFile(fileName))
			{
				throw new ArgumentException("Попытка чтения файла, имеющего расширение отличное от \"xlsx\"");
			}*/

			var xlsxRowValues = new List<IList<string>>();

			var oledbConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=D:\\Работа\\Программист Веселый Водовоз\\файлы выписок\\ibc2-20240721-40702810094510024535.xls; Extended Properties='Excel 8.0;HDR=NO;IMEX=1;'";
			var oleDbConnection = new OleDbConnection(oledbConn);
			oleDbConnection.Open();
			
			//ExcelApp
			
			OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oleDbConnection);

			// Create new OleDbDataAdapter
			OleDbDataAdapter oleda = new OleDbDataAdapter();

			oleda.SelectCommand = cmd;

			// Create a DataSet which will hold the data extracted from the worksheet.
			DataSet ds = new DataSet();

			// Fill the DataSet from the data extracted from the worksheet.
			oleda.Fill(ds, "Employees");

			/*using(var document = SpreadsheetDocument.Open(path, false, new OpenSettings()))
			{
				var workbookPart = document.WorkbookPart;
				var tablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
				var sharedStringTable = tablePart.SharedStringTable;

				var worksheetPart = workbookPart.WorksheetParts.First();
				var worksheet = worksheetPart.Worksheet;

				var rows = worksheet.Descendants<Row>();

				xlsxRowValues = GetRowsCellsValues(rows, sharedStringTable).ToList();
			}*/

			return xlsxRowValues;
		}
		
		private static IList<IList<string>> GetRowsCellsValues(IEnumerable<Row> rows, SharedStringTable sharedStringTable)
		{
			var rowsValues = new List<IList<string>>();

			foreach(var row in rows)
			{
				var rowData = GetRowCellsValues(row, sharedStringTable);

				rowsValues.Add(rowData);
			}

			return rowsValues;
		}
		
		private static IList<string> GetRowCellsValues(Row row, SharedStringTable sharedStringTable)
		{
			var rowData = new List<string>();

			foreach(Cell cell in row.Elements<Cell>())
			{
				var cellValue = GetCellValue(cell, sharedStringTable);

				rowData.Add(cellValue);
			}

			return rowData;
		}
		
		private static string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
		{
			string cellValue;

			if((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
			{
				int ssid = int.Parse(cell.CellValue.Text);
				cellValue = sharedStringTable.ChildElements[ssid].InnerText;
			}
			else
			{
				cellValue = cell.CellValue?.Text ?? string.Empty;
			}

			return cellValue;
		}
	}
}

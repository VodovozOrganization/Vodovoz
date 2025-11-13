using ClosedXML.Excel;
using DateTimeHelpers;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vodovoz.Application.BankStatements;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;
using VodovozBusiness.Extensions;
using VodovozInfrastructure.StringHandlers;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class CompanyBalanceByDateViewModel : UowDialogViewModelBase, IAskSaveOnCloseViewModel
	{
		private const string _bankStatementsDirectory = @"Z:\SystemStatements\activities\CurrentStatements";
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly ICashRepository _cashRepository;
		private readonly IGenericRepository<CompanyBalanceByDay> _companyBalanceByDayRepository;
		private readonly BankStatementHandler _bankStatementHandler;
		private readonly IPermissionResult _permissionResult;

		public CompanyBalanceByDateViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			ICashRepository cashRepository,
			IGenericRepository<CompanyBalanceByDay> companyBalanceByDayRepository,
			BankStatementHandler bankStatementHandler,
			IDatePickerViewModelFactory datePickerViewModelFactory,
			ICurrentPermissionService permissionService,
			IStringHandler stringHandler) : base(unitOfWorkFactory, navigation)
		{
			if(datePickerViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(datePickerViewModelFactory));
			}

			if(permissionService == null)
			{
				throw new ArgumentNullException(nameof(permissionService));
			}

			_permissionResult = permissionService.ValidateEntityPermission(typeof(CompanyBalanceByDay));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_cashRepository = cashRepository ?? throw new ArgumentNullException(nameof(cashRepository));
			_companyBalanceByDayRepository =
				companyBalanceByDayRepository ?? throw new ArgumentNullException(nameof(companyBalanceByDayRepository));
			_bankStatementHandler = bankStatementHandler ?? throw new ArgumentNullException(nameof(bankStatementHandler));
			StringHandler = stringHandler ?? throw new ArgumentNullException(nameof(stringHandler));

			if(!_permissionResult.CanRead)
			{
				throw new AbortCreatingPageException("У Вас нет прав просмотра данной вкладки", "Недостаточно прав");
			}

			Title = typeof(CompanyBalanceByDay).GetCustomAttribute<AppellativeAttribute>(true).NominativePlural;

			CreateCommands();
			Initialize(datePickerViewModelFactory, permissionService);
		}

		public event Action CompanyBalanceChangedAction;

		public string ResultMessage { get; private set; }
		public bool AskSaveOnClose => CanUpdateData;
		public IStringHandler StringHandler { get; }
		public CompanyBalanceByDay Entity { get; private set; }
		public IList<CompanyBalanceByDay> CompanyBalances { get; private set; }
		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand LoadAndProcessDataCommand { get; private set; }
		public DelegateCommand ExportCommand { get; private set; }
		public DatePickerViewModel DatePickerViewModel { get; private set; }
		private bool CanUpdateData => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;
		private bool CanEditCompanyBalanceByDayInPreviousPeriods { get; set; }
		private DialogSettings OpenDirectorySettings { get; set; }
		private IEnumerable<(int SubdivisionId, decimal Income, decimal Expense)> CashSubdivisionsBalances { get; set; }

		public override bool Save()
		{
			if(!Validate())
			{
				return false;
			}

			UoW.Save(Entity);
			UoW.Commit();
			
			return true;
		}

		private void CreateCommands()
		{
			SaveCommand = new DelegateCommand(() => SaveAndClose(), () => CanUpdateData);
			SaveCommand.CanExecuteChangedWith(this, x => x.CanUpdateData);

			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			
			LoadAndProcessDataCommand = new DelegateCommand(
				() =>
				{
					var banksStatementsData =
						_bankStatementHandler.ProcessBankStatementsFromDirectory(_bankStatementsDirectory, DatePickerViewModel.SelectedDate);
					
					UpdateLocalData(banksStatementsData);
					Save();
				},
				() => CanUpdateData
			);
			LoadAndProcessDataCommand.CanExecuteChangedWith(this, x => x.CanUpdateData);

			ExportCommand = new DelegateCommand(ExportReport, () => CanUpdateData);
			ExportCommand.CanExecuteChangedWith(this, x => x.CanUpdateData);
		}

		private void UpdateLocalData(BankStatementProcessedResult banksStatementsData)
		{
			decimal? totalBalance = 0m;
			foreach(var fundsSummary in Entity.FundsSummary)
			{
				decimal? totalFundsBalance = 0m;
				foreach(var businessActivitySummary in fundsSummary.BusinessActivitySummary)
				{
					decimal? totalActivityBalance = 0m;
					foreach(var businessAccountSummary in businessActivitySummary.BusinessAccountsSummary)
					{
						if(TryGetCashSubdivisionBalanceAndFill(businessAccountSummary, ref totalActivityBalance))
						{
							continue;
						}
						
						if(string.IsNullOrWhiteSpace(businessAccountSummary.BusinessAccount.Number))
						{
							TryUpdateLocalTotalActivityBalance(businessAccountSummary, ref totalActivityBalance);
							continue;
						}
						
						if(banksStatementsData.BankStatementData.TryGetValue(businessAccountSummary.BusinessAccount.Number, out var data))
						{
							businessAccountSummary.Total = data.Balance;
							totalActivityBalance += data.Balance;
						}
						else
						{
							TryUpdateLocalTotalActivityBalance(businessAccountSummary, ref totalActivityBalance);
						}
					}
					
					if(totalActivityBalance == 0 && businessActivitySummary.BusinessAccountsSummary.All(x => !x.Total.HasValue))
					{
						businessActivitySummary.Total = null;
					}
					else
					{
						businessActivitySummary.Total = totalActivityBalance;
					}

					totalFundsBalance += totalActivityBalance;
				}

				if(totalFundsBalance == 0 && fundsSummary.BusinessActivitySummary.All(x => !x.Total.HasValue))
				{
					fundsSummary.Total = null;
				}
				else
				{
					fundsSummary.Total = totalFundsBalance;
				}
				totalBalance += totalFundsBalance;
			}

			if(totalBalance == 0 && Entity.FundsSummary.All(x => !x.Total.HasValue))
			{
				Entity.Total = null;
			}
			else
			{
				Entity.Total = totalBalance;
			}
			
			CompanyBalanceChangedAction?.Invoke();
			UpdateResultMessage(banksStatementsData);
		}

		private void TryUpdateLocalTotalActivityBalance(
			BusinessAccountSummary businessAccountSummary,
			ref decimal? totalActivityBalance)
		{
			if(businessAccountSummary.Total.HasValue)
			{
				totalActivityBalance += businessAccountSummary.Total;
			}
		}

		private bool TryGetCashSubdivisionBalanceAndFill(
			BusinessAccountSummary businessAccountSummary,
			ref decimal? totalActivityBalance)
		{
			if(businessAccountSummary.BusinessAccount is null)
			{
				return false;
			}
			
			(int SubdivisionId, decimal Income, decimal Expense) cashSubdivisionBCBalance = default;
			
			switch(businessAccountSummary.BusinessAccount.AccountFillType)
			{
				case AccountFillType.CashSubdivision:
					cashSubdivisionBCBalance =
						CashSubdivisionsBalances.SingleOrDefault(x =>
							x.SubdivisionId == businessAccountSummary.BusinessAccount.SubdivisionId);
					break;
			}
			
			if(cashSubdivisionBCBalance.SubdivisionId != default)
			{
				businessAccountSummary.Total = cashSubdivisionBCBalance.Income - cashSubdivisionBCBalance.Expense;
				totalActivityBalance += businessAccountSummary.Total;
				return true;
			}

			return false;
		}

		private void UpdateResultMessage(BankStatementProcessedResult result)
		{
			ResultMessage = result.GetResult();
			OnPropertyChanged(nameof(ResultMessage));
		}

		#region Экспорт

		private void ExportReport()
		{
			var date = DatePickerViewModel.SelectedDate;
			
			using(var wb = new XLWorkbook())
			{
				var sheetName = $"Общий итог на {date:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				FillTotalBalance(ws, date);
				ws.Columns().AdjustToContents();
				FillTotalFundsBySheets(wb, date);

				if(TryGetSavePath(out string path, date))
				{
					wb.SaveAs(path);
				}
			}
		}

		private void FillTotalBalance(IXLWorksheet ws, DateTime date)
		{
			GenerateFirstColumns(ws, date);
			FillTotalFundsAndActivities(ws);
		}

		private void GenerateFirstColumns(IXLWorksheet ws, DateTime date)
		{
			var colNames = new[]
			{
				$"Дата: {date:dd.MM.yyyy}",
				"Всего"
			};
			
			var index = 1;
			foreach(var name in colNames)
			{
				var columnTitleCell = ws.Cell(1, index);
				columnTitleCell.Value = name;
				columnTitleCell.SetBoldFont();
				index++;
			}
		}
		
		private void FillTotalFundsBySheets(IXLWorkbook wb, DateTime date)
		{
			FillTotalFundBalance(wb, date);
		}

		private void FillTotalFundsAndActivities(IXLWorksheet ws)
		{
			var activitiesColumnsDict = new Dictionary<int, int>();
			var fundsTotalRowsDict = new Dictionary<int, int>();
			
			for(var i = 0; i < Entity.FundsSummary.Count; i++)
			{
				for(var j = 0; j < Entity.FundsSummary[i].BusinessActivitySummary.Count; j++)
				{
					var lastAddedColumn = ws.Columns().Count();
					var activitySummary = Entity.FundsSummary[i].BusinessActivitySummary[j];
					if(!activitiesColumnsDict.ContainsKey(activitySummary.BusinessActivity.Id))
					{
						activitiesColumnsDict.Add(activitySummary.BusinessActivity.Id, lastAddedColumn + 1);
					}

					var activityColumn = activitiesColumnsDict[activitySummary.BusinessActivity.Id];
					var activityTitleCell = ws.Cell(1, activityColumn);
					activityTitleCell.Value = $"{activitySummary.Name}";
					activityTitleCell.SetBoldFont();
				}

				var rowFundsTotal = ws.Rows().Count() + 1;
				var fundTotalTitleCell = ws.Cell(rowFundsTotal, 1);
				fundTotalTitleCell.Value = $"{Entity.FundsSummary[i].Name} всего";
				var fundTotalCell = ws.Cell(rowFundsTotal, 2);
				fundTotalCell.Value = Entity.FundsSummary[i].Total ?? 0m;
				fundTotalCell.SetCurrencyFormat();
				fundsTotalRowsDict.Add(Entity.FundsSummary[i].Funds.Id, rowFundsTotal);
			}

			var totalRow = ws.Rows().Count() + 1;
			var totalTitleCell = ws.Cell(totalRow, 1);
			totalTitleCell.Value = "ИТОГО";
			totalTitleCell.SetBoldFont();
			var totalCell = ws.Cell(totalRow, 2);
			totalCell.Value = Entity.Total ?? 0m;
			totalCell
				.SetBoldFont()
				.SetCurrencyFormat();

			foreach(var activitiesKeyPairValue in activitiesColumnsDict)
			{
				var activityId = activitiesKeyPairValue.Key;
				
				foreach(var fundsKeyPairValue in fundsTotalRowsDict)
				{
					var fundsId = fundsKeyPairValue.Key;
					var activityFundTotal =
						Entity.FundsSummary
							.Where(x => x.Funds.Id == fundsId)
							.SelectMany(x => x.BusinessActivitySummary)
							.Where(x => x.BusinessActivity.Id == activityId)
							.Sum(x => x.Total);
					
					var activityFundTotalCell = ws.Cell(fundsKeyPairValue.Value, activitiesKeyPairValue.Value);
					activityFundTotalCell.Value = activityFundTotal ?? 0m;
					activityFundTotalCell.SetCurrencyFormat();
				}
				
				var activityTotal = Entity.FundsSummary
					.SelectMany(x => x.BusinessActivitySummary)
					.Where(x => x.BusinessActivity.Id == activityId)
					.Sum(x => x.Total);
				
				var activityTotalCell = ws.Cell(totalRow, activitiesKeyPairValue.Value);
				activityTotalCell.Value = activityTotal ?? 0m;
				activityTotalCell
					.SetBoldFont()
					.SetCurrencyFormat();
			}
		}

		private void FillTotalFundBalance(IXLWorkbook wb, DateTime date)
		{
			var activitiesColumnsDict = new Dictionary<int, (int AccountNameColumn, int BankColumn, int AccountNumberColumn, int TotalColumn)>();
			
			for(var i = 0; i < Entity.FundsSummary.Count; i++)
			{
				var sheetName = $"{Entity.FundsSummary[i].Name}";
				var fundWorkSheet = wb.Worksheets.Add(sheetName);
				GenerateFirstColumns(fundWorkSheet, date);
				activitiesColumnsDict.Clear();
				
				for(var j = 0; j < Entity.FundsSummary[i].BusinessActivitySummary.Count; j++)
				{
					var lastAddedColumn = fundWorkSheet.Columns().Count();
					var activitySummary = Entity.FundsSummary[i].BusinessActivitySummary[j];
					activitiesColumnsDict.Add(
						activitySummary.BusinessActivity.Id,
						(lastAddedColumn + 1, lastAddedColumn + 2, lastAddedColumn + 3, lastAddedColumn + 4));
					var (accountNameColumn, bankColumn, accountNumberColumn, totalColumn) = activitiesColumnsDict[activitySummary.BusinessActivity.Id];

					fundWorkSheet.Range(1, accountNameColumn, 1, totalColumn).Value = activitySummary.Name;
					fundWorkSheet.Range(1, accountNameColumn, 1, totalColumn).Merge();
					fundWorkSheet.Range(1, accountNameColumn, 1, totalColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
					fundWorkSheet.Range(1, accountNameColumn, 1, totalColumn).Style.Font.Bold = true;

					var rowBeginActivity = 2;
					
					for(int k = 0; k < activitySummary.BusinessAccountsSummary.Count; k++)
					{
						var account = activitySummary.BusinessAccountsSummary[k];
						fundWorkSheet.Cell(rowBeginActivity, 1).Value = Entity.FundsSummary[i].Name;
						fundWorkSheet.Cell(rowBeginActivity, accountNameColumn).Value = account.Name;
						fundWorkSheet.Cell(rowBeginActivity, bankColumn).Value = account.Bank;
						fundWorkSheet.Cell(rowBeginActivity, accountNumberColumn).SetValue(account.AccountNumber);
						var accountTotalCell = fundWorkSheet.Cell(rowBeginActivity, totalColumn);

						if(account.Total.HasValue)
						{
							accountTotalCell.Value = account.Total ?? 0m;
							accountTotalCell.SetCurrencyFormat();
						}

						rowBeginActivity++;
					}
				}

				var rowFundsTotal = fundWorkSheet.Rows().Count() + 1;
				var fundTotalTitleCell = fundWorkSheet.Cell(rowFundsTotal, 1);
				fundTotalTitleCell.Value = $"{Entity.FundsSummary[i].Name} всего";
				fundTotalTitleCell.SetBoldFont();
				var fundTotalCell = fundWorkSheet.Cell(rowFundsTotal, 2);
				fundTotalCell.Value = Entity.FundsSummary[i].Total ?? 0m;
				fundTotalCell
					.SetBoldFont()
					.SetCurrencyFormat();
				
				for(var j = 0; j < Entity.FundsSummary[i].BusinessActivitySummary.Count; j++)
				{
					var activitySummary = Entity.FundsSummary[i].BusinessActivitySummary[j];

					var totalActivityColumn = activitiesColumnsDict[activitySummary.BusinessActivity.Id].TotalColumn;
					var activityTotalCell = fundWorkSheet.Cell(rowFundsTotal, totalActivityColumn);
					activityTotalCell.Value = activitySummary.Total ?? 0m;
					activityTotalCell
						.SetBoldFont()
						.SetCurrencyFormat();
				}
				
				fundWorkSheet.Columns().AdjustToContents();
			}
		}

		private bool TryGetSavePath(out string path, DateTime date)
		{
			var extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = $"{Title} на {date:dd-MM-yyyy}{extension}"
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{extension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}

		#endregion
		
		private void ClearResultMessage()
		{
			ResultMessage = string.Empty;
			OnPropertyChanged(nameof(ResultMessage));
		}

		private void Initialize(
			IDatePickerViewModelFactory datePickerViewModelFactory, ICurrentPermissionService permissionService)
		{
			CanEditCompanyBalanceByDayInPreviousPeriods =
				permissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CompanyBalanceByDayPermissions.CanEditCompanyBalanceByDayInPreviousPeriods);
			CompanyBalances = new List<CompanyBalanceByDay>();
			DatePickerViewModel = datePickerViewModelFactory.CreateNewDatePickerViewModel(
				DateTime.Today,
				ChangeDateType.Day,
				canSelectNextDateFunc: CanSelectNextDate,
				canSelectPreviousDateFunc: CanSelectPreviousDate);
			DatePickerViewModel.CanEditDateFromCalendar = true;
			DatePickerViewModel.DateChangedByUser += OnDatePickerViewModelPropertyChanged;

			OpenDirectorySettings = new DialogSettings
			{
				InitialDirectory = @"Z:\SystemStatements\activities\CurrentStatements",
				Title = "Выбрать каталог"
			};
			
			InitializeData();
		}
		
		private void OnDatePickerViewModelPropertyChanged(object sender, EventArgs e)
		{
			InitializeData();
			ClearResultMessage();
		}

		private bool CanSelectNextDate(DateTime dateTime)
		{
			return dateTime.Date < DateTime.Today.Date;
		}
		
		private bool CanSelectPreviousDate(DateTime dateTime)
		{
			if(CanEditCompanyBalanceByDayInPreviousPeriods)
			{
				return true;
			}

			return dateTime == DateTime.Today || dateTime == DateTime.Today.AddDays(-1);
		}

		private void InitializeData()
		{
			UoW.Session.Clear();
			CompanyBalances.Clear();
			var date = DatePickerViewModel.SelectedDate;
			Entity = _companyBalanceByDayRepository.Get(UoW, e => e.Date == date).FirstOrDefault();

			var cashSubdivisionsId =
				UoW.GetAll<BusinessAccount>()
					.Where(x => x.SubdivisionId.HasValue)
					.Select(x => x.SubdivisionId.Value)
					.ToArray();
			
			CashSubdivisionsBalances = _cashRepository.CashForSubdivisionsByDate(
				UoW,
				cashSubdivisionsId,
				date.LatestDayTime());

			if(Entity is null)
			{
				Entity = CompanyBalanceByDay.Create(date);
				GenerateNewData(Entity);
			}

			CompanyBalances.Add(Entity);
			CompanyBalanceChangedAction?.Invoke();
		}

		private void GenerateNewData(CompanyBalanceByDay companyBalanceByDay)
		{
			var accountsByFunds = UoW.GetAll<BusinessAccount>()
				.Where(x => !x.IsArchive)
				.OrderBy(x => x.Funds.Id)
				.ThenBy(x => x.BusinessActivity.Id)
				.ToLookup(x => x.Funds);
			
			foreach(var accountsByFund in accountsByFunds)
			{
				var fundSummary = FundsSummary.Create(accountsByFund.Key, companyBalanceByDay);
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

		public override void Dispose()
		{
			DatePickerViewModel.PropertyChanged -= OnDatePickerViewModelPropertyChanged;
			DatePickerViewModel.Dispose();
			base.Dispose();
		}

		public void RecalculateTotal(BusinessAccountSummary accountSummary)
		{
			var recalculatingActivityTotal =
				Entity.FundsSummary
					.SelectMany(x => x.BusinessActivitySummary)
					.Where(x => x == accountSummary.BusinessActivitySummary)
					.ToArray();

			foreach(var activitySummary in recalculatingActivityTotal)
			{
				activitySummary.Total = activitySummary.BusinessAccountsSummary.Sum(x => x.Total);
				activitySummary.FundsSummary.Total = activitySummary.FundsSummary.BusinessActivitySummary.Sum(x => x.Total);
			}

			Entity.Total = Entity.FundsSummary.Sum(x => x.Total);
		}
	}
}

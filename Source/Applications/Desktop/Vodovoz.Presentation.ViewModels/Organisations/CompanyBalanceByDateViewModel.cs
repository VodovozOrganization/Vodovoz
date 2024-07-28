using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ClosedXML.Excel;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.ViewModels.Dialog;
using QS.ViewModels.Extension;
using Vodovoz.Application.BankStatements;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class CompanyBalanceByDateViewModel : UowDialogViewModelBase, IAskSaveOnCloseViewModel
	{
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IGenericRepository<CompanyBalanceByDay> _companyBalanceByDayRepository;
		private readonly BankStatementHandler _bankStatementHandler;
		private readonly IPermissionResult _permissionResult;

		public CompanyBalanceByDateViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IGenericRepository<CompanyBalanceByDay> companyBalanceByDayRepository,
			BankStatementHandler bankStatementHandler,
			IDatePickerViewModelFactory datePickerViewModelFactory,
			ICurrentPermissionService permissionService) : base(unitOfWorkFactory, navigation)
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
			_companyBalanceByDayRepository =
				companyBalanceByDayRepository ?? throw new ArgumentNullException(nameof(companyBalanceByDayRepository));
			_bankStatementHandler = bankStatementHandler ?? throw new ArgumentNullException(nameof(bankStatementHandler));

			if(!_permissionResult.CanRead)
			{
				throw new AbortCreatingPageException("У Вас нет прав просмотра данной вкладки", "Недостаточно прав");
			}

			Title = typeof(CompanyBalanceByDay).GetCustomAttribute<AppellativeAttribute>(true).NominativePlural;

			CreateCommands();
			Initialize(datePickerViewModelFactory);
		}

		public event Action CompanyBalanceChangedAction;

		public string ResultMessage { get; private set; }
		public bool AskSaveOnClose => CanUpdateData;
		public CompanyBalanceByDay Entity { get; private set; }
		public IList<CompanyBalanceByDay> CompanyBalances { get; private set; }
		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand LoadAndProcessDataCommand { get; private set; }
		public DelegateCommand ExportCommand { get; private set; }
		public DatePickerViewModel DatePickerViewModel { get; private set; }
		private bool CanUpdateData => (Entity.Id == 0 && _permissionResult.CanCreate) || _permissionResult.CanUpdate;

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
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			
			LoadAndProcessDataCommand = new DelegateCommand(
				() =>
				{
					var result = _fileDialogService.RunOpenDirectoryDialog();
					
					if(!result.Successful) return;

					/*var directoryPath1 = @"D:\Работа\Программист Веселый Водовоз\файлы выписок2";
					var directoryPath2 = @"D:\Работа\Программист Веселый Водовоз\файлы выписок2\ГК ВВ";*/

					var banksStatementsData =
						_bankStatementHandler.ProcessBankStatementsFromDirectory(result.Path, DatePickerViewModel.SelectedDate);
					
					/*var banksStatementsData = _bankStatementHandler.ProcessBankStatementsFromFile(
						@"D:\Работа\Программист Веселый Водовоз\файлы выписок2\StatReports_16.xls",
						DatePickerViewModel.SelectedDate);*/
					
					UpdateLocalData(banksStatementsData);
				}
				,
				() => CanUpdateData
			);

			ExportCommand = new DelegateCommand(ExportReport, () => CanUpdateData);
		}

		private void UpdateLocalData(BankStatementProcessedResult banksStatementsData)
		{
			var totalBalance = 0m;
			foreach(var fundsSummary in Entity.FundsSummary)
			{
				var totalFundsBalance = 0m;
				foreach(var businessActivitySummary in fundsSummary.BusinessActivitySummary)
				{
					var totalActivityBalance = 0m;
					foreach(var businessAccountSummary in businessActivitySummary.BusinessAccountsSummary)
					{
						if(string.IsNullOrWhiteSpace(businessAccountSummary.BusinessAccount.Number))
						{
							continue;
						}
						
						if(banksStatementsData.BankStatementData.TryGetValue(businessAccountSummary.BusinessAccount.Number, out var data))
						{
							businessAccountSummary.Total = data.Balance;
							totalActivityBalance += data.Balance;
						}
					}

					businessActivitySummary.Total = totalActivityBalance;
					totalFundsBalance += totalActivityBalance;
				}

				fundsSummary.Total = totalFundsBalance;
				totalBalance += totalFundsBalance;
			}

			Entity.Total = totalBalance;
			CompanyBalanceChangedAction?.Invoke();
			UpdateResultMessage(banksStatementsData);
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
				var sheetName = $"{date:dd.MM.yyyy}";
				var ws = wb.Worksheets.Add(sheetName);

				InsertBalanceValues(ws, date);
				ws.Columns().AdjustToContents();

				if(TryGetSavePath(out string path, date))
				{
					wb.SaveAs(path);
				}
			}
		}

		private void InsertBalanceValues(IXLWorksheet ws, DateTime date)
		{
			var colNames = new[]
			{
				$"Дата: {date:dd.MM.yyyy}",
				"Всего"
			};
			
			var index = 1;
			foreach(var name in colNames)
			{
				ws.Cell(1, index).Value = name;
				index++;
			}

			AddBusinessActivitiesColumns(ws, index);
		}

		private void AddBusinessActivitiesColumns(IXLWorksheet ws, int index)
		{
			int? rowFundsTotal = null;
			var activitiesDict = new Dictionary<int, (int FirstColumn, int LastColumn)>();
			
			for(var i = 0; i < Entity.FundsSummary.Count; i++)
			{
				for(var j = 0; j < Entity.FundsSummary[i].BusinessActivitySummary.Count; j++)
				{
					var lastAddedColumn = ws.Columns().Count();
					var activitySummary = Entity.FundsSummary[i].BusinessActivitySummary[j];
					if(!activitiesDict.ContainsKey(activitySummary.BusinessActivity.Id))
					{
						activitiesDict.Add(activitySummary.BusinessActivity.Id, (lastAddedColumn + 1, lastAddedColumn + 2));
					}

					var (firstColumn, endIndex) = activitiesDict[activitySummary.BusinessActivity.Id];
					index = firstColumn;

					var rowBeginActivity = rowFundsTotal.HasValue ? rowFundsTotal.Value + 1 : 2;
					ws.Range(1, index, 1, endIndex).Value = $"{activitySummary.Name}";
					
					for(int k = 0; k < activitySummary.BusinessAccountsSummary.Count; k++)
					{
						var account = activitySummary.BusinessAccountsSummary[k];
						ws.Cell(rowBeginActivity, index).Value = account.Name;
						ws.Cell(rowBeginActivity, endIndex).Value = account.Total;

						rowBeginActivity++;
					}
				}

				rowFundsTotal = ws.Rows().Count() + 1;
				ws.Cell(rowFundsTotal.Value, 1).Value = $"{Entity.FundsSummary[i].Name}";
				ws.Cell(rowFundsTotal.Value, 2).Value = $"{Entity.FundsSummary[i].Total}";
				
				for(var j = 0; j < Entity.FundsSummary[i].BusinessActivitySummary.Count; j++)
				{
					var activitySummary = Entity.FundsSummary[i].BusinessActivitySummary[j];
					var activitiesColumns = activitiesDict[activitySummary.BusinessActivity.Id];
					ws.Cell(rowFundsTotal.Value, activitiesColumns.LastColumn).Value = $"{activitySummary.Total}";
				}
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

		private void Initialize(IDatePickerViewModelFactory datePickerViewModelFactory)
		{
			CompanyBalances = new List<CompanyBalanceByDay>();
			
			DatePickerViewModel = datePickerViewModelFactory.CreateNewDatePickerViewModel(
				DateTime.Today,
				ChangeDateType.Day,
				canSelectNextDateFunc: CanSelectNextDate,
				canSelectPreviousDateFunc: CanSelectPreviousDate);
			DatePickerViewModel.CanEditDateFromCalendar = true;
			DatePickerViewModel.DateChangedByUser += OnDatePickerViewModelPropertyChanged;
			
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
			return true;
		}

		private void InitializeData()
		{
			UoW.Session.Clear();
			CompanyBalances.Clear();
			var date = DatePickerViewModel.SelectedDate;
			Entity = _companyBalanceByDayRepository.Get(UoW, e => e.Date == date).FirstOrDefault();

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

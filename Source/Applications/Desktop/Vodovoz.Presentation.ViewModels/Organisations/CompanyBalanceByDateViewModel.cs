using System;
using System.Collections.Generic;
using System.Linq;
using QS.Commands;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Services.FileDialog;
using QS.ViewModels.Dialog;
using Vodovoz.Application.BankStatements;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Presentation.ViewModels.Factories;
using Vodovoz.Presentation.ViewModels.Widgets.Profitability;

namespace Vodovoz.Presentation.ViewModels.Organisations
{
	public class CompanyBalanceByDateViewModel : UowDialogViewModelBase
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly IGenericRepository<CompanyBalanceByDay> _companyBalanceByDayRepository;
		private readonly BankStatementHandler _bankStatementHandler;
		private DateTime _date = DateTime.Today;

		public CompanyBalanceByDateViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IGenericRepository<CompanyBalanceByDay> companyBalanceByDayRepository,
			BankStatementHandler bankStatementHandler,
			IDatePickerViewModelFactory datePickerViewModelFactory) : base(unitOfWorkFactory, navigation)
		{
			if(datePickerViewModelFactory == null)
			{
				throw new ArgumentNullException(nameof(datePickerViewModelFactory));
			}

			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_companyBalanceByDayRepository =
				companyBalanceByDayRepository ?? throw new ArgumentNullException(nameof(companyBalanceByDayRepository));
			_bankStatementHandler = bankStatementHandler ?? throw new ArgumentNullException(nameof(bankStatementHandler));

			CreateCommands();
			Initialize(datePickerViewModelFactory);
		}

		public string ResultMessage { get; private set; }
		public CompanyBalanceByDay Entity { get; private set; }

		public IReadOnlyList<BusinessActivity> BusinessActivities { get; set; }

		public DelegateCommand SaveCommand { get; private set; }
		public DelegateCommand CancelCommand { get; private set; }
		public DelegateCommand LoadAndProcessDataCommand { get; private set; }
		public DatePickerViewModel DatePickerViewModel { get; private set; }

		private void CreateCommands()
		{
			SaveCommand = new DelegateCommand(() => SaveAndClose());
			CancelCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
			
			LoadAndProcessDataCommand = new DelegateCommand(
				() =>
				{
					//var result = _fileDialogService.RunOpenDirectoryDialog();
					
					//if(!result.Successful) return;

					var directoryPath1 = @"D:\Работа\Программист Веселый Водовоз\файлы выписок2";
					var directoryPath2 = @"D:\Работа\Программист Веселый Водовоз\файлы выписок2\ГК ВВ";

					var banksStatementsData =
						_bankStatementHandler.ProcessBankStatementsFromDirectory(directoryPath2, DatePickerViewModel.SelectedDate);
					
					/*var banksStatementsData = _bankStatementHandler.ProcessBankStatementsFromFile(
						//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\ГК ВВ\40702810100000163183_19.07.2024_21.07.2024.xml", +
						//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\56456416.xlsx", +
						//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\40802810800000177942_15.07.2024_15.07.2024.xlsx", +
						//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\statement_40702810590320004953_20240719_20240721.xlsx", -
						//@"D:\Работа\Программист Веселый Водовоз\файлы выписок\ibc2-20240721-40702810094510024535.xls",
						@"D:\Работа\Программист Веселый Водовоз\файлы выписок2\BankStatement_11637_2024-07-22_07-28-35-419.xlsx",
						DatePickerViewModel.SelectedDate);*/
					
					UpdateLocalData(banksStatementsData);
				}
			);
		}

		private void UpdateLocalData(BankStatementProcessedResult banksStatementsData)
		{
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
			}

			UpdateErrorMessage(banksStatementsData);
		}

		private void UpdateErrorMessage(BankStatementProcessedResult result)
		{
			ResultMessage = result.GetResult();
			OnPropertyChanged(nameof(ResultMessage));
		}

		private void Initialize(IDatePickerViewModelFactory datePickerViewModelFactory)
		{
			DatePickerViewModel = datePickerViewModelFactory.CreateNewDatePickerViewModel(
				DateTime.Today,
				ChangeDateType.Day,
				canSelectNextDateFunc: CanSelectNextDate,
				canSelectPreviousDateFunc: CanSelectPreviousDate);
			DatePickerViewModel.CanEditDateFromCalendar = true;
			
			InitializeData();
		}
		
		private bool CanSelectNextDate(DateTime dateTime)
		{
			return dateTime.Date != DateTime.Today.Date;
		}
		
		private bool CanSelectPreviousDate(DateTime dateTime)
		{
			return true;
		}

		private void InitializeData()
		{
			var date = DatePickerViewModel.SelectedDate;
			Entity = _companyBalanceByDayRepository.Get(UoW, e => e.Date == date).FirstOrDefault();

			if(Entity is null)
			{
				Entity = CompanyBalanceByDay.Create(date);
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

		public override void Dispose()
		{
			DatePickerViewModel?.Dispose();
			base.Dispose();
		}
	}

	public class CompanyBalanceByDayNode
	{
		public string FundsName { get; set; }
		public decimal FundsTotal { get; set; }
		public IEnumerable<BusinessActivitySummary> BusinessActivitiesSummaryNodes { get; set; }
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Infrastructure;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Services.FileDialog;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Payments;
using Vodovoz.Extensions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Reports.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class BankAccountsMovementsJournalViewModel : JournalViewModelBase
	{
		private readonly BankAccountsMovementsJournalFilterViewModel _filterViewModel;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IPaymentSettings _paymentSettings;
		private bool _isExportToExcelInProcess;

		public BankAccountsMovementsJournalViewModel(
			BankAccountsMovementsJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IPaymentSettings paymentSettings) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_interactiveService = interactiveService;
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			Title = "Движения средств по расчетным счетам";

			ConfigureLoader();
			SearchEnabled = false;
			ConfigureFilter();
			CreateNodeActions();
			
			SelectionMode = JournalSelectionMode.None;
		}
		
		public bool IsExportToExcelInProcess
		{
			get => _isExportToExcelInProcess;
			private set
			{
				SetField(ref _isExportToExcelInProcess, value);
				UpdateJournalActions();
			}
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateExportToExcelAction();
		}

		private void ConfigureLoader()
		{
			var dataLoader = new ThreadDataLoader<BankAccountsMovementsJournalNode>(UnitOfWorkFactory);
			dataLoader.AddQuery(GetLoadedAccountMovements);
			dataLoader.PostLoadProcessingFunc = PostLoadProcessingFunc;
			DataLoader = dataLoader;
		}

		private void ConfigureFilter()
		{
			_filterViewModel.SetParentTab(this);
			JournalFilter = _filterViewModel;
			_filterViewModel.IsShow = true;
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private IQueryOver<BankAccountMovement> GetLoadedAccountMovements(IUnitOfWork uow)
		{
			BankAccountMovement accountMovementAlias = null;
			BankAccountMovement subAccountMovementAlias = null;
			BankAccountMovementData accountMovementDataAlias = null;
			BankAccountMovementData subAccountMovementDataAlias = null;
			Account accountAlias = null;
			Account paymentOrganizationAccountAlias = null;
			Bank bankAlias = null;
			Payment paymentAlias = null;
			BankAccountsMovementsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => accountMovementAlias)
				.Left.JoinAlias(() => accountMovementAlias.BankAccountMovements, () => accountMovementDataAlias)
				.Left.JoinAlias(() => accountMovementAlias.Bank, () => bankAlias)
				.Left.JoinAlias(() => accountMovementAlias.Account, () => accountAlias);
			
			var income = QueryOver.Of(() => paymentAlias)
				.JoinAlias(() => paymentAlias.OrganizationAccount, () => paymentOrganizationAccountAlias)
				.Where(p => p.RefundedPayment == null && p.RefundPaymentFromOrderId == null)
				.And(p => !p.IsManuallyCreated)
				.And(CustomRestrictions.Between(
					CustomProjections.Date(() => paymentAlias.Date),
					CustomProjections.Date(Projections.Property(() => accountMovementAlias.StartDate)),
					CustomProjections.Date(Projections.Property(() => accountMovementAlias.EndDate))))
				.And(() => paymentOrganizationAccountAlias.Id == accountAlias.Id)
				.Select(Projections.Sum(() => paymentAlias.Total));
			
			var yesterdayFinalBalance = QueryOver.Of(() => subAccountMovementDataAlias)
				.JoinAlias(() => subAccountMovementDataAlias.AccountMovement, () => subAccountMovementAlias)
				.Where(
					Restrictions.EqProperty(
						CustomProjections.Date(Projections.Property(() => subAccountMovementAlias.EndDate)),
						DateProjections.DateSub(
							CustomProjections.Date(Projections.Property(() => accountMovementAlias.StartDate)),
							Projections.Constant(1),
							"day")
					)
				)
				.And(() => subAccountMovementAlias.Id != accountMovementAlias.Id)
				.And(() => subAccountMovementAlias.Account.Id == accountMovementAlias.Account.Id)
				.And(() => subAccountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.FinalBalance)
				.Select(Projections.Property(() => subAccountMovementDataAlias.Amount));

			var paymentsSubquery =
				Projections.Conditional(
					new[]
					{
						new ConditionalProjectionCase(
							Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.InitialBalance),
							Projections.SubQuery(yesterdayFinalBalance)),
						new ConditionalProjectionCase(
							Restrictions.Where(() => accountMovementDataAlias.AccountMovementDataType == BankAccountMovementDataType.TotalReceived),
							Projections.SubQuery(income))
					},
					Projections.Constant(null, NHibernateUtil.Decimal)
				);

			var startDate = _filterViewModel.StartDate;
			var endDate = _filterViewModel.EndDate;

			if(startDate.HasValue)
			{
				query.Where(x => x.StartDate >= startDate.Value);
			}
			
			if(endDate.HasValue)
			{
				query.Where(x => x.EndDate <= endDate.Value);
			}
			
			if(_filterViewModel.OnlyWithDiscrepancies)
			{
				query.Where(Restrictions.Conjunction()
					.Add(Restrictions.Not(Restrictions.Eq(paymentsSubquery, 0)))
					.Add(Restrictions.IsNotNull(paymentsSubquery)));
			}

			if(_filterViewModel.OrganizationAccount != null)
			{
				query.Where(() => accountAlias.Id == _filterViewModel.OrganizationAccount.Id);
			}
			
			if(_filterViewModel.OrganizationBank != null)
			{
				query.Where(() => bankAlias.Id == _filterViewModel.OrganizationBank.Id);
			}

			query.SelectList(list => list
					.Select(bam => bam.Id).WithAlias(() => resultAlias.Id)
					.Select(bam => bam.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(bam => bam.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.Account)
					.Select(() => bankAlias.Name).WithAlias(() => resultAlias.Bank)
					.Select(() => accountMovementDataAlias.AccountMovementDataType).WithAlias(() => resultAlias.AccountMovementDataType)
					.Select(() => accountMovementDataAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(paymentsSubquery).WithAlias(() => resultAlias.AmountFromProgram)
				)
				.TransformUsing(Transformers.AliasToBean<BankAccountsMovementsJournalNode>())
				.OrderBy(x => x.StartDate).Desc();
			
			return query;
		}
		
		private void PostLoadProcessingFunc(IList items, uint addedSince)
		{
			var startDateGeneration = _filterViewModel.StartDate ?? _paymentSettings.ControlPointStartDate;

			var endDateGeneration = DateTime.Today;
			
			if(_filterViewModel.EndDate.HasValue && _filterViewModel.EndDate < endDateGeneration)
			{
				endDateGeneration = _filterViewModel.EndDate.Value;
			}
			
			var dates = DateGenerator.GenerateDates(startDateGeneration, endDateGeneration);
			var notLoadedData = new List<BankAccountsMovementsJournalNode>();
			var loadedNodes = items
				.Cast<BankAccountsMovementsJournalNode>()
				.Skip((int)addedSince);

			//var organizationsAccounts = ;

			foreach(var date in dates)
			{
				//foreach(var account in organizationsAccounts)
				//{
					var loadedNode = loadedNodes.FirstOrDefault(x => (x.StartDate == date || x.EndDate == date));

					if(loadedNode != null)
					{
						continue;
					}

					notLoadedData.AddRange(
						Enum.GetValues(typeof(BankAccountMovementDataType))
							.Cast<BankAccountMovementDataType>()
							.Select(dateType => BankAccountsMovementsJournalNode.NotLoaded(date, date, null, null, dateType)));
				//}
			}

			foreach(var node in notLoadedData)
			{
				items.Add(node);
			}
		}
		
		private IQueryOver<BankAccountMovement> GetNotLoadedAccountMovements(IUnitOfWork uow)
		{
			BankAccountMovement accountMovementAlias = null;
			BankAccountMovementData accountMovementDataAlias = null;
			Account accountAlias = null;
			Bank bankAlias = null;
			BankAccountsMovementsJournalNode resultAlias = null;
			
			var startDateGeneration = _filterViewModel.StartDate ?? _paymentSettings.ControlPointStartDate;
			var endDateGeneration = _filterViewModel.EndDate ?? DateTime.Today;
			
			var dates = DateGenerator.GenerateDates(startDateGeneration, endDateGeneration);
			
			var query = uow.Session.QueryOver(() => accountMovementAlias)
				.Left.JoinAlias(() => accountMovementAlias.BankAccountMovements, () => accountMovementDataAlias)
				.Left.JoinAlias(() => accountMovementAlias.Bank, () => bankAlias)
				.Left.JoinAlias(() => accountMovementAlias.Account, () => accountAlias);

			if(_filterViewModel.OrganizationAccount != null)
			{
				query.Where(() => accountAlias.Id == _filterViewModel.OrganizationAccount.Id);
			}
			
			if(_filterViewModel.OrganizationBank != null)
			{
				query.Where(() => bankAlias.Id == _filterViewModel.OrganizationBank.Id);
			}

			query.SelectList(list => list
					.Select(bam => bam.Id).WithAlias(() => resultAlias.Id)
					.Select(bam => bam.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(bam => bam.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.Account)
					.Select(() => bankAlias.Name).WithAlias(() => resultAlias.Bank)
					.Select(() => accountMovementDataAlias.AccountMovementDataType).WithAlias(() => resultAlias.AccountMovementDataType)
				)
				.TransformUsing(Transformers.AliasToBean<BankAccountsMovementsJournalNode>())
				.OrderBy(x => x.StartDate).Desc();
			
			return query;
		}
		
		private void CreateExportToExcelAction()
		{
			var createExportToExcelAction = new JournalAction(
				"Выгрузить в Excel",
				(selected) => !IsExportToExcelInProcess,
				(selected) => true,
				async (selected) => await ExportToExcel()
			);
			NodeActionsList.Add(createExportToExcelAction);
		}
		
		private async Task ExportToExcel()
		{
			if(!_filterViewModel.StartDate.HasValue)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Info, "Нужно выбрать начальный период выгрузки");
				return;
			}
			
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				DefaultFileExtention = ".xlsx",
				FileName = $"{Title} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx"
			};

			var saveDialogResul = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(!saveDialogResul.Successful)
			{
				return;
			}

			IsExportToExcelInProcess = true;

			await Task.Run(() =>
			{
				var nodes = GetReportData();

				var ordersReport = new BankAccountsMovementsJournalReport(
					_filterViewModel.StartDate.Value,
					_filterViewModel.EndDate,
					nodes);

				ordersReport.Export(saveDialogResul.Path);
			});

			IsExportToExcelInProcess = false;
		}
		
		private IEnumerable<BankAccountsMovementsJournalNode> GetReportData()
		{
			var loadedMovements = GetLoadedAccountMovements(UoW).List<BankAccountsMovementsJournalNode>();
			var notLoadedMovements = GetNotLoadedAccountMovements(UoW).List<BankAccountsMovementsJournalNode>();

			var accountMovements = loadedMovements;
				//.Concat(notLoadedMovements);

			return accountMovements;
		}
	}
}

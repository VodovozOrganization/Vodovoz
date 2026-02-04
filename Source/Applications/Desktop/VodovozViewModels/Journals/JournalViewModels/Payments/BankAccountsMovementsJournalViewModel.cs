using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
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
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Services;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Reports.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class BankAccountsMovementsJournalViewModel : JournalViewModelBase
	{
		private readonly BankAccountsMovementsJournalFilterViewModel _filterViewModel;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IInteractiveService _interactiveService;
		private readonly IFileDialogService _fileDialogService;
		private readonly IPaymentSettings _paymentSettings;
		private readonly IOrganizationRepository _organizationRepository;
		private bool _isExportToExcelInProcess;

		public BankAccountsMovementsJournalViewModel(
			BankAccountsMovementsJournalFilterViewModel filterViewModel,
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IFileDialogService fileDialogService,
			IPaymentSettings paymentSettings,
			IOrganizationRepository organizationRepository) : base(unitOfWorkFactory, interactiveService, navigation)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_interactiveService = interactiveService;
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_paymentSettings = paymentSettings ?? throw new ArgumentNullException(nameof(paymentSettings));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
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
			dataLoader.AddQuery(GetNotLoadedAccountMovements);
			dataLoader.MergeInOrderBy(x => x.StartDate, true);
			dataLoader.MergeInOrderBy(x => x.EndDate, true);
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

		private IQueryOver<Organization> GetLoadedAccountMovements(IUnitOfWork uow)
		{
			BankAccountMovement accountMovementAlias = null;
			BankAccountMovement subAccountMovementAlias = null;
			BankAccountMovementData accountMovementDataAlias = null;
			BankAccountMovementData subAccountMovementDataAlias = null;
			Account accountAlias = null;
			Organization organizationAlias = null;
			Account paymentOrganizationAccountAlias = null;
			Bank bankAlias = null;
			Payment paymentAlias = null;
			BankAccountsMovementsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => organizationAlias)
				.JoinAlias(() => organizationAlias.Accounts, () => accountAlias)
				.JoinEntityAlias(() => accountMovementAlias, () => accountAlias.Id == accountMovementAlias.Account.Id)
				.Left.JoinAlias(() => accountMovementAlias.BankAccountMovements, () => accountMovementDataAlias)
				.Left.JoinAlias(() => accountMovementAlias.Bank, () => bankAlias);
			
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
				query.Where(() => accountMovementAlias.StartDate >= startDate);
			}

			if(endDate.HasValue)
			{
				query.Where(() => accountMovementAlias.EndDate <= endDate);
			}
			
			if(_filterViewModel.OnlyWithDiscrepancies)
			{
				query.Where(
					Restrictions.NotEqProperty(
						paymentsSubquery,
						Projections.Property(() => accountMovementDataAlias.Amount)));
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
					.Select(() => accountMovementAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => accountMovementAlias.StartDate).WithAlias(() => resultAlias.StartDate)
					.Select(() => accountMovementAlias.EndDate).WithAlias(() => resultAlias.EndDate)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.Account)
					.Select(() => bankAlias.Name).WithAlias(() => resultAlias.Bank)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
					.Select(() => accountMovementDataAlias.AccountMovementDataType).WithAlias(() => resultAlias.AccountMovementDataType)
					.Select(() => accountMovementDataAlias.Amount).WithAlias(() => resultAlias.Amount)
					.Select(paymentsSubquery).WithAlias(() => resultAlias.AmountFromProgram)
				)
				.TransformUsing(Transformers.AliasToBean<BankAccountsMovementsJournalNode>())
				.OrderBy(() => accountMovementAlias.StartDate).Desc();
			
			return query;
		}
		
		private IQueryOver<CalendarEntity> GetNotLoadedAccountMovements(IUnitOfWork uow)
		{
			CalendarEntity calendarAlias = null;
			BankAccountMovement accountMovementAlias = null;
			BankAccountMovementData accountMovementDataAlias = null;
			Account accountAlias = null;
			Organization organizationAlias = null;
			Bank bankAlias = null;
			BankAccountsMovementsJournalNode resultAlias = null;
			
			var query = uow.Session.QueryOver(() => calendarAlias)
				.JoinEntityAlias(() => accountMovementDataAlias, () => accountMovementDataAlias.AccountMovement == null)
				.JoinEntityAlias(() => organizationAlias, () => organizationAlias.IsNeedCashlessMovementControl)
				.JoinAlias(() => organizationAlias.Accounts, () => accountAlias)
				.JoinAlias(() => accountAlias.InBank, () => bankAlias);
			
			var loadedAccountMovements = QueryOver.Of(() => accountMovementAlias)
				.Where(
					Restrictions.Conjunction()
						.Add(() => accountMovementAlias.Account.Id == accountAlias.Id)
						.Add(CustomRestrictions.Between(
							Projections.Property(() => calendarAlias.Date), 
							Projections.Property(() => accountMovementAlias.StartDate),
							Projections.Property(() => accountMovementAlias.EndDate)))
					)
				.Select(Projections.Property(() => accountMovementAlias.Id));

			var startDate = _filterViewModel.StartDate ?? _paymentSettings.ControlPointStartDate;
			//грузят выписки за прошлый день
			var endDate = DateTime.Today.AddDays(-1);
			
			if(_filterViewModel.EndDate.HasValue && _filterViewModel.EndDate < endDate)
			{
				endDate = _filterViewModel.EndDate.Value;
			}

			query.Where(() => calendarAlias.Date >= startDate)
				.And(() => calendarAlias.Date <= endDate)
				.And(() => !accountAlias.Inactive)
				.WithSubquery.WhereNotExists(loadedAccountMovements);

			if(_filterViewModel.OrganizationAccount != null)
			{
				query.Where(() => accountAlias.Id == _filterViewModel.OrganizationAccount.Id);
			}
			
			if(_filterViewModel.OrganizationBank != null)
			{
				query.Where(() => bankAlias.Id == _filterViewModel.OrganizationBank.Id);
			}

			query.SelectList(list => list
					.Select(() => calendarAlias.Date).WithAlias(() => resultAlias.StartDate)
					.Select(() => calendarAlias.Date).WithAlias(() => resultAlias.EndDate)
					.Select(() => accountAlias.Number).WithAlias(() => resultAlias.Account)
					.Select(() => bankAlias.Name).WithAlias(() => resultAlias.Bank)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Organization)
					.Select(() => accountMovementDataAlias.AccountMovementDataType).WithAlias(() => resultAlias.AccountMovementDataType)
					.Select(() => accountMovementDataAlias.Amount).WithAlias(() => resultAlias.Amount)
				)
				.TransformUsing(Transformers.AliasToBean<BankAccountsMovementsJournalNode>())
				.OrderBy(() => calendarAlias.Date).Desc()
				.OrderBy(() => organizationAlias.Id).Asc()
				.OrderBy(() => accountAlias.Id).Asc()
				.OrderBy(() => accountMovementDataAlias.Id).Asc()
				;
			
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

			IsExportToExcelInProcess = true;

			var nodes = GetReportData();
				var ordersReport = _lifetimeScope.Resolve<BankAccountsMovementsJournalReport>();
				ordersReport.Export(
					_filterViewModel.StartDate.Value,
					_filterViewModel.EndDate,
					nodes);

			IsExportToExcelInProcess = false;
		}
		
		private IEnumerable<BankAccountsMovementsJournalNode> GetReportData()
		{
			var loadedMovements = GetLoadedAccountMovements(UoW).List<BankAccountsMovementsJournalNode>();
			var notLoadedMovements = GetNotLoadedAccountMovements(UoW).List<BankAccountsMovementsJournalNode>();

			var accountMovements = loadedMovements
				.Concat(notLoadedMovements)
				.OrderByDescending(x => x.StartDate)
				.ThenByDescending(x => x.EndDate);

			return accountMovements;
		}
	}
}

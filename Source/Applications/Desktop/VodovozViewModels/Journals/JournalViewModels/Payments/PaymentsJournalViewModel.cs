using Autofac;
using DateTimeHelpers;
using DynamicData;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Deletion;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.Tdi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QS.Banks.Domain;
using QS.Dialog;
using QS.Project.Services.FileDialog;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Accounting.Payments;
using Vodovoz.ViewModels.Cash.Payments;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Payments;
using Vodovoz.ViewModels.ViewModels.Reports.Payments;
using VodovozBusiness.Domain.Operations;
using VodovozBusiness.Domain.Payments;
using static Vodovoz.Filters.ViewModels.PaymentsJournalFilterViewModel;
using BaseOrg = Vodovoz.Domain.Organizations.Organization;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public partial class PaymentsJournalViewModel : MultipleEntityJournalViewModelBase<PaymentJournalNode>
	{
		private readonly ICommonServices _commonServices;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ILifetimeScope _scope;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly ICurrentPermissionService _permissionService;
		private readonly IFileDialogService _fileDialogService;
		private bool _canCreateNewManualPayment;
		private bool _canCancelManualPaymentFromBankClient;
		private IPermissionResult _paymentPermissionResult;

		private PaymentsJournalFilterViewModel _filterViewModel;
		private ThreadDataLoader<PaymentJournalNode> _threadDataLoader;
		private bool _isExportToExcelInProcess;

		public PaymentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			ILifetimeScope scope,
			IPaymentsRepository paymentsRepository,
			IPaymentFromBankClientController paymentFromBankClientController,
			ICurrentPermissionService permissionService,
			IFileDialogService fileDialogService,
			Action<PaymentsJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices, navigationManager)
		{
			_commonServices = commonServices
				?? throw new ArgumentNullException(nameof(commonServices));
			_scope = scope
				?? throw new ArgumentNullException(nameof(scope));
			_paymentsRepository = paymentsRepository
				?? throw new ArgumentNullException(nameof(paymentsRepository));
			_paymentFromBankClientController = paymentFromBankClientController
				?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_permissionService = permissionService
				?? throw new ArgumentNullException(nameof(permissionService));
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			TabName = "Журнал платежей из банк-клиента";

			CreateFilter(filterParams);

			SetPermissions();

			RegisterLoadPayments();
			RegisterPaymentWriteOffs();
			RegisterOutgoingPayments();

			_threadDataLoader = DataLoader as ThreadDataLoader<PaymentJournalNode>;

			UseSlider = false;

			FinishJournalConfiguration();

			UpdateOnChanges(
				typeof(PaymentItem),
				typeof(Payment),
				typeof(PaymentWriteOff),
				typeof(VodOrder));

			Refresh();
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

		protected IQueryOver<Payment> PaymentsQuery(IUnitOfWork uow)
		{
			PaymentJournalNode resultAlias = null;
			Payment paymentAlias = null;
			BaseOrg organizationAlias = null;
			PaymentItem paymentItemAlias = null;
			PaymentItem paymentItemAlias2 = null;
			ProfitCategory profitCategoryAlias = null;
			Counterparty counterpartyAlias = null;
			Account orgAccountAlias = null;
			Bank orgBankAlias = null;
			CashlessMovementOperation incomeOperationAlias = null;

			var paymentQuery = uow.Session.QueryOver(() => paymentAlias)
				.Left.JoinAlias(p => p.CashlessMovementOperation, () => incomeOperationAlias,
					cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Left.JoinAlias(p => p.Organization, () => organizationAlias)
				.Left.JoinAlias(p => p.ProfitCategory, () => profitCategoryAlias)
				.Left.JoinAlias(p => p.Items, () => paymentItemAlias)
				.Left.JoinAlias(p => p.Counterparty, () => counterpartyAlias)
				.Left.JoinAlias(p => p.OrganizationAccount, () => orgAccountAlias)
				.Left.JoinAlias(() => orgAccountAlias.InBank, () => orgBankAlias);

			var numOrdersProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "GROUP_CONCAT( ?1 SEPARATOR ?2)"),
				NHibernateUtil.String,
				Projections.Property(() => paymentItemAlias.Order.Id),
				Projections.Constant("\n"));

			var unAllocatedSumProjection =
				Projections.SqlFunction(
					new SQLFunctionTemplate(NHibernateUtil.Decimal,
						"CASE " +
						$"WHEN ?1 = '{PaymentState.Cancelled}' OR ?1 = '{PaymentState.undistributed}' AND ?2 IS NOT NULL THEN ?6 " +
						$"WHEN ?1 = '{PaymentState.undistributed}' AND ?2 IS NULL THEN ?3 " +
						"ELSE IFNULL(?4, ?6) - IFNULL(?5, ?6) END"),
					NHibernateUtil.Decimal,
					Projections.Property(() => paymentAlias.Status),
					Projections.Property(() => paymentAlias.RefundPaymentFromOrderId),
					Projections.Property(() => paymentAlias.Total),
					Projections.Property(() => incomeOperationAlias.Income),
					Projections.Sum(() => paymentItemAlias2.Sum),
					Projections.Constant(0));

			var unAllocatedSum = QueryOver.Of(() => paymentItemAlias2)
				.Where(pi => pi.Payment.Id == paymentAlias.Id)
				.And(pi => pi.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(unAllocatedSumProjection);

			var counterpartyNameProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "IFNULL(?1, '')"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.Name));

			#region filter

			if(_filterViewModel != null)
			{
				if(_filterViewModel.DocumentType != null
					&& _filterViewModel.DocumentType != typeof(Payment))
				{
					paymentQuery.Where(p => p.Id == -1);
				}

				if(_filterViewModel.Counterparty != null)
				{
					paymentQuery.Where(p => p.Counterparty.Id == _filterViewModel.Counterparty.Id);
				}
				
				if(_filterViewModel.Organization != null)
				{
					paymentQuery.Where(p => p.Organization.Id == _filterViewModel.Organization.Id);
				}

				var startDate = _filterViewModel.StartDate;
				var endDate = _filterViewModel.EndDate;

				if(startDate.HasValue)
				{
					paymentQuery.Where(p => p.Date >= startDate);
				}

				if(endDate.HasValue)
				{
					paymentQuery.Where(p => p.Date <= endDate.Value.LatestDayTime());
				}

				if(_filterViewModel.HideCompleted)
				{
					paymentQuery.Where(p => p.Status != PaymentState.completed);
				}

				if(_filterViewModel.HideCancelledPayments)
				{
					paymentQuery.Where(p => p.Status != PaymentState.Cancelled);
				}

				if(_filterViewModel.HidePaymentsWithoutCounterparty)
				{
					paymentQuery.Where(p => p.Counterparty != null);
				}

				if(_filterViewModel.HideAllocatedPayments)
				{
					paymentQuery.Where(Restrictions.Gt(Projections.SubQuery(unAllocatedSum), 0));
				}

				if(_filterViewModel.PaymentState.HasValue)
				{
					paymentQuery.Where(p => p.Status == _filterViewModel.PaymentState);
				}

				if(_filterViewModel.IsManuallyCreated.HasValue)
				{
					if(_filterViewModel.IsManuallyCreated.Value)
					{
						paymentQuery.Where(p => p.IsManuallyCreated);
					}
					else
					{
						paymentQuery.Where(p => !p.IsManuallyCreated);
					}
				}

				if(_filterViewModel.IsSortingDescByUnAllocatedSum)
				{
					paymentQuery = paymentQuery.OrderBy(Projections.SubQuery(unAllocatedSum)).Desc;
				}

				switch(_filterViewModel.SortType)
				{
					case PaymentJournalSortType.Status:
						paymentQuery.OrderBy(() => paymentAlias.Status).Asc();
						paymentQuery.OrderBy(() => paymentAlias.CounterpartyName).Asc();
						paymentQuery.OrderBy(() => paymentAlias.Total).Asc();
						break;
					case PaymentJournalSortType.Date:
						paymentQuery.OrderBy(() => paymentAlias.Date.Date).Desc();
						paymentQuery.OrderBy(() => paymentAlias.PaymentNum).Desc();
						break;
					case PaymentJournalSortType.PaymentNum:
						paymentQuery.OrderBy(() => paymentAlias.PaymentNum).Desc();
						paymentQuery.OrderBy(() => paymentAlias.Date.Date).Desc();
						break;
					case PaymentJournalSortType.TotalSum:
						paymentQuery.OrderBy(() => paymentAlias.Total).Desc();
						paymentQuery.OrderBy(() => paymentAlias.Date.Date).Desc();
						break;
				}
			}

			var selectedProfitCategories = _filterViewModel.GetSelectedProfitCategoriesIds();

			if(selectedProfitCategories.Any())
			{
				paymentQuery.WhereRestrictionOn(() => paymentAlias.ProfitCategory.Id).IsInG(selectedProfitCategories);
			}

			#endregion filter

			paymentQuery.Where(GetSearchCriterion(
				() => paymentAlias.PaymentNum,
				() => paymentAlias.Total,
				() => paymentAlias.CounterpartyName,
				() => paymentItemAlias.Order.Id));

			var resultQuery = paymentQuery
				.SelectList(list => list
					.SelectGroup(() => paymentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => paymentAlias.PaymentNum).WithAlias(() => resultAlias.PaymentNum)
					.Select(() => paymentAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(() => paymentAlias.Total).WithAlias(() => resultAlias.Total)
					.Select(numOrdersProjection).WithAlias(() => resultAlias.Orders)
					.Select(() => paymentAlias.CounterpartyName).WithAlias(() => resultAlias.PayerName)
					.Select(counterpartyNameProjection).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.Organization)
					.Select(() => orgAccountAlias.Number).WithAlias(() => resultAlias.OrganizationAccountNumber)
					.Select(() => orgBankAlias.Name).WithAlias(() => resultAlias.OrganizationBank)
					.Select(() => paymentAlias.PaymentPurpose).WithAlias(() => resultAlias.PaymentPurpose)
					.Select(() => profitCategoryAlias.Name).WithAlias(() => resultAlias.ProfitCategory)
					.Select(() => paymentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => paymentAlias.IsManuallyCreated).WithAlias(() => resultAlias.IsManualCreated)
					.SelectSubQuery(unAllocatedSum).WithAlias(() => resultAlias.UnAllocatedSum)
					.Select(() => typeof(Payment)).WithAlias(() => resultAlias.EntityType))
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());

			return resultQuery;
		}

		protected IQueryOver<PaymentWriteOff> PaymentWriteOffsQuery(IUnitOfWork uow)
		{
			PaymentJournalNode resultAlias = null;
			PaymentWriteOff paymentWriteOff = null;
			BaseOrg organizationAlias = null;
			Counterparty counterpartyAlias = null;
			CashlessMovementOperation expenseOperationAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			var paymentQuery = uow.Session.QueryOver(() => paymentWriteOff)
				.Inner.JoinAlias(p => p.CashlessMovementOperation, () => expenseOperationAlias)
				.JoinEntityAlias(
					() => counterpartyAlias,
					() => paymentWriteOff.CounterpartyId == counterpartyAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => organizationAlias,
					() => paymentWriteOff.OrganizationId == organizationAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(
					() => financialExpenseCategoryAlias,
					() => paymentWriteOff.FinancialExpenseCategoryId == financialExpenseCategoryAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			var counterpartyNameProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "IFNULL(?1, '')"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.Name));

			#region filter

			if(_filterViewModel != null)
			{
				var selectedProfitCategories = _filterViewModel.GetSelectedProfitCategoriesIds();
				
				if((_filterViewModel.DocumentType != null && _filterViewModel.DocumentType != typeof(PaymentWriteOff))
					|| selectedProfitCategories.Any())
				{
					paymentQuery.Where(p => p.Id == -1);
				}

				if(_filterViewModel.Counterparty != null)
				{
					paymentQuery.Where(p => p.CounterpartyId == _filterViewModel.Counterparty.Id);
				}
				
				if(_filterViewModel.Organization != null)
				{
					paymentQuery.Where(p => p.OrganizationId == _filterViewModel.Organization.Id);
				}

				var startDate = _filterViewModel.StartDate;
				var endDate = _filterViewModel.EndDate;

				if(startDate.HasValue)
				{
					paymentQuery.Where(p => p.Date >= startDate);
				}

				if(endDate.HasValue)
				{
					paymentQuery.Where(p => p.Date <= endDate.Value.LatestDayTime());
				}
				
				if(_filterViewModel.IsManuallyCreated.HasValue)
				{
					if(!_filterViewModel.IsManuallyCreated.Value)
					{
						paymentQuery.Where(p => p.Id == 0);
					}
				}

				if(_filterViewModel.HidePaymentsWithoutCounterparty)
				{
					paymentQuery.Where(p => p.CounterpartyId != null);
				}

				switch(_filterViewModel.SortType)
				{
					case PaymentJournalSortType.Status:
						paymentQuery.OrderBy(() => counterpartyAlias.Name).Asc();
						paymentQuery.OrderBy(() => paymentWriteOff.Sum).Asc();
						break;
					case PaymentJournalSortType.Date:
						paymentQuery.OrderBy(() => paymentWriteOff.Date.Date).Desc();
						paymentQuery.OrderBy(() => paymentWriteOff.PaymentNumber).Desc();
						break;
					case PaymentJournalSortType.PaymentNum:
						paymentQuery.OrderBy(() => paymentWriteOff.PaymentNumber).Desc();
						paymentQuery.OrderBy(() => paymentWriteOff.Date.Date).Desc();
						break;
					case PaymentJournalSortType.TotalSum:
						paymentQuery.OrderBy(() => paymentWriteOff.Sum).Desc();
						paymentQuery.OrderBy(() => paymentWriteOff.Date.Date).Desc();
						break;
				}
			}

			#endregion filter

			paymentQuery.Where(GetSearchCriterion(
				() => paymentWriteOff.PaymentNumber,
				() => paymentWriteOff.Sum,
				() => counterpartyAlias.Name));

			var resultQuery = paymentQuery
				.SelectList(list => list
					.SelectGroup(() => paymentWriteOff.Id).WithAlias(() => resultAlias.Id)
					.Select(() => paymentWriteOff.PaymentNumber).WithAlias(() => resultAlias.PaymentNum)
					.Select(() => paymentWriteOff.Date).WithAlias(() => resultAlias.Date)
					.Select(() => paymentWriteOff.Sum).WithAlias(() => resultAlias.Total)
					.Select(Projections.Constant("")).WithAlias(() => resultAlias.Orders)
					.Select(Projections.Constant("")).WithAlias(() => resultAlias.PayerName)
					.Select(counterpartyNameProjection).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.Organization)
					.Select(() => paymentWriteOff.Reason).WithAlias(() => resultAlias.PaymentPurpose)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ProfitCategory)
					.Select(Projections.Constant(true)).WithAlias(() => resultAlias.IsManualCreated)
					.Select(Projections.Constant(PaymentState.completed)).WithAlias(() => resultAlias.Status)
					.Select(() => typeof(PaymentWriteOff)).WithAlias(() => resultAlias.EntityType)
					)
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());

			return resultQuery;
		}

		protected IQueryOver<OutgoingPayment> OutgoingPaymentsQuery(IUnitOfWork unitOfWork)
		{
			PaymentJournalNode resultAlias = null;
			
			OutgoingPayment outgoingPaymentAlias = null;
			
			BaseOrg organizationAlias = null;
			Counterparty counterpartyAlias = null;
			FinancialExpenseCategory financialExpenseCategoryAlias = null;

			var paymentQuery = unitOfWork.Session.QueryOver(() => outgoingPaymentAlias)
				.JoinEntityAlias(() => organizationAlias,
					Restrictions.EqProperty(Projections.Property(() => outgoingPaymentAlias.OrganizationId),
						Projections.Property(() => organizationAlias.Id)),
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => counterpartyAlias,
					Restrictions.EqProperty(Projections.Property(() => outgoingPaymentAlias.CounterpartyId),
						Projections.Property(() => counterpartyAlias.Id)),
					NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinEntityAlias(() => financialExpenseCategoryAlias,
					Restrictions.EqProperty(Projections.Property(() => outgoingPaymentAlias.FinancialExpenseCategoryId),
						Projections.Property(() => financialExpenseCategoryAlias.Id)),
					NHibernate.SqlCommand.JoinType.LeftOuterJoin);

			var counterpartyNameProjection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.String, "IFNULL(?1, '')"),
				NHibernateUtil.String,
				Projections.Property(() => counterpartyAlias.Name));

			#region filter

			if(_filterViewModel != null)
			{
				var selectedProfitCategories = _filterViewModel.GetSelectedProfitCategoriesIds();

				if((_filterViewModel.DocumentType != null && _filterViewModel.DocumentType != typeof(OutgoingPayment))
					|| selectedProfitCategories.Any())
				{
					paymentQuery.Where(p => p.Id == -1);
				}

				if(_filterViewModel.OutgoingPaymentsWithoutCashlessRequestAssigned)
				{
					paymentQuery.Where(p => p.CashlessRequestId == null);
				}

				if(_filterViewModel.Counterparty != null)
				{
					paymentQuery.Where(p => p.CounterpartyId == _filterViewModel.Counterparty.Id);
				}
				
				if(_filterViewModel.Organization != null)
				{
					paymentQuery.Where(p => p.OrganizationId == _filterViewModel.Organization.Id);
				}

				var startDate = _filterViewModel.StartDate;
				var endDate = _filterViewModel.EndDate;

				if(startDate.HasValue)
				{
					paymentQuery.Where(p => p.PaymentDate >= startDate);
				}

				if(endDate.HasValue)
				{
					paymentQuery.Where(p => p.PaymentDate <= endDate.Value.LatestDayTime());
				}
				
				if(_filterViewModel.IsManuallyCreated.HasValue)
				{
					if(!_filterViewModel.IsManuallyCreated.Value)
					{
						paymentQuery.Where(p => p.Id == 0);
					}
				}

				if(_filterViewModel.HidePaymentsWithoutCounterparty)
				{
					paymentQuery.Where(p => p.CounterpartyId != null);
				}

				switch(_filterViewModel.SortType)
				{
					case PaymentJournalSortType.Status:
						paymentQuery.OrderBy(() => counterpartyAlias.Name).Asc();
						paymentQuery.OrderBy(() => outgoingPaymentAlias.Sum).Asc();
						break;
					case PaymentJournalSortType.Date:
						paymentQuery.OrderBy(() => outgoingPaymentAlias.PaymentDate.Date).Desc();
						paymentQuery.OrderBy(() => outgoingPaymentAlias.PaymentNumber).Desc();
						break;
					case PaymentJournalSortType.PaymentNum:
						paymentQuery.OrderBy(() => outgoingPaymentAlias.PaymentNumber).Desc();
						paymentQuery.OrderBy(() => outgoingPaymentAlias.PaymentDate.Date).Desc();
						break;
					case PaymentJournalSortType.TotalSum:
						paymentQuery.OrderBy(() => outgoingPaymentAlias.Sum).Desc();
						paymentQuery.OrderBy(() => outgoingPaymentAlias.PaymentDate.Date).Desc();
						break;
				}
			}

			#endregion filter

			paymentQuery.Where(GetSearchCriterion(
				() => outgoingPaymentAlias.PaymentNumber,
				() => outgoingPaymentAlias.Sum,
				() => counterpartyAlias.Name));

			var resultQuery = paymentQuery
				.SelectList(list => list
					.SelectGroup(() => outgoingPaymentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => outgoingPaymentAlias.PaymentNumber).WithAlias(() => resultAlias.PaymentNum)
					.Select(() => outgoingPaymentAlias.PaymentDate).WithAlias(() => resultAlias.Date)
					.Select(() => outgoingPaymentAlias.Sum).WithAlias(() => resultAlias.Total)
					.Select(Projections.Constant("")).WithAlias(() => resultAlias.Orders)
					.Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.PayerName)
					.Select(counterpartyNameProjection).WithAlias(() => resultAlias.CounterpartyName)
					.Select(() => organizationAlias.FullName).WithAlias(() => resultAlias.Organization)
					.Select(() => outgoingPaymentAlias.PaymentPurpose).WithAlias(() => resultAlias.PaymentPurpose)
					.Select(() => financialExpenseCategoryAlias.Title).WithAlias(() => resultAlias.ProfitCategory)
					.Select(Projections.Constant(true)).WithAlias(() => resultAlias.IsManualCreated)
					.Select(Projections.Constant(PaymentState.completed)).WithAlias(() => resultAlias.Status)
					.Select(() => typeof(OutgoingPayment)).WithAlias(() => resultAlias.EntityType)
					)
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());

			return resultQuery;
		}
		
		private void SetPermissions()
		{
			_canCreateNewManualPayment =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Core.Domain.Permissions.PaymentPermissions.BankClient.CanCreateNewManualPaymentFromBankClient);

			_canCancelManualPaymentFromBankClient =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(
					Vodovoz.Core.Domain.Permissions.PaymentPermissions.BankClient.CanCancelManualPaymentFromBankClient);
		}

		private void CreateFilter(Action<PaymentsJournalFilterViewModel> filterParams = null)
		{
			_filterViewModel = _scope.Resolve<PaymentsJournalFilterViewModel>(new[]{
				new TypedParameter(typeof(ITdiTab), this)
			});

			if(filterParams != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterParams);
			}

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			var sortExpressions = GetOrdering();
			_threadDataLoader.OrderRules.Clear();
			_threadDataLoader.OrderRules.AddRange(sortExpressions);
			Refresh();
		}

		private void RegisterLoadPayments()
		{
			var paymentsConfiguration = RegisterEntity<Payment>(PaymentsQuery)
				.AddDocumentConfiguration(
					CreateLoadPaymentsDialog,
					EditPaymentDialog,
					node => node.EntityType == typeof(Payment),
					"Платежи(загрузка выписки)",
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			paymentsConfiguration.FinishConfiguration();
		}

		private void RegisterPaymentWriteOffs()
		{
			var paymentsConfiguration = RegisterEntity<PaymentWriteOff>(PaymentWriteOffsQuery)
				.AddDocumentConfiguration(
					CreatePaymentWriteOffDialog,
					EditPaymentWriteOffDialog,
					node => node.EntityType == typeof(PaymentWriteOff),
					typeof(PaymentWriteOff).GetClassUserFriendlyName().Nominative.CapitalizeSentence(),
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);

			paymentsConfiguration.FinishConfiguration();
		}

		private void RegisterOutgoingPayments()
		{
			var paymentsConfiguration = RegisterEntity<OutgoingPayment>(OutgoingPaymentsQuery)
				.AddDocumentConfiguration(
					CreateOutgoingPaymentDialog,
					EditOutgoingPaymentDialog,
					node => node.EntityType == typeof(OutgoingPayment),
					typeof(OutgoingPayment).GetClassUserFriendlyName().Nominative.CapitalizeSentence(),
					new JournalParametersForDocument { HideJournalForCreateDialog = false, HideJournalForOpenDialog = true }
				);
			paymentsConfiguration.FinishConfiguration();
		}

		private IList<SortRule<PaymentJournalNode>> GetOrdering()
		{
			switch(_filterViewModel.SortType)
			{
				case PaymentJournalSortType.Status:
					return new List<SortRule<PaymentJournalNode>>
						{
							new SortRule<PaymentJournalNode>(node => node.Status, false),
							new SortRule<PaymentJournalNode>(node => node.CounterpartyName, false),
							new SortRule<PaymentJournalNode>(node => node.Total, false)
						};
				case PaymentJournalSortType.Date:
					return new List<SortRule<PaymentJournalNode>>
						{
							new SortRule<PaymentJournalNode>(node => node.Date.Date, true),
							new SortRule<PaymentJournalNode>(node => node.PaymentNum, true)
						};
				case PaymentJournalSortType.PaymentNum:
					return new List<SortRule<PaymentJournalNode>>
						{
							new SortRule<PaymentJournalNode>(node => node.PaymentNum, true),
							new SortRule<PaymentJournalNode>(node => node.Date.Date, true)
						};
				case PaymentJournalSortType.TotalSum:
					return new List<SortRule<PaymentJournalNode>>
						{
							new SortRule<PaymentJournalNode>(node => node.Total, true),
							new SortRule<PaymentJournalNode>(node => node.Date.Date, true)
						};
				default:
					throw new InvalidOperationException("Невозможная сортировка");
			}
		}

		protected ITdiTab CreateLoadPaymentsDialog() =>
			NavigationManager.OpenViewModel<PaymentLoaderViewModel>(this).ViewModel;

		protected ITdiTab EditPaymentDialog(PaymentJournalNode node) =>
			NavigationManager.OpenViewModel<ManualPaymentMatchingViewModel, IEntityUoWBuilder>(
				this,
				EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)))?.ViewModel;

		protected ITdiTab CreatePaymentWriteOffDialog() =>
			NavigationManager.OpenViewModel<PaymentWriteOffViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel;

		protected ITdiTab EditPaymentWriteOffDialog(PaymentJournalNode node) =>
			NavigationManager.OpenViewModel<PaymentWriteOffViewModel, IEntityUoWBuilder>(
				this,
				EntityUoWBuilder.ForOpen(DomainHelper.GetId(node))).ViewModel;

		protected ITdiTab CreateOutgoingPaymentDialog() =>
			NavigationManager.OpenViewModel<OutgoingPaymentCreateViewModel, IEntityUoWBuilder>(this, EntityUoWBuilder.ForCreate()).ViewModel;

		protected ITdiTab EditOutgoingPaymentDialog(PaymentJournalNode node) =>
			NavigationManager.OpenViewModel<OutgoingPaymentEditViewModel, IEntityUoWBuilder>(
				this,
				EntityUoWBuilder.ForOpen(DomainHelper.GetId(node))).ViewModel;

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			CreateDefaultSelectAction();

			CreateAddActions();
			CreateEditAction();
			CreateDeleteAction();

			_paymentPermissionResult = _permissionService.ValidateEntityPermission(typeof(Payment));

			CreateCancelPaymentAction();

			NodeActionsList.Add(new JournalAction(
				"Завершить распределение",
				x => true,
				x => true,
				selectedItems => CompleteAllocation()));

			CreateExportToExcelAction();
		}

		protected void CreateAddActions()
		{
			if(!EntityConfigs.Any())
			{
				return;
			}

			var addParentNodeAction = new JournalAction("Добавить", (selected) => true, (selected) => true, (selected) => { });

			foreach(var entityConfig in EntityConfigs.Values)
			{
				foreach(var documentConfig in entityConfig.EntityDocumentConfigurations)
				{
					foreach(var createDlgConfig in documentConfig.GetCreateEntityDlgConfigs())
					{
						var childNodeAction = new JournalAction(createDlgConfig.Title,
							(selected) => entityConfig.PermissionResult.CanCreate,
							(selected) =>
							{
								if(typeof(OutgoingPayment) == entityConfig.EntityType)
								{
									return _canCreateNewManualPayment;
								}
								
								return entityConfig.PermissionResult.CanCreate;
							},
							(selected) =>
							{
								createDlgConfig.OpenEntityDialogFunction.Invoke();
								if(documentConfig.JournalParameters.HideJournalForCreateDialog)
								{
									HideJournal(TabParent);
								}
							}
						);
						addParentNodeAction.ChildActionsList.Add(childNodeAction);
					}
				}
			}

			addParentNodeAction.ChildActionsList.Add(CreateAddNewPaymentAction());
			NodeActionsList.Add(addParentNodeAction);
		}

		protected void CreateEditAction()
		{
			var editAction = new JournalAction("Изменить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PaymentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					PaymentJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanUpdate;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PaymentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					PaymentJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					var foundDocumentConfig = config.EntityDocumentConfigurations.FirstOrDefault(x => x.IsIdentified(selectedNode));

					foundDocumentConfig.GetOpenEntityDlgFunction().Invoke(selectedNode);
					if(foundDocumentConfig.JournalParameters.HideJournalForOpenDialog)
					{
						HideJournal(TabParent);
					}
				}
			);
			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
			NodeActionsList.Add(editAction);
		}

		protected void CreateDeleteAction()
		{
			var deleteAction = new JournalAction("Удалить",
				(selected) =>
				{
					var selectedNodes = selected.OfType<PaymentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return false;
					}
					PaymentJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return false;
					}
					if(selectedNode.EntityType == typeof(Payment))
					{
						return false;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					return config.PermissionResult.CanDelete;
				},
				(selected) => true,
				(selected) =>
				{
					var selectedNodes = selected.OfType<PaymentJournalNode>();
					if(selectedNodes == null || selectedNodes.Count() != 1)
					{
						return;
					}
					PaymentJournalNode selectedNode = selectedNodes.First();
					if(!EntityConfigs.ContainsKey(selectedNode.EntityType))
					{
						return;
					}
					var config = EntityConfigs[selectedNode.EntityType];
					if(config.PermissionResult.CanDelete)
					{
						DeleteHelper.DeleteEntity(selectedNode.EntityType, selectedNode.Id);
					}
				},
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		private IJournalAction CreateAddNewPaymentAction()
		{
			return new JournalAction(
				"Ручной платеж",
				x => _paymentPermissionResult.CanCreate,
				x => _canCreateNewManualPayment,
				selectedItems =>
				{
					NavigationManager.OpenViewModel<CreateManualPaymentFromBankClientViewModel, IEntityUoWBuilder>(
						this, EntityUoWBuilder.ForCreate());
				});
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
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Нужно выбрать начальный период выгрузки");
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

				var ordersReport = new PaymentsFromBankClientReport(
					_filterViewModel.StartDate.Value,
					_filterViewModel.EndDate,
					nodes);

				ordersReport.Export(saveDialogResul.Path);
			});

			IsExportToExcelInProcess = false;
		}
		
		private IEnumerable<PaymentJournalNode> GetReportData()
		{
			var paymentsRows = PaymentsQuery(UoW).List<PaymentJournalNode>();
			var writeOffs = PaymentWriteOffsQuery(UoW).List<PaymentJournalNode>();
			var outgoingPayments = OutgoingPaymentsQuery(UoW).List<PaymentJournalNode>();

			var sortedRules = GetOrdering();

			var paymentNodes = paymentsRows
				.Concat(writeOffs)
				.Concat(outgoingPayments);

			return sortedRules.Aggregate(paymentNodes,
				(current, rule) =>
					rule.Descending ? current.OrderByDescending(rule.GetOrderByValue) : current.OrderBy(rule.GetOrderByValue));
		}

		private void CreateCancelPaymentAction()
		{
			var userIsAdmin = _commonServices.UserService.GetCurrentUser().IsAdmin;

			var cancelPaymentAction = new JournalAction(
				"Отменить платеж",
				(selected) =>
				{
					var selectedNodes = selected.Cast<PaymentJournalNode>();

					if(!selectedNodes.Any())
					{
						return false;
					}

					if(selectedNodes.Any(pjn => pjn.EntityType != typeof(Payment)))
					{
						return false;
					}

					if(selectedNodes.Any(p => p.Status == PaymentState.Cancelled))
					{
						return false;
					}

					var canCancel = (_canCancelManualPaymentFromBankClient && !selectedNodes.Any(p => !p.IsManualCreated))
						|| userIsAdmin;

					return canCancel;
				},
				(selected) => userIsAdmin || _canCancelManualPaymentFromBankClient,
				(selected) =>
				{
					CancelPayments(selected.OfType<PaymentJournalNode>().ToArray());
				});

			NodeActionsList.Add(cancelPaymentAction);
		}

		private void CompleteAllocation()
		{
			var distributedPayments = _paymentsRepository.GetAllDistributedPayments(UoW);

			if(distributedPayments.Any())
			{
				foreach(var payment in distributedPayments)
				{
					payment.Status = PaymentState.completed;
					UoW.Save(payment);
				}

				UoW.Commit();
			}
		}

		protected void CancelPayments(PaymentJournalNode[] nodes)
		{
			if(!_commonServices.InteractiveService.Question("Платеж будет отменен.\n\nПродолжить?"))
			{
				return;
			}

			UoW.Session.Clear();

			foreach(var node in nodes)
			{
				_paymentFromBankClientController.CancellPaymentWithAllocationsByUserRequest(UoW, node.Id);
			}
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}

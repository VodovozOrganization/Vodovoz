using Autofac;
using Autofac.Core;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using System;
using System.Linq;
using Vodovoz.Controllers;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Payments;
using BaseOrg = Vodovoz.Domain.Organizations.Organization;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class PaymentsJournalViewModel : EntityJournalViewModelBase<Payment, ManualPaymentMatchingViewModel, PaymentJournalNode>
	{
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ILifetimeScope _scope;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private bool _canCreateNewManualPayment;
		private bool _canCancelManualPaymentFromBankClient;
		private IPermissionResult _paymentPermissionResult;
		
		private PaymentsJournalFilterViewModel _filterViewModel;

		public PaymentsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IPaymentsRepository paymentsRepository,
			IDeleteEntityService deleteEntityService,
			ILifetimeScope scope,
			IPaymentFromBankClientController paymentFromBankClientController,
			params Action<PaymentsJournalFilterViewModel>[] filterParams)
			: base(unitOfWorkFactory,
				commonServices?.InteractiveService,
				navigationManager,
				deleteEntityService,
				commonServices?.CurrentPermissionService)
		{
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
			_paymentFromBankClientController = paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			TabName = "Журнал платежей из банк-клиента";

			CreateFilter(filterParams);

			SetPermissions();

			CreateCancelPaymentAction();
			UpdateOnChanges(
				typeof(PaymentItem),
				typeof(Payment),
				typeof(VodOrder));
		}

		private void SetPermissions()
		{
			_canCreateNewManualPayment =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Payment.BankClient.CanCreateNewManualPaymentFromBankClient);

			_canCancelManualPaymentFromBankClient =
				_commonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Permissions.Payment.BankClient.CanCancelManualPaymentFromBankClient);
		}

		private void CreateFilter(params Action<PaymentsJournalFilterViewModel>[] filterParams)
		{
			Parameter[] parameters = {
				new TypedParameter(typeof(ITdiTab), this),
				new TypedParameter(typeof(Action<PaymentsJournalFilterViewModel>[]), filterParams)
			};
			
			_filterViewModel = _scope.Resolve<PaymentsJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Payment> ItemsQuery(IUnitOfWork uow)
		{
			PaymentJournalNode resultAlias = null;
			Payment paymentAlias = null;
			BaseOrg organizationAlias = null;
			PaymentItem paymentItemAlias = null;
			PaymentItem paymentItemAlias2 = null;
			ProfitCategory profitCategoryAlias = null;
			Counterparty counterpartyAlias = null;
			CashlessMovementOperation incomeOperationAlias = null;

			var paymentQuery = uow.Session.QueryOver(() => paymentAlias)
				.Left.JoinAlias(p => p.CashlessMovementOperation, () => incomeOperationAlias,
					cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Left.JoinAlias(p => p.Organization, () => organizationAlias)
				.Left.JoinAlias(p => p.ProfitCategory, () => profitCategoryAlias)
				.Left.JoinAlias(p => p.PaymentItems, () => paymentItemAlias)
				.Left.JoinAlias(p => p.Counterparty, () => counterpartyAlias);
			
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
				if(_filterViewModel.Counterparty != null)
				{
					paymentQuery.Where(p => p.Counterparty.Id == _filterViewModel.Counterparty.Id);
				}
				
				if(_filterViewModel.StartDate.HasValue)
				{
					paymentQuery.Where(p => p.Date >= _filterViewModel.StartDate);
				}

				if(_filterViewModel.EndDate.HasValue)
				{
					paymentQuery.Where(p => p.Date <= _filterViewModel.EndDate.Value.AddDays(1).AddMilliseconds(-1));
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

				if(_filterViewModel.IsManuallyCreated)
				{
					paymentQuery.Where(p => p.IsManuallyCreated);
				}
				
				if(_filterViewModel.IsSortingDescByUnAllocatedSum)
				{
					paymentQuery = paymentQuery.OrderBy(Projections.SubQuery(unAllocatedSum)).Desc;
				}
			}

			#endregion filter

			paymentQuery.Where(GetSearchCriterion(
				() => paymentAlias.PaymentNum,
				() => paymentAlias.Total,
				() => paymentAlias.CounterpartyName,
				() => paymentItemAlias.Order.Id
			));

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
					.Select(() => paymentAlias.PaymentPurpose).WithAlias(() => resultAlias.PaymentPurpose)
					.Select(() => profitCategoryAlias.Name).WithAlias(() => resultAlias.ProfitCategory)
					.Select(() => paymentAlias.Status).WithAlias(() => resultAlias.Status)
					.Select(() => paymentAlias.IsManuallyCreated).WithAlias(() => resultAlias.IsManualCreated)
					.SelectSubQuery(unAllocatedSum).WithAlias(() => resultAlias.UnAllocatedSum))
				.OrderBy(() => paymentAlias.Status).Asc
				.OrderBy(() => paymentAlias.CounterpartyName).Asc
				.OrderBy(() => paymentAlias.Total).Asc
				.TransformUsing(Transformers.AliasToBean<PaymentJournalNode>());
			
			return resultQuery;
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<PaymentLoaderViewModel>(this);
		}

		protected override void EditEntityDialog(PaymentJournalNode node)
		{
			NavigationManager.OpenViewModel<ManualPaymentMatchingViewModel, IEntityUoWBuilder>(
				this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)));
		}
		
		protected override void CreateNodeActions()
		{
			_paymentPermissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(Payment));

			NodeActionsList.Clear();
			CreateAddNewPaymentAction();

			var addAction = new JournalAction("Добавить",
				(selected) => _paymentPermissionResult.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => _paymentPermissionResult.CanUpdate && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.OfType<PaymentJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}

			NodeActionsList.Add(new JournalAction(
					"Завершить распределение", 
					x => true,
					x => true,
					selectedItems => CompleteAllocation()
				)
			);
		}

		private void CreateAddNewPaymentAction()
		{
			NodeActionsList.Add(new JournalAction(
					"Создать новый платеж", 
					x => _paymentPermissionResult.CanCreate,
					x => _canCreateNewManualPayment,
					selectedItems =>
					{
						_navigationManager.OpenViewModel<CreateManualPaymentFromBankClientViewModel, IEntityUoWBuilder>(
							this, EntityUoWBuilder.ForCreate());
					}
				)
			);
		}

		private void CreateCancelPaymentAction()
		{
			var userIsAdmin = _commonServices.UserService.GetCurrentUser().IsAdmin;

			var cancelPaymentAction = new JournalAction("Отменить платеж",
					(selected) =>
					{
						if(!selected.Any())
						{
							return false;
						}

						var selectedPayments = selected.OfType<PaymentJournalNode>().ToArray();

						if(selectedPayments.Any(p => p.Status == PaymentState.Cancelled))
						{
							return false;
						}

						var canCancel = (_canCancelManualPaymentFromBankClient && !selectedPayments.Any(p => !p.IsManualCreated))
							|| userIsAdmin;

						return canCancel;
					},
					(selected) => userIsAdmin || _canCancelManualPaymentFromBankClient,
					(selected) =>
					{
						CancelPayments(selected.OfType<PaymentJournalNode>().ToArray());
					}
				);

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

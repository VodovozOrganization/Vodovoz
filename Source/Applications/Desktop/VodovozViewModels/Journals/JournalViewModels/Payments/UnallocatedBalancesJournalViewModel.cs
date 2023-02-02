﻿using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Services;
using Vodovoz.ViewModels.Payments;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class UnallocatedBalancesJournalViewModel
		: EntityJournalViewModelBase<Payment, PaymentsJournalViewModel, UnallocatedBalancesJournalNode>
	{
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly ILifetimeScope _scope;
		private readonly int _closingDocumentDeliveryScheduleId;
		private bool _canEdit;
		private UnallocatedBalancesJournalFilterViewModel _filterViewModel;
		public UnallocatedBalancesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			IPaymentsRepository paymentsRepository,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			IDeliveryScheduleParametersProvider deliveryScheduleParametersProvider,
			ILifetimeScope scope,
			IDeleteEntityService deleteEntityService = null,
			params Action<UnallocatedBalancesJournalFilterViewModel>[] filterParams)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(navigationManager == null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}
			if(currentPermissionService == null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			_closingDocumentDeliveryScheduleId =
				(deliveryScheduleParametersProvider ?? throw new ArgumentNullException(nameof(deliveryScheduleParametersProvider)))
				.ClosingDocumentDeliveryScheduleId;
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			TabName = "Журнал нераспределенных балансов";

			CreateFilter(filterParams);
			CreateAutomaticallyAllocationAction();
		}

		private void CreateFilter(params Action<UnallocatedBalancesJournalFilterViewModel>[] filterParams)
		{
			Autofac.Core.Parameter[] parameters = {
				new TypedParameter(typeof(ITdiTab), this),
				new TypedParameter(typeof(Action<UnallocatedBalancesJournalFilterViewModel>[]), filterParams),
				new TypedParameter(typeof(string), UoW.GetById<DeliverySchedule>(_closingDocumentDeliveryScheduleId).Name),
			};

			_filterViewModel = _scope.Resolve<UnallocatedBalancesJournalFilterViewModel>(parameters);
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		private void CreateAutomaticallyAllocationAction()
		{
			var automaticallyAllocationAction = new JournalAction("Автоматическое распределение положительного баланса",
				(selected) => _canEdit && selected.Any(),
				(selected) => true,
				(selected) =>
				{
					var ctorTypes = new[]
					{
						typeof(IUnitOfWork),
						typeof(UnallocatedBalancesJournalNode),
						typeof(int),
						typeof(IList<UnallocatedBalancesJournalNode>)
					};
					var ctorValues = new object[]{
						UoW,
						selected.OfType<UnallocatedBalancesJournalNode>().ToArray()[0],
						_closingDocumentDeliveryScheduleId,
						Items
					};

					ShowInfoMessage("Будет произведен разнос всех нераспределенных платежей по неоплаченным заказам, начиная с самого раннего");
					var page = NavigationManager.OpenViewModelTypedArgs<AutomaticallyAllocationBalanceWindowViewModel>(
						this, ctorTypes, ctorValues, OpenPageOptions.IgnoreHash);
					page.PageClosed += (sender, args) => Refresh();
				}
			);
			NodeActionsList.Add(automaticallyAllocationAction);
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();

			_canEdit = CurrentPermissionService.ValidateEntityPermission(typeof(Payment)).CanUpdate;

			var editAction = new JournalAction("Изменить",
				(selected) => _canEdit && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.OfType<UnallocatedBalancesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		protected override IQueryOver<Payment> ItemsQuery(IUnitOfWork uow)
		{
			UnallocatedBalancesJournalNode resultAlias = null;
			VodOrder orderAlias = null;
			VodOrder orderAlias2 = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			Counterparty counterpartyAlias = null;
			Organization organizationAlias = null;
			CounterpartyContract counterpartyContractAlias = null;
			Organization orderOrganizationAlias = null;
			CashlessMovementOperation cashlessMovementOperationAlias = null;

			var query = uow.Session.QueryOver<Payment>()
				.Inner.JoinAlias(cmo => cmo.Counterparty, () => counterpartyAlias)
				.Inner.JoinAlias(cmo => cmo.Organization, () => organizationAlias);

			var income = QueryOver.Of<CashlessMovementOperation>()
				.Where(cmo => cmo.Counterparty.Id == counterpartyAlias.Id)
				.And(cmo => cmo.Organization.Id == organizationAlias.Id)
				.And(cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Income));

			var expense = QueryOver.Of<CashlessMovementOperation>()
				.Where(cmo => cmo.Counterparty.Id == counterpartyAlias.Id)
				.And(cmo => cmo.Organization.Id == organizationAlias.Id)
				.And(cmo => cmo.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum<CashlessMovementOperation>(cmo => cmo.Expense));

			var balanceProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - ?2"),
					NHibernateUtil.Decimal,
						Projections.SubQuery(income),
						Projections.SubQuery(expense));

			var orderSumProjection = OrderProjections.GetOrderSumProjection();

			var totalNotPaidOrders = QueryOver.Of(() => orderAlias)
				.Inner.JoinAlias(o => o.OrderItems, () => orderItemAlias)
				.Inner.JoinAlias(o => o.Contract, () => counterpartyContractAlias)
				.Inner.JoinAlias(() => counterpartyContractAlias.Organization, () => orderOrganizationAlias)
				.Where(() => orderAlias.Client.Id == counterpartyAlias.Id)
				.And(() => orderOrganizationAlias.Id == organizationAlias.Id)
				.And(() => orderAlias.PaymentType == PaymentType.cashless)
				.And(() => orderAlias.OrderPaymentStatus != OrderPaymentStatus.Paid)
				.Select(orderSumProjection)
				.Where(Restrictions.Gt(orderSumProjection, 0));

			var totalPayPartiallyPaidOrders = QueryOver.Of(() => paymentItemAlias)
				.JoinEntityAlias(() => orderAlias2, () => paymentItemAlias.Order.Id == orderAlias2.Id, JoinType.InnerJoin)
				.Inner.JoinAlias(() => orderAlias2.Contract, () => counterpartyContractAlias)
				.Inner.JoinAlias(() => counterpartyContractAlias.Organization, () => orderOrganizationAlias)
				.Inner.JoinAlias(() => paymentItemAlias.CashlessMovementOperation, () => cashlessMovementOperationAlias)
				.Where(() => orderAlias2.Client.Id == counterpartyAlias.Id)
				.And(() => orderOrganizationAlias.Id == organizationAlias.Id)
				.And(() => cashlessMovementOperationAlias.CashlessMovementOperationStatus != AllocationStatus.Cancelled)
				.And(() => orderAlias2.PaymentType == PaymentType.cashless)
				.And(() => orderAlias2.OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid)
				.Select(Projections.Sum(() => cashlessMovementOperationAlias.Expense));

			var counterpartyDebtProjection = Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "?1 - IFNULL(?2, ?3)"),
				NHibernateUtil.Decimal,
					Projections.SubQuery(totalNotPaidOrders),
					Projections.SubQuery(totalPayPartiallyPaidOrders),
					Projections.Constant(0));

			query.Where(GetSearchCriterion(
				() => counterpartyAlias.Id,
				() => counterpartyAlias.Name,
				() => balanceProjection,
				() => counterpartyDebtProjection));

			#region filter

			if(_filterViewModel.Counterparty != null)
			{
				query.Where(() => counterpartyAlias.Id == _filterViewModel.Counterparty.Id);
			}

			if(_filterViewModel.Organization != null)
			{
				query.Where(() => organizationAlias.Id == _filterViewModel.Organization.Id);
			}

			#endregion

			return query.SelectList(list => list
				.SelectGroup(() => counterpartyAlias.Id).WithAlias(() => resultAlias.CounterpartyId)
				.SelectGroup(() => organizationAlias.Id).WithAlias(() => resultAlias.OrganizationId)
				.Select(p => counterpartyAlias.INN).WithAlias(() => resultAlias.CounterpartyINN)
				.Select(p => counterpartyAlias.Name).WithAlias(() => resultAlias.CounterpartyName)
				.Select(p => organizationAlias.Name).WithAlias(() => resultAlias.OrganizationName)
				.Select(balanceProjection).WithAlias(() => resultAlias.CounterpartyBalance)
				.Select(counterpartyDebtProjection).WithAlias(() => resultAlias.CounterpartyDebt))
				.Where(Restrictions.Gt(balanceProjection, 0))
				.And(Restrictions.Gt(counterpartyDebtProjection, 0))
				.OrderBy(balanceProjection).Desc
				.TransformUsing(Transformers.AliasToBean<UnallocatedBalancesJournalNode>())
				.SetTimeout(120);
		}

		protected override void CreateEntityDialog()
		{
			throw new InvalidOperationException("Что-то пошло не так... Нельзя открывать диалог создания из этого журнала");
		}

		protected override void EditEntityDialog(UnallocatedBalancesJournalNode node)
		{
			var filterParams = new Action<PaymentsJournalFilterViewModel>[]
			{
				f => f.Counterparty = UoW.GetById<Counterparty>(node.CounterpartyId),
				f => f.IsSortingDescByUnAllocatedSum = true,
				f => f.HideAllocatedPayments = true
			};

			NavigationManager.OpenViewModel<PaymentsJournalViewModel, Action<PaymentsJournalFilterViewModel>[]>(
				this, filterParams, OpenPageOptions.IgnoreHash);
		}
	}
}

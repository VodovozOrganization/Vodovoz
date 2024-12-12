﻿using Autofac;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.TempAdapters;
using VodOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ManualPaymentMatchingViewModel : EntityTabViewModelBase<Payment>
	{
		private const string _error = "Ошибка";
		private DateTime? _startDate = DateTime.Now.AddMonths(-1);
		private DateTime? _endDate = DateTime.Now.AddMonths(1);
		private IEnumerable<OrderStatus> _orderStatuses = EnumHelper.GetValuesList<OrderStatus>();
		private IEnumerable<OrderPaymentStatus> _orderPaymentStatuses = EnumHelper.GetValuesList<OrderPaymentStatus>();
		private ManualPaymentMatchingViewModelAllocatedNode _selectedAllocatedNode;
		private decimal _allocatedSum;
		private decimal _currentBalance;
		private decimal _counterpartyTotalDebt;
		private decimal _counterpartyClosingDocumentsOrdersDebt;
		private decimal _counterpartyWaitingForPaymentOrdersDebt;
		private decimal _counterpartyOtherOrdersDebt;
		private decimal _sumToAllocate;
		private decimal _lastBalance;

		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentItemsRepository _paymentItemsRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IDialogsFactory _dialogsFactory;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private DelegateCommand _revertAllocatedSum;

		public ManualPaymentMatchingViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IOrderRepository orderRepository,
			IPaymentItemsRepository paymentItemsRepository,
			IPaymentsRepository paymentsRepository,
			IDialogsFactory dialogsFactory,
			IOrganizationRepository organizationRepository,
			ICounterpartyJournalFactory counterpartyJournalFactory,
			IDeliveryScheduleSettings deliveryScheduleSettings) : base(uowBuilder, uowFactory, commonServices)
		{
			if(lifetimeScope == null)
			{
				throw new ArgumentNullException(nameof(lifetimeScope));
			}

			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_dialogsFactory = dialogsFactory ?? throw new ArgumentNullException(nameof(dialogsFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			CounterpartyAutocompleteSelectorFactory =
				(counterpartyJournalFactory ?? throw new ArgumentNullException(nameof(counterpartyJournalFactory)))
				.CreateCounterpartyAutocompleteSelectorFactory(lifetimeScope);

			if(uowBuilder.IsNewEntity)
			{
				throw new AbortCreatingPageException(
					"Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания",
					_error);
			}

			var curEditor = Entity.CurrentEditorUser;
			if(curEditor != null)
			{
				throw new AbortCreatingPageException(
					$"Невозможно открыть диалог ручного распределения платежа №{Entity.PaymentNum}," +
					$" т.к. он уже открыт пользователем: {curEditor.Name}",
					_error);
			}

			UpdateCurrentEditor();
			TabName = "Ручное распределение платежей";

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += (sender, args) => UpdateNodes();

			CanRevertPayFromOrderPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_revert_pay_from_order");

			GetLastBalance();
			UpdateSumToAllocate();
			UpdateCurrentBalance();
			CreateCommands();
			GetCounterpartyDebt();
			ConfigureEntityChangingRelations();
			UpdateNodes();

			if(HasPaymentItems)
			{
				UpdateAllocatedNodes();
			}

			TabClosed += OnTabClosed;

			Entity.ObservableItems.ElementRemoved += (_, _1, _2) => OnPropertyChanged(nameof(CanChangeCounterparty));

			Entity.ObservableItems.ElementAdded += (_, _1) => OnPropertyChanged(nameof(CanChangeCounterparty));
		}

		#region Свойства

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public IEnumerable<OrderStatus> OrderStatuses
		{
			get => _orderStatuses;
			set
			{
				SetField(ref _orderStatuses, value);
				UpdateNodes();
			}
		}

		public IEnumerable<OrderPaymentStatus> OrderPaymentStatuses
		{
			get => _orderPaymentStatuses;
			set
			{
				SetField(ref _orderPaymentStatuses, value);
				UpdateNodes();
			}
		}

		public decimal AllocatedSum
		{
			get => _allocatedSum;
			set => SetField(ref _allocatedSum, value);
		}

		public decimal CurrentBalance
		{
			get => _currentBalance;
			set => SetField(ref _currentBalance, value);
		}

		public decimal CounterpartyTotalDebt
		{
			get => _counterpartyTotalDebt;
			set => SetField(ref _counterpartyTotalDebt, value);
		}

		public decimal CounterpartyWaitingForPaymentOrdersDebt
		{
			get => _counterpartyWaitingForPaymentOrdersDebt;
			set => SetField(ref _counterpartyWaitingForPaymentOrdersDebt, value);
		}

		public decimal CounterpartyClosingDocumentsOrdersDebt
		{
			get => _counterpartyClosingDocumentsOrdersDebt;
			set => SetField(ref _counterpartyClosingDocumentsOrdersDebt, value);
		}

		public decimal CounterpartyOtherOrdersDebt
		{
			get => _counterpartyOtherOrdersDebt;
			set => SetField(ref _counterpartyOtherOrdersDebt, value);
		}

		public decimal SumToAllocate
		{
			get => _sumToAllocate;
			set => SetField(ref _sumToAllocate, value);
		}

		public decimal LastBalance
		{
			get => _lastBalance;
			set => SetField(ref _lastBalance, value);
		}

		public bool CanRevertPayFromOrderPermission { get; }

		public bool HasPaymentItems => Entity.PaymentItems.Any();
		public bool CounterpartyIsNull => Entity.Counterparty == null;

		public IJournalSearch Search { get; }

		public IList<ManualPaymentMatchingViewModelNode> ListNodes { get; } =
			new GenericObservableList<ManualPaymentMatchingViewModelNode>();

		public IList<ManualPaymentMatchingViewModelAllocatedNode> ListAllocatedNodes { get; } =
			new GenericObservableList<ManualPaymentMatchingViewModelAllocatedNode>();

		public ManualPaymentMatchingViewModelAllocatedNode SelectedAllocatedNode
		{
			get => _selectedAllocatedNode;
			set
			{
				if(SetField(ref _selectedAllocatedNode, value))
				{
					OnPropertyChanged(nameof(CanRevertPay));
				}
			}
		}

		public bool CanRevertPay =>
			SelectedAllocatedNode != null
			&& SelectedAllocatedNode.PaymentItemStatus != AllocationStatus.Cancelled
			&& CanRevertPayFromOrderPermission;

		#endregion

		public void GetLastBalance()
		{
			LastBalance = Entity.Counterparty != null
				? _paymentsRepository.GetCounterpartyLastBalance(UoW, Entity.Counterparty.Id, Entity.Organization.Id)
				: default(decimal);
		}

		public void UpdateSumToAllocate()
		{
			if(Entity.CashlessMovementOperation == null && !Entity.IsRefundPayment)
			{
				SumToAllocate = Entity.Total + LastBalance;
			}
			else
			{
				SumToAllocate = LastBalance;
			}
		}

		public void Calculate(ManualPaymentMatchingViewModelNode node)
		{
			if(CurrentBalance <= 0)
			{
				return;
			}

			var tempSum = AllocatedSum + node.ActualOrderSum - node.LastPayments - node.CurrentPayment;

			if(tempSum <= SumToAllocate)
			{
				AllocatedSum += node.ActualOrderSum - node.LastPayments - node.CurrentPayment;
				node.CurrentPayment = node.ActualOrderSum - node.LastPayments;
			}
			else
			{
				node.CurrentPayment += SumToAllocate - AllocatedSum;
				AllocatedSum = SumToAllocate;
			}

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		public void ReCalculate(ManualPaymentMatchingViewModelNode node)
		{
			if(node.CurrentPayment == 0)
			{
				return;
			}

			AllocatedSum -= node.CurrentPayment;

			node.CurrentPayment = 0;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = 0;

			UpdateCurrentBalance();
		}

		public void CurrentPaymentChangedByUser(ManualPaymentMatchingViewModelNode node)
		{
			if(node?.CurrentPayment == 0 && node?.OldCurrentPayment == 0)
			{
				return;
			}

			if(node.CurrentPayment < 0)
			{
				node.CurrentPayment = node.OldCurrentPayment;
				return;
			}

			var difference = node.ActualOrderSum - node.LastPayments;

			if(difference == 0)
			{
				node.CurrentPayment = difference;
				return;
			}

			if(node.CurrentPayment > difference)
			{
				node.CurrentPayment = difference;
			}

			AllocatedSum += node.CurrentPayment - node.OldCurrentPayment;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		private bool RevertPay()
		{
			var paymentItem = Entity.PaymentItems.SingleOrDefault(x => x.Id == SelectedAllocatedNode.PaymentItemId);

			if(paymentItem is null)
			{
				return false;
			}

			paymentItem.CancelAllocation(true);

			UoW.Save();

			if(Entity.PaymentItems.Any())
			{
				UpdateAllocatedNodes();
			}
			else
			{
				ListAllocatedNodes.Clear();
			}

			return true;
		}

		private void CreateCommands()
		{
			CreateOpenOrderCommand();
			CreateAddCounterpatyCommand();
			CreateCompleteAllocationCommand();
			CreateSaveViewModelCommand();
		}

		public void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

		private void UpdateCounterpartyDebt(ManualPaymentMatchingViewModelNode node)
		{
			var addedPaymentSum = node.CurrentPayment - node.OldCurrentPayment;

			CounterpartyTotalDebt -= addedPaymentSum;

			if(node.IsClosingDocumentsOrder)
			{
				CounterpartyClosingDocumentsOrdersDebt -= addedPaymentSum;
			}

			if(node.OrderStatus == OrderStatus.WaitForPayment)
			{
				CounterpartyWaitingForPaymentOrdersDebt -= addedPaymentSum;
			}

			if(!node.IsClosingDocumentsOrder && node.OrderStatus != OrderStatus.WaitForPayment)
			{
				CounterpartyOtherOrdersDebt -= addedPaymentSum;
			}
		}

		#region Commands
		public DelegateCommand SaveViewModelCommand { get; private set; }
		private void CreateSaveViewModelCommand()
		{
			SaveViewModelCommand = new DelegateCommand(
				CompleteAllocation,
				() => true
			);
		}

		public DelegateCommand<VodOrder> OpenOrderCommand { get; private set; }
		private void CreateOpenOrderCommand()
		{
			OpenOrderCommand = new DelegateCommand<VodOrder>(
				order =>
				{
					var dlg = _dialogsFactory.CreateReadOnlyOrderDlg(order.Id);
					TabParent.AddSlaveTab(this, dlg);
				},
				order => order != null
			);
		}

		public DelegateCommand AddCounterpatyCommand { get; private set; }
		private void CreateAddCounterpatyCommand()
		{
			AddCounterpatyCommand = new DelegateCommand(
				() =>
				{
					var parameters = new NewCounterpartyParameters
					{
						Name = Entity.CounterpartyName,
						FullName = Entity.CounterpartyName,
						INN = Entity.CounterpartyInn,
						KPP = Entity.CounterpartyKpp ?? string.Empty,
						PaymentMethod = PaymentType.Cashless,
						TypeOfOwnership = TryGetOrganizationType(Entity.CounterpartyName)
					};

					if(parameters.TypeOfOwnership != null)
					{
						parameters.PersonType = PersonType.legal;
					}
					else
					{
						parameters.PersonType =
							AskQuestion(
								$"Не удалось определить тип контрагента. Контрагент \"{Entity.CounterpartyName}\" является юридическим лицом?")
								? PersonType.legal
								: PersonType.natural;
					}

					parameters.CounterpartyBik = Entity.CounterpartyBik;
					parameters.CounterpartyBank = Entity.CounterpartyBank;
					parameters.CounterpartyCorrespondentAcc = Entity.CounterpartyCorrespondentAcc;
					parameters.CounterpartyCurrentAcc = Entity.CounterpartyCurrentAcc;

					var dlg = _dialogsFactory.CreateCounterpartyDlg(parameters);

					TabParent.AddSlaveTab(this, dlg);
					dlg.EntitySaved += NewCounterpartySaved;
				}
			);
		}

		public DelegateCommand CompleteAllocationCommand { get; private set; }
		private void CreateCompleteAllocationCommand()
		{
			CompleteAllocationCommand = new DelegateCommand(
				CompleteAllocation,
				() => true
			);
		}

		public DelegateCommand RevertAllocatedSum => _revertAllocatedSum ?? (_revertAllocatedSum = new DelegateCommand(
			() =>
			{
				if(RevertPay())
				{
					GetLastBalance();
					UpdateSumToAllocate();
					GetCounterpartyDebt();
					UpdateNodes();
				}
			},
			() => HasPaymentItems
			)
		);

		public IEntityAutocompleteSelectorFactory CounterpartyAutocompleteSelectorFactory { get; }

		public bool CanChangeCounterparty => !Entity.ObservableItems.Any(x => x.PaymentItemStatus == AllocationStatus.Accepted);

		#endregion Commands

		private void CompleteAllocation()
		{
			var valid = CommonServices.ValidationService.Validate(Entity);
			if(!valid)
			{
				return;
			}

			if(Entity.Status == PaymentState.Cancelled)
			{
				ShowWarningMessage($"Платеж находится в статусе {Entity.Status.GetEnumTitle()} распределения не возможны!");
				SaveAndCloseDialog();
				return;
			}

			if(CurrentBalance < 0)
			{
				ShowWarningMessage("Остаток не может быть отрицательным!");
				return;
			}

			if(CurrentBalance > 0)
			{
				if(!AskQuestion("Внимание! Имеется нераспределенный остаток. " +
								"Оставить его на балансе и завершить распределение?", "Внимание!"))
				{
					return;
				}
			}

			AllocateOrders();
			CreateOperations();

			var notCancelledItems = Entity.PaymentItems.Where(x => x.PaymentItemStatus != AllocationStatus.Cancelled);

			foreach(var item in notCancelledItems)
			{
				if(item.Order.OrderPaymentStatus == OrderPaymentStatus.Paid)
				{
					continue;
				}

				var otherPaymentsSum =
					_paymentItemsRepository.GetAllocatedSumForOrderWithoutCurrentPayment(UoW, item.Order.Id, Entity.Id);

				var totalSum = otherPaymentsSum + item.Sum;

				item.Order.OrderPaymentStatus = item.Order.OrderSum > totalSum
					? OrderPaymentStatus.PartiallyPaid
					: OrderPaymentStatus.Paid;
			}

			Entity.Status = PaymentState.completed;

			SaveAndCloseDialog();
		}

		private void SaveAndCloseDialog()
		{
			UpdateCurrentEditor();

			try
			{
				SaveAndClose();
			}
			catch
			{
				ShowErrorMessage("При сохранении платежа произошла ошибка. Переоткройте диалог.");
				UoW.Session.Clear();
				RemoveCurrentEditor(UoW);
				Close(false, CloseSource.Self);
			}
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				p => p.Counterparty,
				() => CounterpartyIsNull);
		}

		private void NewCounterpartySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			if(!(e.Entity is Domain.Client.Counterparty counterparty))
			{
				return;
			}

			var savedCounterparty = UoW.GetById<Domain.Client.Counterparty>(counterparty.Id);

			Entity.Counterparty = savedCounterparty;
			Entity.CounterpartyAccount = savedCounterparty.DefaultAccount;
		}

		private void CreateOperations()
		{
			Entity.CreateIncomeOperation();

			foreach(PaymentItem item in Entity.ObservableItems)
			{
				item.CreateOrUpdateExpenseOperation();
			}
		}

		private void AllocateOrders()
		{
			var list = ListNodes.Where(x => x.CurrentPayment > 0);

			foreach(var node in list)
			{
				var order = UoW.GetById<VodOrder>(node.Id);
				Entity.AddPaymentItem(order, node.CurrentPayment);
			}
		}

		public void ClearProperties()
		{
			AllocatedSum = default(decimal);
			CurrentBalance = SumToAllocate;
		}

		public void UpdateNodes()
		{
			ListNodes.Clear();
			ClearProperties();

			ManualPaymentMatchingViewModelNode resultAlias = null;
			VodOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			Domain.Organizations.Organization organisationAlias = null;
			CounterpartyContract contractAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => organisationAlias)
				.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(_orderRepository.GetUndeliveryStatuses())
				.And(o => o.PaymentType == PaymentType.Cashless)
				.And(() => organisationAlias.Id == Entity.Organization.Id);

			if(Entity.Counterparty != null)
			{
				incomePaymentQuery.Where(x => x.Client.Id == Entity.Counterparty.Id);
			}
			else
			{
				incomePaymentQuery.Where(x => x.Client.Id == -1);
			}

			if(StartDate.HasValue && EndDate.HasValue)
			{
				incomePaymentQuery.Where(x => x.DeliveryDate >= StartDate && x.DeliveryDate <= EndDate);
			}

			if(OrderStatuses != null)
			{
				incomePaymentQuery.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderStatus), OrderStatuses.ToArray()));
			}

			if(OrderPaymentStatuses != null)
			{
				incomePaymentQuery.Where(Restrictions.In(Projections.Property(() => orderAlias.OrderPaymentStatus), OrderPaymentStatuses.ToArray()));
			}

			var lastPayment = QueryOver.Of(() => paymentItemAlias)
				.Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
				.And(() => paymentItemAlias.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => paymentItemAlias.Sum));

			var orderSum = QueryOver.Of(() => orderItemAlias)
				.Where(x => x.Order.Id == orderAlias.Id)
				.Select(OrderProjections.GetOrderSumProjection());

			var isClosingDocumentsOrderProjection =
				Projections.Conditional(
					Restrictions.Eq(Projections.Property(() => orderAlias.DeliverySchedule.Id), _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId),
					Projections.Constant(true),
					Projections.Constant(false));

			incomePaymentQuery.Where(
				GetSearchCriterion(
					() => orderAlias.Id
				)
			);

			var resultQuery = incomePaymentQuery
				.SelectList(list => list
					.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
					.Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
					.SelectSubQuery(orderSum).WithAlias(() => resultAlias.ActualOrderSum)
					.SelectSubQuery(lastPayment).WithAlias(() => resultAlias.LastPayments)
					.Select(isClosingDocumentsOrderProjection).WithAlias(() => resultAlias.IsClosingDocumentsOrder))
				.TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingViewModelNode>())
				.List<ManualPaymentMatchingViewModelNode>();

			foreach(var item in resultQuery)
			{
				ListNodes.Add(item);
			}
		}

		private void UpdateAllocatedNodes()
		{
			ListAllocatedNodes.Clear();

			ManualPaymentMatchingViewModelAllocatedNode resultAlias = null;
			VodOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			PaymentItem paymentItemAlias2 = null;

			var query = UoW.Session.QueryOver(() => paymentItemAlias)
				.Inner.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
				.Where(() => paymentItemAlias.Payment.Id == Entity.Id);

			var allAllocatedSum = QueryOver.Of(() => paymentItemAlias2)
				.Where(() => paymentItemAlias2.Order.Id == orderAlias.Id)
				.And(() => paymentItemAlias2.PaymentItemStatus != AllocationStatus.Cancelled)
				.Select(Projections.Sum(() => paymentItemAlias2.Sum));

			var orderSum = QueryOver.Of(() => orderItemAlias)
				.Where(x => x.Order.Id == orderAlias.Id)
				.Select(OrderProjections.GetOrderSumProjection());

			var resultQuery = query
				.SelectList(list => list
					.SelectGroup(() => paymentItemAlias.Id).WithAlias(() => resultAlias.PaymentItemId)
					.Select(() => orderAlias.Id).WithAlias(() => resultAlias.OrderId)
					.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
					.Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
					.Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
					.Select(() => paymentItemAlias.PaymentItemStatus).WithAlias(() => resultAlias.PaymentItemStatus)
					.Select(Projections.Conditional(
						Restrictions.Eq(Projections.Property(() => paymentItemAlias.PaymentItemStatus), AllocationStatus.Accepted),
						Projections.Property(() => paymentItemAlias.Sum),
						Projections.Constant(0m))).WithAlias(() => resultAlias.AllocatedSum)
					.SelectSubQuery(orderSum).WithAlias(() => resultAlias.OrderSum)
					.SelectSubQuery(allAllocatedSum).WithAlias(() => resultAlias.AllAllocatedSum))
				.TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingViewModelAllocatedNode>())
				.List<ManualPaymentMatchingViewModelAllocatedNode>();

			foreach(var item in resultQuery)
			{
				ListAllocatedNodes.Add(item);
			}
		}

		public void GetCounterpartyDebt()
		{
			CounterpartyTotalDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyDebt(UoW, Entity.Counterparty.Id)
				: default;

			CounterpartyOtherOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyNotWaitingForPaymentAndNotClosingDocumentsOrdersDebt(UoW, Entity.Counterparty.Id, _deliveryScheduleSettings)
				: default;

			CounterpartyWaitingForPaymentOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyWaitingForPaymentOrdersDebt(UoW, Entity.Counterparty.Id)
				: default;

			CounterpartyClosingDocumentsOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyClosingDocumentsOrdersDebtAndNotWaitingForPayment(UoW, Entity.Counterparty.Id, _deliveryScheduleSettings)
				: default;
		}

		private string TryGetOrganizationType(string name)
		{
			var allOrganizationOwnershipTypes = _organizationRepository.GetAllOrganizationOwnershipTypes(UoW);
			foreach(var organizationType in allOrganizationOwnershipTypes)
			{
				string pattern = $@".*(^|\(|\s|\W|['""]){organizationType.Abbreviation}($|\)|\s|\W|['""]).*";
				string fullPattern = $@".*(^|\(|\s|\W|['""]){organizationType.FullName}($|\)|\s|\W|['""]).*";
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
				{
					return organizationType.Abbreviation;
				}

				regex = new Regex(fullPattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
				{
					return organizationType.Abbreviation;
				}
			}
			return null;
		}

		private void UpdateCurrentEditor()
		{
			if(Entity.CurrentEditorUser != null)
			{
				DeleteCurrentEditor();
			}
			else
			{
				Entity.CurrentEditorUser = CurrentUser;
				UoW.Save();
			}
		}

		private void DeleteCurrentEditor()
		{
			Entity.CurrentEditorUser = null;
		}

		private void OnTabClosed(object sender, EventArgs e)
		{
			if(Entity.CurrentEditorUser != null)
			{
				using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
				{
					RemoveCurrentEditor(uow);
				}
			}
		}

		private void RemoveCurrentEditor(IUnitOfWork uow)
		{
			var curPayment = uow.GetById<Payment>(Entity.Id);
			curPayment.CurrentEditorUser = null;
			uow.Save(curPayment);
			uow.Commit();
		}

		private ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			var searchCriterion = new SearchCriterion(Search);
			var result = searchCriterion.By(aliasPropertiesExpr).Finish();
			return result;
		}

		private ICriterion GetSearchCriterion<TRootEntity>(params Expression<Func<TRootEntity, object>>[] propertiesExpr)
		{
			var searchCriterion = new SearchCriterionGeneric<TRootEntity>(Search);
			var result = searchCriterion.By(propertiesExpr).Finish();
			return result;
		}

		public override bool Save(bool close)
		{
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return false;
			}

			return base.Save(close);
		}

		public override void Close(bool askSave, CloseSource source)
		{
			if(TabParent != null && TabParent.CheckClosingSlaveTabs(this))
			{
				return;
			}

			base.Close(askSave, source);
		}

		public void UpdateCMOCounterparty()
		{
			if(Entity.CashlessMovementOperation != null && Entity.Counterparty?.Id != Entity.CashlessMovementOperation.Counterparty?.Id)
			{
				Entity.CashlessMovementOperation.Counterparty = Entity.Counterparty;
			}
		}
	}

	public class ManualPaymentMatchingViewModelNode : JournalEntityNodeBase<VodOrder>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal ActualOrderSum { get; set; }
		public decimal LastPayments { get; set; }
		public decimal OldCurrentPayment { get; set; }
		public decimal CurrentPayment { get; set; }
		public bool Calculate { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		public bool IsClosingDocumentsOrder { get; set; }
	}

	public class ManualPaymentMatchingViewModelAllocatedNode
	{
		public int PaymentItemId { get; set; }
		public int OrderId { get; set; }
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal OrderSum { get; set; }
		public decimal AllocatedSum { get; set; }
		public decimal AllAllocatedSum { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
		public AllocationStatus PaymentItemStatus { get; set; }
	}
}

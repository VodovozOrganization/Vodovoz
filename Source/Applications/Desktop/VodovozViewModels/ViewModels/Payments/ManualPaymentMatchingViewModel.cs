using Autofac;
using Gamma.Utilities;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Services;
using QS.Utilities.Enums;
using QS.ViewModels;
using QS.ViewModels.Control.EEVM;
using ResourceLocker.Library;
using ResourceLocker.Library.Factories;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.Journals.JournalViewModels.Payments;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Services;
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
		private readonly IPaymentService _paymentService;
		private readonly IDialogsFactory _dialogsFactory;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;
		private readonly ViewModelEEVMBuilder<ProfitCategory> _profitCategoryEevmBuilder;
		private IResourceLocker _resourceLocker;

		public ManualPaymentMatchingViewModel(
			ILifetimeScope lifetimeScope,
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IOrderRepository orderRepository,
			IPaymentItemsRepository paymentItemsRepository,
			IPaymentsRepository paymentsRepository,
			IPaymentService paymentService,
			IDialogsFactory dialogsFactory,
			IOrganizationRepository organizationRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings,
			IResourceLockerFactory resourceLockerFactory,
			IUserRepository userRepository,
			ViewModelEEVMBuilder<ProfitCategory> profitCategoryEevmBuilder) : base(uowBuilder, uowFactory, commonServices)
		{
			if(resourceLockerFactory == null)
			{
				throw new ArgumentNullException(nameof(resourceLockerFactory));
			}

			if(userRepository == null)
			{
				throw new ArgumentNullException(nameof(userRepository));
			}

			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
			_dialogsFactory = dialogsFactory ?? throw new ArgumentNullException(nameof(dialogsFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));
			_profitCategoryEevmBuilder = profitCategoryEevmBuilder ?? throw new ArgumentNullException(nameof(profitCategoryEevmBuilder));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			if(uowBuilder.IsNewEntity)
			{
				throw new AbortCreatingPageException(
					"Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания",
					_error);
			}

			_resourceLocker = resourceLockerFactory.Create($"{nameof(ManualPaymentMatchingViewModel)}[Id:{Entity.Id}]");
			
			var lockResult = _resourceLocker.TryLockResourceAsync().GetAwaiter().GetResult();

			if(!lockResult.IsSuccess)
			{
				var ownerUser = userRepository.GetUserByLogin(UoW, lockResult.OwnerLockValue?.Split(':')[0]);

				throw new AbortCreatingPageException(
					$"Не удалось открыть выписку {Entity.Id}. Она уже открыта пользователем: {ownerUser?.Name}.",
					"Не удалось открыть диалог",
					ImportanceLevel.Warning);
			}
			
			TabName = "Ручное распределение платежей";

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += (sender, args) => UpdateNodes();

			CanRevertPayFromOrderPermission = CommonServices.CurrentPermissionService.ValidatePresetPermission("can_revert_pay_from_order");

			GetLastBalance();
			UpdateSumToAllocate();
			UpdateCurrentBalance();
			InitializeCommands();
			GetCounterpartyDebt();
			ConfigureEntityChangingRelations();
			InitializeViewModels();
			UpdateNodes();

			if(HasPaymentItems)
			{
				UpdateAllocatedNodes();
			}

			TabClosed += OnTabClosed;
			Entity.Items.CollectionChanged += OnPaymentItemsChanged;
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

		public bool HasPaymentItems => Entity.Items.Any();
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
			if(Entity.CashlessMovementOperation == null
				&& !Entity.IsRefundPayment
				&& Entity.Status != PaymentState.Cancelled)
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
			var paymentItem = Entity.Items.SingleOrDefault(x => x.Id == SelectedAllocatedNode.PaymentItemId);

			if(paymentItem is null)
			{
				return false;
			}

			_paymentService.CancelAllocationWithUpdateOrderPayments(UoW, paymentItem);

			UoW.Save();

			if(Entity.Items.Any())
			{
				UpdateAllocatedNodes();
			}
			else
			{
				ListAllocatedNodes.Clear();
			}

			return true;
		}

		private void InitializeCommands()
		{
			CreateOpenOrderCommand();
			CreateAddCounterpatyCommand();
			CreateCompleteAllocationCommand();
			CreateSaveViewModelCommand();

			RevertAllocatedSum = new DelegateCommand(RevertAllocationSum, () => HasPaymentItems);
			CloseCommand = new DelegateCommand(() => Close(false, CloseSource.Cancel));
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

		public ICommand RevertAllocatedSum { get; private set; }
		public ICommand CloseCommand { get; private set; }

		public bool CanChangeCounterparty => !Entity.Items.Any(x => x.PaymentItemStatus == AllocationStatus.Accepted);
		public ILifetimeScope LifetimeScope { get; private set; }
		public INavigationManager NavigationManager { get; }
		public IEntityEntryViewModel ProfitCategoryEntryViewModel { get; private set; }

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

			var notCancelledItems = Entity.Items.Where(x => x.PaymentItemStatus != AllocationStatus.Cancelled);

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
			ReleaseLock();

			try
			{
				SaveAndClose();
			}
			catch
			{
				ShowErrorMessage("При сохранении платежа произошла ошибка. Переоткройте диалог.");
				UoW.Session.Clear();
				Close(false, CloseSource.Self);
			}
		}

		private void ReleaseLock()
		{
			_resourceLocker.DisposeAsync().AsTask().GetAwaiter().GetResult();
		}

		private void ConfigureEntityChangingRelations()
		{
			SetPropertyChangeRelation(
				p => p.Counterparty,
				() => CounterpartyIsNull);
		}

		private void RevertAllocationSum()
		{
			if(!RevertPay())
			{
				return;
			}

			GetLastBalance();
			UpdateSumToAllocate();
			GetCounterpartyDebt();
			UpdateNodes();
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

			foreach(PaymentItem item in Entity.Items)
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
			OrderDocument orderDocumentAlias = null;
			DocumentOrganizationCounter documentOrganizationCounterAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => organisationAlias)
				.Left.JoinAlias(
					o => o.OrderDocuments,
					() => orderDocumentAlias,
					Restrictions.And(
						Restrictions.Or(
							Restrictions.Where(() => orderDocumentAlias.GetType() == typeof(UPDDocument)),
							Restrictions.Where(() => orderDocumentAlias.GetType() == typeof(SpecialUPDDocument))
							),
							Restrictions.Where(() => orderDocumentAlias.Order.Id == orderAlias.Id)))
				.Left.JoinAlias(
					() => orderDocumentAlias.DocumentOrganizationCounter,
					() => documentOrganizationCounterAlias
				)
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
					.Select(() => documentOrganizationCounterAlias.DocumentNumber).WithAlias(() => resultAlias.UpdDocumentName)
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
				? _orderRepository.GetCounterpartyDebt(UoW, Entity.Counterparty.Id, Entity.Organization.Id)
				: default;

			CounterpartyOtherOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyNotWaitingForPaymentAndNotClosingDocumentsOrdersDebt(
					UoW, Entity.Counterparty.Id, _deliveryScheduleSettings, Entity.Organization.Id)
				: default;

			CounterpartyWaitingForPaymentOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyWaitingForPaymentOrdersDebt(UoW, Entity.Counterparty.Id, Entity.Organization.Id)
				: default;

			CounterpartyClosingDocumentsOrdersDebt = Entity.Counterparty != null
				? _orderRepository.GetCounterpartyClosingDocumentsOrdersDebtAndNotWaitingForPayment(
					UoW, Entity.Counterparty.Id, _deliveryScheduleSettings, Entity.Organization.Id)
				: default;
		}
		
		private void InitializeViewModels()
		{
			var profitCategoryEntryViewModel = _profitCategoryEevmBuilder
				.SetUnitOfWork(UoW)
				.SetViewModel(this)
				.ForProperty(Entity, x => x.ProfitCategory)
				.UseViewModelJournalAndAutocompleter<ProfitCategoriesJournalViewModel>()
				.UseViewModelDialog<ProfitCategoryViewModel>()
				.Finish();

			profitCategoryEntryViewModel.IsEditable = Entity.Status == PaymentState.undistributed;
			profitCategoryEntryViewModel.CanViewEntity = false;
			ProfitCategoryEntryViewModel = profitCategoryEntryViewModel;
		}
		
		private void OnPaymentItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add
				|| e.Action == NotifyCollectionChangedAction.Remove)
			{
				OnPropertyChanged(nameof(CanChangeCounterparty));
			}
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

		private void OnTabClosed(object sender, EventArgs e)
		{
			ReleaseLock();
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

		public override void Dispose()
		{
			LifetimeScope = null;
			Entity.Items.CollectionChanged -= OnPaymentItemsChanged;
			base.Dispose();
		}
	}

	public class ManualPaymentMatchingViewModelNode : JournalEntityNodeBase<VodOrder>
	{
		public override string Title => $"{EntityType.GetSubjectNames()} №{Id}";
		public string UpdDocumentName { get; set; }
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

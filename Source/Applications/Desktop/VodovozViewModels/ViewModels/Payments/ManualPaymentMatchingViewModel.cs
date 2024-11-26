using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Autofac;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.Search;
using QS.Project.Search;
using QS.Services;
using QS.Tdi;
using QS.Utilities.Enums;
using QS.Validation;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Payments;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.Settings.Delivery;
using Vodovoz.ViewModels.TempAdapters;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class ManualPaymentMatchingViewModel : WidgetViewModelBase
	{
		private readonly IInteractiveService _interactiveService;
		private readonly IValidator _validator;
		private decimal _counterpartyTotalDebt;
		private decimal _counterpartyClosingDocumentsOrdersDebt;
		private decimal _counterpartyWaitingForPaymentOrdersDebt;
		private decimal _counterpartyOtherOrdersDebt;
		private decimal _allocatedSum;
		private decimal _currentBalance;
		private decimal _sumToAllocate;
		private decimal _lastBalance;

		private DateTime? _startDate = DateTime.Now.AddMonths(-1);
		private DateTime? _endDate = DateTime.Now.AddMonths(1);
		private IEnumerable<OrderStatus> _orderStatuses = EnumHelper.GetValuesList<OrderStatus>();
		private IEnumerable<OrderPaymentStatus> _orderPaymentStatuses = EnumHelper.GetValuesList<OrderPaymentStatus>();
		private ManualPaymentMatchingViewModelAllocatedNode _selectedAllocatedNode;
		private ManualPaymentMatchingAllocatingNode _selectedAllocatingNode;

		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentItemsRepository _paymentItemsRepository;
		private readonly IPaymentsRepository _paymentsRepository;
		private readonly IDialogsFactory _dialogsFactory;
		private readonly IOrganizationRepository _organizationRepository;
		private readonly IDeliveryScheduleSettings _deliveryScheduleSettings;

		public ManualPaymentMatchingViewModel(
			IInteractiveService interactiveService,
			ICurrentPermissionService currentPermissionService,
			IValidator validator,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IOrderRepository orderRepository,
			IPaymentItemsRepository paymentItemsRepository,
			IPaymentsRepository paymentsRepository,
			IDialogsFactory dialogsFactory,
			IOrganizationRepository organizationRepository,
			IDeliveryScheduleSettings deliveryScheduleSettings)
		{
			if(currentPermissionService == null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentItemsRepository = paymentItemsRepository ?? throw new ArgumentNullException(nameof(paymentItemsRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));
			_dialogsFactory = dialogsFactory ?? throw new ArgumentNullException(nameof(dialogsFactory));
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_deliveryScheduleSettings = deliveryScheduleSettings ?? throw new ArgumentNullException(nameof(deliveryScheduleSettings));

			//Поиск
			Search = new SearchViewModel();
			Search.OnSearch += FilterOrders;

			CanRevertPayFromOrderPermission = currentPermissionService.ValidatePresetPermission("can_revert_pay_from_order");
		}

		public void Initialize(Payment payment, IUnitOfWork uow, ITdiTab parentTab)
		{
			UoW = uow;
			Entity = payment;
			ParentTab = parentTab;

			//4914 Feature
			InitializeCommands();
			ReloadData();
			//ConfigureEntityChangingRelations();
			
			if(HasPaymentItems)
			{
				UpdateAllocatedNodes();
			}
		}

		private void ReloadData()
		{
			GetLastBalance();
			UpdateSumToAllocate();
			UpdateCurrentBalance();
			GetCounterpartyDebt();
			UpdateNodes();
		}

		public Payment Entity { get; private set; }
		
		public INavigationManager NavigationManager { get; }
		public ILifetimeScope LifetimeScope { get; private set; }
		public IUnitOfWork UoW { get; private set; }
		public ITdiTab ParentTab { get; private set; }

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

		public Domain.Client.Counterparty Counterparty
		{
			get => Entity.Counterparty;
			set
			{
				//UpdateCMOCounterparty();
				Entity.Counterparty = value;
				ReloadData();
			}
		}

		#region Фильтры

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

		#endregion
		
		public bool CanRevertPayFromOrderPermission { get; }
		public bool HasPaymentItems => Entity.PaymentItems.Any();
		public bool CounterpartyIsNull => Entity.Counterparty == null;
		
		public IJournalSearch Search { get; }

		public IObservableList<ManualPaymentMatchingAllocatingNode> ListNodes { get; } =
			new ObservableList<ManualPaymentMatchingAllocatingNode>();

		public IObservableList<ManualPaymentMatchingViewModelAllocatedNode> ListAllocatedNodes { get; } =
			new ObservableList<ManualPaymentMatchingViewModelAllocatedNode>();

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
		
		public ManualPaymentMatchingAllocatingNode SelectedAllocatingNode
		{
			get => _selectedAllocatingNode;
			set
			{
				if(SetField(ref _selectedAllocatingNode, value))
				{
					//OnPropertyChanged(nameof(CanRevertPay));
				}
			}
		}
		
		public bool CanRevertPay =>
			SelectedAllocatedNode != null
			&& SelectedAllocatedNode.PaymentItemStatus != AllocationStatus.Cancelled
			&& CanRevertPayFromOrderPermission;
		
		public ICommand OpenOrderCommand { get; private set; }
		public ICommand AddCounterpartyCommand { get; private set; }
		public ICommand RevertAllocatedSumCommand { get; private set; }
		
		public void GetLastBalance()
		{
			LastBalance = Entity.Counterparty != null
				? _paymentsRepository.GetCounterpartyLastBalance(UoW, Entity.Counterparty.Id, Entity.OrganizationId)
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
		
		public void TryCalculate()
		{
			if(SelectedAllocatingNode is null)
			{
				return;
			}

			if(SelectedAllocatingNode.Calculate)
			{
				Calculate(SelectedAllocatingNode);
			}
			else
			{
				ReCalculate(SelectedAllocatingNode);
			}
		}

		public void Calculate(ManualPaymentMatchingAllocatingNode node)
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

		public void ReCalculate(ManualPaymentMatchingAllocatingNode node)
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

		public void CurrentPaymentChangedByUser(ManualPaymentMatchingAllocatingNode node)
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

		public void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

		private void UpdateCounterpartyDebt(ManualPaymentMatchingAllocatingNode node)
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
		
		public void UpdateNodes()
		{
			ListNodes.Clear();
			ResetAllocation();

			ManualPaymentMatchingAllocatingNode resultAlias = null;
			Order orderAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;
			Domain.Organizations.Organization organisationAlias = null;
			CounterpartyContract contractAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => orderAlias)
				.Left.JoinAlias(o => o.Contract, () => contractAlias)
				.Left.JoinAlias(() => contractAlias.Organization, () => organisationAlias)
				.WhereRestrictionOn(o => o.OrderStatus).Not.IsIn(_orderRepository.GetUndeliveryStatuses())
				.And(o => o.PaymentType == PaymentType.Cashless)
				.And(() => organisationAlias.Id == Entity.OrganizationId);

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
				.TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingAllocatingNode>())
				.List<ManualPaymentMatchingAllocatingNode>();

			foreach(var item in resultQuery)
			{
				ListNodes.Add(item);
			}
		}
		
		private void UpdateAllocatedNodes()
		{
			ListAllocatedNodes.Clear();

			ManualPaymentMatchingViewModelAllocatedNode resultAlias = null;
			Order orderAlias = null;
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
		
		public void ResetAllocation()
		{
			AllocatedSum = default(decimal);
			CurrentBalance = SumToAllocate;
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
		
		public bool CanChangeCounterparty => Entity.ObservableItems.All(x => x.PaymentItemStatus != AllocationStatus.Accepted);
		
		private void RevertAllocatedSum()
		{
			if(RevertPay())
			{
				GetLastBalance();
				UpdateSumToAllocate();
				GetCounterpartyDebt();
				UpdateNodes();
			}
		}

		private void InitializeCommands()
		{
			OpenOrderCommand = new DelegateCommand(
				OpenOrderDlgAsReadOnly,
				() => SelectedAllocatedNode?.OrderId > 0
			);
			
			AddCounterpartyCommand = new DelegateCommand(CreateCounterpartyFromPayer);

			RevertAllocatedSumCommand = new DelegateCommand(
				RevertAllocatedSum,
				() => HasPaymentItems);
		}

		private void CreateCounterpartyFromPayer()
		{
			var income = Entity.CashlessIncome;
			
			var parameters = new NewCounterpartyParameters
			{
				Name = income.PayerName,
				FullName = income.PayerName,
				INN = income.PayerInn,
				KPP = income.PayerKpp ?? string.Empty,
				PaymentMethod = PaymentType.Cashless,
				TypeOfOwnership = TryGetOrganizationType(income.PayerName)
			};

			if(parameters.TypeOfOwnership != null)
			{
				parameters.PersonType = PersonType.legal;
			}
			else
			{
				parameters.PersonType =
					_interactiveService.Question(
						$"Не удалось определить тип контрагента. Контрагент \"{income.PayerName}\" является юридическим лицом?")
						? PersonType.legal
						: PersonType.natural;
			}

			parameters.CounterpartyBik = income.PayerBankBik;
			parameters.CounterpartyBank = income.PayerBank;
			parameters.CounterpartyCorrespondentAcc = income.PayerCorrespondentAcc;
			parameters.CounterpartyCurrentAcc = income.PayerCurrentAcc;

			var dlg = _dialogsFactory.CreateCounterpartyDlg(parameters);

			//4914 Feature
			//TabParent.AddSlaveTab(this, dlg);
			dlg.EntitySaved += NewCounterpartySaved;
		}

		//4914 Feature
		private void OpenOrderDlgAsReadOnly()
		{
			var dlg = _dialogsFactory.CreateReadOnlyOrderDlg(SelectedAllocatedNode.OrderId);
			//TabParent.AddSlaveTab(this, dlg);
		}

		private void FilterOrders(object sender, EventArgs e)
		{
			UpdateNodes();
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
		
		private ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr)
		{
			var searchCriterion = new SearchCriterion(Search);
			var result = searchCriterion.By(aliasPropertiesExpr).Finish();
			return result;
		}

		public void Dispose()
		{
			Search.OnSearch -= FilterOrders;
		}
	}
}

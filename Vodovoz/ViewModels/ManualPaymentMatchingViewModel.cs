using System;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using VodOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Client;
using NHibernate.Transform;
using QS.Project.Journal;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QS.Project.Domain;
using NHibernate.Criterion;
using Vodovoz.Domain.Orders;
using NHibernate.Dialect.Function;
using NHibernate;
using QS.Commands;
using System.Linq;
using QS.Validation;
using QS.Banks.Domain;
using QSBanks.Repositories;
using QSProjectsLib;
using System.Text.RegularExpressions;
using QS.Project.Search;
using QS.Project.Journal.Search;
using System.Linq.Expressions;
using QS.Navigation;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;

namespace Vodovoz.ViewModels
{
	public class ManualPaymentMatchingViewModel : EntityTabViewModelBase<Payment>
	{
		private DateTime? _startDate = DateTime.Now.AddMonths(-1);
		private DateTime? _endDate = DateTime.Now;
		private OrderStatus? _orderStatus;
		private OrderPaymentStatus? _orderPaymentStatus;
		private decimal _allocatedSum;
		private decimal _currentBalance;
		private decimal _previousCounterpartyDebt;
		private decimal _counterpartyDebt;
		private decimal _sumToAllocate;
		private decimal _lastBalance;
		private bool _hasPaymentItems;

		private readonly SearchHelper _searchHelper;
		private readonly IOrderRepository _orderRepository;
		private readonly IPaymentsRepository _paymentsRepository;

		private DelegateCommand _revertAllocatedSum = null;

		public ManualPaymentMatchingViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices,
			IOrderRepository orderRepository,
			IPaymentsRepository paymentsRepository) : base(uowBuilder, uowFactory, commonServices)
		{
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_paymentsRepository = paymentsRepository ?? throw new ArgumentNullException(nameof(paymentsRepository));

			if(uowBuilder.IsNewEntity)
			{
				AbortOpening("Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания");
			}

			TabName = "Ручное распределение платежей";

			//Поиск
			Search = new SearchViewModel();
			_searchHelper = new SearchHelper(Search);
			Search.OnSearch += (sender, args) => UpdateNodes();

			CanRevertPayFromOrder = CommonServices.PermissionService.ValidateUserPresetPermission("can_revert_pay_from_order", CurrentUser.Id);

			GetLastBalance();
			FillSumToAllocate();
			CurrentBalance = SumToAllocate - AllocatedSum;
			CreateCommands();

			GetCounterpatyDebt();

			HasPaymentItems = Entity.PaymentItems.Any();
			Entity.ObservableItems.ElementRemoved +=
				(list, idx, aObject) => HasPaymentItems = Entity.PaymentItems.Any();

			if(HasPaymentItems)
			{
				UpdateAllocatedNodes();
			}
		}

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

		public OrderStatus? OrderStatusVM
		{
			get => _orderStatus;
			set => SetField(ref _orderStatus, value);
		}

		public OrderPaymentStatus? OrderPaymentStatusVM
		{
			get => _orderPaymentStatus;
			set => SetField(ref _orderPaymentStatus, value);
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

		public decimal CounterpartyDebt
		{
			get => _counterpartyDebt;
			set => SetField(ref _counterpartyDebt, value);
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

		public bool CanRevertPayFromOrder { get; }

		public bool HasPaymentItems
		{
			get => _hasPaymentItems;
			set => SetField(ref _hasPaymentItems, value);
		}

		public IJournalSearch Search { get; set; }

		public IList<ManualPaymentMatchingViewModelNode> ListNodes { get; set; } =
			new GenericObservableList<ManualPaymentMatchingViewModelNode>();

		public IList<ManualPaymentMatchingViewModelAllocatedNode> ListAllocatedNodes { get; } =
			new GenericObservableList<ManualPaymentMatchingViewModelAllocatedNode>();

		public void GetLastBalance()
		{
			if(Entity.Counterparty != null)
			{
				LastBalance = _paymentsRepository.GetCounterpartyLastBalance(UoW, Entity.Counterparty.Id);
			}
		}

		private void FillSumToAllocate()
		{
			if(Entity.Status == PaymentState.undistributed)
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

		public void TreeViewAllocatedSumChangedByUser(ManualPaymentMatchingViewModelAllocatedNode node)
		{

			if(node == null)
			{
				return;
			}

			if(node.AllocatedSum > node.LastPayments)
			{
				node.AllocatedSum = node.LastPayments;
			}

			if(node.AllocatedSum < 0)
			{
				node.AllocatedSum = default(decimal);
			}
		}

		private bool RevertPay()
		{
			var list = ListAllocatedNodes.Where(x => x.AllocatedSum != x.LastPayments)
										 .ToList();

			if(list.Any())
			{
				foreach(var node in list)
				{
					Entity.UpdateAllocatedSum(UoW, node.Id, node.AllocatedSum);
					var order = UoW.GetById<VodOrder>(node.Id);

					if(order.OrderPaymentStatus != OrderPaymentStatus.UnPaid)
					{
						order.OrderPaymentStatus = node.AllocatedSum == 0
							? OrderPaymentStatus.UnPaid
							: OrderPaymentStatus.PartiallyPaid;
						UoW.Save(order);
					}
				}

				if(Entity.CashlessMovementOperation != null)
				{
					Entity.UpdateIncomeOperation(false);
					UoW.Save(Entity.CashlessMovementOperation);
				}

				if(Entity.Status != PaymentState.undistributed)
				{
					Entity.Status = PaymentState.undistributed;
				}

				UoW.Save();

				if(HasPaymentItems)
				{
					UpdateAllocatedNodes();
				}
				else
				{
					ListAllocatedNodes.Clear();
				}

				return true;
			}

			return false;
		}

		private void CreateCommands()
		{
			CreateOpenOrderCommand();
			CreateAddCounterpatyCommand();
			CreateCompleteAllocation();
			CreateSaveViewModelCommand();
			CreateCloseViewModelCommand();
		}

		private void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

		private void UpdateCounterpartyDebt(ManualPaymentMatchingViewModelNode node)
		{
			if(CounterpartyDebt > 0)
			{
				CounterpartyDebt -= node.CurrentPayment - node.OldCurrentPayment;

				if(CounterpartyDebt <= 0)
				{
					_previousCounterpartyDebt = CounterpartyDebt;

					if(CounterpartyDebt != 0)
					{
						CounterpartyDebt = 0;
					}
				}
			}
			else
			{
				_previousCounterpartyDebt -= node.CurrentPayment - node.OldCurrentPayment;
				CounterpartyDebt = _previousCounterpartyDebt > 0 ? _previousCounterpartyDebt : 0;
			}
		}

		#region Commands
		public DelegateCommand SaveViewModelCommand { get; private set; }
		private void CreateSaveViewModelCommand()
		{
			SaveViewModelCommand = new DelegateCommand(
				() =>
				{
					if(Entity.Status != PaymentState.undistributed)
					{
						ShowWarningMessage("Невозможно распределять платежи не в статусе 'Нераспределен'");
						return;
					}

					if(CurrentBalance < 0)
					{
						ShowWarningMessage("Остаток не может быть отрицательным!");
						return;
					}

					AllocateOrders();
					SaveAndClose();
				},
				() => true
			);
		}

		public DelegateCommand CloseViewModelCommand { get; private set; }
		private void CreateCloseViewModelCommand()
		{
			CloseViewModelCommand = new DelegateCommand(
				() =>
				{
					Close(true, CloseSource.Cancel);
				},
				() => true
			);
		}

		public DelegateCommand<VodOrder> OpenOrderCommand { get; private set; }
		private void CreateOpenOrderCommand()
		{
			OpenOrderCommand = new DelegateCommand<VodOrder>(
				order =>
				{
					var dlg = new OrderDlg(order);
					dlg.HasChanges = false;
					dlg.SetDlgToReadOnly();
					TabParent.AddSlaveTab(this, dlg);
				},
				order => order != null
			);
		}

		public DelegateCommand<Payment> AddCounterpatyCommand { get; private set; }
		private void CreateAddCounterpatyCommand()
		{
			AddCounterpatyCommand = new DelegateCommand<Payment>(
				payment =>
				{

					var client = new Counterparty();
					client.Name = payment.CounterpartyName;
					client.FullName = payment.CounterpartyName;
					client.INN = payment.CounterpartyInn;
					client.KPP = payment.CounterpartyKpp ?? string.Empty;
					client.PaymentMethod = PaymentType.cashless;
					client.TypeOfOwnership = TryGetOrganizationType(payment.CounterpartyName);

					if(client.TypeOfOwnership != null)
					{
						client.PersonType = PersonType.legal;
					}
					else
					{
						if(AskQuestion($"Не удалось определить тип контрагента. Контрагент \"{payment.CounterpartyName}\" является юридическим лицом?"))
						{
							client.PersonType = PersonType.legal;
						}
						else
						{
							client.PersonType = PersonType.natural;
						}
					}

					Bank bank = FillBank(payment);

					client.AddAccount(new Account
					{
						Number = payment.CounterpartyCurrentAcc,
						InBank = bank
					});

					UoW.Save(client);

					var dlg = new CounterpartyDlg(EntityUoWBuilder.ForOpenInChildUoW(client.Id, UoW), UnitOfWorkFactory);
					TabParent.AddSlaveTab(this, dlg);
					dlg.EntitySaved += NewCounterpartySaved;
				},
				payment => payment.Counterparty == null
			);
		}

		public DelegateCommand CompleteAllocation { get; private set; }
		private void CreateCompleteAllocation()
		{
			CompleteAllocation = new DelegateCommand(
				() =>
				{
					var valid = new QSValidator<Payment>(UoWGeneric.Root);
					if(valid.RunDlgIfNotValid())
					{
						return;
					}

					if(CurrentBalance < 0)
					{
						ShowWarningMessage("Остаток не может быть отрицательным!");
						return;
					}

					if(Entity.Status != PaymentState.undistributed)
					{
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

					foreach(var item in Entity.PaymentItems)
					{
						if(item.Order.OrderPaymentStatus == OrderPaymentStatus.Paid)
						{
							continue;
						}

						var otherPaymentsSum = _orderRepository.GetPaymentItemsForOrder(UoW, item.Order.Id)
													  .Sum(x => x.Sum);

						var totalSum = otherPaymentsSum + item.Sum;

						item.Order.OrderPaymentStatus = item.Order.ActualTotalSum > totalSum
							? OrderPaymentStatus.PartiallyPaid
							: OrderPaymentStatus.Paid;

						UoW.Save(item.Order);
					}

					Entity.Status = PaymentState.completed;
					SaveAndClose();
				},
				() => true
			);
		}

		public DelegateCommand RevertAllocatedSum => _revertAllocatedSum ?? (_revertAllocatedSum = new DelegateCommand(
			() =>
			{
				if(RevertPay())
				{
					GetLastBalance();
					FillSumToAllocate();
					GetCounterpatyDebt();
					UpdateNodes();
				}
			},
			() => HasPaymentItems
			)
		);

		#endregion Commands

		private Bank FillBank(Payment payment)
		{
			var bank = BankRepository.GetBankByBik(UoW, payment.CounterpartyBik);

			if(bank == null)
			{

				bank = new Bank
				{
					Bik = payment.CounterpartyBik,
					Name = payment.CounterpartyBank
				};
				var corAcc = new CorAccount { CorAccountNumber = payment.CounterpartyCorrespondentAcc };
				bank.CorAccounts.Add(corAcc);
				bank.DefaultCorAccount = corAcc;
				UoW.Save(bank);
			}

			return bank;
		}

		private void NewCounterpartySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			var client = e.Entity as Counterparty;

			Entity.Counterparty = client;
			Entity.CounterpartyAccount = client.DefaultAccount;
		}

		private void CreateOperations()
		{
			Entity.UpdateIncomeOperation(true);
			UoW.Save(Entity.CashlessMovementOperation);

			foreach(PaymentItem item in Entity.ObservableItems)
			{
				item.CreateExpenseOperation();
				UoW.Save(item.CashlessMovementOperation);
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
					.Left.JoinAlias(x => x.Contract, () => contractAlias)
					.Left.JoinAlias(() => contractAlias.Organization, () => organisationAlias)
					.Where(x => x.OrderStatus != OrderStatus.Canceled)
					.And(x => x.OrderStatus != OrderStatus.DeliveryCanceled)
					.And(x => x.OrderStatus != OrderStatus.NotDelivered)
					.And(x => x.PaymentType == PaymentType.cashless)
					.And(() => organisationAlias.Id == Entity.Organization.Id);

			if(Entity.Counterparty != null)
			{
				incomePaymentQuery.Where(x => x.Client == Entity.Counterparty);
			}

			if(StartDate.HasValue && EndDate.HasValue)
			{
				incomePaymentQuery.Where(x => x.DeliveryDate >= StartDate && x.DeliveryDate <= EndDate);
			}

			if(OrderStatusVM != null)
			{
				incomePaymentQuery.Where(x => x.OrderStatus == OrderStatusVM);
			}

			if(OrderPaymentStatusVM != null)
			{
				incomePaymentQuery.Where(x => x.OrderPaymentStatus == OrderPaymentStatusVM);
			}

			var lastPayment = QueryOver.Of(() => paymentItemAlias)
					.Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
					.Select(Projections.Sum(() => paymentItemAlias.Sum));

			var totalSum = QueryOver.Of(() => orderItemAlias)
					.Where(x => x.Order.Id == orderAlias.Id)
					.Select(
						Projections.Sum(
							Projections.SqlFunction(
								new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3) - ?4, 2)"),
									NHibernateUtil.Decimal, new IProjection[] {
									Projections.Property(() => orderItemAlias.Price),
									Projections.Property(() => orderItemAlias.ActualCount),
									Projections.Property(() => orderItemAlias.Count),
									Projections.Property(() => orderItemAlias.DiscountMoney)
								}
							)
						)
					);

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
					.SelectSubQuery(totalSum).WithAlias(() => resultAlias.ActualOrderSum)
					.SelectSubQuery(lastPayment).WithAlias(() => resultAlias.LastPayments)
				)
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
			Payment paymentAlias = null;
			PaymentItem paymentItemAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => paymentAlias)
										.Left.JoinAlias(() => paymentAlias.PaymentItems, () => paymentItemAlias)
										.Left.JoinAlias(() => paymentItemAlias.Order, () => orderAlias)
										.Where(() => paymentAlias.Id == Entity.Id);

			var totalSum = QueryOver.Of(() => orderItemAlias)
									.Where(x => x.Order.Id == orderAlias.Id)
									.Select(
										Projections.Sum(
											Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "ROUND(?1 * IFNULL(?2, ?3) - ?4, 2)"),
												NHibernateUtil.Decimal, new IProjection[] {
													Projections.Property(() => orderItemAlias.Price),
													Projections.Property(() => orderItemAlias.ActualCount),
													Projections.Property(() => orderItemAlias.Count),
													Projections.Property(() => orderItemAlias.DiscountMoney)})
										)
									);

			var resultQuery = incomePaymentQuery
							  .SelectList(list => list
												  .SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
												  .Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
												  .Select(() => orderAlias.OrderPaymentStatus).WithAlias(() => resultAlias.OrderPaymentStatus)
												  .Select(() => orderAlias.DeliveryDate).WithAlias(() => resultAlias.OrderDate)
												  .Select(() => paymentItemAlias.Sum).WithAlias(() => resultAlias.AllocatedSum)
												  .Select(() => paymentItemAlias.Sum).WithAlias(() => resultAlias.LastPayments)
												  .SelectSubQuery(totalSum).WithAlias(() => resultAlias.ActualOrderSum)
							  )
							  .TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingViewModelAllocatedNode>())
							  .List<ManualPaymentMatchingViewModelAllocatedNode>();

			foreach(var item in resultQuery)
			{
				ListAllocatedNodes.Add(item);
			}
		}

		public void GetCounterpatyDebt()
		{
			if(Entity.Counterparty != null)
			{
				CounterpartyDebt = _orderRepository.GetCounterpartyDebt(UoW, Entity.Counterparty.Id);
			}
		}

		private string TryGetOrganizationType(string name)
		{
			foreach(var pair in InformationHandbook.OrganizationTypes)
			{
				string pattern = string.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
				string fullPattern = string.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
				{
					return pair.Key;
				}

				regex = new Regex(fullPattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
				{
					return pair.Key;
				}
			}
			return null;
		}

		private ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) =>
			_searchHelper.GetSearchCriterion(aliasPropertiesExpr);

		private ICriterion GetSearchCriterion<TRootEntity>(params Expression<Func<TRootEntity, object>>[] propertiesExpr) =>
			_searchHelper.GetSearchCriterion(propertiesExpr);

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
	}

	public class ManualPaymentMatchingViewModelNode : JournalEntityNodeBase<VodOrder>
	{
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal ActualOrderSum { get; set; }
		public decimal LastPayments { get; set; }
		public decimal OldCurrentPayment { get; set; }
		public decimal CurrentPayment { get; set; }
		public bool Calculate { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
	}

	public class ManualPaymentMatchingViewModelAllocatedNode : JournalEntityNodeBase<VodOrder>
	{
		public OrderStatus OrderStatus { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal ActualOrderSum { get; set; }
		public decimal AllocatedSum { get; set; }
		public decimal LastPayments { get; set; }
		public OrderPaymentStatus OrderPaymentStatus { get; set; }
	}
}

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
using QS.Project.Domain;
using NHibernate.Criterion;
using Vodovoz.Domain.Orders;
using NHibernate.Dialect.Function;
using NHibernate;
using QS.Commands;
using Vodovoz.Repositories.Payments;
using System.Linq;
using QS.Validation;
using QS.Banks.Domain;
using QSBanks.Repositories;
using QSProjectsLib;
using System.Text.RegularExpressions;
using QS.Project.Search;
using QS.Project.Journal.Search;
using System.Linq.Expressions;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels
{
	public class ManualPaymentMatchingVM : EntityTabViewModelBase<Payment>
	{
		private DateTime? startDate = DateTime.Now.AddMonths(-1);
		public DateTime? StartDate {
			get => startDate;
			set => SetField(ref startDate, value);
		}

		private DateTime? endDate = DateTime.Now;
		public DateTime? EndDate {
			get => endDate;
			set => SetField(ref endDate, value);
		}

		private OrderStatus? orderStatus;
		public OrderStatus? OrderStatusVM {
			get => orderStatus;
			set => SetField(ref orderStatus, value);
		}

		private OrderPaymentStatus? orderPaymentStatus;
		public OrderPaymentStatus? OrderPaymentStatusVM {
			get => orderPaymentStatus;
			set => SetField(ref orderPaymentStatus, value);
		}

		private decimal allocatedSum;
		public decimal AllocatedSum {
			get => allocatedSum;
			set => SetField(ref allocatedSum, value);
		}

		private decimal currentBalance;
		public decimal CurrentBalance {
			get => currentBalance;
			set => SetField(ref currentBalance, value);
		}
		
		private decimal counterpartyDebt;
		public decimal CounterpartyDebt {
			get => counterpartyDebt;
			set => SetField(ref counterpartyDebt, value);
		}

		public decimal SumToAllocate { get; set; }

		public decimal LastBalance { get; set; }

		public IJournalSearch Search { get; set; }

		private IList<ManualPaymentMatchingVMNode> listNodes { get; set; } = new List<ManualPaymentMatchingVMNode>();

		readonly ICommonServices commonServices;
		readonly SearchHelper searchHelper;

		public ManualPaymentMatchingVM(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			if(uowBuilder.IsNewEntity) {
				AbortOpening("Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания");
			}

			//Поиск
			Search = new SearchViewModel();
			searchHelper = new SearchHelper(Search);

			GetLastBalance();

			if(Entity.Status == PaymentState.undistributed)
				SumToAllocate = Entity.Total + LastBalance;
			else
				SumToAllocate = LastBalance;

			CurrentBalance = SumToAllocate - AllocatedSum;
			CreateCommands();

			GetCounterpatyDebt();
		}

		private void GetLastBalance()
		{
			if(Entity.Counterparty != null)
				LastBalance = PaymentsRepository.GetCounterpartyLastBalance(UoW, Entity.Counterparty.Id);
		}

		public void Calculate(ManualPaymentMatchingVMNode node)
		{
			if(CurrentBalance <= 0)
				return;

			var tempSum = AllocatedSum + node.ActualOrderSum - node.LastPayments - node.CurrentPayment;

			if(tempSum <= SumToAllocate) {
				AllocatedSum += node.ActualOrderSum - node.LastPayments - node.CurrentPayment;
				node.CurrentPayment = node.ActualOrderSum - node.LastPayments;
			} else {
				node.CurrentPayment += SumToAllocate - AllocatedSum;
				AllocatedSum = SumToAllocate;
			}

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		public void ReCalculate(ManualPaymentMatchingVMNode node)
		{
			if(node.CurrentPayment == 0)
				return;

			AllocatedSum -= node.CurrentPayment;

			node.CurrentPayment = 0;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = 0;

			UpdateCurrentBalance();
		}

		public void CurrentPaymentChangedByUser(ManualPaymentMatchingVMNode node)
		{
			if(node.CurrentPayment < 0) {
				node.CurrentPayment = node.OldCurrentPayment;
				return;
			}

			var difference = node.ActualOrderSum - node.LastPayments;

			if(difference == 0) {
				node.CurrentPayment = difference;
				return;
			}

			if(node.CurrentPayment > difference) {
				node.CurrentPayment = difference;
			}

			AllocatedSum += node.CurrentPayment - node.OldCurrentPayment;

			UpdateCounterpartyDebt(node);

			node.OldCurrentPayment = node.CurrentPayment;

			UpdateCurrentBalance();
		}

		public void CreateCommands()
		{
			CreateOpenOrderCommand();
			CreateAddCounterpatyCommand();
			CreateCompleteAllocation();
			CreateSaveViewModelCommand();
			CreateCloseViewModelCommand();
		}

		void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

		void UpdateCounterpartyDebt(ManualPaymentMatchingVMNode node) => CounterpartyDebt -= node.CurrentPayment - node.OldCurrentPayment;

		#region Commands
		public DelegateCommand SaveViewModelCommand { get; private set; }

		void CreateSaveViewModelCommand()
		{
			SaveViewModelCommand = new DelegateCommand(
				() => {
					AllocateOrders();

					if(Save()) {
						UoW.Commit();
						Close(false, QS.Navigation.CloseSource.Save);
					}
				},
				() => true
			);
		}
		
		public DelegateCommand CloseViewModelCommand { get; private set; }

		void CreateCloseViewModelCommand()
		{
			CloseViewModelCommand = new DelegateCommand(
				() => {
					Close(false, QS.Navigation.CloseSource.Cancel);
				},
				() => true
			);
		}
		
		public DelegateCommand<VodOrder> OpenOrderCommand { get; private set; }

		void CreateOpenOrderCommand()
		{
			OpenOrderCommand = new DelegateCommand<VodOrder>(
				order => {
					var dlg = new OrderDlg(order);
					dlg.HasChanges = false;
					dlg.SetDlgToReadOnly();
					TabParent.AddSlaveTab(this, dlg);
				},
				order => order != null
			);
		}

		public DelegateCommand<Payment> AddCounterpatyCommand { get; private set; }

		void CreateAddCounterpatyCommand()
		{
			AddCounterpatyCommand = new DelegateCommand<Payment>(
				payment => {

					var client = new Counterparty();
					client.Name = payment.CounterpartyName;
					client.FullName = payment.CounterpartyName;
					client.INN = payment.CounterpartyInn;
					client.KPP = payment.CounterpartyKpp ?? string.Empty;
					client.PaymentMethod = PaymentType.cashless;
					client.TypeOfOwnership = TryGetOrganizationType(payment.CounterpartyName);
					if(client.TypeOfOwnership != null)
						client.PersonType = PersonType.legal;
					else {
						if(AskQuestion($"Не удалось определить тип контрагента. Контрагент \"{payment.CounterpartyName}\" является юридическим лицом?"))
							client.PersonType = PersonType.legal;
						else
							client.PersonType = PersonType.natural;
					}

					Bank bank = FillBank(payment);

					client.AddAccount(new Account {
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

		void CreateCompleteAllocation()
		{
			CompleteAllocation = new DelegateCommand(
				() => {

					var valid = new QSValidator<Payment>(UoWGeneric.Root);
					if(valid.RunDlgIfNotValid())
						return;

					if(CurrentBalance < 0) {
						ShowWarningMessage("Остаток не может быть отрицательным!");
						return;
					}

					if(SumToAllocate == 0)
						return;

					if(CurrentBalance > 0) {
						if(!AskQuestion("Внимание! Имеется нераспределенный остаток. " +
							"Оставить его на балансе и завершить распределение?", "Внимание!"))
							return;
					}

					AllocateOrders();
					CreateOperations();

					foreach (var item in Entity.PaymentItems)
					{
						item.Order.OrderPaymentStatus = item.Order.ActualTotalSum > item.Sum
							? OrderPaymentStatus.PartiallyPaid
							: OrderPaymentStatus.Paid;
						
						UoW.Save(item.Order);
					}
					
					Entity.Status = PaymentState.completed;

					if(Save()) { 
						UoW.Commit();
						Close(false, QS.Navigation.CloseSource.Self);
					}
				},
				() => true
			);
		}
		
		#endregion Commands

		private Bank FillBank(Payment payment)
		{
			var bank = BankRepository.GetBankByBik(UoW, payment.CounterpartyBik);

			if(bank == null) {

				bank = new Bank {
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

		void NewCounterpartySaved(object sender, QS.Tdi.EntitySavedEventArgs e)
		{
			var client = e.Entity as Counterparty;

			Entity.Counterparty = client;
			Entity.CounterpartyAccount = client.DefaultAccount;
		}
		
		private void CreateOperations()
		{
			Entity.CreateIncomeOperation();
			UoW.Save(Entity.CashlessMovementOperation);

			foreach(PaymentItem item in Entity.ObservableItems) {
				item.CreateExpenseOperation();
				UoW.Save(item.CashlessMovementOperation);
			}
		}

		private void AllocateOrders()
		{
			var list = listNodes.Where(x => x.CurrentPayment > 0);

			foreach(var node in list) {

				var order = UoW.GetById<VodOrder>(node.Id);
				var sum = node.CurrentPayment + node.LastPayments;
				Entity.AddPaymentItem(order, node.CurrentPayment);
			}
		}

		public void ClearProperties()
		{
			AllocatedSum = default(int);
			CurrentBalance = SumToAllocate;
		}

		public IList<ManualPaymentMatchingVMNode> UpdateNodes()
		{
			ManualPaymentMatchingVMNode resultAlias = null;
			VodOrder orderAlias = null;
			OrderItem orderItemAlias = null;
			PaymentItem paymentItemAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Where(x => x.OrderStatus != OrderStatus.Canceled)
					.And(x => x.OrderStatus != OrderStatus.DeliveryCanceled)
					.And(x => x.OrderStatus != OrderStatus.NotDelivered)
					.Where(x => x.PaymentType == PaymentType.cashless);

			if(Entity.Counterparty != null)
				incomePaymentQuery.Where(x => x.Client == Entity.Counterparty);

			if(StartDate.HasValue && EndDate.HasValue)
				incomePaymentQuery.Where(x => x.BillDate >= StartDate && x.BillDate <= EndDate);

			if(OrderStatusVM != null)
				incomePaymentQuery.Where(x => x.OrderStatus == OrderStatusVM);

			if(OrderPaymentStatusVM != null)
				incomePaymentQuery.Where(x => x.OrderPaymentStatus == OrderPaymentStatusVM);

			var lastPayment = QueryOver.Of(() => paymentItemAlias)
					.Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
					.Select(Projections.Sum(() => paymentItemAlias.Sum));

			var totalSum = QueryOver.Of(() => orderItemAlias)
					.Where(x => x.Order.Id == orderAlias.Id)
					.Select(
						Projections.Sum(
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * IFNULL(?2, ?3) - ?4)"),
							NHibernateUtil.Decimal, new IProjection[] {
							Projections.Property(() => orderItemAlias.Price),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count),
							Projections.Property(() => orderItemAlias.DiscountMoney)})
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
				.TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingVMNode>())
				.List<ManualPaymentMatchingVMNode>();

			listNodes = resultQuery;

			return resultQuery;
		}

		public void GetCounterpatyDebt()
		{
			if(Entity.Counterparty != null)
				CounterpartyDebt = OrderSingletonRepository.GetInstance().GetCounterpartyDebt(UoW, Entity.Counterparty.Id);
		}

		private string TryGetOrganizationType(string name)
		{
			foreach(var pair in InformationHandbook.OrganizationTypes) {
				string pattern = string.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
				string fullPattern = string.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
					return pair.Key;

				regex = new Regex(fullPattern, RegexOptions.IgnoreCase);

				if(regex.IsMatch(name))
					return pair.Key;
			}
			return null;
		}

		private ICriterion GetSearchCriterion(params Expression<Func<object>>[] aliasPropertiesExpr) => 
			searchHelper.GetSearchCriterion(aliasPropertiesExpr);

		private ICriterion GetSearchCriterion<TRootEntity>(params Expression<Func<TRootEntity, object>>[] propertiesExpr) => 
			searchHelper.GetSearchCriterion(propertiesExpr);
	}

	public class ManualPaymentMatchingVMNode : JournalEntityNodeBase<VodOrder>
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
}

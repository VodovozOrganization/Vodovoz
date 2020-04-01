using System;
using Gamma.ColumnConfig;
using QS.DomainModel.UoW;
using QS.Services;
using QS.ViewModels;
using VodOrder = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Payments;
using Vodovoz.JournalNodes;
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
using Vodovoz.Domain.Operations;
using Vodovoz.Repositories.Payments;
using System.Linq;
using QS.Validation;

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

		public decimal SumToAllocate { get; set; }

		public decimal LastBalance { get; set; }

		private IList<ManualPaymentMatchingVMNode> listNodes { get; set; }

		readonly ICommonServices commonServices;

		public ManualPaymentMatchingVM(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory uowFactory,
			ICommonServices commonServices) : base(uowBuilder, uowFactory, commonServices)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));

			if(uowBuilder.IsNewEntity) {
				AbortOpening("Невозможно создать новую загрузку выписки из текущего диалога, необходимо использовать диалоги создания");
			}

			GetLastBalance();

			if(Entity.Status == PaymentState.undistributed)
				SumToAllocate = Entity.Total + LastBalance;
			else
				SumToAllocate = LastBalance;

			CurrentBalance = SumToAllocate - AllocatedSum;
			CreateCommands();
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

			var order = UoW.GetById<VodOrder>(node.Id);
			var tempSum = AllocatedSum + node.ActualOrderSum - node.LastPayments;

			if(tempSum <= SumToAllocate) {
				AllocatedSum += node.ActualOrderSum - node.LastPayments - node.CurrentPayment;
				node.CurrentPayment = node.ActualOrderSum - node.LastPayments;
			} else {
				node.CurrentPayment += SumToAllocate - AllocatedSum;
				AllocatedSum = SumToAllocate;
			}

			node.OldCurrentPayment = node.CurrentPayment;
			UpdateCurrentBalance();
		}

		public void ReCalculate(ManualPaymentMatchingVMNode node)
		{
			if(node.CurrentPayment == 0)
				return;

			var order = UoW.GetById<VodOrder>(node.Id);
			AllocatedSum -= node.CurrentPayment;

			node.OldCurrentPayment = 0;
			node.CurrentPayment = 0;

			UpdateCurrentBalance();
		}

		public void CurrentPaymentChangedByUser(ManualPaymentMatchingVMNode node)
		{
			if(node.CurrentPayment < 0) {
				node.CurrentPayment = node.OldCurrentPayment;
				return;
			}

			var order = UoW.GetById<VodOrder>(node.Id);
			AllocatedSum += node.CurrentPayment - node.OldCurrentPayment;

			node.OldCurrentPayment = node.CurrentPayment;
			UpdateCurrentBalance();
		}

		public void CreateCommands()
		{
			CreateOpenOrderCommand();
			CreateCompleteAllocation();
		}

		void UpdateCurrentBalance() => CurrentBalance = SumToAllocate - AllocatedSum;

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

					CreateOperations();
					Entity.Status = PaymentState.completed;
					base.Save();
					UoW.Commit();
				},
				() => true
			);
		}

		private void CreateOperations()
		{
			var list = listNodes.Where(x => x.CurrentPayment > 0);
			AllocateOrders(list);

			Entity.CreateOperation();
		}

		private void AllocateOrders(IEnumerable<ManualPaymentMatchingVMNode> nodes)
		{
			foreach(var node in nodes) {

				var order = UoW.GetById<VodOrder>(node.Id);
				Entity.ObservableItems.Add(new PaymentItem { Order = order, Payment = Entity, Sum = node.CurrentPayment});
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
			CashlessMovementOperation cashlessOperationAlias = null;

			var incomePaymentQuery = UoW.Session.QueryOver(() => orderAlias)
					.Left.JoinAlias(() => orderAlias.OrderItems, () => orderItemAlias)
					.Where(x => x.Client.Id == Entity.Counterparty.Id)
					.Where(x => x.PaymentType == PaymentType.cashless);

			if(StartDate.HasValue && EndDate.HasValue)
				incomePaymentQuery.Where(x => x.BillDate >= StartDate && x.BillDate <= EndDate);

			if(OrderStatusVM != null)
				incomePaymentQuery.Where(x => x.OrderStatus == OrderStatusVM);

			if(OrderPaymentStatusVM != null)
				incomePaymentQuery.Where(x => x.OrderPaymentStatus == OrderPaymentStatusVM);

			var lastPayment = QueryOver.Of(() => cashlessOperationAlias)
					.Left.JoinAlias(() => cashlessOperationAlias.PaymentItem, () => paymentItemAlias)
					.Where(() => paymentItemAlias.Order.Id == orderAlias.Id)
					.Select(Projections.Sum(() => cashlessOperationAlias.Expense));

			var totalSum = QueryOver.Of(() => orderItemAlias)
					.Where(x => x.Order.Id == orderAlias.Id)
					.Select(
						Projections.Sum(
							Projections.SqlFunction(new SQLFunctionTemplate(NHibernateUtil.Decimal, "(?1 * IFNULL(?2, ?3) - ?4)"),
							NHibernateUtil.Decimal, new IProjection[] {
							Projections.Property(() => orderItemAlias.Price),
							Projections.Property(() => orderItemAlias.ActualCount),
							Projections.Property(() => orderItemAlias.Count),
							Projections.Property(() => orderItemAlias.DiscountMoney)}
						)));

			var resultQuery = incomePaymentQuery
					.SelectList(list => list
				   	.SelectGroup(() => orderAlias.Id).WithAlias(() => resultAlias.Id)
				   	.Select(() => orderAlias.OrderStatus).WithAlias(() => resultAlias.OrderStatus)
				   	.Select(() => orderAlias.BillDate).WithAlias(() => resultAlias.OrderDate)
					.SelectSubQuery(totalSum).WithAlias(() => resultAlias.ActualOrderSum)
					.SelectSubQuery(lastPayment).WithAlias(() => resultAlias.LastPayments)
				)
				.TransformUsing(Transformers.AliasToBean<ManualPaymentMatchingVMNode>())
				.List<ManualPaymentMatchingVMNode>();

			listNodes = resultQuery;

			return resultQuery;
		}

		private void CheckProblems(Payment payment)
		{
			if(payment.Counterparty == null)
				ShowWarningMessage("Заполните поле контрагент!");

			if(payment.CounterpartyAccount == null)
				ShowWarningMessage("Расчетный счет с которого поступил платеж не обнаружен. " +
					"Добавьте информацию о счете с которого поступил платеж!");
		}

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
	}
}

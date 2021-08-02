using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using QS.Banks.Domain;
using System.Linq;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи",
		Nominative = "платёж",
		Prepositional = "платеже",
		PrepositionalPlural = "платежах")]

	[HistoryTrace]
	[EntityPermission]
	public class Payment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		int paymentNum;
		[Display(Name = "Номер")]
		public virtual int PaymentNum {
			get => paymentNum;
			set => SetField(ref paymentNum, value);
		}

		DateTime date;
		[Display(Name = "Дата")]
		public virtual DateTime Date {
			get => date;
			set => SetField(ref date, value);
		}

		decimal total;
		[Display(Name = "Сумма")]
		public virtual decimal Total {
			get => total;
			set => SetField(ref total, value);
		}

		IList<PaymentItem> paymentItems = new List<PaymentItem>();
		[Display(Name = "Строки платежа")]
		public virtual IList<PaymentItem> PaymentItems {
			get => paymentItems;
			set => SetField(ref paymentItems, value);
		}

		GenericObservableList<PaymentItem> observableItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PaymentItem> ObservableItems {
			get {
				observableItems = observableItems ?? new GenericObservableList<PaymentItem>(PaymentItems);
				return observableItems;
			}
		}

		CashlessMovementOperation cashlessMovementOperation;
		[Display(Name = "Операция передвижения безнала")]
		public virtual CashlessMovementOperation CashlessMovementOperation {
			get => cashlessMovementOperation;
			set => SetField(ref cashlessMovementOperation, value); 
		}

		Counterparty counterparty;
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value);
		}

		Account counterpartyAccount;
		public virtual Account CounterpartyAccount {
			get => counterpartyAccount;
			set => SetField(ref counterpartyAccount, value);
		}

		Organization organization;
		[Display(Name = "Организация")]
		public virtual Organization Organization {
			get => organization;
			set => SetField(ref organization, value);
		}

		Account organizationAccount;
		public virtual Account OrganizationAccount {
			get => organizationAccount;
			set => SetField(ref organizationAccount, value);
		}

		string paymentPurpose;
		[Display(Name = "Назначение платежа")]
		public virtual string PaymentPurpose {
			get => paymentPurpose;
			set => SetField(ref paymentPurpose, value);
		}

		PaymentState status;
		[Display(Name = "Статус платежа")]
		public virtual PaymentState Status {
			get => status;
			set => SetField(ref status, value);
		}

		CategoryProfit profitCategory;
		[Display(Name = "Категория дохода")]
		public virtual CategoryProfit ProfitCategory {
			get => profitCategory;
			set => SetField(ref profitCategory, value);
		}

		string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}

		private string counterpartyAcc;
		public virtual string CounterpartyAcc {						// р/сч плательщика
			get => counterpartyAcc;
			set => SetField(ref counterpartyAcc, value);
		} 

		private string counterpartyCurrentAcc;
		public virtual string CounterpartyCurrentAcc { 				// р/сч плательщика
			get => counterpartyCurrentAcc;
			set => SetField(ref counterpartyCurrentAcc, value);
		} 

		private string counterpartyInn;
		public virtual string CounterpartyInn {
			get => counterpartyInn;
			set=> SetField(ref counterpartyInn, value);
		}

		private string counterpartyKpp;
		public virtual string CounterpartyKpp {
			get => counterpartyKpp;
			set => SetField(ref counterpartyKpp, value);
		}

		private string counterpartyName;
		public virtual string CounterpartyName {
			get => counterpartyName;
			set => SetField(ref counterpartyName, value);
		}

		private string counterpartyBank;
		public virtual string CounterpartyBank {
			get => counterpartyBank;
			set => SetField(ref counterpartyBank, value);
		}

		private string counterpartyBik;
		public virtual string CounterpartyBik {
			get => counterpartyBik;
			set => SetField(ref counterpartyBik, value);
		}

		private string counterpartyCorrespondentAcc;
		public virtual string CounterpartyCorrespondentAcc {
			get => counterpartyCorrespondentAcc;
			set => SetField(ref counterpartyCorrespondentAcc, value);
		}

		public virtual string NumOrders { get; set; }

		public Payment() { }

		public Payment(TransferDocument doc, Organization org, Counterparty counterparty)
		{
			PaymentNum = int.Parse(doc.DocNum);
			Date = doc.Date;
			Total = doc.Total;
			CounterpartyInn = doc.PayerInn;
			CounterpartyKpp = doc.PayerKpp;
			CounterpartyName = doc.PayerName;
			PaymentPurpose = doc.PaymentPurpose;
			CounterpartyBank = doc.PayerBank;
			CounterpartyAcc = doc.PayerAccount;
			CounterpartyCurrentAcc = doc.PayerCurrentAccount;
			CounterpartyCorrespondentAcc = doc.PayerCorrespondentAccount;
			CounterpartyBik = doc.PayerBik;

			if(org != null) {
				Organization = org;
				OrganizationAccount = org.Accounts.FirstOrDefault(acc => acc.Number == doc.RecipientCurrentAccount);
			}

			if(counterparty != null) {
				Counterparty = counterparty;
				CounterpartyAccount = counterparty.Accounts.FirstOrDefault(acc => acc.Number == doc.PayerCurrentAccount);
			}
		}

		public virtual void AddPaymentItem(Order order)
		{
			var paymentItem = new PaymentItem 
			{
				Order = order,
				Payment = this,
				Sum = order.ActualTotalSum
			};

			ObservableItems.Add(paymentItem);
		}

		public virtual void AddPaymentItem(Order order, decimal sum)
		{
			var item = ObservableItems.SingleOrDefault(x => x.Order.Id == order.Id);

			if(item == null) {

				var paymentItem = new PaymentItem {
					Order = order,
					Payment = this,
					Sum = sum
				};

				ObservableItems.Add(paymentItem);
			}
			else
				item.Sum += sum;
		}

		public virtual void UpdateAllocatedSum(IUnitOfWork uow, int orderId, decimal sum) {
			var item = ObservableItems.SingleOrDefault(x => x.Order.Id == orderId);

			if (sum != 0) {
				item?.UpdateSum(sum);
			}
			else {
				RemovePaymentItem(uow, item);
			}
		}

		private void RemovePaymentItem(IUnitOfWork uow, PaymentItem item) {
			if (item.CashlessMovementOperation != null) {
				uow.Delete(item.CashlessMovementOperation);
				item.CashlessMovementOperation = null;
			}

			ObservableItems.Remove(item);
		}

		public virtual void CreateIncomeOperation()
		{
			if (CashlessMovementOperation == null)
			{
				CashlessMovementOperation = new CashlessMovementOperation
				{
					Income = Total,
					Counterparty = Counterparty,
					OperationTime = DateTime.Now
				};
			}
		}
		
		public virtual void UpdateIncomeOperation(bool sumFromPayment, decimal sum = 0M) {
			if (CashlessMovementOperation == null) {
				CreateIncomeOperation();
			}
			else {
				CashlessMovementOperation.Income = sumFromPayment ? Total : sum;
				CashlessMovementOperation.Counterparty = Counterparty;
				CashlessMovementOperation.OperationTime = DateTime.Now;
			}
		}
		
		public virtual Payment CreatePaymentForReturnMoneyToClientBalance(decimal paymentSum, int orderId)
		{
			return new Payment
			{
				PaymentNum = PaymentNum,
				Date = DateTime.Now,
				Total = paymentSum,
				ProfitCategory = ProfitCategory,
				PaymentPurpose = $"Возврат суммы оплаты заказа №{orderId} на баланс клиента",
				Organization = Organization,
				Counterparty = Counterparty,
				CounterpartyName = counterpartyName,
				Status = PaymentState.undistributed
			};
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
				yield return new ValidationResult("Заполните контрагента.", new[] { nameof(Counterparty) });
		}
	}

	public enum PaymentState
	{
		[Display(Name = "Нераспределен")]
		undistributed,
		[Display(Name = "Распределен")]
		distributed,
		[Display(Name = "Завершен")]
		completed
	}

	public class PaymentStateStringType : NHibernate.Type.EnumStringType
	{
		public PaymentStateStringType() : base(typeof(PaymentState))
		{
		}
	}
}

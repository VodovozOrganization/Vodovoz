using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Client;
using QS.Banks.Domain;
using System.Linq;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Payments
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "платежи",
		Nominative = "платёж",
		Prepositional = "платеже",
		PrepositionalPlural = "платежах")]

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

		IList<Order> orders = new List<Order>();
		[Display(Name = "Заказы")]
		public virtual IList<Order> Orders {
			get => orders;
			set => SetField(ref orders, value);
		}

		GenericObservableList<Order> observableOrders;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<Order> ObservableOrders {
			get {
				observableOrders = observableOrders ?? new GenericObservableList<Order>(Orders);
				return observableOrders;
			}
		}

		IList<CashlessIncomeOperation> cashlessIncomeOperations = new List<CashlessIncomeOperation>();
		[Display(Name = "Операция передвижения безнала")]
		public virtual IList<CashlessIncomeOperation> CashlessIncomeOperations {
			get => cashlessIncomeOperations;
			set => SetField(ref cashlessIncomeOperations, value);
		}

		GenericObservableList<CashlessIncomeOperation> observableCashlessOperations;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CashlessIncomeOperation> ObservableCashlessOperations {
			get {
				observableCashlessOperations = observableCashlessOperations ?? 
					new GenericObservableList<CashlessIncomeOperation>(CashlessIncomeOperations);

				return observableCashlessOperations;
			}
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

		public virtual bool IsDuplicate { get; set; }

		public Payment() { }

		public Payment(string num, DateTime date, decimal total, string clientName, string clientInn, string clientKpp,
			string clientBank, string clientAcc, string clientCurAcc, string clientCorrespondentAcc, string clientBik,
			string paymentPurpose, string orgCurrentAccount, Organization org, Counterparty counterparty) 
		{
			PaymentNum = int.Parse(num);
			Date = date;
			Total = total;
			CounterpartyInn = clientInn;
			CounterpartyKpp = clientKpp;
			CounterpartyName = clientName;
			PaymentPurpose = paymentPurpose;
			CounterpartyBank = clientBank;
			CounterpartyAcc = clientAcc;
			CounterpartyCurrentAcc = clientCurAcc;
			CounterpartyCorrespondentAcc = clientCorrespondentAcc;
			CounterpartyBik = clientBik;

			if(org != null) {
				Organization = org;
				OrganizationAccount = org.Accounts.FirstOrDefault(acc => acc.Number == orgCurrentAccount);
			}

			if(counterparty != null) {
				Counterparty = counterparty;
				CounterpartyAccount = counterparty.Accounts.FirstOrDefault(acc => acc.Number == clientCurAcc);
			}
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(Counterparty == null)
				yield return new ValidationResult("Контрагент не найден.", new[] { nameof(Counterparty) });

			if(CounterpartyAccount == null)
				yield return new ValidationResult("Расчетный счет не распознан.", new[] { nameof(CounterpartyAccount) });
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

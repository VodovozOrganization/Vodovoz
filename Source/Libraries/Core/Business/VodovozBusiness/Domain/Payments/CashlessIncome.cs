using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QS.Banks.Domain;
using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Payments;
using Vodovoz.Services;

namespace VodovozBusiness.Domain.Payments
{
	/// <summary>
	/// Безналичный приход от юр лиц
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "Приходы по безналу",
		Nominative = "Приход по безналу",
		Prepositional = "Приходе по безналу",
		PrepositionalPlural = "Приходах по безналу")]
	//[HistoryTrace]
	public class CashlessIncome : PropertyChangedBase, IDomainObject
	{
		private DateTime _date;
		private int _number;
		private decimal _total;
		private string _paymentPurpose;
		private string _payerAcc;
		private string _payerCurrentAcc;
		private string _payerInn;
		private string _payerKpp;
		private string _payerName;
		private string _payerBank;
		private string _payerBankBik;
		private string _payerCorrespondentAcc;
		private bool _isManuallyCreated;
		private Organization _organization;
		private Account _organizationAccount;
		private IObservableList<Payment> _payments = new ObservableList<Payment>();
		
		public CashlessIncome() { }
		
		protected CashlessIncome(TransferDocument doc, Organization org)
		{
			Number = int.Parse(doc.DocNum);
			Date = doc.Date;
			Total = doc.Total;
			PayerInn = doc.PayerInn;
			PayerKpp = doc.PayerKpp;
			PayerName = doc.PayerName;
			PaymentPurpose = doc.PaymentPurpose;
			PayerBank = doc.PayerBank;
			PayerAcc = doc.PayerAccount;
			PayerCurrentAcc = doc.PayerCurrentAccount;
			PayerCorrespondentAcc = doc.PayerCorrespondentAccount;
			PayerBankBik = doc.PayerBik;

			if(org != null)
			{
				Organization = org;
				OrganizationAccount = org.Accounts.FirstOrDefault(acc => acc.Number == doc.RecipientCurrentAccount);
			}
		}

		public virtual int Id { get; set; }
		
		/// <summary>
		/// Дата
		/// </summary>
		[Display(Name = "Дата")]
		public virtual DateTime Date
		{
			get => _date;
			set => _date = value;
		}
		
		/// <summary>
		/// Номер
		/// </summary>
		[Display(Name = "Номер")]
		public virtual int Number
		{
			get => _number;
			set => _number = value;
		}

		/// <summary>
		/// Сумма
		/// </summary>
		[Display(Name = "Сумма")]
		public virtual decimal Total
		{
			get => _total;
			set => _total = value;
		}
		
		/// <summary>
		/// Назначение платежа
		/// </summary>
		[Display(Name = "Назначение платежа")]
		public virtual string PaymentPurpose
		{
			get => _paymentPurpose;
			set => _paymentPurpose = value;
		}
		
		/// <summary>
		/// Р/сч плательщика
		/// </summary>
		public virtual string PayerAcc
		{
			get => _payerAcc;
			set => _payerAcc = value;
		}

		/// <summary>
		/// Р/сч плательщика
		/// </summary>
		public virtual string PayerCurrentAcc
		{
			get => _payerCurrentAcc;
			set => _payerCurrentAcc = value;
		}

		/// <summary>
		/// ИНН плательщика
		/// </summary>
		public virtual string PayerInn
		{
			get => _payerInn;
			set => _payerInn = value;
		}

		/// <summary>
		/// КПП плательщика
		/// </summary>
		public virtual string PayerKpp
		{
			get => _payerKpp;
			set => _payerKpp = value;
		}

		/// <summary>
		/// Наименование плательщика
		/// </summary>
		public virtual string PayerName
		{
			get => _payerName;
			set => _payerName = value;
		}

		/// <summary>
		/// Банк плательщика
		/// </summary>
		public virtual string PayerBank
		{
			get => _payerBank;
			set => _payerBank = value;
		}

		/// <summary>
		/// БИК банка плательщика
		/// </summary>
		public virtual string PayerBankBik
		{
			get => _payerBankBik;
			set => _payerBankBik = value;
		}

		/// <summary>
		/// К/сч плательщика
		/// </summary>
		public virtual string PayerCorrespondentAcc
		{
			get => _payerCorrespondentAcc;
			set => _payerCorrespondentAcc = value;
		}
		
		/// <summary>
		/// Создан вручную
		/// </summary>
		[Display(Name = "Создан вручную")]
		public virtual bool IsManuallyCreated
		{
			get => _isManuallyCreated;
			set => _isManuallyCreated = value;
		}
		
		/// <summary>
		/// Организация
		/// </summary>
		[Display(Name = "Организация")]
		public virtual Organization Organization
		{
			get => _organization;
			set => _organization = value;
		}
		
		public virtual Account OrganizationAccount
		{
			get => _organizationAccount;
			set => _organizationAccount = value;
		}

		public virtual IObservableList<Payment> Payments
		{
			get => _payments;
			set => _payments = value;
		}
		
		public virtual void UpdatePayerDetails(Counterparty counterparty)
		{
			PayerInn = counterparty.INN;
			PayerKpp = counterparty.KPP;
			PayerName = counterparty.Name;
		}
		
		public virtual void UpdatePayerAccountDetails(Account defaultAccount)
		{
			PayerBank = defaultAccount.InBank?.Name;
			PayerBankBik = defaultAccount.InBank?.Bik;
			PayerCurrentAcc = defaultAccount.Number;
			PayerAcc = defaultAccount.Number;
			PayerCorrespondentAcc = defaultAccount.BankCorAccount?.CorAccountNumber;
		}

		public virtual bool TryAddNewPayment(decimal allocatedSum, out Payment payment)
		{
			if(allocatedSum >= Total)
			{
				payment = null;
				return false;
			}

			payment = CreateNewPayment();
			Payments.Add(payment);
			
			return true;
		}

		public static CashlessIncome Create(TransferDocument doc, Organization org, Counterparty counterparty)
		{
			var income = new CashlessIncome(doc, org);
			var payment = Payment.Create();
			
			if(counterparty != null)
			{
				payment.Counterparty = counterparty;
				payment.CounterpartyAccount = counterparty.Accounts.FirstOrDefault(acc => acc.Number == doc.PayerCurrentAccount);
			}

			income.Payments.Add(payment);

			return income;
		}

		public virtual void DefaultManuallyIncome(
			int paymentNum,
			int organizationId,
			PaymentState paymentState,
			IPaymentSettings paymentSettings,
			int? counterpartyId = null,
			DateTime? date = null)
		{
			var payment = Payment.CreateDefaultManuallyIncome(
				paymentNum,
				paymentState,
				paymentSettings,
				counterpartyId,
				date);
			
			payment.CashlessIncome = this;
			
			Organization = new Organization
			{
				Id = organizationId
			};
			
			Payments.Add(payment);
		}

		public virtual void UpdateFirstPayment(Counterparty counterparty, ProfitCategory profitCategory, string comment)
		{
			var firstPayment = Payments.FirstOrDefault();

			if(firstPayment is null)
			{
				return;
			}

			firstPayment.Counterparty = counterparty;
			firstPayment.ProfitCategory = profitCategory;
			firstPayment.Comment = comment;
		}

		//4914 Feature
		private Payment CreateNewPayment()
		{
			var payment = Payment.Create();
			
			//payment.Total = Total;
			//payment.ProfitCategory = ;
			payment.Date = Date;
			payment.PaymentNum = Number;
			payment.CashlessIncome = this;
			payment.Status = PaymentState.undistributed;
			
			return payment;
		}
	}
}

using System;
using System.ComponentModel.DataAnnotations;
using QSBanks;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Accounting
{
	[OrmSubject (Gender = GrammaticalGender.Masculine,
		NominativePlural = "поступления на р/с",
		Nominative = "поступление на р/с")]
	public class AccountIncome: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int number;

		public virtual int Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date);
				if (MoneyOperation.OperationTime != Date)
					MoneyOperation.OperationTime = Date;
			}
		}

		decimal total;

		public virtual decimal Total {
			get { return total; }
			set { SetField (ref total, value, () => Total);
				if (MoneyOperation.Money != Total)
				{
					MoneyOperation.Money = Total;
				}}
		}

		string description;

		public virtual string Description {
			get { return description; }
			set { SetField (ref description, value, () => Description); }
		}

		Organization organization;

		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		Account organizationAccount;

		public virtual Account OrganizationAccount {
			get { return organizationAccount; }
			set { SetField (ref organizationAccount, value, () => OrganizationAccount); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty { 
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty);
				if (MoneyOperation.Counterparty != Counterparty)
				{
					MoneyOperation.Counterparty = Counterparty;
				}}
		}

		Account counterpartyAccount;

		public virtual Account CounterpartyAccount {
			get { return counterpartyAccount; }
			set { SetField (ref counterpartyAccount, value, () => CounterpartyAccount); }
		}

		IncomeCategory category;

		public virtual IncomeCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}
			
		private MoneyMovementOperation moneyOperation = new MoneyMovementOperation();

		[Display(Name = "Операция движения денег")]
		public virtual MoneyMovementOperation MoneyOperation
		{
			get { return moneyOperation; }
			set { SetField(ref moneyOperation, value, () => MoneyOperation); }
		}

		public string Title
		{
			get {
				return string.Format("Поступление денег на р/с №{1} от {2:d} на сумму {3}₽", Id, Date, Total);
			}
		}

		#endregion

		public AccountIncome ()
		{
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}



using System;
using System.ComponentModel.DataAnnotations;
using QSBanks;
using QSOrmProject;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Accounting
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "операции расхода",
		Nominative = "операция расхода")]
	public class AccountExpense: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		int number;

		public virtual int Number {
			get { return number; }
			set { SetField (ref number, value, () => Number); }
		}

		DateTime date;

		[Display (Name = "Дата")]
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		decimal total;

		public virtual decimal Total {
			get { return total; }
			set { SetField (ref total, value, () => Total); }
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
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		Account counterpartyAccount;

		public virtual Account CounterpartyAccount {
			get { return counterpartyAccount; }
			set { SetField (ref counterpartyAccount, value, () => CounterpartyAccount); }
		}

		Employee employee;

		public virtual Employee Employee { 
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
		}

		Account employeeAccount;

		public virtual Account EmployeeAccount {
			get { return employeeAccount; }
			set { SetField (ref employeeAccount, value, () => EmployeeAccount); }
		}

		ExpenseCategory category;

		public virtual ExpenseCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}


		#endregion

		public AccountExpense ()
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


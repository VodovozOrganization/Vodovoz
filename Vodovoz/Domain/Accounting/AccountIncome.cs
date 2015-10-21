using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using QSBanks;

namespace Vodovoz.Domain.Accounting
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "названия",
		Nominative = "название")]
	public class AccountIncome: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		decimal total { get; set; }

		public virtual decimal Total {
			get { return total; }
			set { SetField (ref total, value, () => Total); }
		}

		string description { get; set; }

		public virtual string Description {
			get { return description; }
			set { SetField (ref description, value, () => Description); }
		}

		Organization organization { get; set; }

		public virtual Organization Organization {
			get { return organization; }
			set { SetField (ref organization, value, () => Organization); }
		}

		Account organizationAccount { get; set; }

		public virtual Account OrganizationAccount {
			get { return organizationAccount; }
			set { SetField (ref organizationAccount, value, () => OrganizationAccount); }
		}

		Counterparty counterparty { get; set; }

		public virtual Counterparty Counterparty { 
			get { return Counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		Account counterpartyAccount { get; set; }

		public virtual Account CounterpartyAccount {
			get { return counterpartyAccount; }
			set { SetField (ref counterpartyAccount, value, () => CounterpartyAccount); }
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



using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "авансовые отчеты",
		Nominative = "авансовый отчет")]
	public class AdvanceReport : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		Employee casher;

		[Display (Name = "Кассир")]
		public virtual Employee Casher {
			get { return casher; }
			set { SetField (ref casher, value, () => Casher); }
		}

		Employee accountable;

		[Display (Name = "Подотчетное лицо")]
		public virtual Employee Accountable {
			get { return accountable; }
			set { SetField (ref accountable, value, () => Accountable); }
		}

		ExpenseCategory expenseCategory;

		[Display (Name = "Статья расхода")]
		public virtual ExpenseCategory ExpenseCategory {
			get { return expenseCategory; }
			set { SetField (ref expenseCategory, value, () => ExpenseCategory); }
		}

		string description;

		[Display (Name = "Основание")]
		public virtual string Description {
			get { return description; }
			set { SetField (ref description, value, () => Description); }
		}

		decimal money;

		[Display (Name = "Сумма")]
		public virtual decimal Money {
			get { return money; }
			set {
				SetField (ref money, value, () => Money); 
			}
		}

		public virtual string Title { 
			get { return String.Format ("Авансовый отчет №{0} от {1:d}", Id, Date); }
		}
			
		#endregion

		public AdvanceReport ()
		{
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Accountable == null)
				yield return new ValidationResult ("Подотчетное лицо должно быть указано.",
					new[] { this.GetPropertyName (o => o.Accountable) });
			if (ExpenseCategory == null)
				yield return new ValidationResult ("Статья расхода должна быть указана.",
					new[] { this.GetPropertyName (o => o.ExpenseCategory) });

			if(Money <= 0)
				yield return new ValidationResult ("Сумма должна иметь значение отличное от 0.",
					new[] { this.GetPropertyName (o => o.Money) });

			if(String.IsNullOrWhiteSpace (Description))
				yield return new ValidationResult ("Основание должно быть заполнено.",
					new[] { this.GetPropertyName (o => o.Description) });
			
		}

		#endregion

	}
}


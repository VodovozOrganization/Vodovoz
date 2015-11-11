using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "расходные одера",
		Nominative = "расходный ордер")]
	public class Expense : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private ExpenseType typeOperation;

		[Display (Name = "Тип операции")]
		public ExpenseType TypeOperation {
			get { return typeOperation; }
			set {
				if(SetField (ref typeOperation, value, () => TypeOperation))
				{
					if (TypeOperation == ExpenseType.Advance && AdvanceClosed == null)
						AdvanceClosed = false;
					if (TypeOperation != ExpenseType.Advance && AdvanceClosed.HasValue)
						AdvanceClosed = null;
				}
			}
		}

		Employee casher;

		[Display (Name = "Кассир")]
		public virtual Employee Casher {
			get { return casher; }
			set { SetField (ref casher, value, () => Casher); }
		}

		Employee employee;

		[Display (Name = "Сотрудник")]
		public virtual Employee Employee {
			get { return employee; }
			set { SetField (ref employee, value, () => Employee); }
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

		bool? advanceClosed;

		[Display (Name = "Аванс закрыт")]
		public virtual bool? AdvanceClosed {
			get { return advanceClosed; }
			set { SetField (ref advanceClosed, value, () => AdvanceClosed); }
		}

		public virtual string Title { 
			get { return String.Format ("Расходный ордер №{0} от {1:d}", Id, Date); }
		}
			
		#endregion

		public Expense ()
		{
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(TypeOperation == ExpenseType.Advance)
			{
				if (Employee == null)
					yield return new ValidationResult ("Подотчетное лицо должно быть указано.",
						new[] { this.GetPropertyName (o => o.Employee) });
				if (ExpenseCategory == null)
					yield return new ValidationResult ("Статья расхода под которую выдаются деньги должна быть заполнена.",
						new[] { this.GetPropertyName (o => o.ExpenseCategory) });

				if (!AdvanceClosed.HasValue)
					yield return new ValidationResult ("Отсутствует иформация поле Закрытия аванса. Поле не может быть null.",
						new[] { this.GetPropertyName (o => o.AdvanceClosed) });
				
			}
			else
			{
				if (AdvanceClosed.HasValue)
					yield return new ValidationResult (String.Format ("Если это не выдача под аванс {0} должно быть null.", this.GetPropertyName (o => o.AdvanceClosed)),
						new[] { this.GetPropertyName (o => o.AdvanceClosed) });
			}

			if(TypeOperation == ExpenseType.Expense)
			{
				if (ExpenseCategory == null)
					yield return new ValidationResult ("Статья расхода должна быть указана.",
						new[] { this.GetPropertyName (o => o.ExpenseCategory) });
			}

			if(Money <= 0)
				yield return new ValidationResult ("Сумма должна иметь значение отличное от 0.",
					new[] { this.GetPropertyName (o => o.Money) });

			if(String.IsNullOrWhiteSpace (Description))
				yield return new ValidationResult ("Основание должно быть заполнено.",
					new[] { this.GetPropertyName (o => o.Description) });
								
		}

		#endregion
	}

	public enum ExpenseType
	{
		[Display (Name = "Прочий расход")]
		Expense,
		[Display (Name = "Аванс подотчетному лицу")]
		Advance,
	}

	public class ExpenseTypeStringType : NHibernate.Type.EnumStringType
	{
		public ExpenseTypeStringType () : base (typeof(ExpenseType))
		{
		}
	}

}


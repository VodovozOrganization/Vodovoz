using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "приходные одера",
		Nominative = "приходный ордер")]
	public class Income : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		private DateTime date;

		[Display (Name = "Дата")]
		public DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private IncomeType typeOperation;

		[Display (Name = "Тип операции")]
		public IncomeType TypeOperation {
			get { return typeOperation; }
			set { SetField (ref typeOperation, value, () => TypeOperation); }
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

		Counterparty customer;

		[Display (Name = "Клиент")]
		public virtual Counterparty Customer {
			get { return customer; }
			set { SetField (ref customer, value, () => Customer); }
		}

		IncomeCategory incomeCategory;

		[Display (Name = "Статья дохода")]
		public virtual IncomeCategory IncomeCategory {
			get { return incomeCategory; }
			set { SetField (ref incomeCategory, value, () => IncomeCategory); }
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
			get { return String.Format ("Приходный ордер №{0} от {1:d}", Id, Date); }
		}
			
		#endregion

		public Income ()
		{
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(TypeOperation == IncomeType.Return)
			{
				if (Employee == null)
					yield return new ValidationResult ("Подотчетное лицо должно быть указано.",
						new[] { this.GetPropertyName (o => o.Employee) });
			}

			if(TypeOperation != IncomeType.Return)
			{
				if (IncomeCategory == null)
					yield return new ValidationResult ("Статья дохода должна быть указана.",
						new[] { this.GetPropertyName (o => o.IncomeCategory) });
			}

			if(Money <= 0)
				yield return new ValidationResult ("Сумма должна иметь значение отличное от 0.",
					new[] { this.GetPropertyName (o => o.Money) });
		}

		#endregion

	}

	public enum IncomeType
	{
		[Display (Name = "Прочий приход")]
		Common,
		[Display (Name = "Оплата покупателя")]
		Payment,
//		[Display (Name = "Приход от водителя")]   Временно пока не используется в закрытии маршрутника.
//		DriverReport,
		[Display (Name = "Возврат от подотчетного лица")]
		Return,
	}

	public class IncomeTypeStringType : NHibernate.Type.EnumStringType
	{
		public IncomeTypeStringType () : base (typeof(IncomeType))
		{
		}
	}

}


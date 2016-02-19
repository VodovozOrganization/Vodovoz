using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;

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
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private IncomeType typeOperation;

		[Display (Name = "Тип операции")]
		public virtual IncomeType TypeOperation {
			get { return typeOperation; }
			set { 
				if(SetField (ref typeOperation, value, () => TypeOperation))
				{
					switch(TypeOperation)
					{
					case IncomeType.Return:
						IncomeCategory = null;
						Customer = null;
						break;
					case IncomeType.Common:
						ExpenseCategory = null;
						Customer = null;
						break;
					case IncomeType.Payment:
						ExpenseCategory = null;
						break;
					}
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

		ExpenseCategory expenseCategory;

		/// <summary>
		/// Используется только для отслеживания возвратных возвратных денег с типом операции Return
		/// </summary>
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

		#endregion

		public virtual string Title { 
			get { return String.Format ("Приходный ордер №{0} от {1:d}", Id, Date); }
		}
			
		#region RunTimeOnly

		public virtual bool NoCloseAdvance { get; set;}

		#endregion

		public Income ()
		{
		}

		public virtual AdvanceClosing CloseAdvance(Expense expense)
		{
			if (TypeOperation != IncomeType.Return) {
				throw new InvalidOperationException ("Приходный ордер должен иметь тип '"+Gamma.Utilities.AttributeUtil.GetEnumTitle(IncomeType.Return)+"'");
			}
			if (expense.TypeOperation != ExpenseType.Advance) {
				throw new InvalidOperationException ("Расходный ордер должен иметь тип '"+Gamma.Utilities.AttributeUtil.GetEnumTitle(ExpenseType.Advance)+"'");
			}
			expense.AdvanceClosed = true;
			return new AdvanceClosing (this, expense);
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(TypeOperation == IncomeType.Return)
			{
				if (Employee == null)
					yield return new ValidationResult ("Подотчетное лицо должно быть указано.",
						new[] { this.GetPropertyName (o => o.Employee) });

				if (ExpenseCategory == null)
					yield return new ValidationResult ("Статья по которой брались деньги должна быть указана.",
						new[] { this.GetPropertyName (o => o.ExpenseCategory) });
				
			}

			if(TypeOperation != IncomeType.Return)
			{
				if (IncomeCategory == null)
					yield return new ValidationResult ("Статья дохода должна быть указана.",
						new[] { this.GetPropertyName (o => o.IncomeCategory) });
			}

			if(TypeOperation == IncomeType.Payment)
			{
				if (Customer == null)
					yield return new ValidationResult ("Клиент должен быть указан.",
						new[] { this.GetPropertyName (o => o.Customer) });
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


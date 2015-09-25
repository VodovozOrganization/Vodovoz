using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "расходные одера",
		Nominative = "расходный ордер")]
	public class Expense : PropertyChangedBase, IDomainObject
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
			get { return String.Format ("Расходный ордер №{0} от {1:d}", Id, Date); }
		}
			
		#endregion

		public Expense ()
		{
		}
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


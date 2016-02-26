using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Logistic;

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
		public virtual DateTime Date {
			get { return date; }
			set { SetField (ref date, value, () => Date); }
		}

		private ExpenseType typeOperation;

		[Display (Name = "Тип операции")]
		public virtual ExpenseType TypeOperation {
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

		IList<AdvanceClosing> advanceCloseItems;

		[Display (Name = "Документы закрытия аванса")]
		public virtual IList<AdvanceClosing> AdvanceCloseItems {
			get { return advanceCloseItems; }
			set { SetField (ref advanceCloseItems, value, () => AdvanceCloseItems); }
		}

		RouteListClosing routeListClosing;

		public virtual RouteListClosing RouteListClosing
		{
			get{ return routeListClosing; }
			set
			{
				SetField(ref routeListClosing, value, () => RouteListClosing);
			}
		}

		#endregion

		#region Вычисляемые

		public virtual string Title { 
			get { return String.Format ("Расходный ордер №{0} от {1:d}", Id, Date); }
		}

		public virtual decimal ClosedMoney{
			get{
				if (AdvanceCloseItems == null)
					return 0;

				return AdvanceCloseItems.Sum(x => x.Money);
			}
		}

		public virtual decimal UnclosedMoney{
			get{
				return Money - ClosedMoney;
			}
		}

		#endregion

		#region Функции

		public virtual void CalculateCloseState()
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод CalculateCloseState() можно вызываться только для выдачи аванса.");

			if (AdvanceCloseItems == null)
			{
				AdvanceClosed = false;
				return;
			}

			AdvanceClosed = ClosedMoney == Money;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(Income income, decimal sum)
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() можно вызываться только для выдачи аванса.");
			
			var closing = new AdvanceClosing(this, income, sum);
			if (AdvanceCloseItems == null)
				AdvanceCloseItems = new List<AdvanceClosing>();
			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		public virtual AdvanceClosing AddAdvanceCloseItem(AdvanceReport report, decimal sum)
		{
			if (TypeOperation != ExpenseType.Advance)
				throw new InvalidOperationException("Метод AddAdvanceCloseItem() можно вызываться только для выдачи аванса.");

			var closing = new AdvanceClosing(this, report, sum);
			if (AdvanceCloseItems == null)
				AdvanceCloseItems = new List<AdvanceClosing>();
			AdvanceCloseItems.Add(closing);
			CalculateCloseState();
			return closing;
		}

		#endregion

		public Expense ()
		{
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
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


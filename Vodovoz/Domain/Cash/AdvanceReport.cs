using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;

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

		Income changeReturn;

		[Display (Name = "Возврат сдачи")]
		public virtual Income ChangeReturn {
			get { return changeReturn; }
			set { SetField (ref changeReturn, value, () => ChangeReturn); }
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

		public List<AdvanceClosing> CloseAdvances(out Expense surcharge, out Income returnChange, List<Expense> advances )
		{
			if (advances.Any (a => a.ExpenseCategory != ExpenseCategory))
				throw new InvalidOperationException ("Нельзя что бы авансовый отчет, закрывал авансы выданные по другим статьям.");

			surcharge = null; returnChange = null;

			decimal totalExpense = advances.Sum (a => a.Money);
			decimal balance = totalExpense - Money;
			List<AdvanceClosing> resultClosing = new List<AdvanceClosing> ();

			if(balance < 0)
			{
				surcharge = new Expense{
					Casher = Casher,
					Date = Date,
					Employee = Accountable,
					TypeOperation = ExpenseType.Advance,
					ExpenseCategory = ExpenseCategory,
					Money = Math.Abs (balance),
					Description = String.Format ("Доплата денежных средств сотруднику по авансовому отчету №{0}", Id),
					AdvanceClosed = true
				};
				resultClosing.Add (new AdvanceClosing(this, surcharge));
			}
			else if(balance > 0)
			{
				returnChange = new Income{
					Casher = Casher,
					Date = Date,
					Employee = Accountable,
					ExpenseCategory = ExpenseCategory,
					TypeOperation = IncomeType.Return,
					Money = Math.Abs (balance),
					Description = String.Format ("Возврат в кассу денежных средств по авансовому отчету №{0}", Id)
				};
				ChangeReturn = returnChange;
			}

			foreach(var adv in advances)
			{
				adv.AdvanceClosed = true;
				resultClosing.Add (new AdvanceClosing(this, adv));
			}

			return resultClosing;
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


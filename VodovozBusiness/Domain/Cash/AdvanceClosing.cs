using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Cash
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "закрытия авансов",
		Nominative = "связка с документом закрытия аванса")]
	public class AdvanceClosing : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		Expense advanceExpense;

		[Display (Name = "Выдача аванса")]
		public virtual Expense AdvanceExpense {
			get { return advanceExpense; }
			set { SetField (ref advanceExpense, value, () => AdvanceExpense); }
		}

		AdvanceReport advanceReport;

		[Display (Name = "Авансовый отчет")]
		public virtual AdvanceReport AdvanceReport {
			get { return advanceReport; }
			set { SetField (ref advanceReport, value, () => AdvanceReport); }
		}

		Income income;

		[Display (Name = "Приходной ордер")]
		public virtual Income Income {
			get { return income; }
			set { SetField (ref income, value, () => Income); }
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

		protected AdvanceClosing ()
		{
		}

		public AdvanceClosing (Expense advanceExpense, AdvanceReport advanceReport, decimal sum)
		{
			this.advanceExpense = advanceExpense;
			this.advanceReport = advanceReport;
			this.money = sum;
		}

		public AdvanceClosing (Expense advanceExpense, Income income, decimal sum)
		{
			this.advanceExpense = advanceExpense;
			this.income = income;
			this.money = sum;
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (AdvanceExpense == null)
				yield return new ValidationResult ("Выданный аванс должен быть заполнен.",
					new[] { this.GetPropertyName (o => o.AdvanceExpense) });
			if (AdvanceReport == null && Income==null)
				yield return new ValidationResult ("Авансовый отчет или приходной ордер должен быть заполнен.",
					new[] {this.GetPropertyName (o => o.AdvanceReport) });
		}

		#endregion

	}
}


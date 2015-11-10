using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Cash
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
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

		Income changeReturn;

		[Display (Name = "Возврат сдачи")]
		public virtual Income ChangeReturn {
			get { return changeReturn; }
			set { SetField (ref changeReturn, value, () => ChangeReturn); }
		}
			
		#endregion

		public AdvanceClosing ()
		{
		}

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (AdvanceExpense == null)
				yield return new ValidationResult ("Выданный аванс должен быть заполнен.",
					new[] { this.GetPropertyName (o => o.AdvanceExpense) });
			if (ChangeReturn == null && AdvanceReport == null)
				yield return new ValidationResult ("Авансовый отчет или возврат сдачи должны быть заполнены.",
					new[] { this.GetPropertyName (o => o.ChangeReturn), this.GetPropertyName (o => o.AdvanceReport) });
		}

		#endregion

	}
}


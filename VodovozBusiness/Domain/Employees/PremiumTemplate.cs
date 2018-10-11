using System;
using QS.DomainModel.Entity;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	[OrmSubject(Gender = GrammaticalGender.Masculine,
		NominativePlural = "шаблоны комментариев для премий",
		Nominative = "шаблон комментария для премии")]
	public class PremiumTemplate: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		#region Свойства

		public virtual int Id { get; set; }

		string reason;

		[Display(Name = "Причина")]
		public virtual string Reason {
			get { return reason; }
			set { SetField(ref reason, value, () => Reason); }
		}

		private decimal premiumMoney;

		[Display(Name = "Сумма премии")]
		public virtual decimal PremiumMoney {
			get { return premiumMoney; }
			set { SetField(ref premiumMoney, value, () => PremiumMoney); }
		}


		#endregion

		public PremiumTemplate()
		{
			Reason = String.Empty;
		}

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrEmpty(Reason))
				yield return new ValidationResult("Текст комментария должен быть заполнен.", new[] { "Comment" });
		}

		#endregion
	}
}
